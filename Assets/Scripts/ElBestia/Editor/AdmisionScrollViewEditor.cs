using ElBestia.Dojo;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(AdmisionScrollView))]
    public sealed class AdmisionScrollViewEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                AdmisionScrollView scrollView = (AdmisionScrollView)target;

                if (GUILayout.Button("Populate", GUILayout.Height(26f)))
                {
                    scrollView.Populate();
                    EditorUtility.SetDirty(scrollView);
                }

                if (GUILayout.Button("Clear Buttons", GUILayout.Height(26f)))
                {
                    scrollView.ClearButtons();
                    EditorUtility.SetDirty(scrollView);
                }
            }
        }
    }
}
