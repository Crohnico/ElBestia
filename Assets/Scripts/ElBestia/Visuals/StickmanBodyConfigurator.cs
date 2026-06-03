using System;
using System.Collections.Generic;
using UnityEngine;

namespace ElBestia.Visuals
{
    [ExecuteAlways]
    public sealed class StickmanBodyConfigurator : MonoBehaviour
    {
        [SerializeField] private List<StickmanBodyPartGroup> groups = new List<StickmanBodyPartGroup>();
        [SerializeField] private bool applyContinuously;

        public int GroupCount => groups != null ? groups.Count : 0;

        public StickmanBodyPartGroup GetGroup(int index)
        {
            return groups[index];
        }

        public void EnsureDefaultGroups()
        {
            if (groups != null && groups.Count > 0)
            {
                return;
            }

            ResetDefaultGroups();
        }

        public void ResetDefaultGroups()
        {
            groups = new List<StickmanBodyPartGroup>
            {
                new StickmanBodyPartGroup("Pecho", "Pecho"),
                new StickmanBodyPartGroup("Barriga", "Barriga"),
                new StickmanBodyPartGroup("Brazos", "BrazoSuperior"),
                new StickmanBodyPartGroup("AnteBrazos", "BrazoInferior", "AnteBrazo", "Antebrazo"),
                new StickmanBodyPartGroup("Pierna Superior", "PiernaSuperior"),
                new StickmanBodyPartGroup("Pierna Inferior", "PiernaInferior")
            };
        }

        public void CollectSegments()
        {
            EnsureDefaultGroups();
            ApplyDefaultVisibilityRules();
            StickmanSegmentMesh[] segments = GetComponentsInChildren<StickmanSegmentMesh>(true);

            for (int i = 0; i < groups.Count; i++)
            {
                groups[i].ClearSegments();
            }

            foreach (StickmanSegmentMesh segment in segments)
            {
                if (segment == null)
                {
                    continue;
                }

                string segmentName = segment.gameObject.name;
                for (int i = 0; i < groups.Count; i++)
                {
                    if (groups[i].Matches(segmentName))
                    {
                        groups[i].AddSegment(segment);
                        break;
                    }
                }
            }
        }

        public void CaptureCurrentRadii()
        {
            EnsureDefaultGroups();
            ApplyDefaultVisibilityRules();
            foreach (StickmanBodyPartGroup group in groups)
            {
                group.CaptureCurrentRadii();
            }
        }

        public void ApplyAll()
        {
            EnsureDefaultGroups();
            foreach (StickmanBodyPartGroup group in groups)
            {
                group.Apply();
            }
        }

        public void ApplyDefaultVisibilityRules()
        {
            EnsureDefaultGroups();
            foreach (StickmanBodyPartGroup group in groups)
            {
                group.ShowAllSections();
            }
        }

        public bool PruneMissingSegments()
        {
            EnsureDefaultGroups();
            bool changed = false;
            foreach (StickmanBodyPartGroup group in groups)
            {
                changed |= group.PruneMissingSegments();
            }

            return changed;
        }

        private void Reset()
        {
            ResetDefaultGroups();
            CollectSegments();
            CaptureCurrentRadii();
        }

        private void OnValidate()
        {
            EnsureDefaultGroups();
        }

        private void Update()
        {
            if (applyContinuously)
            {
                ApplyAll();
            }
        }
    }

    [Serializable]
    public sealed class StickmanBodyPartGroup
    {
        [SerializeField] private string displayName;
        [SerializeField] private List<string> nameContains = new List<string>();
        [SerializeField] private List<StickmanSegmentMesh> segments = new List<StickmanSegmentMesh>();
        [SerializeField] private List<float> radii = new List<float>();
        [SerializeField] private bool expanded = true;
        [SerializeField] private List<int> visibleSectionIndices = new List<int>();

        public string DisplayName => displayName;
        public int SegmentCount => segments != null ? segments.Count : 0;
        public int SectionCount => radii != null ? radii.Count : 0;
        public bool Expanded { get => expanded; set => expanded = value; }

        public StickmanBodyPartGroup(string displayName, params string[] patterns)
            : this(displayName, null, patterns)
        {
        }

        public StickmanBodyPartGroup(string displayName, int[] visibleSectionIndices, params string[] patterns)
        {
            this.displayName = displayName;
            nameContains = new List<string>(patterns);
            segments = new List<StickmanSegmentMesh>();
            radii = new List<float>();
            this.visibleSectionIndices = visibleSectionIndices != null ? new List<int>(visibleSectionIndices) : new List<int>();
            expanded = true;
        }

        public StickmanSegmentMesh GetSegment(int index)
        {
            return segments[index];
        }

        public bool IsSectionVisible(int sectionIndex)
        {
            if (visibleSectionIndices == null || visibleSectionIndices.Count == 0)
            {
                return true;
            }

            return visibleSectionIndices.Contains(sectionIndex);
        }

        public void SetVisibleSections(params int[] indices)
        {
            visibleSectionIndices = indices != null ? new List<int>(indices) : new List<int>();
        }

        public void ShowAllSections()
        {
            visibleSectionIndices = new List<int>();
        }

        public void HideSections(params int[] indices)
        {
            int sectionCount = SectionCount;
            visibleSectionIndices = new List<int>();
            for (int i = 0; i < sectionCount; i++)
            {
                if (indices == null || Array.IndexOf(indices, i) < 0)
                {
                    visibleSectionIndices.Add(i);
                }
            }
        }

