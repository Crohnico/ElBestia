using ElBestia.Champions;
using ElBestia.Visuals;
using UnityEngine;

namespace ElBestia.Dojo
{
    public sealed class ChampionShowcase : MonoBehaviour
    {
        public Transform baseStickman;
        public Transform animationRoot;
        public bool collapseAnimationRootAfterFirstShow = true;

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

            if (!firstShowCompleted && collapseAnimationRootAfterFirstShow && animationRoot != null)
            {
                animationRoot.localScale = Vector3.zero;
            }

            firstShowCompleted = true;
        }
    }
}
