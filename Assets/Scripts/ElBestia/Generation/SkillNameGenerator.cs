using System.Collections.Generic;
using ElBestia.Skills;

namespace ElBestia.Generation
{
    public readonly struct SkillNameResult
    {
        public readonly string EnglishName;
        public readonly string LocalizationId;

        public SkillNameResult(string englishName, string localizationId)
        {
            EnglishName = englishName;
            LocalizationId = localizationId;
        }
    }

    public static class SkillNameGenerator
    {
        public static SkillNameResult Generate(SkillData skill)
        {
            if (TryGetSpecialCombo(skill, out SkillNameResult combo))
            {
                return combo;
            }

            var prefixWords = new List<string>();
            var prefixIds = new List<string>();
            AddPrefixModifiers(skill, prefixWords, prefixIds);

            string actionWord = GetActionWord(skill);
            string actionId = GetActionId(skill);
            string elementWord = GetElementWord(skill.element);
            string elementId = GetElementId(skill.element);
            string coreName = string.IsNullOrEmpty(elementWord) ? actionWord : $"{elementWord} {actionWord}";
            string coreId = string.IsNullOrEmpty(elementId) ? actionId : $"{elementId}_{actionId}";

            string effectWord = GetEffectPhrase(skill);
            string effectId = GetEffectId(skill);

            string englishName = BuildEnglishName(prefixWords, coreName, effectWord);
            string localizationId = BuildLocalizationId(prefixIds, coreId, effectId);
            return new SkillNameResult(englishName, localizationId);
        }

        private static bool TryGetSpecialCombo(SkillData skill, out SkillNameResult result)
        {
            bool burns = AppliesCharge(skill, ChargeType.Burn);
            bool poisons = AppliesCharge(skill, ChargeType.Poison);
            bool slows = AppliesCharge(skill, ChargeType.Slow);
            WeaponType weapon = GetPrimaryWeapon(skill);

            if (weapon == WeaponType.Spear && skill.element == SkillElement.Electricity && slows)
            {
                result = new SkillNameResult("Thunder Pin", "skill_name.combo.thunder_pin");
                return true;
            }

            if (weapon == WeaponType.Sword && skill.element == SkillElement.Fire && burns)
            {
                string name = poisons ? "Venomous Flamebrand" : "Flamebrand";
                string id = poisons ? "skill_name.combo.venomous_flamebrand" : "skill_name.combo.flamebrand";
                result = new SkillNameResult(name, id);
                return true;
            }

            if (weapon == WeaponType.Axe && skill.element == SkillElement.Earth && HasWeakenEnemy(skill))
            {
                string name = burns ? "Burning Gravebreaker" : "Gravebreaker";
                string id = burns ? "skill_name.combo.burning_gravebreaker" : "skill_name.combo.gravebreaker";
                result = new SkillNameResult(name, id);
                return true;
            }

            result = default;
            return false;
        }

        private static string BuildEnglishName(List<string> prefixes, string coreName, string effectWord)
        {
            string prefix = prefixes.Count > 0 ? string.Join(" ", prefixes) + " " : string.Empty;

            if (!string.IsNullOrEmpty(effectWord))
            {
                return $"{prefix}{coreName} of {effectWord}";
            }

            return $"{prefix}{coreName}";
        }

        private static string BuildLocalizationId(List<string> prefixIds, string coreId, string effectId)
        {
            var parts = new List<string> { "skill_name" };
            parts.AddRange(prefixIds);
            parts.Add(coreId);

            if (!string.IsNullOrEmpty(effectId))
            {
                parts.Add(effectId);
            }

            return string.Join(".", parts);
        }