        public float GetRadius(int sectionIndex)
        {
            EnsureRadiusCount();
            return radii[Mathf.Clamp(sectionIndex, 0, radii.Count - 1)];
        }

        public float GetMinRadius(int sectionIndex)
        {
            if (segments == null || segments.Count == 0)
            {
                return 0.001f;
            }

            float min = 0.001f;
            foreach (StickmanSegmentMesh segment in segments)
            {
                if (segment == null)
                {
                    continue;
                }

                int mappedIndex = MapSectionIndex(sectionIndex, SectionCount, segment.SectionCount);
                min = Mathf.Max(min, segment.GetSectionMinRadius(mappedIndex));
            }

            return min;
        }

        public float GetMaxRadius(int sectionIndex)
        {
            if (segments == null || segments.Count == 0)
            {
                return 1f;
            }

            float max = float.MaxValue;
            foreach (StickmanSegmentMesh segment in segments)
            {
                if (segment == null)
                {
                    continue;
                }

                int mappedIndex = MapSectionIndex(sectionIndex, SectionCount, segment.SectionCount);
                max = Mathf.Min(max, segment.GetSectionMaxRadius(mappedIndex));
            }

            return max < float.MaxValue ? max : 1f;
        }

        public void SetRadius(int sectionIndex, float radius)
        {
            EnsureRadiusCount();
            if (sectionIndex < 0 || sectionIndex >= radii.Count)
            {
                return;
            }

            radii[sectionIndex] = Mathf.Clamp(radius, GetMinRadius(sectionIndex), GetMaxRadius(sectionIndex));
            Apply();
        }

        public bool Matches(string segmentName)
        {
            if (string.IsNullOrEmpty(segmentName) || nameContains == null)
            {
                return false;
            }

            for (int i = 0; i < nameContains.Count; i++)
            {
                string pattern = nameContains[i];
                if (!string.IsNullOrEmpty(pattern) && segmentName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearSegments()
        {
            if (segments == null)
            {
                segments = new List<StickmanSegmentMesh>();
            }

            segments.Clear();
        }

        public void AddSegment(StickmanSegmentMesh segment)
        {
            if (segments == null)
            {
                segments = new List<StickmanSegmentMesh>();
            }

            if (segment != null && !segments.Contains(segment))
            {
                segments.Add(segment);
            }
        }

        public bool PruneMissingSegments()
        {
            if (segments == null)
            {
                segments = new List<StickmanSegmentMesh>();
                return true;
            }

            bool changed = false;
            for (int i = segments.Count - 1; i >= 0; i--)
            {
                if (segments[i] == null)
                {
                    segments.RemoveAt(i);
                    changed = true;
                }
            }

            return changed;
        }

        public void CaptureCurrentRadii()
        {
            PruneMissingSegments();
            int targetSectionCount = GetLargestSectionCount();
            if (targetSectionCount <= 0)
            {
                return;
            }

            if (radii == null)
            {
                radii = new List<float>();
            }

            radii.Clear();
            for (int i = 0; i < targetSectionCount; i++)
            {
                float total = 0f;
                int count = 0;
                foreach (StickmanSegmentMesh segment in segments)
                {
                    if (segment == null)
                    {
                        continue;
                    }

                    int mappedIndex = MapSectionIndex(i, targetSectionCount, segment.SectionCount);
                    total += segment.GetSectionRadius(mappedIndex);
                    count++;
                }

                radii.Add(count > 0 ? total / count : 0.08f);
            }
        }

        public void Apply()
        {
            PruneMissingSegments();
            EnsureRadiusCount();

            if (segments == null)
            {
                return;
            }

            for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
            {
                StickmanSegmentMesh segment = segments[segmentIndex];
                if (segment == null)
                {
                    continue;
                }

                for (int i = 0; i < segment.SectionCount; i++)
                {
                    int sourceIndex = MapSectionIndex(i, segment.SectionCount, radii.Count);
                    segment.SetSectionRadius(i, radii[sourceIndex]);
                }

                segment.Rebuild();
            }
        }

        private void EnsureRadiusCount()
        {
            if (radii == null)
            {
                radii = new List<float>();
            }

            int targetSectionCount = GetLargestSectionCount();
            if (targetSectionCount <= 0)
            {
                targetSectionCount = Mathf.Max(1, radii.Count);
            }

            while (radii.Count < targetSectionCount)
            {
                float value = radii.Count > 0 ? radii[radii.Count - 1] : 0.08f;
                radii.Add(value);
            }

            while (radii.Count > targetSectionCount)
            {
                radii.RemoveAt(radii.Count - 1);
            }
        }

        private int GetLargestSectionCount()
        {
            int sectionCount = 0;
            if (segments == null)
            {
                return sectionCount;
            }

            foreach (StickmanSegmentMesh segment in segments)
            {
                if (segment != null)
                {
                    sectionCount = Mathf.Max(sectionCount, segment.SectionCount);
                }
            }

            return sectionCount;
        }

        private static int MapSectionIndex(int index, int sourceCount, int targetCount)
        {
            if (targetCount <= 1 || sourceCount <= 1)
            {
                return 0;
            }

            float t = index / (float)(sourceCount - 1);
            return Mathf.Clamp(Mathf.RoundToInt(t * (targetCount - 1)), 0, targetCount - 1);
        }
    }
}
