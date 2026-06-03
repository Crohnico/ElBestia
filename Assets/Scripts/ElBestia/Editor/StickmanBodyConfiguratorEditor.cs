using ElBestia.Visuals;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(StickmanBodyConfigurator))]
    public sealed class StickmanBodyConfiguratorEditor : UnityEditor.Editor
    {
        private const float LabelWidth = 120f;
        private const float EdgeLabelWidth = 16f;

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                return;
            }

            var configurator = (StickmanBodyConfigurator)target;
            if (configurator.PruneMissingSegments())
            {
                EditorUtility.SetDirty(configurator);
            }

            DrawNormalizedSlider(configurator, "Pecho", "Pecho", 3);
            DrawNormalizedSlider(configurator, "Barriga", "Barriga", 3);
            DrawNormalizedSlider(configurator, "Brazos", "Brazos", 2);
            DrawNormalizedSlider(configurator, "AnteBrazos", "AnteBrazos", 2);
            DrawNormalizedSlider(configurator, "Pierna Superior", "PiernaSuperior", 2);
            DrawNormalizedSlider(configurator, "Pierna Inferior", "PiernaInferior", 2);
        }

        private static void DrawNormalizedSlider(
            StickmanBodyConfigurator configurator,
            string label,
            string groupName,
            int sectionIndex)
        {
            StickmanBodyPartGroup group = FindGroup(configurator, groupName);
            bool hasSection = group != null && sectionIndex >= 0 && sectionIndex < group.SectionCount;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{label}:", GUILayout.Width(LabelWidth));
                EditorGUILayout.LabelField("0", GUILayout.Width(EdgeLabelWidth));

                using (new EditorGUI.DisabledScope(!hasSection))
                {
                    float normalized = 0f;
                    float min = 0f;
                    float max = 1f;

                    if (hasSection)
                    {
                        min = group.GetMinRadius(sectionIndex);
                        max = group.GetMaxRadius(sectionIndex);
                        float value = group.GetRadius(sectionIndex);
                        normalized = max > min ? Mathf.InverseLerp(min, max, value) : 0f;
                    }

                    EditorGUI.BeginChangeCheck();
                    normalized = GUILayout.HorizontalSlider(normalized, 0f, 1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(configurator, "Change Stickman Body Part Radius");
                        RecordGroupSegments(group);
                        float value = Mathf.Lerp(min, max, normalized);
                        group.SetRadius(sectionIndex, value);
                        EditorUtility.SetDirty(configurator);
                        SetGroupSegmentsDirty(group);
                    }
                }

                EditorGUILayout.LabelField("1", GUILayout.Width(EdgeLabelWidth));
            }
        }

        private static StickmanBodyPartGroup FindGroup(StickmanBodyConfigurator configurator, string groupName)
        {
            string normalizedGroupName = NormalizeGroupName(groupName);
            for (int i = 0; i < configurator.GroupCount; i++)
            {
                StickmanBodyPartGroup group = configurator.GetGroup(i);
                if (group != null && NormalizeGroupName(group.DisplayName) == normalizedGroupName)
                {
                    return group;
                }
            }

            return null;
        }

        private static void RecordGroupSegments(StickmanBodyPartGroup group)
        {
            for (int i = 0; i < group.SegmentCount; i++)
            {
                StickmanSegmentMesh segment = group.GetSegment(i);
                if (segment != null)
                {
                    Undo.RecordObject(segment, "Change Stickman Body Part Radius");
                }
            }
        }

        private static void SetGroupSegmentsDirty(StickmanBodyPartGroup group)
        {
            for (int i = 0; i < group.SegmentCount; i++)
            {
                StickmanSegmentMesh segment = group.GetSegment(i);
                if (segment != null)
                {
                    EditorUtility.SetDirty(segment);
                }
            }
        }

        private static string NormalizeGroupName(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace(" ", string.Empty).ToLowerInvariant();
        }
    }
}
