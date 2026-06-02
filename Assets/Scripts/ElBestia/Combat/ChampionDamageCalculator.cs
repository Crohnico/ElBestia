using ElBestia.Champions;
using ElBestia.Combat.Charges;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionDamageCalculator
    {
        private readonly ChampionBehaviour owner;

        public ChampionDamageCalculator(ChampionBehaviour owner)
        {
            this.owner = owner;
        }

        public int CalculateOutgoingAmount(float amount)
        {
            float modified = Mathf.Max(0f, amount);
            if (owner.HasChargeForCombat(ChargeType.Empowered))
            {
                modified *= 1.2f;
            }

            if (owner.HasChargeForCombat(ChargeType.Weakened))
            {
                modified *= 0.8f;
            }

            return Mathf.Max(1, Mathf.RoundToInt(modified));
        }

        public int GetIntelligence()
        {
            return owner.Champion != null && owner.Champion.Stats != null ? Mathf.Max(0, owner.Champion.Stats.intelligence) : 0;
        }

        public int ApplyOutgoingElementalDamage(int amount, SkillElement element)
        {
            ChampionStats stats = owner.Champion != null ? owner.Champion.Stats : null;
            if (stats == null || element == SkillElement.None || amount <= 0)
            {
                return Mathf.Max(0, amount);
            }

            float modified = amount + stats.GetElementalDamageBonus(element);
            modified *= stats.GetElementalDamageMultiplier(element);
            return Mathf.Max(1, Mathf.RoundToInt(modified));
        }

        public int CalculateOutgoingSkillDamage(int actionAmount, SkillData skill)
        {
            float damage = Mathf.Max(0, actionAmount);
            ChampionStats stats = owner.Champion != null ? owner.Champion.Stats : null;
            if (stats != null && skill != null && skill.statScaling != null)
            {
                foreach (StatScaling scaling in skill.statScaling)
                {
                    if (scaling == null)
                    {
                        continue;
                    }

                    float gradeMultiplier = scaling.usesGradeScaling ? CombatStatUtility.GetScalingMultiplier(scaling.scaling) : 0.75f;
                    damage += CombatStatUtility.GetStatValue(stats, scaling.stat) * gradeMultiplier;
                }
            }

            WeaponType weapon = CombatStatUtility.GetSkillWeapon(skill);
            int proficiency = stats != null ? stats.GetProficiency(weapon) : 0;
            damage *= 1f + proficiency / 100f;
            return CalculateOutgoingAmount(damage);
        }

        public int CalculateIncomingDamage(int amount, SkillElement element)
        {
            ChampionStats stats = owner.Champion != null ? owner.Champion.Stats : null;
            float modified = Mathf.Max(0, amount);
            modified = ApplyIncomingChargeDamageModifiers(modified, element);
            float resistance = stats != null ? stats.GetElementalResistancePercent(element) : 0f;
            return Mathf.RoundToInt(modified * Mathf.Clamp01(1f - resistance / 100f));
        }

        public float GetLifeStealPercent()
        {
            float percent = owner.HasChargeForCombat(ChargeType.LifeSteal) ? 0.2f : 0f;
            if (owner.Champion == null || owner.Champion.Perks == null)
            {
                return percent;
            }

            foreach (PerkSO perk in owner.Champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.LifeSteal)
                {
                    percent += perk.FlatValue / 100f;
                }
            }

            return percent;
        }

        private float ApplyIncomingChargeDamageModifiers(float amount, SkillElement element)
        {
            float result = Mathf.Max(0f, amount);
            if (result <= 0f)
            {
                return 0f;
            }

            if (owner.ConsumeChargeIfAvailableForCombat(ChargeType.Fortified))
            {
                result *= 0.8f;
            }

            if (owner.ConsumeChargeIfAvailableForCombat(ChargeType.Vulnerable))
            {
                result *= 1.2f;
            }

            ChargeType elementalFortification = ChargeEffectCatalog.GetElementalFortification(element);
            if (elementalFortification != ChargeType.None && owner.ConsumeChargeIfAvailableForCombat(elementalFortification))
            {
                result *= 0.7f;
            }

            return result;
        }
    }
}
