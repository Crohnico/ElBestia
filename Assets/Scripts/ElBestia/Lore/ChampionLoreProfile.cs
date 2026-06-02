using System;

namespace ElBestia.Lore
{
    [Serializable]
    public sealed class ChampionLoreProfile
    {
        public ChampionLoreEntrySO birth;
        public ChampionLoreEntrySO childhood;
        public ChampionLoreEntrySO youth;
        public string story;
    }
}