        private static void AddPrefixModifiers(SkillData skill, List<string> words, List<string> ids)
        {
            if (HasIncreaseDamage(skill))
            {
                AddUnique(words, ids, "Empowered", "empowered");
            }

            if (GrantsCharge(skill, ChargeType.Haste))
            {
                AddUnique(words, ids, "Swift", "swift");
            }

            if (GrantsCharge(skill, ChargeType.LifeSteal))
            {
                AddUnique(words, ids, "Vampiric", "vampiric");
            }

            if (GrantsCharge(skill, ChargeType.Counterattack))
            {
                AddUnique(words, ids, "Vengeful", "vengeful");
            }

            if (GrantsCharge(skill, ChargeType.Regeneration))
            {
                AddUnique(words, ids, "Regenerating", "regenerating");
            }

            if (GrantsCharge(skill, ChargeType.Fortified))
            {
                AddUnique(words, ids, "Fortified", "fortified");
            }

            if (GrantsCharge(skill, ChargeType.Thorns))
            {
                AddUnique(words, ids, "Barbed", "barbed");
            }

            if (HasHealingCast(skill))
            {
                AddUnique(words, ids, "Restorative", "restorative");
            }
        }

        private static string GetEffectPhrase(SkillData skill)
        {
            bool weakens = HasWeakenEnemy(skill);
            var effects = new List<string>();

            AddChargeEffect(skill, effects, ChargeType.Burn, "Burning");
            AddChargeEffect(skill, effects, ChargeType.Poison, "Venomous");
            AddChargeEffect(skill, effects, ChargeType.Drowning, "Drowning");
            AddChargeEffect(skill, effects, ChargeType.Shock, "Shocking");
            AddChargeEffect(skill, effects, ChargeType.Splinter, "Splintering");
            AddChargeEffect(skill, effects, ChargeType.Crush, "Crushing");
            AddChargeEffect(skill, effects, ChargeType.WindShear, "Razor Wind");
            AddChargeEffect(skill, effects, ChargeType.Slow, "Binding");
            AddChargeEffect(skill, effects, ChargeType.Bleed, "Bleeding");
            AddChargeEffect(skill, effects, ChargeType.Vulnerable, "Exposing");
            AddChargeEffect(skill, effects, ChargeType.Caltrops, "Barbed Ground");
            AddChargeEffect(skill, effects, ChargeType.Recoil, "Recoil");
            AddChargeEffect(skill, effects, ChargeType.Dizzle, "Dizzling");

            if (weakens)
            {
                effects.Insert(0, "Withering");
            }

            if (effects.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" ", effects);
        }

        private static string GetEffectId(SkillData skill)
        {
            bool weakens = HasWeakenEnemy(skill);
            var effects = new List<string>();

            AddChargeEffect(skill, effects, ChargeType.Burn, "burning");
            AddChargeEffect(skill, effects, ChargeType.Poison, "venomous");
            AddChargeEffect(skill, effects, ChargeType.Drowning, "drowning");
            AddChargeEffect(skill, effects, ChargeType.Shock, "shocking");
            AddChargeEffect(skill, effects, ChargeType.Splinter, "splintering");
            AddChargeEffect(skill, effects, ChargeType.Crush, "crushing");
            AddChargeEffect(skill, effects, ChargeType.WindShear, "razor_wind");
            AddChargeEffect(skill, effects, ChargeType.Slow, "binding");
            AddChargeEffect(skill, effects, ChargeType.Bleed, "bleeding");
            AddChargeEffect(skill, effects, ChargeType.Vulnerable, "exposing");
            AddChargeEffect(skill, effects, ChargeType.Caltrops, "barbed_ground");
            AddChargeEffect(skill, effects, ChargeType.Recoil, "recoil");
            AddChargeEffect(skill, effects, ChargeType.Dizzle, "dizzling");

            if (weakens)
            {
                effects.Insert(0, "withering");
            }

            return effects.Count > 0 ? string.Join("_", effects) : string.Empty;
        }

        private static string GetActionWord(SkillData skill)
        {
            if (HasHealingCast(skill) && !HasDamageCast(skill))
            {
                return "Pulse";
            }

            switch (GetPrimaryWeapon(skill))
            {
                case WeaponType.Spear:
                    return "Thrust";
                case WeaponType.Sword:
                    return "Slash";
                case WeaponType.Axe:
                    return "Cleave";
                case WeaponType.Staff:
                    return "Sweep";
                case WeaponType.Fists:
                    return "Strike";
                default:
                    return "Strike";
            }
        }

