using ElBestia.Champions;
using ElBestia.UI;
using ElBestia.Visuals;
using UnityEngine;

namespace ElBestia.Dojo
{
    public sealed class ChampionShowcase : MonoBehaviour
    {
        public Transform baseStickman;
        public Transform animationRoot;
        public bool collapseAnimationRootAfterFirstShow = true;
        public ChampionUIVisualizer championUIVisualizer;

        private StickmanBodyConfigurator configurator;
        private bool firstShowCompleted;

        private void Awake()
        {
            configurator = baseStickman.GetComponentInChildren<StickmanBodyConfigurator>(true);
        }

        public void Show(ChampionData championData)
        {
            if (animationRoot != null)
            {
                animationRoot.localScale = Vector3.one;
            }

            configurator.ConfigureFromAppearance(championData.Appearance, championData.EquippedWeapon);

            if (championUIVisualizer != null)
            {
                championUIVisualizer.SetUpUI(championData);
            }

            if (!firstShowCompleted && collapseAnimationRootAfterFirstShow && animationRoot != null)
            {
                animationRoot.localScale = Vector3.zero;
            }

            firstShowCompleted = true;
        }
    }
}
