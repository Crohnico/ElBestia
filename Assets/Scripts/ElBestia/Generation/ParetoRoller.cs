using System;
using ElBestia.Champions;
using ElBestia.Perks;

namespace ElBestia.Generation
{
    public static class ParetoRoller
    {
        public static GrowthGrade RollGrade(Random rng)
        {
            GrowthGrade[] grades =
            {
                GrowthGrade.F,
                GrowthGrade.E,
                GrowthGrade.D,
                GrowthGrade.C,
                GrowthGrade.B,
                GrowthGrade.A,
                GrowthGrade.S,
                GrowthGrade.SS
            };

            int index = RollIndex(rng, grades.Length);
            return grades[index];
        }

        public static PerkRarity RollRarity(Random rng)
        {
            PerkRarity[] rarities =
            {
                PerkRarity.Common,
                PerkRarity.Rare,
                PerkRarity.VeryRare,
                PerkRarity.Epic,
                PerkRarity.Legendary
            };

            int index = RollIndex(rng, rarities.Length);
            return rarities[index];
        }

        public static int RollCountOneToThree(Random rng)
        {
            if (rng.NextDouble() < 0.8)
            {
                return 1;
            }

            return rng.NextDouble() < 0.8 ? 2 : 3;
        }

        private static int RollIndex(Random rng, int length)
        {
            for (int i = 0; i < length - 1; i++)
            {
                if (rng.NextDouble() < 0.8)
                {
                    return i;
                }
            }

            return length - 1;
        }
    }
}

