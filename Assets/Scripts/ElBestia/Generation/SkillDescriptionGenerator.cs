using System.Collections.Generic;
using ElBestia.Skills;

namespace ElBestia.Generation
{
    public static class SkillDescriptionGenerator
    {
        public static string Generate(SkillData skill)
        {
            if (skill == null)
            {
                return string.Empty;
            }

            string action = GetActionPhrase(skill);
            string preparation = GetPreparationPhrase(skill.preCast);
            string aftermath = GetAftermathPhrase(skill.postCast);
            var lines = new List<string>();

            string sentence = "The user ";
            if (!string.IsNullOrEmpty(preparation))
            {
                sentence += preparation + " before ";
            }

            sentence += action;

            if (!string.IsNullOrEmpty(aftermath))
            {
                sentence += ", " + aftermath;
            }

            sentence += ".";
            lines.Add(sentence);
            lines.Add($"- Cooldown: {skill.cooldownSeconds:0.#}s");

            int damage = GetDamage(skill.cast);
            if (damage != 0)
            {
                lines.Add(damage > 0 ? $"- Damage: {damage}" : $"- Healing: {-damage}");
            }

            string effects = GetEffectsSummary(skill);
            if (!string.IsNullOrEmpty(effects))
            {
                lines.Add("- Effects: " + effects);
            }

            return string.Join("\n", lines);
        }

        private static string GetActionPhrase(SkillData skill)
        {
            string verb = GetWeaponVerb(skill);
            int damage = GetDamage(skill.cast);

            if (damage < 0)
            {
                return "channels a restorative technique";
            }

            if (skill.isBaseSkill)
            {
                return GetBaseActionPhrase(skill);
            }

            return "executes " + verb;
        }

        private static string GetBaseActionPhrase(SkillData skill)
        {
            WeaponType weapon = skill.weapons != null && skill.weapons.Length > 0 ? skill.weapons[0] : WeaponType.None;
            switch (weapon)
            {
                case WeaponType.Axe:
                    return "swings the axe in a simple heavy blow";
                case WeaponType.Sword:
                    return "cuts with a simple sword slash";
                case WeaponType.Spear:
                    return "thrusts with the spear";
                case WeaponType.Bow:
                    return "fires a simple arrow";
                case WeaponType.Staff:
                    return "strikes with the staff";
                case WeaponType.Fists:
                    return "throws a simple punch";
                default:
                    return "makes a simple attack";
            }
        }

        private static string GetWeaponVerb(SkillData skill)
        {
            WeaponType weapon = skill.weapons != null && skill.weapons.Length > 0 ? skill.weapons[0] : WeaponType.None;
            switch (weapon)
            {
                case WeaponType.Axe:
                    return "a heavy axe cleave";
                case WeaponType.Sword:
                    return "a sharp sword slash";
                case WeaponType.Spear:
                    return "a piercing spear thrust";
                case WeaponType.Bow:
                    return "a focused bow shot";
                case WeaponType.Staff:
                    return "a sweeping staff strike";
                case WeaponType.Fists:
                    return "a bare-knuckle strike";
                default:
                    return "a direct strike";
            }
        }

        private static string GetPreparationPhrase(SkillAction[] actions)
        {
            if (Contains(actions, SkillActionType.IncreaseDamage, SkillTarget.Self))
            {
                return "steadies their stance and empowers the next blow";
            }

            if (ContainsCharge(actions, SkillTarget.Self))
            {
                return "draws power inward";
            }

            return string.Empty;
        }

        private static string GetAftermathPhrase(SkillAction[] actions)
        {
            if (Contains(actions, SkillActionType.WeakenEnemy, SkillTarget.Enemy))
            {
                return "leaving the enemy weakened";
            }

            if (ContainsCharge(actions, SkillTarget.Enemy))
            {
                return "leaving lingering pressure on the enemy";
            }

            if (ContainsCharge(actions, SkillTarget.Self))
            {
                return "building momentum afterward";
            }

            return string.Empty;
        }

        private static string GetEffectsSummary(SkillData skill)
        {
            var parts = new List<string>();
            AppendActions(parts, "pre", skill.preCast);
            AppendActions(parts, "action", skill.cast);
            AppendActions(parts, "post", skill.postCast);
            return string.Join("; ", parts);
        }

        private static void AppendActions(List<string> parts, string phase, SkillAction[] actions)
        {
            if (actions == null)
            {
                return;
            }

            foreach (SkillAction action in actions)
            {
                if (action == null)
                {
                    continue;
                }

                string text = $"{phase}: {action.target}: {action.action}";
                if (action.charge != ChargeType.None)
                {
                    text += $" {action.charge}";
                }

                text += $" x{action.amount}";
                parts.Add(text);
            }
        }

        private static int GetDamage(SkillAction[] actions)
        {
            if (actions == null)
            {
                return 0;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.action == SkillActionType.DoDamage)
                {
                    return action.amount;
                }
            }

            return 0;
        }

        private static bool Contains(SkillAction[] actions, SkillActionType actionType, SkillTarget target)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.action == actionType && action.target == target)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsCharge(SkillAction[] actions, SkillTarget target)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.target == target && action.charge != ChargeType.None)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
