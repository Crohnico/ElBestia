using ElBestia.Visuals;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(StickmanGluteScaler))]
    public sealed class StickmanGluteScalerEditor : UnityEditor.Editor
    {
        private SerializedProperty leftGlute;
        private SerializedProperty rightGlute;
        private SerializedProperty size;
        private SerializedProperty minScale;
        private SerializedProperty maxScale;
        private SerializedProperty growthDirection;
        private SerializedProperty displacementFactor;

        private void OnEnable()
        {
            leftGlute = serializedObject.FindProperty("leftGlute");
            rightGlute = serializedObject.FindProperty("rightGlute");
            size = serializedObject.FindProperty("size");
            minScale = serializedObject.FindProperty("minScale");
            maxScale = serializedObject.FindProperty("maxScale");
            growthDirection = serializedObject.FindProperty("growthDirection");
            displacementFactor = serializedObject.FindProperty("displacementFactor");
        }

        public override void OnInspectorGUI()
        {
            if (target == null)
            {
                return;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(leftGlute, new GUIContent("Gluteo Izquierdo"));
            EditorGUILayout.PropertyField(rightGlute, new GUIContent("Gluteo Derecho"));
            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(minScale, new GUIContent("Escala Minima"));
            EditorGUILayout.PropertyField(maxScale, new GUIContent("Escala Maxima"));
            EditorGUILayout.PropertyField(growthDirection, new GUIContent("Direccion Crecimiento"));
            EditorGUILayout.PropertyField(displacementFactor, new GUIContent("Empuje"));
            EditorGUILayout.Space(6);
            EditorGUILayout.PropertyField(size, new GUIContent("Tamano"));

            serializedObject.ApplyModifiedProperties();

            var scaler = (StickmanGluteScaler)target;
            if (GUI.changed)
            {
                scaler.Apply();
                EditorUtility.SetDirty(scaler);
            }

            EditorGUILayout.Space(6);
            if (GUILayout.Button("Capturar Base Actual"))
            {
                Undo.RecordObject(scaler, "Capture Stickman Glute Base");
                scaler.CaptureBase();
                EditorUtility.SetDirty(scaler);
            }
        }
    }
}
