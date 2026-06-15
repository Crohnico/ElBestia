using ElBestia.UI;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(UIScrollRectResetToTop))]
    public sealed class UIScrollRectResetToTopEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                UIScrollRectResetToTop resetter = (UIScrollRectResetToTop)target;

                if (GUILayout.Button("Reset To Top", GUILayout.Height(26f)))
                {
                    resetter.ResetToTop();
                    EditorUtility.SetDirty(resetter);
                }

                if (GUILayout.Button("Reset To Top Next Frame", GUILayout.Height(26f)))
                {
                    if (Application.isPlaying)
                    {
                        resetter.ResetToTopNextFrame();
                    }
                }
            }
        }
    }
}
