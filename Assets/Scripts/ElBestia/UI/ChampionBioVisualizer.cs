using ElBestia.Champions;
using TMPro;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class ChampionBioVisualizer : MonoBehaviour
    {
        public TMP_Text content;

        public void SetUp(ChampionData champion)
        {
            if (content == null)
            {
                return;
            }

            content.text = champion != null && champion.Lore != null
                ? champion.Lore.story
                : string.Empty;
        }
    }
}