        private static string GetActionId(SkillData skill)
        {
            if (HasHealingCast(skill) && !HasDamageCast(skill))
            {
                return "pulse";
            }

            switch (GetPrimaryWeapon(skill))
            {
                case WeaponType.Spear:
                    return "thrust";
                case WeaponType.Sword:
                    return "slash";
                case WeaponType.Axe:
                    return "cleave";
                case WeaponType.Staff:
                    return "sweep";
                case WeaponType.Fists:
                    return "strike";
                default:
                    return "strike";
            }
        }

        private static string GetElementWord(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Water:
                    return "Water";
                case SkillElement.Electricity:
                    return "Thunder";
                case SkillElement.Fire:
                    return "Fire";
                case SkillElement.Poison:
                    return "Venom";
                case SkillElement.Earth:
                    return "Earth";
                case SkillElement.Air:
                    return "Wind";
                case SkillElement.Wood:
                    return "Wood";
                default:
                    return string.Empty;
            }
        }

        private static string GetElementId(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Water:
                    return "water";
                case SkillElement.Electricity:
                    return "thunder";
                case SkillElement.Fire:
                    return "fire";
                case SkillElement.Poison:
                    return "venom";
                case SkillElement.Earth:
                    return "earth";
                case SkillElement.Air:
                    return "wind";
                case SkillElement.Wood:
                    return "wood";
                default:
                    return string.Empty;
            }
        }

        private static void AddChargeEffect(SkillData skill, List<string> effects, ChargeType charge, string label)
        {
            if (AppliesCharge(skill, charge))
            {
                effects.Add(label);
            }
        }

        private static void AddUnique(List<string> words, List<string> ids, string word, string id)
        {
            if (ids.Contains(id))
            {
                return;
            }

            words.Add(word);
            ids.Add(id);
        }

        private static WeaponType GetPrimaryWeapon(SkillData skill)
        {
            if (skill.weapons == null || skill.weapons.Length == 0)
            {
                return WeaponType.None;
            }

            return skill.weapons[0];
        }

        private static bool HasHealingCast(SkillData skill)
        {
            return ContainsDamageAmountBelowZero(skill.cast);
        }

        private static bool HasDamageCast(SkillData skill)
        {
            if (skill.cast == null)
            {
                return false;
            }

            foreach (SkillAction action in skill.cast)
            {
                if (action != null && action.action == SkillActionType.DoDamage && action.amount > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsDamageAmountBelowZero(SkillAction[] actions)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.action == SkillActionType.DoDamage && action.amount < 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasIncreaseDamage(SkillData skill)
        {
            return ContainsAction(skill.preCast, SkillActionType.IncreaseDamage)
                || ContainsAction(skill.cast, SkillActionType.IncreaseDamage)
                || ContainsAction(skill.postCast, SkillActionType.IncreaseDamage);
        }

        private static bool HasWeakenEnemy(SkillData skill)
        {
            return ContainsEnemyAction(skill.preCast, SkillActionType.WeakenEnemy)
                || ContainsEnemyAction(skill.cast, SkillActionType.WeakenEnemy)
                || ContainsEnemyAction(skill.postCast, SkillActionType.WeakenEnemy);
        }

        private static bool ContainsAction(SkillAction[] actions, SkillActionType actionType)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.action == actionType)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsEnemyAction(SkillAction[] actions, SkillActionType actionType)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.target == SkillTarget.Enemy && action.action == actionType)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool GrantsCharge(SkillData skill, ChargeType charge)
        {
            return ContainsChargeForTarget(skill.preCast, charge, SkillTarget.Self)
                || ContainsChargeForTarget(skill.cast, charge, SkillTarget.Self)
                || ContainsChargeForTarget(skill.postCast, charge, SkillTarget.Self);
        }

        private static bool AppliesCharge(SkillData skill, ChargeType charge)
        {
            return ContainsChargeForTarget(skill.preCast, charge, SkillTarget.Enemy)
                || ContainsChargeForTarget(skill.cast, charge, SkillTarget.Enemy)
                || ContainsChargeForTarget(skill.postCast, charge, SkillTarget.Enemy);
        }

        private static bool ContainsChargeForTarget(SkillAction[] actions, ChargeType charge, SkillTarget target)
        {
            if (actions == null)
            {
                return false;
            }

            foreach (SkillAction action in actions)
            {
                if (action != null && action.target == target && action.charge == charge)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
