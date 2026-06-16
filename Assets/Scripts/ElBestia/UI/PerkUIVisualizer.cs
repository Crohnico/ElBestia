using System.Collections.Generic;
using ElBestia.Perks;
using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    public sealed class PerkUIVisualizer : MonoBehaviour
    {
        public PerkDataVisualizer perkDataVisualizerPrefab;
        public Transform contentParent;
        public PerkDataInfoContainer dataContainer;

        private readonly List<PerkDataVisualizer> spawnedVisualizers = new List<PerkDataVisualizer>();

        private void Awake()
        {
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

            dataContainer.gameObject.SetActive(AnyPerkHovered());
        }

        public void SetUp(PerkSO[] perks)
        {
            Clear();

            if (perkDataVisualizerPrefab == null || contentParent == null || perks == null)
            {
                HideDataContainer();
                return;
            }

            for (int i = 0; i < perks.Length; i++)
            {
                if (perks[i] == null)
                {
                    continue;
                }

                PerkDataVisualizer visualizer = Instantiate(perkDataVisualizerPrefab, contentParent);
                visualizer.gameObject.SetActive(true);
                visualizer.Setup(perks[i], dataContainer);
                spawnedVisualizers.Add(visualizer);
            }

            LayoutRebuilder.MarkLayoutForRebuild(contentParent as RectTransform);
            HideDataContainer();
        }

        public void Clear()
        {
            for (int i = spawnedVisualizers.Count - 1; i >= 0; i--)
            {
                DestroyVisualizer(spawnedVisualizers[i]);
            }

            spawnedVisualizers.Clear();
            HideDataContainer();
        }

        private bool AnyPerkHovered()
        {
            for (int i = 0; i < spawnedVisualizers.Count; i++)
            {
                PerkDataVisualizer visualizer = spawnedVisualizers[i];
                if (visualizer != null && visualizer.gameObject.activeInHierarchy && visualizer.IsHover)
                {
                    return true;
                }
            }

            return false;
        }

        private void HideDataContainer()
        {
            if (dataContainer != null)
            {
                dataContainer.gameObject.SetActive(false);
            }
        }

        private static void DestroyVisualizer(PerkDataVisualizer visualizer)
        {
            if (visualizer == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(visualizer.gameObject);
                return;
            }

            DestroyImmediate(visualizer.gameObject);
        }
    }
}
