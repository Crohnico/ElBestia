using System;
using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Visuals
{
    [ExecuteAlways]
    public sealed class StickmanBodyConfigurator : MonoBehaviour
    {
        [SerializeField] private List<StickmanBodyPartGroup> groups = new List<StickmanBodyPartGroup>();
        [SerializeField] private bool applyContinuously;
        [SerializeField] private StickmanGluteScaler gluteScaler;
        [Header("Attachments")]
        [SerializeField] private Transform headAttachment;
        [SerializeField] private Transform beardAttachment;
        [SerializeField] private Transform earAttachment;
        [SerializeField] private Transform leftHandAttachment;
        [SerializeField] private Transform rightHandAttachment;
        [Header("Colors")]
        [SerializeField] private Renderer[] bodyRenderers = Array.Empty<Renderer>();

        public int GroupCount => groups != null ? groups.Count : 0;
        public StickmanGluteScaler GluteScaler { get => gluteScaler; set => gluteScaler = value; }

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

        public void ConfigureFromChampion(ChampionSO champion)
        {
            ChampionAppearance appearance = champion != null ? champion.Appearance : ChampionAppearance.CreateRandom(new System.Random());
            ConfigureFromAppearance(appearance, champion != null ? champion.EquippedWeapon : WeaponType.Fists);
        }

        public void ConfigureFromAppearance(ChampionAppearance appearance, WeaponType weaponType)
        {
            if (appearance == null)
            {
                appearance = ChampionAppearance.CreateRandom(new System.Random());
            }
            else if (LooksLikeLegacyAppearance(appearance))
            {
                appearance = ChampionAppearance.CreateRandom(new System.Random());
            }

            ClearAttachmentChildren();
            SetNormalizedRadius("Pecho", 3, appearance.chest);
            SetNormalizedRadius("Barriga", 3, appearance.belly);
            SetNormalizedRadius("Brazos", 2, appearance.arms);
            SetNormalizedRadius("AnteBrazos", 2, appearance.forearms);
            SetNormalizedRadius("PiernaSuperior", 2, appearance.upperLegs);
            SetNormalizedRadius("PiernaInferior", 2, appearance.lowerLegs);

            if (gluteScaler != null)
            {
                gluteScaler.Size = appearance.glutes;
            }

            Color bodyColor = GetUsableColor(appearance.bodyColor, new Color(0.72f, 0.49f, 0.36f, 1f));
            Color hairColor = GetUsableColor(appearance.hairColor, new Color(0.18f, 0.11f, 0.07f, 1f));
            ApplyColorToBody(bodyColor);
            InstantiateCatalogPart(HairSO.Instance, appearance.sex, headAttachment, bodyColor, hairColor);
            InstantiateCatalogPart(BeardSO.Instance, appearance.sex, beardAttachment, bodyColor, hairColor);
            InstantiateCatalogPart(EarSO.Instance, appearance.sex, earAttachment, bodyColor, hairColor);
            InstantiateWeapon(weaponType, bodyColor, hairColor);
            FreezeGeneratedMeshes();
        }

        public void ApplyColorToBody(Color color)
        {
            if (bodyRenderers == null)
            {
                return;
            }

            for (int i = 0; i < bodyRenderers.Length; i++)
            {
                ApplyColor(bodyRenderers[i], color);
            }
        }

        public void FreezeGeneratedMeshes()
        {
            StickmanSegmentMesh[] segments = GetComponentsInChildren<StickmanSegmentMesh>(true);
            for (int i = 0; i < segments.Length; i++)
            {
                if (segments[i] == null)
                {
                    continue;
                }

                segments[i].Rebuild();
                segments[i].RebuildContinuously = false;
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

        private void SetNormalizedRadius(string groupName, int sectionIndex, float normalized)
        {
            StickmanBodyPartGroup group = FindGroup(groupName);
            if (group == null || sectionIndex < 0 || sectionIndex >= group.SectionCount)
            {
                return;
            }

            float min = group.GetMinRadius(sectionIndex);
            float max = group.GetMaxRadius(sectionIndex);
            group.SetRadius(sectionIndex, Mathf.Lerp(min, max, Mathf.Clamp01(normalized)));
        }

        private StickmanBodyPartGroup FindGroup(string groupName)
        {
            string normalizedGroupName = NormalizeGroupName(groupName);
            for (int i = 0; i < GroupCount; i++)
            {
                StickmanBodyPartGroup group = groups[i];
                if (group != null && NormalizeGroupName(group.DisplayName) == normalizedGroupName)
                {
                    return group;
                }
            }

            return null;
        }

        private void InstantiateWeapon(WeaponType weaponType, Color bodyColor, Color hairColor)
        {
            if (weaponType == WeaponType.None || weaponType == WeaponType.Fists)
            {
                return;
            }

            WeaponPartCatalogSO catalog = WeaponPartCatalogSO.GetCatalog(weaponType);
            if (catalog == null)
            {
                return;
            }

            Transform parent = rightHandAttachment != null ? rightHandAttachment : leftHandAttachment;
            InstantiateCatalogPart(catalog, ChampionSex.H, parent, bodyColor, hairColor);
        }

        private static void InstantiateCatalogPart(BodyPartCatalogSO catalog, ChampionSex sex, Transform parent, Color bodyColor, Color hairColor)
        {
            if (catalog == null || parent == null)
            {
                return;
            }

            GameObject prefab = catalog.GetRandomPrefab(sex);
            if (prefab == null)
            {
                return;
            }

            GameObject instance = Instantiate(prefab, parent);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            TintInstance(instance, bodyColor, hairColor);
        }

        private void ClearAttachmentChildren()
        {
            ClearChildren(headAttachment);
            ClearChildren(beardAttachment);
            ClearChildren(earAttachment);
            ClearChildren(leftHandAttachment);
            ClearChildren(rightHandAttachment);
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }
        }

        private static void TintInstance(GameObject instance, Color bodyColor, Color hairColor)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer targetRenderer = renderers[i];
                Color targetColor = IsHairRenderer(targetRenderer) ? hairColor : bodyColor;
                ApplyColor(targetRenderer, targetColor);
            }
        }

        private static bool IsHairRenderer(Renderer targetRenderer)
        {
            string objectName = targetRenderer != null ? targetRenderer.gameObject.name : string.Empty;
            return objectName.IndexOf("Hair", StringComparison.OrdinalIgnoreCase) >= 0
                || objectName.IndexOf("Beard", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void ApplyColor(Renderer targetRenderer, Color color)
        {
            if (targetRenderer == null)
            {
                return;
            }

            Material[] materials = targetRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    continue;
                }

                if (material.HasProperty("_BaseColor"))
                {
                    material.SetColor("_BaseColor", color);
                }
                else if (material.HasProperty("_Color"))
                {
                    material.SetColor("_Color", color);
                }
            }
        }

        private static Color GetUsableColor(Color color, Color fallback)
        {
            return color.a > 0.001f ? color : fallback;
        }

        private static bool LooksLikeLegacyAppearance(ChampionAppearance appearance)
        {
            return appearance.bodyColor.a <= 0.001f
                && appearance.hairColor.a <= 0.001f
                && Mathf.Approximately(appearance.chest, 0f)
                && Mathf.Approximately(appearance.belly, 0f)
                && Mathf.Approximately(appearance.arms, 0f)
                && Mathf.Approximately(appearance.forearms, 0f)
                && Mathf.Approximately(appearance.upperLegs, 0f)
                && Mathf.Approximately(appearance.lowerLegs, 0f)
                && Mathf.Approximately(appearance.glutes, 0f);
        }

        private static string NormalizeGroupName(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace(" ", string.Empty).ToLowerInvariant();
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
