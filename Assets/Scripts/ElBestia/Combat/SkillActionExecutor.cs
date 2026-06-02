using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class SkillActionExecutor
    {
        private readonly ChampionBehaviour owner;

        public SkillActionExecutor(ChampionBehaviour owner)
        {
            this.owner = owner;
        }

        public void ExecuteMany(SkillAction[] actions, SkillExecutionContext context, bool allowCounterattack)
        {
            if (actions == null)
            {
                return;
            }

            foreach (SkillAction skillAction in actions)
            {
                Execute(skillAction, context, allowCounterattack);
            }
        }

        public void Execute(SkillAction skillAction, SkillExecutionContext context, bool allowCounterattack)
        {
            if (skillAction == null)
            {
                return;
            }

            ChampionBehaviour target = ResolveTarget(skillAction.target);
            if (target == null)
            {
                return;
            }

            switch (skillAction.action)
            {
                case SkillActionType.DoDamage:
                    owner.DebugCombatFlow(nameof(SkillActionExecutor), "SkillAction", $"{SkillLabel(context?.skill)} DoDamage amount={skillAction.amount} target={Label(target)} allowCounter={allowCounterattack}");
                    owner.Damage.ExecuteDamageAction(skillAction, context, target, allowCounterattack);
                    break;
                case SkillActionType.IncreaseDamage:
                    owner.DebugCombatFlow(nameof(SkillActionExecutor), "SkillAction", $"{SkillLabel(context?.skill)} IncreaseDamage amount={skillAction.amount} target={Label(target)}");
                    target.AddCharges(ChargeType.Empowered, Mathf.Max(0, skillAction.amount), owner);
                    break;
                case SkillActionType.WeakenEnemy:
                    owner.DebugCombatFlow(nameof(SkillActionExecutor), "SkillAction", $"{SkillLabel(context?.skill)} WeakenEnemy amount={skillAction.amount} target={Label(target)}");
                    target.AddCharges(ChargeType.Weakened, Mathf.Max(0, skillAction.amount), owner);
                    break;
                case SkillActionType.GainCharges:
                    owner.DebugCombatFlow(nameof(SkillActionExecutor), "SkillAction", $"{SkillLabel(context?.skill)} GainCharges {skillAction.charge} amount={skillAction.amount} target={Label(target)}");
                    target.AddCharges(skillAction.charge, Mathf.Max(0, skillAction.amount), owner);
                    break;
                case SkillActionType.ApplyCharges:
                    owner.DebugCombatFlow(nameof(SkillActionExecutor), "SkillAction", $"{SkillLabel(context?.skill)} ApplyCharges {skillAction.charge} amount={skillAction.amount} target={Label(target)}");
                    target.AddCharges(skillAction.charge, Mathf.Max(0, skillAction.amount), owner);
                    break;
            }
        }

        private ChampionBehaviour ResolveTarget(SkillTarget target)
        {
            return target == SkillTarget.Self ? owner : owner.RivalForCombat;
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
