using ElBestia.UI;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(PerkDataVisualizer))]
    public sealed class PerkDataVisualizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                PerkDataVisualizer visualizer = (PerkDataVisualizer)target;

                if (GUILayout.Button("Setup", GUILayout.Height(26f)))
                {
                    visualizer.Setup();
                    EditorUtility.SetDirty(visualizer);
                }
            }
        }
    }
}
