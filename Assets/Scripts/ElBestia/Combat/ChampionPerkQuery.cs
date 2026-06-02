using ElBestia.Champions;
using ElBestia.Perks;

namespace ElBestia.Combat
{
    internal static class ChampionPerkQuery
    {
        public static PerkSO GetFirst(ChampionData champion, PerkEffectType effectType)
        {
            if (champion == null || champion.perks == null)
            {
                return null;
            }

            foreach (PerkSO perk in champion.perks)
            {
                if (perk != null && perk.EffectType == effectType)
                {
                    return perk;
                }
            }

            return null;
        }

        public static PerkSO GetFirst(ChampionSO champion, PerkEffectType effectType)
        {
            if (champion == null || champion.Perks == null)
            {
                return null;
            }

            foreach (PerkSO perk in champion.Perks)
            {
                if (perk != null && perk.EffectType == effectType)
                {
                    return perk;
                }
            }

            return null;
        }

        public static bool Has(ChampionSO champion, PerkEffectType effectType)
        {
            return GetFirst(champion, effectType) != null;
        }

        public static bool Has(ChampionData champion, PerkEffectType effectType)
        {
            return GetFirst(champion, effectType) != null;
        }
    }
}
