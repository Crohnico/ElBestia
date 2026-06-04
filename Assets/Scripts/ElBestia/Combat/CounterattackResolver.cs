using System;
using System.Collections.Generic;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class CounterattackResolver
    {
        private const int PendingSafetyLimit = 32;
        private const int StackSafetyLimit = 64;

        private readonly ChampionBehaviour owner;
        private readonly List<ChampionBehaviour> pendingCounterattackers = new List<ChampionBehaviour>();
        private ChampionBehaviour previousRival;

        public CounterattackResolver(ChampionBehaviour owner)
        {
            this.owner = owner;
        }

        public bool IsCounterattacking { get; private set; }

        public void Clear()
        {
            pendingCounterattackers.Clear();
            previousRival = null;
            IsCounterattacking = false;
        }

        public void Queue(ChampionBehaviour counterattacker)
        {
            if (counterattacker == null || !counterattacker.Counterattacks.CanCounterattackAgainst(owner) || pendingCounterattackers.Contains(counterattacker))
            {
                owner.DebugCombatFlow(nameof(CounterattackResolver), "CounterQueue", $"Ignored counterattacker={Label(counterattacker)}");
                return;
            }

            pendingCounterattackers.Add(counterattacker);
            owner.DebugCombatFlow(nameof(CounterattackResolver), "CounterQueue", $"Queued counterattacker={Label(counterattacker)} pending={pendingCounterattackers.Count}");
        }

        public void ResolvePending(Action onComplete)
        {
            owner.DebugCombatFlow(nameof(CounterattackResolver), "CounterResolve", $"Start pending={pendingCounterattackers.Count}");
            int safety = PendingSafetyLimit;
            while (pendingCounterattackers.Count > 0 && safety-- > 0)
            {
                ChampionBehaviour counterattacker = pendingCounterattackers[0];
                pendingCounterattackers.RemoveAt(0);
                if (counterattacker == null || !counterattacker.Counterattacks.CanCounterattackAgainst(owner))
                {
                    owner.DebugCombatFlow(nameof(CounterattackResolver), "CounterResolve", $"Skip counterattacker={Label(counterattacker)}");
                    continue;
                }

                owner.DebugCombatFlow(nameof(CounterattackResolver), "CounterResolve", $"Execute counterattacker={Label(counterattacker)}");
                counterattacker.Counterattacks.ExecuteAgainst(owner, () => ResolvePending(onComplete));
                return;
            }

            owner.DebugCombatFlow(nameof(CounterattackResolver), "CounterResolve", "Complete");
            onComplete?.Invoke();
        }

        public bool CanCounterattackAgainst(ChampionBehaviour attacker)
        {
            return Application.isPlaying
                && attacker != null
                && owner.IsAlive
                && attacker.IsAlive
                && owner.HasChargeForCombat(ChargeType.Counterattack)
                && owner.Champion != null
                && owner.Champion.BaseSkill != null
                && !IsCounterattacking;
        }

        public void ForceEnd()
        {
            if (!IsCounterattacking)
            {
                return;
            }

            owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", $"Force end state={owner.DebugStateSummary}");
            RestoreRival();
            IsCounterattacking = false;
            owner.ForceClearAsyncStepForCombat();
            owner.StartReturnHomeForCombat();
        }

        public void ForceEndPendingCounterattackers()
        {
            foreach (ChampionBehaviour pendingCounterattacker in pendingCounterattackers)
            {
                pendingCounterattacker?.Counterattacks.ForceEnd();
            }

            pendingCounterattackers.Clear();
        }

        private void ExecuteAgainst(ChampionBehaviour attacker, Action onComplete)
        {
            owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", $"Start attacker={Label(attacker)} stacks={owner.GetChargeAmountForCombat(ChargeType.Counterattack)}");
            SkillData baseSkill = owner.Champion != null ? owner.Champion.BaseSkill : null;
            if (baseSkill == null || attacker == null)
            {
                owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", "No base skill or attacker. Complete.");
                Finish();
                onComplete?.Invoke();
                return;
            }

            IsCounterattacking = true;
            previousRival = owner.RivalForCombat;
            owner.RivalForCombat = attacker;
            var context = new SkillExecutionContext(owner, attacker, baseSkill);

            ResolveStackLoop(attacker, baseSkill, context, onComplete, StackSafetyLimit);
        }

        private void ResolveStackLoop(ChampionBehaviour attacker, SkillData baseSkill, SkillExecutionContext context, Action onComplete, int safety)
        {
            if (safety <= 0 || !owner.IsAlive || attacker == null || !attacker.IsAlive || owner.GetChargeAmountForCombat(ChargeType.Counterattack) <= 0)
            {
                owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", $"End loop safety={safety} alive={owner.IsAlive} attackerAlive={attacker != null && attacker.IsAlive} stacks={owner.GetChargeAmountForCombat(ChargeType.Counterattack)}");
                owner.StartReturnHomeForCombat();
                Finish();
                onComplete?.Invoke();
                return;
            }

            owner.ApplyCaltropsActionTaxForCombat();
            if (!owner.IsAlive)
            {
                owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", "Died from Caltrops tax.");
                owner.StartReturnHomeForCombat();
                Finish();
                onComplete?.Invoke();
                return;
            }

            owner.MoveIntoSkillRangeForCombat(baseSkill, () =>
            {
                owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", "In range. Base attack anim.");
                owner.FaceTargetForCombat(attacker, Time.unscaledDeltaTime);
                owner.PlayActionAnimationForCombat(baseSkill, () =>
                {
                    owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", "Base attack hit.");
                    ExecuteBaseCast(baseSkill, context, attacker);
                    owner.ConsumeChargeForCombat(ChargeType.Counterattack, 1);
                    owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", $"Consumed stack. Remaining={owner.GetChargeAmountForCombat(ChargeType.Counterattack)}");
                    ResolveStackLoop(attacker, baseSkill, context, onComplete, safety - 1);
                });
            });
        }

        private void ExecuteBaseCast(SkillData baseSkill, SkillExecutionContext context, ChampionBehaviour attacker)
        {
            if (baseSkill == null || baseSkill.cast == null)
            {
                return;
            }

            foreach (SkillAction action in baseSkill.cast)
            {
                ChampionBehaviour target = action != null && action.target == SkillTarget.Self ? owner : attacker;
                if (target != null && action != null && action.action == SkillActionType.DoDamage)
                {
                    owner.Damage.ExecuteDamageAction(action, context, target, false);
                }
            }
        }

        private void Finish()
        {
            RestoreRival();
            IsCounterattacking = false;
            owner.DebugCombatFlow(nameof(CounterattackResolver), "Counterattack", "Finished flags");
        }

        private void RestoreRival()
        {
            if (previousRival != null)
            {
                owner.RivalForCombat = previousRival;
            }

            previousRival = null;
        }

        private static string Label(ChampionBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return "null";
            }

            return behaviour.Champion != null && !string.IsNullOrEmpty(behaviour.Champion.ChampionName)
                ? behaviour.Champion.ChampionName
                : behaviour.name;
        }
    }
}
