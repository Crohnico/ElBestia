using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public static partial class CombatSimulator
    {
        public static CombatSimulationResult Run(ChampionSO leftChampion, ChampionSO rightChampion, CombatSimulationOptions options)
        {
            return Run(leftChampion != null ? leftChampion.ToData() : null, rightChampion != null ? rightChampion.ToData() : null, options);
        }

        public static CombatSimulationResult Run(ChampionData leftChampion, ChampionData rightChampion, CombatSimulationOptions options)
        {
            options ??= new CombatSimulationOptions();
            var rng = new System.Random(options.seed);
            var result = new CombatSimulationResult { seed = options.seed };
            var left = new SimChampion(CombatSimulationSide.Left, leftChampion);
            var right = new SimChampion(CombatSimulationSide.Right, rightChampion);
            left.Enemy = right;
            right.Enemy = left;

            AddEvent(result, 0f, CombatSimulationEventType.MatchStart, CombatSimulationSide.None, CombatSimulationSide.None, null, SkillElement.None, ChargeType.None, 0, 0, $"seed={options.seed}");
            ApplyOpeningCharges(left, result, 0f);
            ApplyOpeningCharges(right, result, 0f);
            Schedule(left, 0f, rng, result);
            Schedule(right, 0f, rng, result);

            float time = 0f;
            while (left.IsAlive && right.IsAlive && time <= options.maxDurationSeconds && result.events.Count < options.maxEvents)
            {
                SimChampion actor = PickNextActor(left, right, rng);
                if (actor.NextActionTime > options.maxDurationSeconds)
                {
                    break;
                }

                time = actor.NextActionTime;
                AddEvent(result, time, CombatSimulationEventType.TurnStart, actor.Side, actor.Enemy.Side, null, SkillElement.None, ChargeType.None, 0, actor.CurrentHealth, null);
                ApplyStartTurnEffects(actor, result, time, options, rng);
                if (!actor.IsAlive || !actor.Enemy.IsAlive)
                {
                    break;
                }

                ExecuteTurn(actor, time, options, rng, result);
                if (!actor.IsAlive || !actor.Enemy.IsAlive)
                {
                    break;
                }

                actor.CompletedActions++;
                Schedule(actor, time, rng, result);
            }

            result.completed = true;
            result.duration = Mathf.Min(time, options.maxDurationSeconds);
            result.leftHealth = left.CurrentHealth;
            result.rightHealth = right.CurrentHealth;
            result.winner = ResolveWinner(left, right);
            AddEvent(result, result.duration, CombatSimulationEventType.MatchEnd, result.winner, CombatSimulationSide.None, null, SkillElement.None, ChargeType.None, 0, 0, $"leftHp={left.CurrentHealth} rightHp={right.CurrentHealth}");
            return result;
        }

        private static void ExecuteTurn(SimChampion actor, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            int skillIndex = PickSkillIndex(actor, time, rng);
            SkillData skill = GetSkill(actor, skillIndex);
            if (skill == null)
            {
                return;
            }

            SetSkillCooldown(actor, skillIndex, skill, time);
            AddEvent(result, time, CombatSimulationEventType.SkillSelected, actor.Side, actor.Enemy.Side, skill, skill.element, ChargeType.None, 0, actor.Enemy.CurrentHealth, null);
            ApplyCaltrops(actor, time, options, rng, result);
            if (!actor.IsAlive)
            {
                return;
            }

            ExecuteActions(actor, skill, skill.preCast, false, 1f, time, options, rng, result);
            ExecuteActions(actor, skill, skill.cast, true, 1f, time, options, rng, result);
            ConsumeEndTurnCharges(actor, time, result);
            ExecuteActions(actor, skill, skill.postCast, false, 1f, time, options, rng, result);
            ExecuteSkillEcho(actor, skill, time, options, rng, result);
            ApplyAfterActionPerks(actor, time, result);
        }

        private static void ExecuteActions(SimChampion actor, SkillData skill, SkillAction[] actions, bool allowCounterattack, float damageMultiplier, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            if (actions == null)
            {
                return;
            }

            foreach (SkillAction action in actions)
            {
                ExecuteAction(actor, skill, action, allowCounterattack, damageMultiplier, time, options, rng, result);
            }
        }

        private static void ExecuteAction(SimChampion actor, SkillData skill, SkillAction action, bool allowCounterattack, float damageMultiplier, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            if (action == null)
            {
                return;
            }

            SimChampion target = action.target == SkillTarget.Self ? actor : actor.Enemy;
            switch (action.action)
            {
                case SkillActionType.DoDamage:
                    if (action.amount < 0)
                    {
                        Heal(target, CalculateOutgoingAmount(actor, -action.amount), time, result);
                    }
                    else
                    {
                        int damage = Mathf.RoundToInt(CalculateOutgoingSkillDamage(actor, action.amount, skill) * Mathf.Max(0f, damageMultiplier));
                        DealDamage(actor, target, damage, skill.element, CombatStatUtility.GetSkillWeapon(skill), allowCounterattack, true, true, time, options, rng, result);
                    }
                    break;
                case SkillActionType.IncreaseDamage:
                    AddCharge(target, ChargeType.Empowered, Mathf.Max(0, action.amount), actor, time, result);
                    break;
                case SkillActionType.WeakenEnemy:
                    AddCharge(target, ChargeType.Weakened, Mathf.Max(0, action.amount), actor, time, result);
                    break;
                case SkillActionType.GainCharges:
                case SkillActionType.ApplyCharges:
                    AddCharge(target, action.charge, Mathf.Max(0, action.amount), actor, time, result);
                    break;
            }
        }

        private static void Schedule(SimChampion actor, float startTime, System.Random rng, CombatSimulationResult result)
        {
            float delay = RollNextActionDelay(actor, rng);
            actor.ScheduledAt = startTime;
            actor.BaseActionTime = startTime + delay;
            actor.NextActionTime = actor.ScheduledAt + Mathf.Max(0.001f, actor.BaseActionTime - actor.ScheduledAt) * actor.GetActionTimeMultiplier();
            AddEvent(result, startTime, CombatSimulationEventType.TurnScheduled, actor.Side, actor.Enemy.Side, null, SkillElement.None, ChargeType.None, 0, actor.CurrentHealth, $"arrival={actor.NextActionTime:0.00}");
        }

        private static SimChampion PickNextActor(SimChampion left, SimChampion right, System.Random rng)
        {
            if (Mathf.Approximately(left.NextActionTime, right.NextActionTime))
            {
                return rng.NextDouble() < 0.5 ? left : right;
            }

            return left.NextActionTime <= right.NextActionTime ? left : right;
        }
    }
}
