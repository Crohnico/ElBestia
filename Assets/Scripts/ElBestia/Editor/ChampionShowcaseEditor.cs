using ElBestia.Dojo;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ChampionShowcase))]
    public sealed class ChampionShowcaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);
                ChampionShowcase showcase = (ChampionShowcase)target;

                if (GUILayout.Button("Show First Admission Champion", GUILayout.Height(26f)))
                {
                    showcase.Show(ChampionAdmisionList.Instance.champions[0]);
                    EditorUtility.SetDirty(showcase);
                }
            }
        }
    }
}
