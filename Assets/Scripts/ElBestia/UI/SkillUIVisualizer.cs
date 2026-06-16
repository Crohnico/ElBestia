using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class SkillUIVisualizer : MonoBehaviour
    {
        public SkillDataVisualizer skill1;
        public SkillDataVisualizer skill2;
        public SkillDataVisualizer skill3;
        public SkillDataInfoContainer dataContainer;

        private SkillDataVisualizer[] visualizers;

        private void Awake()
        {
            visualizers = new[] { skill1, skill2, skill3 };

            if (dataContainer != null)
            {
                dataContainer.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (dataContainer == null)
            {
                return;
            }

            dataContainer.gameObject.SetActive(AnySkillHovered());
        }

        public void SetUp(SkillData[] skills)
        {
            EnsureVisualizers();

            for (int i = 0; i < visualizers.Length; i++)
            {
                SkillDataVisualizer visualizer = visualizers[i];
                if (visualizer == null)
                {
                    continue;
                }

                SkillData skill = skills != null && i < skills.Length ? skills[i] : null;
                visualizer.OnHover(false);
                visualizer.gameObject.SetActive(skill != null);

                if (skill != null)
                {
                    visualizer.SetUp(skill, dataContainer);
                }
            }

            if (dataContainer != null)
            {
                dataContainer.gameObject.SetActive(false);
            }
        }

        private bool AnySkillHovered()
        {
            EnsureVisualizers();

            for (int i = 0; i < visualizers.Length; i++)
            {
                if (visualizers[i] != null && visualizers[i].gameObject.activeInHierarchy && visualizers[i].IsHover)
                {
                    return true;
                }
            }

            return false;
        }

        private void EnsureVisualizers()
        {
            if (visualizers == null || visualizers.Length != 3)
            {
                visualizers = new[] { skill1, skill2, skill3 };
            }
        }
    }
}
