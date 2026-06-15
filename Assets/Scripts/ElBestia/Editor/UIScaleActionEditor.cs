using ElBestia.UI;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(UIScaleAction))]
    public sealed class UIScaleActionEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                UIScaleAction action = (UIScaleAction)target;

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset", GUILayout.Height(24f)))
                    {
                        action.ResetActionState();
                        MarkDirty(action);
                    }

                    if (GUILayout.Button("Instant", GUILayout.Height(24f)))
                    {
                        action.InstantExecute();
                        MarkDirty(action);
                    }
                }

                if (GUILayout.Button("Execute Once", GUILayout.Height(26f)))
                {
                    action.Execute();
                    MarkDirty(action);
                }

                EditorGUILayout.HelpBox(
                    "Execute Once starts/ticks the action. In Play Mode, keep pressing or let a UIScreenBehaviour tick it.",
                    MessageType.Info);
            }
        }

        private static void MarkDirty(UIScaleAction action)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(action);
            }
        }
    }
}
