using ElBestia.Dojo;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ChampionShowcaseDragRotator))]
    public sealed class ChampionShowcaseDragRotatorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                ChampionShowcaseDragRotator rotator = (ChampionShowcaseDragRotator)target;

                if (GUILayout.Button("Rotate 45 Degrees", GUILayout.Height(26f)))
                {
                    rotator.RotatePreviewStep();
                    EditorUtility.SetDirty(rotator);
                }

                if (GUILayout.Button("Reset Rotation", GUILayout.Height(26f)))
                {
                    rotator.ResetRotation();
                    EditorUtility.SetDirty(rotator);
                }
            }
        }
    }
}
