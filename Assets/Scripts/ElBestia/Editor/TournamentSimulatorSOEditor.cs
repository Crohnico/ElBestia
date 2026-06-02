using System.IO;
using ElBestia.Champions;
using ElBestia.Tournaments;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(TournamentSimulatorSO))]
    public sealed class TournamentSimulatorSOEditor : UnityEditor.Editor
    {
        private const string WinnerFolder = "Assets/Generated/TournamentWinners";

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawConfiguration();
            serializedObject.ApplyModifiedProperties();

            var simulator = (TournamentSimulatorSO)target;
            DrawActions(simulator);
            DrawResult(simulator);
        }

        private void DrawConfiguration()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Tournament", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("participantCount"), new GUIContent("Participants"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("seed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("randomizeSeed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("winsNeeded"), new GUIContent("Wins Needed"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFightsPerMatch"), new GUIContent("Max Fights / Match"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("matchDurationSeconds"), new GUIContent("Match Duration"));
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Generated Champions", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("championLevel"), new GUIContent("Champion Level"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("totalBaseStatPoints"), new GUIContent("Base Stat Points"));
            }
        }

        private static void DrawActions(TournamentSimulatorSO simulator)
        {
            EditorGUILayout.Space(8);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Simular", GUILayout.Height(32)))
                {
                    Undo.RecordObject(simulator, "Simulate Tournament");
                    simulator.Simulate();
                    EditorUtility.SetDirty(simulator);
                    AssetDatabase.SaveAssets();
                }

                using (new EditorGUI.DisabledScope(!simulator.HasResult))
                {
                    if (GUILayout.Button("Crear SO del campeon", GUILayout.Height(32)))
                    {
                        Undo.RecordObject(simulator, "Create Tournament Champion SO");
                        string path = CreateChampionAsset(simulator.WinnerData);
                        simulator.RecordSavedChampionPath(path);
                        EditorUtility.SetDirty(simulator);
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }

        private static void DrawResult(TournamentSimulatorSO simulator)
        {
            if (!string.IsNullOrEmpty(simulator.LastError))
            {
                EditorGUILayout.HelpBox(simulator.LastError, MessageType.Warning);
                return;
            }

            TournamentSnapshot result = simulator.LastResult;
            if (result == null)
            {
                EditorGUILayout.HelpBox("No tournament simulated yet.", MessageType.Info);
                return;
            }

            EditorGUILayout.Space(8);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Result", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Winner", $"{result.winnerName}  |  Slot {result.winnerSlot}  |  {result.winnerWeapon}");
                EditorGUILayout.LabelField("ID", result.winnerId, EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Seed", result.seed.ToString());
                EditorGUILayout.LabelField("Timing", $"gen {result.generationMilliseconds:0.00} ms | sim {result.simulationMilliseconds:0.00} ms | total {result.totalMilliseconds:0.00} ms");
                EditorGUILayout.LabelField("Volume", $"{result.totalMatches} matches | {result.totalFights} fights");

                if (!string.IsNullOrEmpty(simulator.LastSavedChampionPath))
                {
                    EditorGUILayout.LabelField("Saved SO", simulator.LastSavedChampionPath, EditorStyles.miniLabel);
                }
            }

            DrawBracket(result);
        }

        private static void DrawBracket(TournamentSnapshot result)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Bracket", EditorStyles.boldLabel);

            foreach (TournamentRoundSnapshot round in result.rounds)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField($"Round {round.round} - {round.entrants} entrants", EditorStyles.boldLabel);
                    foreach (TournamentMatchSnapshot match in round.matches)
                    {
                        DrawMatch(match);
                    }
                }
            }
        }

        private static void DrawMatch(TournamentMatchSnapshot match)
        {
            string title = $"R{match.round}M{match.match:00}  #{match.leftSlot} {match.leftName} {match.leftWins}-{match.rightWins} #{match.rightSlot} {match.rightName}  ->  {match.winnerName}  ({match.fightSummary})";
            EditorGUILayout.LabelField(title, EditorStyles.wordWrappedLabel);

            foreach (TournamentFightSnapshot fight in match.fights)
            {
                string line = $"    Fight {fight.fight}: seed {fight.seed} | winner {fight.winner} | {fight.duration:0.00}s | hp {fight.leftHealth}/{fight.rightHealth}";
                EditorGUILayout.LabelField(line, EditorStyles.miniLabel);
            }
        }

        private static string CreateChampionAsset(ChampionData championData)
        {
            EnsureFolder("Assets/Generated", "TournamentWinners");
            ChampionSO championAsset = ScriptableObject.CreateInstance<ChampionSO>();
            championAsset.ApplyData(championData);
            championAsset.name = SanitizeAssetName(championData != null ? championData.ChampionName : "Champion");

            string assetName = $"TournamentWinner_{SanitizeAssetName(championAsset.ChampionName)}.asset";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{WinnerFolder}/{assetName}");
            AssetDatabase.CreateAsset(championAsset, path);
            EditorUtility.SetDirty(championAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return path;
        }

        private static void EnsureFolder(string parent, string child)
        {
            if (!AssetDatabase.IsValidFolder(parent))
            {
                string parentOfParent = Path.GetDirectoryName(parent)?.Replace('\\', '/');
                string folder = Path.GetFileName(parent);
                if (!string.IsNullOrEmpty(parentOfParent))
                {
                    AssetDatabase.CreateFolder(parentOfParent, folder);
                }
            }

            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static string SanitizeAssetName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Champion";
            }

            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value.Replace(' ', '_');
        }
    }
}
