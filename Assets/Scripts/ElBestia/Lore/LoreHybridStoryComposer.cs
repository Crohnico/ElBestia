namespace ElBestia.Lore
{
    public static class LoreHybridStoryComposer
    {
        public static string BuildStory(string championName, ChampionLoreEntrySO birth, ChampionLoreEntrySO childhood, ChampionLoreEntrySO youth)
        {
            string name = string.IsNullOrEmpty(championName) ? "This champion" : championName;
            return $"{Fragment(birth, name)} {Fragment(childhood, name)} {Fragment(youth, name)}";
        }

        public static string GetCombinationId(ChampionLoreEntrySO birth, ChampionLoreEntrySO childhood, ChampionLoreEntrySO youth)
        {
            return $"{GetEntryId(birth)}__{GetEntryId(childhood)}__{GetEntryId(youth)}";
        }

        public static int GetCombinationKey(ChampionLoreEntrySO birth, ChampionLoreEntrySO childhood, ChampionLoreEntrySO youth)
        {
            int birthKey = GetEntryKey(birth);
            int childhoodKey = GetEntryKey(childhood);
            int youthKey = GetEntryKey(youth);
            return birthKey * 40000 + childhoodKey * 200 + youthKey;
        }

        private static string Fragment(ChampionLoreEntrySO entry, string championName)
        {
            string text = entry != null && !string.IsNullOrEmpty(entry.StoryFragment)
                ? entry.StoryFragment
                : "{name} left few useful records behind.";

            return text.Replace("{name}", championName);
        }

        private static int GetEntryKey(ChampionLoreEntrySO entry)
        {
            string id = GetEntryId(entry);
            int last = id.LastIndexOf('_');
            int previous = last > 0 ? id.LastIndexOf('_', last - 1) : -1;
            if (previous < 0 || last < 0)
            {
                return 0;
            }

            if (!int.TryParse(id.Substring(previous + 1, last - previous - 1), out int anchor))
            {
                anchor = 0;
            }

            if (!int.TryParse(id.Substring(last + 1), out int tone))
            {
                tone = 0;
            }

            return anchor * 10 + tone;
        }

        private static string GetEntryId(ChampionLoreEntrySO entry)
        {
            return entry != null && !string.IsNullOrEmpty(entry.EntryId) ? entry.EntryId : "unknown_00_00";
        }
    }
}
