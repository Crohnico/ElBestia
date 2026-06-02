using ElBestia.Champions;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Lore
{
    [CreateAssetMenu(menuName = "El Bestia/Lore/Entry", fileName = "LoreEntry")]
    public sealed class ChampionLoreEntrySO : ScriptableObject
    {
        [SerializeField] private string entryId;
        [SerializeField] private ChampionLoreStage stage;
        [SerializeField] private string title;
        [TextArea(2, 4)]
        [SerializeField] private string storyFragment;
        [SerializeField] private ChampionLoreModifier[] modifiers = new ChampionLoreModifier[0];

        public string EntryId => entryId;
        public ChampionLoreStage Stage => stage;
        public string Title => title;
        public string StoryFragment => storyFragment;
        public ChampionLoreModifier[] Modifiers => modifiers;

        public void Configure(string id, ChampionLoreStage loreStage, string entryTitle, string fragment, ChampionLoreModifier[] entryModifiers)
        {
            entryId = id;
            stage = loreStage;
            title = entryTitle;
            storyFragment = fragment;
            modifiers = entryModifiers ?? new ChampionLoreModifier[0];
        }

        public static ChampionLoreEntrySO CreateRuntime(string id, ChampionLoreStage loreStage, string entryTitle, string fragment, ChampionLoreModifier[] entryModifiers)
        {
            ChampionLoreEntrySO entry = CreateInstance<ChampionLoreEntrySO>();
            entry.Configure(id, loreStage, entryTitle, fragment, entryModifiers);
            return entry;
        }
    }
}
