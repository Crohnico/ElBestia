using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public static partial class CombatSimulator
    {
        private static void AddEvent(
            CombatSimulationResult result,
            float time,
            CombatSimulationEventType type,
            CombatSimulationSide actor,
            CombatSimulationSide target,
            SkillData skill,
            SkillElement element,
            ChargeType charge,
            int amount,
            int targetHealthAfter,
            string message)
        {
            if (result == null || result.events.Count >= 10000)
            {
                return;
            }

            result.events.Add(new CombatSimulationEvent
            {
                time = time,
                type = type,
                actor = actor,
                target = target,
                skillName = skill != null ? skill.skillName : string.Empty,
                element = element,
                charge = charge,
                amount = amount,
                targetHealthAfter = targetHealthAfter,
                message = message
            });
        }

        private static CombatSimulationSide ResolveWinner(SimChampion left, SimChampion right)
        {
            if (left.IsAlive && !right.IsAlive)
            {
                return CombatSimulationSide.Left;
            }

            if (right.IsAlive && !left.IsAlive)
            {
                return CombatSimulationSide.Right;
            }

            if (left.CurrentHealth == right.CurrentHealth)
            {
                return CombatSimulationSide.Draw;
            }

            return left.CurrentHealth > right.CurrentHealth ? CombatSimulationSide.Left : CombatSimulationSide.Right;
        }

        private static float RollNextActionDelay(SimChampion actor, System.Random rng)
        {
            int speedRating = actor.Stats != null ? actor.Stats.GetActionSpeedRating() : 10;
            float randomWindow = Mathf.Lerp(2.5f, 5.2f, (float)rng.NextDouble());
            float delay = randomWindow * 100f / (100f + Mathf.Max(0, speedRating));
            return Mathf.Max(0.35f, delay / GetOpeningActionSpeedMultiplier(actor));
        }

        private static void RetargetIfSpeedChanged(SimChampion actor, float previousMultiplier, float currentTime)
        {
            float currentMultiplier = actor.GetActionTimeMultiplier();
            if (Mathf.Approximately(previousMultiplier, currentMultiplier))
            {
                return;
            }

            float duration = Mathf.Max(0.001f, actor.BaseActionTime - actor.ScheduledAt);
            actor.NextActionTime = actor.ScheduledAt + duration * currentMultiplier;
            if (actor.NextActionTime < currentTime)
            {
                actor.NextActionTime = currentTime + 0.2f;
            }
        }

        private static float GetOpeningActionSpeedMultiplier(SimChampion actor)
        {
            float multiplier = 1f;
            if (actor.Champion == null || actor.Champion.Perks == null)
            {
                return multiplier;
            }

            foreach (PerkSO perk in actor.Champion.Perks)
            {
                if (perk == null || perk.EffectType != PerkEffectType.OpeningActionSpeed)
                {
                    continue;
                }

                if (actor.CompletedActions < perk.FlatValue)
                {
                    multiplier = Mathf.Max(multiplier, perk.Multiplier > 0f ? perk.Multiplier : 2f);
                }
            }

            return multiplier;
        }

        private static float GetSkillEchoDamageMultiplier(SimChampion actor)
        {
            float multiplier = 0f;
            if (actor.Champion == null || actor.Champion.Perks == null)
            {
                return multiplier;
            }

            foreach (PerkSO perk in actor.Champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.SkillEcho)
                {
                    multiplier = Mathf.Max(multiplier, perk.Multiplier > 0f ? perk.Multiplier : 0.5f);
                }
            }

            return multiplier;
        }
    }
}
