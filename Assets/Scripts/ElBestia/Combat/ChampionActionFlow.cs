using System;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionActionFlow
    {
        private readonly ChampionBehaviour owner;
        private readonly SkillPhaseExecutor phaseExecutor;

        public ChampionActionFlow(ChampionBehaviour owner)
        {
            this.owner = owner;
            phaseExecutor = new SkillPhaseExecutor(owner);
        }

        public void Start(SkillData skill, Action onComplete)
        {
            var context = new SkillExecutionContext(owner, owner.RivalForCombat, skill);
            owner.DebugCombatFlow(nameof(ChampionActionFlow), "StartActionFlow", $"skill={SkillLabel(skill)} applicationPlaying={Application.isPlaying}");
            if (!Application.isPlaying)
            {
                owner.SetActionFlowActiveForCombat(true);
                owner.ResetEndTurnChargeConsumptionForCombat();
                owner.ExecuteSkillActionsForCombat(skill.preCast, context, false);
                owner.ExecuteSkillActionsForCombat(skill.cast, context, true);
                owner.ConsumeEndTurnChargesForCombat();
                owner.ExecuteSkillActionsForCombat(skill.postCast, context, false);
                owner.ApplyAfterActionPerksForCombat();
                owner.FinishActionForCombat(onComplete);
                return;
            }

            owner.ForceClearAsyncStepForCombat();
            Begin(context, onComplete);
        }

        private void Begin(SkillExecutionContext context, Action onComplete)
        {
            owner.DebugCombatFlow(nameof(ChampionActionFlow), "Flow", $"Begin skill={SkillLabel(context.skill)} rival={Label(owner.RivalForCombat)}");
            owner.SetActionFlowActiveForCombat(true);
            owner.CancelReturnHomeForCombat();
            owner.ResetEndTurnChargeConsumptionForCombat();
            owner.ApplyCaltropsActionTaxForCombat();
            if (!owner.IsAlive)
            {
                owner.DebugCombatFlow(nameof(ChampionActionFlow), "Flow", "Dead after action tax. Finish.");
                owner.FinishActionForCombat(onComplete);
                return;
            }

            owner.DebugCombatFlow(nameof(ChampionActionFlow), "PreCast", "Start");
            phaseExecutor.ExecuteWithMovement(context.skill.preCast, context, false, () =>
            {
                owner.DebugCombatFlow(nameof(ChampionActionFlow), "PreCast", "Complete");
                ExecuteCastPhase(context, () =>
                {
                    owner.DebugCombatFlow(nameof(ChampionActionFlow), "Cast", "Complete. Consuming end-turn charges.");
                    owner.ConsumeEndTurnChargesForCombat();
                    owner.DebugCombatFlow(nameof(ChampionActionFlow), "PostCast", "Start");
                    phaseExecutor.ExecuteWithMovement(context.skill.postCast, context, false, () =>
                    {
                        owner.DebugCombatFlow(nameof(ChampionActionFlow), "PostCast", "Complete");
                        ExecuteSkillEcho(context, () =>
                        {
                            owner.DebugCombatFlow(nameof(ChampionActionFlow), "Flow", "Echo/Perks/Finish");
                            owner.ApplyAfterActionPerksForCombat();
                            owner.StartReturnHomeForCombat();
                            owner.FinishActionForCombat(onComplete);
                        });
                    });
                });
            });
        }

        private void ExecuteCastPhase(SkillExecutionContext context, Action onComplete)
        {
            owner.DebugCombatFlow(nameof(ChampionActionFlow), "Cast", $"Start skill={SkillLabel(context.skill)} needsRange={SkillPhaseExecutor.PhaseNeedsEnemyRange(context.skill.cast)}");
            Action executeCast = () =>
            {
                owner.PlayActionAnimationForCombat(() =>
                {
                    owner.DebugCombatFlow(nameof(ChampionActionFlow), "Cast", "Hit moment");
                    owner.ExecuteSkillActionsForCombat(context.skill.cast, context, true);
                    owner.Counterattacks.ResolvePending(onComplete);
                });
            };

            if (SkillPhaseExecutor.PhaseNeedsEnemyRange(context.skill.cast))
            {
                owner.DebugCombatFlow(nameof(ChampionActionFlow), "Cast", "Move to range");
                owner.MoveIntoSkillRangeForCombat(context.skill, () =>
                {
                    owner.FaceTargetForCombat(owner.RivalForCombat, Time.unscaledDeltaTime);
                    executeCast();
                });
                return;
            }

            executeCast();
        }

        private void ExecuteSkillEcho(SkillExecutionContext originalContext, Action onComplete)
        {
            float multiplier = owner.GetSkillEchoDamageMultiplierForCombat();
            if (multiplier <= 0f || originalContext == null || originalContext.skill == null || !owner.IsAlive)
            {
                owner.DebugCombatFlow(nameof(ChampionActionFlow), "Echo", "Skip");
                onComplete?.Invoke();
                return;
            }

            owner.DebugCombatFlow(nameof(ChampionActionFlow), "Echo", $"Start multiplier={multiplier:0.00}");
            var echoContext = new SkillExecutionContext(owner, owner.RivalForCombat, originalContext.skill)
            {
                damageBonus = originalContext.damageBonus,
                damageMultiplier = multiplier
            };

            phaseExecutor.ExecuteWithMovement(echoContext.skill.preCast, echoContext, false, () => ExecuteEchoCastPhase(echoContext, () =>
            {
                phaseExecutor.ExecuteWithMovement(echoContext.skill.postCast, echoContext, false, onComplete);
            }));
        }

        private void ExecuteEchoCastPhase(SkillExecutionContext context, Action onComplete)
        {
            owner.DebugCombatFlow(nameof(ChampionActionFlow), "EchoCast", $"Start skill={SkillLabel(context.skill)} needsRange={SkillPhaseExecutor.PhaseNeedsEnemyRange(context.skill.cast)}");
            Action executeCast = () =>
            {
                owner.PlayActionAnimationForCombat(() =>
                {
                    owner.DebugCombatFlow(nameof(ChampionActionFlow), "EchoCast", "Hit moment");
                    owner.ExecuteSkillActionsForCombat(context.skill.cast, context, true);
                    owner.Counterattacks.ResolvePending(onComplete);
                });
            };

            if (SkillPhaseExecutor.PhaseNeedsEnemyRange(context.skill.cast))
            {
                owner.DebugCombatFlow(nameof(ChampionActionFlow), "EchoCast", "Move to range");
                owner.MoveIntoSkillRangeForCombat(context.skill, () =>
                {
                    owner.FaceTargetForCombat(owner.RivalForCombat, Time.unscaledDeltaTime);
                    executeCast();
                });
                return;
            }

            executeCast();
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

        private static string SkillLabel(SkillData skill)
        {
            return skill != null && !string.IsNullOrEmpty(skill.skillName) ? skill.skillName : "null";
        }
    }
}
