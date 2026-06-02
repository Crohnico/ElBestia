using ElBestia.Champions;
using ElBestia.Skills;

namespace ElBestia.Combat
{
    internal static class CombatStatUtility
    {
        public static int GetStatValue(ChampionStats stats, ChampionStatType stat)
        {
            switch (stat)
            {
                case ChampionStatType.Strength:
                    return stats.strength;
                case ChampionStatType.Agility:
                    return stats.agility;
                case ChampionStatType.Constitution:
                    return stats.constitution;
                case ChampionStatType.Intelligence:
                    return stats.intelligence;
                case ChampionStatType.Endurance:
                    return stats.endurance;
                default:
                    return 0;
            }
        }

        public static float GetScalingMultiplier(GrowthGrade grade)
        {
            switch (grade)
            {
                case GrowthGrade.F:
                    return 0.2f;
                case GrowthGrade.E:
                    return 0.3f;
                case GrowthGrade.D:
                    return 0.42f;
                case GrowthGrade.C:
                    return 0.55f;
                case GrowthGrade.B:
                    return 0.7f;
                case GrowthGrade.A:
                    return 0.9f;
                case GrowthGrade.S:
                    return 1.15f;
                case GrowthGrade.SS:
                    return 1.45f;
                default:
                    return 0.5f;
            }
        }

        public static WeaponType GetSkillWeapon(SkillData skill)
        {
            return skill != null && skill.weapons != null && skill.weapons.Length > 0 ? skill.weapons[0] : WeaponType.None;
        }
    }
}
