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

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("headAttachment"), new GUIContent("HeadAttachment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("beardAttachment"), new GUIContent("BeardAttachment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("earAttachment"), new GUIContent("EarAttachment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("leftHandAttachment"), new GUIContent("LeftHandAttachment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("rightHandAttachment"), new GUIContent("RightHandAttachment"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bodyRenderers"), new GUIContent("Body Renderers"), true);
            serializedObject.ApplyModifiedProperties();
            EditorGUILayout.Space(6);

            DrawGluteReference(configurator);
            EditorGUILayout.Space(6);
            DrawNormalizedSlider(configurator, "Pecho", "Pecho", 3);
            DrawNormalizedSlider(configurator, "Barriga", "Barriga", 3);
            DrawNormalizedSlider(configurator, "Brazos", "Brazos", 2);
            DrawNormalizedSlider(configurator, "AnteBrazos", "AnteBrazos", 2);
            DrawNormalizedSlider(configurator, "Pierna Superior", "PiernaSuperior", 2);
            DrawNormalizedSlider(configurator, "Pierna Inferior", "PiernaInferior", 2);
            DrawGluteSlider(configurator);

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Randomizar"))
            {
                Undo.RecordObject(configurator, "Randomize Stickman Body");
                ApplyRandom(configurator, "Pecho", 3);
                ApplyRandom(configurator, "Barriga", 3);
                ApplyRandom(configurator, "Brazos", 2);
                ApplyRandom(configurator, "AnteBrazos", 2);
                ApplyRandom(configurator, "PiernaSuperior", 2);
                ApplyRandom(configurator, "PiernaInferior", 2);
                ApplyRandomGlutes(configurator);
                EditorUtility.SetDirty(configurator);
            }
        }

        private static void DrawGluteReference(StickmanBodyConfigurator configurator)
        {
            EditorGUI.BeginChangeCheck();
            var gluteScaler = (StickmanGluteScaler)EditorGUILayout.ObjectField(
                "Gluteos",
                configurator.GluteScaler,
                typeof(StickmanGluteScaler),
                true);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(configurator, "Assign Stickman Glute Scaler");
                configurator.GluteScaler = gluteScaler;
                EditorUtility.SetDirty(configurator);
            }
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

        private static void DrawGluteSlider(StickmanBodyConfigurator configurator)
        {
            StickmanGluteScaler gluteScaler = configurator.GluteScaler;
            bool hasGlutes = gluteScaler != null;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Gluteos:", GUILayout.Width(LabelWidth));
                EditorGUILayout.LabelField("0", GUILayout.Width(EdgeLabelWidth));

                using (new EditorGUI.DisabledScope(!hasGlutes))
                {
                    float value = hasGlutes ? gluteScaler.Size : 0f;
                    EditorGUI.BeginChangeCheck();
                    value = GUILayout.HorizontalSlider(value, 0f, 1f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(gluteScaler, "Change Stickman Glutes");
                        gluteScaler.Size = value;
                        EditorUtility.SetDirty(gluteScaler);
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

        private static void ApplyRandom(StickmanBodyConfigurator configurator, string groupName, int sectionIndex)
        {
            StickmanBodyPartGroup group = FindGroup(configurator, groupName);
            if (group == null || sectionIndex < 0 || sectionIndex >= group.SectionCount)
            {
                return;
            }

            RecordGroupSegments(group);
            float min = group.GetMinRadius(sectionIndex);
            float max = group.GetMaxRadius(sectionIndex);
            float value = Mathf.Lerp(min, max, Random.value);
            group.SetRadius(sectionIndex, value);
            SetGroupSegmentsDirty(group);
        }

        private static void ApplyRandomGlutes(StickmanBodyConfigurator configurator)
        {
            StickmanGluteScaler gluteScaler = configurator.GluteScaler;
            if (gluteScaler == null)
            {
                return;
            }

            Undo.RecordObject(gluteScaler, "Randomize Stickman Glutes");
            gluteScaler.Size = Random.value;
            EditorUtility.SetDirty(gluteScaler);
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
