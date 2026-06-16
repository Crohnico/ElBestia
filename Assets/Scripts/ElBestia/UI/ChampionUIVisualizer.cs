using ElBestia.Champions;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class ChampionUIVisualizer : MonoBehaviour
    {
        public SkillUIVisualizer skillUIVisualizer;
        public PerkUIVisualizer perkUIVisualizer;
        public BaseStatsUIVisualizer baseStatsUIVisualizer;

        private ChampionData champion;

        public void SetUpUI(ChampionData championData)
        {
            SetUp(championData);
        }

        public void SetUp(ChampionData championData)
        {
            champion = championData;

            if (skillUIVisualizer != null)
            {
                skillUIVisualizer.SetUp(champion != null ? champion.Skills : null);
            }

            if (perkUIVisualizer != null)
            {
                perkUIVisualizer.SetUp(champion != null ? champion.Perks : null);
            }

            if (baseStatsUIVisualizer != null)
            {
                baseStatsUIVisualizer.SetUp(champion);
            }
        }
    }
}
