using ElBestia.Dojo;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ChampionAdmisionList))]
    public sealed class ChampionAdmisionListEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                ChampionAdmisionList list = (ChampionAdmisionList)target;

                if (GUILayout.Button("Create List", GUILayout.Height(26f)))
                {
                    list.CreateList();
                    EditorUtility.SetDirty(list);
                }

                if (GUILayout.Button("Clear List", GUILayout.Height(26f)))
                {
                    list.champions.Clear();
                    EditorUtility.SetDirty(list);
                }
            }
        }
    }
}
