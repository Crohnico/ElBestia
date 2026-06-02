using System;
using ElBestia.Generation;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Champions
{
    public struct ChampionStatFormula
    {
        public int baseValue;
        public int additive;
        public float multiplier;
        public int finalValue;
    }

    [Serializable]
    public sealed class ChampionStats
    {
        [Min(0)] public int life;
        [Min(0)] public int energy;

        [Header("Base Stats")]
        [Min(0)] public int strength;
        [Min(0)] public int agility;
        [Min(0)] public int constitution;
        [Min(0)] public int intelligence;
        [Min(0)] public int endurance;

        [Header("Combat Stats")]
        [Min(0)] public int dodge;
        [Min(0)] public int block;
        [Min(0)] public int critical;
        [Min(0)] public int hit;
        [Min(0)] public int recovery;
        [Min(0)] public int speed;
        [Min(0)] public int experienceGain;
        [Min(0)] public int fatalInjuryResistance;
        [Min(0)] public int injurySeverityReduction;

        [Header("Elemental Resistance")]
        [Min(0)] public int fireResistance;
        [Min(0)] public int waterResistance;
        [Min(0)] public int electricityResistance;
        [Min(0)] public int poisonResistance;
        [Min(0)] public int earthResistance;
        [Min(0)] public int airResistance;
        [Min(0)] public int woodResistance;

        [Header("Elemental Damage")]
        [Min(0)] public int fireDamage;
        [Min(0)] public int waterDamage;
        [Min(0)] public int electricityDamage;
        [Min(0)] public int poisonDamage;
        [Min(0)] public int earthDamage;
        [Min(0)] public int airDamage;
        [Min(0)] public int woodDamage;
        [Min(0)] public float fireDamageMultiplier = 1f;
        [Min(0)] public float waterDamageMultiplier = 1f;
        [Min(0)] public float electricityDamageMultiplier = 1f;
        [Min(0)] public float poisonDamageMultiplier = 1f;
        [Min(0)] public float earthDamageMultiplier = 1f;
        [Min(0)] public float airDamageMultiplier = 1f;
        [Min(0)] public float woodDamageMultiplier = 1f;

        [Header("Weapon Proficiency")]
        [Min(0)] public int fistsProficiency;
        [Min(0)] public int swordProficiency;
        [Min(0)] public int axeProficiency;
        [Min(0)] public int spearProficiency;
        [Min(0)] public int staffProficiency;
        [Min(0)] public int bowProficiency;

        public static ChampionStats CreateLevelOne(System.Random rng, int totalBasePoints)
        {
            var stats = new ChampionStats();
            int remaining = Mathf.Max(0, totalBasePoints);

            stats.strength = 1;
            stats.agility = 1;
            stats.constitution = 1;
            stats.intelligence = 1;
            stats.endurance = 1;
            remaining -= 5;

            while (remaining > 0)
            {
                switch (rng.Next(0, 5))
                {
                    case 0:
                        stats.strength++;
                        break;
                    case 1:
                        stats.agility++;
                        break;
                    case 2:
                        stats.constitution++;
                        break;
                    case 3:
                        stats.intelligence++;
                        break;
                    default:
                        stats.endurance++;
                        break;
                }

                remaining--;
            }

            stats.RecalculateDerivedResources();
            return stats;
        }

        public ChampionStats Clone()
        {
            return new ChampionStats
            {
                life = life,
                energy = energy,
                strength = strength,
                agility = agility,
                constitution = constitution,
                intelligence = intelligence,
                endurance = endurance,
                dodge = dodge,
                block = block,
                critical = critical,
                hit = hit,
                recovery = recovery,
                speed = speed,
                experienceGain = experienceGain,
                fatalInjuryResistance = fatalInjuryResistance,
                injurySeverityReduction = injurySeverityReduction,
                fireResistance = fireResistance,
                waterResistance = waterResistance,
                electricityResistance = electricityResistance,
                poisonResistance = poisonResistance,
                earthResistance = earthResistance,
                airResistance = airResistance,
                woodResistance = woodResistance,
                fireDamage = fireDamage,
                waterDamage = waterDamage,
                electricityDamage = electricityDamage,
                poisonDamage = poisonDamage,
                earthDamage = earthDamage,
                airDamage = airDamage,
                woodDamage = woodDamage,
                fireDamageMultiplier = SanitizeMultiplier(fireDamageMultiplier),
                waterDamageMultiplier = SanitizeMultiplier(waterDamageMultiplier),
                electricityDamageMultiplier = SanitizeMultiplier(electricityDamageMultiplier),
                poisonDamageMultiplier = SanitizeMultiplier(poisonDamageMultiplier),
                earthDamageMultiplier = SanitizeMultiplier(earthDamageMultiplier),
                airDamageMultiplier = SanitizeMultiplier(airDamageMultiplier),
                woodDamageMultiplier = SanitizeMultiplier(woodDamageMultiplier),
                fistsProficiency = fistsProficiency,
                swordProficiency = swordProficiency,
                axeProficiency = axeProficiency,
                spearProficiency = spearProficiency,
                staffProficiency = staffProficiency,
                bowProficiency = bowProficiency
            };
        }

        public void RecalculateDerivedResources(bool resetProficiencies = true)
        {
            life = 50 + constitution * 10 + endurance * 3;
            energy = 20 + intelligence * 6;
            dodge = Mathf.Max(0, agility / 2);
            block = Mathf.Max(0, endurance / 3 + strength / 4);
            critical = Mathf.Max(0, agility / 4 + strength / 5);
            hit = Mathf.Max(0, agility / 3 + intelligence / 4);
            recovery = Mathf.Max(0, constitution / 4 + endurance / 4 + intelligence / 6);
            RefreshDerivedCombatTotals();
            experienceGain = 0;
            fatalInjuryResistance = 0;
            injurySeverityReduction = 0;
            int constitutionResistance = constitution / 5;
            fireResistance = Mathf.Max(0, constitutionResistance + endurance / 5 + intelligence / 8);
            waterResistance = Mathf.Max(0, constitutionResistance + endurance / 5 + intelligence / 8);
            electricityResistance = Mathf.Max(0, constitutionResistance + agility / 5 + intelligence / 8);
            poisonResistance = Mathf.Max(0, constitutionResistance + endurance / 6 + intelligence / 8);
            earthResistance = Mathf.Max(0, constitutionResistance + endurance / 4 + strength / 8);
            airResistance = Mathf.Max(0, constitutionResistance + agility / 5 + endurance / 8);
            woodResistance = Mathf.Max(0, constitutionResistance + endurance / 6 + intelligence / 8);
            ResetElementalDamage();
            if (resetProficiencies)
            {
                fistsProficiency = 0;
                swordProficiency = 0;
                axeProficiency = 0;
                spearProficiency = 0;
                staffProficiency = 0;
                bowProficiency = 0;
            }
        }

        public void RefreshDerivedCombatTotals()
        {
            speed = Mathf.Max(0, agility + recovery);
        }

        public int GetActionSpeedRating()
        {
            return speed > 0 ? speed : Mathf.Max(0, agility + recovery);
        }

        public int GetProficiency(WeaponType weapon)
        {
            switch (weapon)
            {
                case WeaponType.Fists:
                    return fistsProficiency;
                case WeaponType.Sword:
                    return swordProficiency;
                case WeaponType.Axe:
                    return axeProficiency;
                case WeaponType.Spear:
                    return spearProficiency;
                case WeaponType.Staff:
                    return staffProficiency;
                case WeaponType.Bow:
                    return bowProficiency;
                default:
                    return 0;
            }
        }

        public float GetDodgeChancePercent()
        {
            return RatingToPercent(dodge);
        }

        public float GetBlockChancePercent()
        {
            return RatingToPercent(block);
        }

        public float GetCriticalChancePercent(WeaponType weapon)
        {
            return RatingToPercent(critical + GetProficiency(weapon));
        }

        public float GetHitPressurePercent()
        {
            return RatingToPercent(hit);
        }

        public static float ConvertRatingToPercent(int rating)
        {
            return RatingToPercent(rating);
        }

        public static float ConvertResistanceRatingToPercent(int rating)
        {
            return RatingToCappedPercent(rating, 70f);
        }

        public int GetElementalResistance(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    return fireResistance;
                case SkillElement.Water:
                    return waterResistance;
                case SkillElement.Electricity:
                    return electricityResistance;
                case SkillElement.Poison:
                    return poisonResistance;
                case SkillElement.Earth:
                    return earthResistance;
                case SkillElement.Air:
                    return airResistance;
                case SkillElement.Wood:
                    return woodResistance;
                default:
                    return 0;
            }
        }

        public float GetElementalResistancePercent(SkillElement element)
        {
            return RatingToCappedPercent(GetElementalResistance(element), 70f);
        }

        public int GetElementalDamageBonus(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    return fireDamage;
                case SkillElement.Water:
                    return waterDamage;
                case SkillElement.Electricity:
                    return electricityDamage;
                case SkillElement.Poison:
                    return poisonDamage;
                case SkillElement.Earth:
                    return earthDamage;
                case SkillElement.Air:
                    return airDamage;
                case SkillElement.Wood:
                    return woodDamage;
                default:
                    return 0;
            }
        }

        public float GetElementalDamageMultiplier(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    return SanitizeMultiplier(fireDamageMultiplier);
                case SkillElement.Water:
                    return SanitizeMultiplier(waterDamageMultiplier);
                case SkillElement.Electricity:
                    return SanitizeMultiplier(electricityDamageMultiplier);
                case SkillElement.Poison:
                    return SanitizeMultiplier(poisonDamageMultiplier);
                case SkillElement.Earth:
                    return SanitizeMultiplier(earthDamageMultiplier);
                case SkillElement.Air:
                    return SanitizeMultiplier(airDamageMultiplier);
                case SkillElement.Wood:
                    return SanitizeMultiplier(woodDamageMultiplier);
                default:
                    return 1f;
            }
        }

        public void AddElementalDamageBonus(SkillElement element, int value)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    fireDamage += value;
                    break;
                case SkillElement.Water:
                    waterDamage += value;
                    break;
                case SkillElement.Electricity:
                    electricityDamage += value;
                    break;
                case SkillElement.Poison:
                    poisonDamage += value;
                    break;
                case SkillElement.Earth:
                    earthDamage += value;
                    break;
                case SkillElement.Air:
                    airDamage += value;
                    break;
                case SkillElement.Wood:
                    woodDamage += value;
                    break;
            }
        }

        public void MultiplyElementalDamage(SkillElement element, float multiplier)
        {
            float safeMultiplier = SanitizeMultiplier(multiplier);
            switch (element)
            {
                case SkillElement.Fire:
                    fireDamageMultiplier = SanitizeMultiplier(fireDamageMultiplier) * safeMultiplier;
                    break;
                case SkillElement.Water:
                    waterDamageMultiplier = SanitizeMultiplier(waterDamageMultiplier) * safeMultiplier;
                    break;
                case SkillElement.Electricity:
                    electricityDamageMultiplier = SanitizeMultiplier(electricityDamageMultiplier) * safeMultiplier;
                    break;
                case SkillElement.Poison:
                    poisonDamageMultiplier = SanitizeMultiplier(poisonDamageMultiplier) * safeMultiplier;
                    break;
                case SkillElement.Earth:
                    earthDamageMultiplier = SanitizeMultiplier(earthDamageMultiplier) * safeMultiplier;
                    break;
                case SkillElement.Air:
                    airDamageMultiplier = SanitizeMultiplier(airDamageMultiplier) * safeMultiplier;
                    break;
                case SkillElement.Wood:
                    woodDamageMultiplier = SanitizeMultiplier(woodDamageMultiplier) * safeMultiplier;
                    break;
            }
        }

        private void ResetElementalDamage()
        {
            fireDamage = 0;
            waterDamage = 0;
            electricityDamage = 0;
            poisonDamage = 0;
            earthDamage = 0;
            airDamage = 0;
            woodDamage = 0;
            fireDamageMultiplier = 1f;
            waterDamageMultiplier = 1f;
            electricityDamageMultiplier = 1f;
            poisonDamageMultiplier = 1f;
            earthDamageMultiplier = 1f;
            airDamageMultiplier = 1f;
            woodDamageMultiplier = 1f;
        }

        private static float SanitizeMultiplier(float multiplier)
        {
            return multiplier > 0f ? multiplier : 1f;
        }

        private static float RatingToPercent(int rating)
        {
            return Mathf.Round((1f - 100f / (100f + Mathf.Max(0, rating))) * 1000f) / 10f;
        }

        private static float RatingToCappedPercent(int rating, float capPercent)
        {
            float normalized = 1f - 100f / (100f + Mathf.Max(0, rating));
            return Mathf.Round(capPercent * normalized * 10f) / 10f;
        }
    }

    [Serializable]
    public sealed class ChampionGrowthProfile
    {
        public GrowthGrade strength;
        public GrowthGrade agility;
        public GrowthGrade constitution;
        public GrowthGrade intelligence;
        public GrowthGrade endurance;

        public static ChampionGrowthProfile CreateRandom(System.Random rng)
        {
            return new ChampionGrowthProfile
            {
                strength = RandomGrade(rng),
                agility = RandomGrade(rng),
                constitution = RandomGrade(rng),
                intelligence = RandomGrade(rng),
                endurance = RandomGrade(rng)
            };
        }

        private static GrowthGrade RandomGrade(System.Random rng)
        {
            return ParetoRoller.RollGrade(rng);
        }

        public GrowthGrade GetOverallGrade()
        {
            float average = ((int)strength + (int)agility + (int)constitution + (int)intelligence + (int)endurance) / 5f;
            int rounded = Mathf.Clamp(Mathf.RoundToInt(average), 0, (int)GrowthGrade.SS);
            return (GrowthGrade)rounded;
        }
    }
}
