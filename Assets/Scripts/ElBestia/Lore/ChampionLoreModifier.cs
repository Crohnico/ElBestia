using System;
using ElBestia.Champions;
using ElBestia.Perks;
using ElBestia.Skills;

namespace ElBestia.Lore
{
    [Serializable]
    public sealed class ChampionLoreModifier
    {
        public ChampionLoreModifierType type;
        public ChampionStatType stat;
        public PerkCombatStatType combatStat;
        public WeaponType weapon;
        public SkillElement element;
        public int value;
        public float multiplier = 1f;
    }
}
