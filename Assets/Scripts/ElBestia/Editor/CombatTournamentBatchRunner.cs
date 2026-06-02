using System.IO;
using System.Text;
using ElBestia.Champions;
using ElBestia.Tournaments;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ElBestia.Editor
{
    public static class CombatTournamentBatchRunner
    {
        private const int TournamentSeed = 260602;
        private const int PlayerCount = 64;
        private const int WinsNeeded = 2;
        private const int MaxFightsPerMatch = 7;
        private const float MatchDurationSeconds = 60f;
        private const string WinnerFolder = "Assets/Generated/TournamentWinners";
        private const string LogPrefix = "[ElBestiaTournament]";

        [MenuItem("El Bestia/Simulation/Run 64 Player BO3 Tournament")]
        public static void RunSixtyFourPlayerBo3Tournament()
        {
            TournamentSimulationResult tournament = TournamentSimulator.RunGeneratedSingleElimination(new TournamentSimulationOptions
            {
                seed = TournamentSeed,
                playerCount = PlayerCount,
                winsNeeded = WinsNeeded,
                maxFightsPerMatch = MaxFightsPerMatch,
                matchDurationSeconds = MatchDurationSeconds
            });

            string savedPath = SaveWinnerAsset(tournament.champion);
            Debug.Log(BuildReport(tournament, savedPath));
        }

        private static string BuildReport(TournamentSimulationResult tournament, string savedPath)
        {
            var report = new StringBuilder();
            report.AppendLine($"{LogPrefix} Tournament seed={tournament.seed} players={tournament.playerCount} format=single_elimination_bo3");
            report.AppendLine($"{LogPrefix} Generated {tournament.playerCount} champions in {tournament.generationMilliseconds:0.00} ms");

            foreach (TournamentRoundResult round in tournament.rounds)
            {
                int entrants = round.matches.Count * 2;
                report.AppendLine($"{LogPrefix} Round {round.round} entrants={entrants}");
                foreach (TournamentMatchResult match in round.matches)
                {
                    report.AppendLine($"{LogPrefix} R{match.round}M{match.match:00} {match.left.champion.ChampionName} {match.leftWins}-{match.rightWins} {match.right.champion.ChampionName} fights={match.fightSummary} winner={match.winner.champion.ChampionName}");
                }
            }

            report.AppendLine($"{LogPrefix} Champion {DescribeChampion(tournament.champion)}");
            report.AppendLine($"{LogPrefix} SavedChampionSO {savedPath}");
            report.AppendLine($"{LogPrefix} Timings generation={tournament.generationMilliseconds:0.00}ms simulation={tournament.simulationMilliseconds:0.00}ms total={tournament.totalMilliseconds:0.00}ms matches={tournament.totalMatches} fights={tournament.totalFights}");
            return report.ToString();
        }

        private static string SaveWinnerAsset(TournamentEntry winner)
        {
            EnsureFolder("Assets/Generated", "TournamentWinners");
            ChampionSO championAsset = ScriptableObject.CreateInstance<ChampionSO>();
            championAsset.ApplyData(winner.champion);
            championAsset.name = SanitizeAssetName(winner.champion.ChampionName);

            string assetName = $"TournamentWinner_{SanitizeAssetName(winner.champion.ChampionName)}.asset";
            string path = AssetDatabase.GenerateUniqueAssetPath($"{WinnerFolder}/{assetName}");
            AssetDatabase.CreateAsset(championAsset, path);
            EditorUtility.SetDirty(championAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return path;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                string parentOfParent = Path.GetDirectoryName(parent)?.Replace('\\', '/');
                string folder = Path.GetFileName(parent);
                if (!string.IsNullOrEmpty(parentOfParent) && !AssetDatabase.IsValidFolder(parent))
                {
                    AssetDatabase.CreateFolder(parentOfParent, folder);
                }
            }

            AssetDatabase.CreateFolder(parent, child);
        }

        private static string DescribeChampion(TournamentEntry entry)
        {
            ChampionData champion = entry.champion;
            ChampionStats stats = champion.Stats;
            return $"{champion.ChampionName} slot={entry.slot} seed={entry.generationSeed} id={champion.ChampionId} weapon={champion.EquippedWeapon} hp={stats.life} energy={stats.energy} stats=[str:{stats.strength} agi:{stats.agility} con:{stats.constitution} int:{stats.intelligence} end:{stats.endurance}] combat=[speed:{stats.speed} dodge:{stats.dodge} block:{stats.block} hit:{stats.hit} crit:{stats.critical}]";
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
