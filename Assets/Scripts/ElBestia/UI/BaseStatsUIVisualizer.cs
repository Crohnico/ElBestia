using ElBestia.Champions;
using TMPro;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class BaseStatsUIVisualizer : MonoBehaviour
    {
        public TMP_Text statsText;

        public void SetUp(ChampionData champion)
        {
            if (statsText == null)
            {
                return;
            }

            ChampionStats stats = champion != null ? champion.Stats : null;
            if (stats == null)
            {
                statsText.text = string.Empty;
                return;
            }

            statsText.text =
                $"{stats.strength}\n" +
                $"{stats.agility}\n" +
                $"{stats.constitution}\n" +
                $"{stats.intelligence}\n" +
                $"{stats.endurance}";
        }
    }
}
