using ElBestia.Visuals;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(StickmanSegmentMesh))]
    public sealed class StickmanSegmentMeshEditor : UnityEditor.Editor
    {
        private SerializedProperty startJoint;
        private SerializedProperty endJoint;
        private SerializedProperty sectionCount;
        private SerializedProperty radialSegments;
        private SerializedProperty ringsPerSection;
        private SerializedProperty capHeightFactor;
        private SerializedProperty endInsetFactor;
        private SerializedProperty minRadius;
        private SerializedProperty useCustomStartMinRadius;
        private SerializedProperty customStartMinRadius;
        private SerializedProperty maxRadius;
        private SerializedProperty maxRadiusDelta;
        private SerializedProperty smoothRadii;
        private SerializedProperty rebuildContinuously;

        private void OnEnable()
        {
            startJoint = serializedObject.FindProperty("startJoint");
            endJoint = serializedObject.FindProperty("endJoint");
            sectionCount = serializedObject.FindProperty("sectionCount");
            radialSegments = serializedObject.FindProperty("radialSegments");
            ringsPerSection = serializedObject.FindProperty("ringsPerSection");
            capHeightFactor = serializedObject.FindProperty("capHeightFactor");
            endInsetFactor = serializedObject.FindProperty("endInsetFactor");
            minRadius = serializedObject.FindProperty("minRadius");
            useCustomStartMinRadius = serializedObject.FindProperty("useCustomStartMinRadius");
            customStartMinRadius = serializedObject.FindProperty("customStartMinRadius");
            maxRadius = serializedObject.FindProperty("maxRadius");
            maxRadiusDelta = serializedObject.FindProperty("maxRadiusDelta");
            smoothRadii = serializedObject.FindProperty("smoothRadii");
            rebuildContinuously = serializedObject.FindProperty("rebuildContinuously");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(startJoint, new GUIContent("Articulacion Inicio"));
            EditorGUILayout.PropertyField(endJoint, new GUIContent("Articulacion Final"));
            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(sectionCount, new GUIContent("Particiones"));
            EditorGUILayout.PropertyField(radialSegments, new GUIContent("Resolucion Radial"));
            EditorGUILayout.PropertyField(ringsPerSection, new GUIContent("Suavidad Entre Particiones"));
            EditorGUILayout.PropertyField(capHeightFactor, new GUIContent("Altura Cierre"));
            EditorGUILayout.PropertyField(endInsetFactor, new GUIContent("Separacion Cierre"));
            EditorGUILayout.PropertyField(minRadius, new GUIContent("Radio Minimo"));
            EditorGUILayout.PropertyField(useCustomStartMinRadius, new GUIContent("Custom Min Inicio"));
            if (useCustomStartMinRadius.boolValue)
            {
                EditorGUILayout.PropertyField(customStartMinRadius, new GUIContent("Radio Minimo Inicio"));
            }
            EditorGUILayout.PropertyField(maxRadius, new GUIContent("Radio Maximo"));
            EditorGUILayout.PropertyField(maxRadiusDelta, new GUIContent("Diferencia Maxima"));
            EditorGUILayout.PropertyField(smoothRadii, new GUIContent("Interpolacion Suave"));
            EditorGUILayout.PropertyField(rebuildContinuously, new GUIContent("Actualizar Al Mover"));

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Grosor por Particion", EditorStyles.boldLabel);
                var segment = (StickmanSegmentMesh)target;
                for (int i = 0; i < segment.SectionCount; i++)
                {
                    float value = segment.GetSectionRadius(i);
                    EditorGUI.BeginChangeCheck();
                    value = EditorGUILayout.Slider($"Particion {i + 1}", value, segment.GetSectionMinRadius(i), segment.MaxRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(segment, "Change Stickman Segment Radius");
                        segment.SetSectionRadius(i, value);
                        EditorUtility.SetDirty(segment);
                    }
                }
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Reconstruir Mesh"))
            {
                var segment = (StickmanSegmentMesh)target;
                Undo.RecordObject(segment, "Rebuild Stickman Segment Mesh");
                segment.Rebuild();
                EditorUtility.SetDirty(segment);
            }
        }
    }
}
