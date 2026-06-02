using System.Collections.Generic;
using ElBestia.Perks;
using UnityEngine;

namespace ElBestia.Generation
{
    public static class PerkGenerator
    {
        public static PerkSO[] RollStartingPerks(System.Random rng)
        {
            PerkSO[] allPerks = Resources.LoadAll<PerkSO>("Perks");
            if (allPerks == null || allPerks.Length == 0)
            {
                return new PerkSO[0];
            }

            int count = ParetoRoller.RollCountOneToThree(rng);
            var selected = new List<PerkSO>();

            for (int i = 0; i < count; i++)
            {
                PerkRarity rarity = ParetoRoller.RollRarity(rng);
                PerkSO perk = PickPerkByRarity(allPerks, rarity, rng, selected);
                if (perk != null)
                {
                    selected.Add(perk);
                }
            }

            return selected.ToArray();
        }

        private static PerkSO PickPerkByRarity(PerkSO[] allPerks, PerkRarity rarity, System.Random rng, List<PerkSO> excluded)
        {
            var candidates = new List<PerkSO>();
            foreach (PerkSO perk in allPerks)
            {
                if (perk != null && perk.Rarity == rarity && CanSelect(perk, excluded))
                {
                    candidates.Add(perk);
                }
            }

            if (candidates.Count == 0)
            {
                foreach (PerkSO perk in allPerks)
                {
                    if (perk != null && CanSelect(perk, excluded))
                    {
                        candidates.Add(perk);
                    }
                }
            }

            return candidates.Count > 0 ? candidates[rng.Next(0, candidates.Count)] : null;
        }

        private static bool CanSelect(PerkSO perk, List<PerkSO> selected)
        {
            if (IsDeprecatedDefaultPerk(perk))
            {
                return false;
            }

            if (selected.Contains(perk))
            {
                return false;
            }

            if (!perk.IsUnique || string.IsNullOrEmpty(perk.UniqueGroup))
            {
                return true;
            }

            foreach (PerkSO selectedPerk in selected)
            {
                if (selectedPerk != null
                    && selectedPerk.IsUnique
                    && selectedPerk.UniqueGroup == perk.UniqueGroup)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsDeprecatedDefaultPerk(PerkSO perk)
        {
            switch (perk.PerkId)
            {
                case "haste_rhythm":
                case "haste_engine":
                case "haste_crown":
                case "haste_legend":
                case "slow_rhythm":
                case "slow_engine":
                case "slow_crown":
                case "slow_legend":
                    return true;
                default:
                    return false;
            }
        }
    }
}
