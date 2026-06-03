using ElBestia.Visuals;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(StickmanBodyConfigurator))]
    public sealed class StickmanBodyConfiguratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                return;
            }

            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("applyContinuously"), new GUIContent("Aplicar Continuamente"));
            serializedObject.ApplyModifiedProperties();

            var configurator = (StickmanBodyConfigurator)target;

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Crear Grupos Base"))
                {
                    Undo.RecordObject(configurator, "Reset Stickman Body Groups");
                    configurator.ResetDefaultGroups();
                    configurator.CollectSegments();
                    configurator.CaptureCurrentRadii();
                    EditorUtility.SetDirty(configurator);
                }

                if (GUILayout.Button("Buscar Segmentos"))
                {
                    Undo.RecordObject(configurator, "Collect Stickman Segments");
                    configurator.CollectSegments();
                    configurator.CaptureCurrentRadii();
                    EditorUtility.SetDirty(configurator);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Capturar Actual"))
                {
                    Undo.RecordObject(configurator, "Capture Stickman Body Radii");
                    configurator.CaptureCurrentRadii();
                    EditorUtility.SetDirty(configurator);
                }

                if (GUILayout.Button("Aplicar Todo"))
                {
                    Undo.RecordObject(configurator, "Apply Stickman Body Config");
                    configurator.ApplyAll();
                    EditorUtility.SetDirty(configurator);
                }
            }

            if (GUILayout.Button("Aplicar Visibilidad Base"))
            {
                Undo.RecordObject(configurator, "Apply Stickman Body Visibility Rules");
                configurator.ApplyDefaultVisibilityRules();
                EditorUtility.SetDirty(configurator);
            }

            EditorGUILayout.Space(8);
            for (int i = 0; i < configurator.GroupCount; i++)
            {
                DrawGroup(configurator, i);
            }
        }

        private static void DrawGroup(StickmanBodyConfigurator configurator, int groupIndex)
        {
            StickmanBodyPartGroup group = configurator.GetGroup(groupIndex);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                string title = $"{group.DisplayName} ({group.SegmentCount})";
                group.Expanded = EditorGUILayout.Foldout(group.Expanded, title, true);
                if (!group.Expanded)
                {
                    return;
                }

                using (new EditorGUI.DisabledScope(true))
                {
                    for (int i = 0; i < group.SegmentCount; i++)
                    {
                        StickmanSegmentMesh segment = group.GetSegment(i);
                        if (segment != null)
                        {
                            EditorGUILayout.ObjectField(segment, typeof(StickmanSegmentMesh), true);
                        }
                    }
                }

                EditorGUILayout.Space(4);
                for (int sectionIndex = 0; sectionIndex < group.SectionCount; sectionIndex++)
                {
                    if (!group.IsSectionVisible(sectionIndex))
                    {
                        continue;
                    }

                    float min = group.GetMinRadius(sectionIndex);
                    float max = group.GetMaxRadius(sectionIndex);
                    float value = group.GetRadius(sectionIndex);

                    EditorGUI.BeginChangeCheck();
                    value = EditorGUILayout.Slider($"Particion {sectionIndex + 1}", value, min, max);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(configurator, "Change Stickman Body Part Radius");
                        group.SetRadius(sectionIndex, value);
                        EditorUtility.SetDirty(configurator);
                    }
                }
            }
        }
    }
}
