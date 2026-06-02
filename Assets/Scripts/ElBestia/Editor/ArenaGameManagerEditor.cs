using ElBestia.Combat;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ArenaGameManager))]
    public sealed class ArenaGameManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Debug Controls", EditorStyles.boldLabel);

                ArenaGameManager manager = (ArenaGameManager)target;
                if (GUILayout.Button("SetUpGame", GUILayout.Height(28f)))
                {
                    manager.SetUpGame();
                    MarkDirty(manager);
                }

                if (GUILayout.Button("StartGame", GUILayout.Height(28f)))
                {
                    manager.StartGame();
                    MarkDirty(manager);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("LeftReciveRandomDamage", GUILayout.Height(24f)))
                    {
                        manager.LeftReciveRandomDamage();
                        MarkDirty(manager);
                    }

                    if (GUILayout.Button("RightReceiveRandomDamage", GUILayout.Height(24f)))
                    {
                        manager.RightReceiveRandomDamage();
                        MarkDirty(manager);
                    }
                }

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Skill Cooldowns", EditorStyles.boldLabel);
                DrawSkillCooldownButtons("Left", manager.LeftTriggerSkillCooldown);
                DrawSkillCooldownButtons("Right", manager.RightTriggerSkillCooldown);
            }
        }

        private static void DrawSkillCooldownButtons(string label, System.Action<int> trigger)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(34f));
                for (int i = 0; i < 4; i++)
                {
                    string buttonLabel = i == 0 ? "Base" : $"S{i}";
                    if (GUILayout.Button(buttonLabel, GUILayout.Height(22f)))
                    {
                        trigger(i);
                    }
                }
            }
        }

        private static void MarkDirty(ArenaGameManager manager)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
