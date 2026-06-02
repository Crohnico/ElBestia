using System;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class SkillPhaseExecutor
    {
        private readonly ChampionBehaviour owner;

        public SkillPhaseExecutor(ChampionBehaviour owner)
        {
            this.owner = owner;
        }

        public void ExecuteWithMovement(SkillAction[] actions, SkillExecutionContext context, bool allowCounterattack, Action onComplete)
        {
            if (actions == null)
            {
                owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"No action array allowCounter={allowCounterattack}");
                onComplete?.Invoke();
                return;
            }

            owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"Start count={actions.Length} allowCounter={allowCounterattack}");
            ExecuteAtIndex(actions, 0, context, allowCounterattack, onComplete);
        }

        public static bool PhaseNeedsEnemyRange(SkillAction[] actions)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (NeedsEnemyRange(action))
                {
                    return true;
                }
            }

            return false;
        }

        private void ExecuteAtIndex(SkillAction[] actions, int index, SkillExecutionContext context, bool allowCounterattack, Action onComplete)
        {
            if (actions == null || index >= actions.Length)
            {
                owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"Complete allowCounter={allowCounterattack}");
                onComplete?.Invoke();
                return;
            }

            SkillAction skillAction = actions[index];
            owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"Index={index} type={skillAction?.action} target={skillAction?.target} charge={skillAction?.charge} amount={skillAction?.amount} allowCounter={allowCounterattack}");
            Action executeCurrent = () =>
            {
                owner.ExecuteSkillActionForCombat(skillAction, context, allowCounterattack);
                if (allowCounterattack && CanTriggerCounterattack(skillAction))
                {
                    owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"Index={index} resolving counterattacks");
                    owner.Counterattacks.ResolvePending(() => ExecuteAtIndex(actions, index + 1, context, allowCounterattack, onComplete));
                    return;
                }

                owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"Index={index} complete");
                ExecuteAtIndex(actions, index + 1, context, allowCounterattack, onComplete);
            };

            if (NeedsEnemyRange(skillAction))
            {
                owner.DebugCombatFlow(nameof(SkillPhaseExecutor), "PhaseActions", $"Index={index} needs range");
                owner.MoveIntoSkillRangeForCombat(context.skill, () =>
                {
                    owner.FaceTargetForCombat(owner.RivalForCombat, Time.unscaledDeltaTime);
                    executeCurrent();
                });
                return;
            }

            executeCurrent();
        }

        private static bool CanTriggerCounterattack(SkillAction action)
        {
            return action != null
                && action.target == SkillTarget.Enemy
                && action.action == SkillActionType.DoDamage
                && action.amount > 0;
        }

        private static bool NeedsEnemyRange(SkillAction skillAction)
        {
            return skillAction != null && skillAction.target == SkillTarget.Enemy;
        }
    }
}
