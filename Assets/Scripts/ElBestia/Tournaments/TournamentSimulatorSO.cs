using System;
using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Combat;
using UnityEngine;

namespace ElBestia.Tournaments
{
    [CreateAssetMenu(menuName = "El Bestia/Tournaments/Tournament Simulator", fileName = "TournamentSimulator")]
    public sealed class TournamentSimulatorSO : ScriptableObject
    {
        [Header("Tournament")]
        [SerializeField] private int participantCount = 64;
        [SerializeField] private int seed = 260602;
        [SerializeField] private bool randomizeSeed;
        [SerializeField] private int winsNeeded = 2;
        [SerializeField] private int maxFightsPerMatch = 7;
        [SerializeField] private float matchDurationSeconds = 60f;

        [Header("Generated Champions")]
        [SerializeField] private int championLevel = 1;
        [SerializeField] private int totalBaseStatPoints = 50;

        [Header("Last Result")]
        [SerializeField] private TournamentSnapshot lastResult;
        [SerializeField] private ChampionData winnerData;
        [SerializeField] private string lastError;
        [SerializeField] private string lastSavedChampionPath;

        public TournamentSnapshot LastResult => lastResult;
        public ChampionData WinnerData => winnerData;
        public string LastError => lastError;
        public string LastSavedChampionPath => lastSavedChampionPath;
        public bool HasResult => lastResult != null && winnerData != null;

        public void Simulate()
        {
            lastError = string.Empty;
            lastSavedChampionPath = string.Empty;

            int playerCount = Mathf.Max(2, participantCount);
            if (!IsPowerOfTwo(playerCount))
            {
                lastResult = null;
                winnerData = null;
                lastError = "Participant count must be a power of two for the current single-elimination bracket.";
                return;
            }

            if (randomizeSeed)
            {
                seed = UnityEngine.Random.Range(1, int.MaxValue);
            }

            TournamentSimulationResult result = TournamentSimulator.RunGeneratedSingleElimination(new TournamentSimulationOptions
            {
                seed = seed,
                playerCount = playerCount,
                winsNeeded = Mathf.Max(1, winsNeeded),
                maxFightsPerMatch = Mathf.Max(1, maxFightsPerMatch),
                matchDurationSeconds = Mathf.Max(1f, matchDurationSeconds),
                championLevel = Mathf.Max(1, championLevel),
                totalBaseStatPoints = Mathf.Max(5, totalBaseStatPoints)
            });

            participantCount = playerCount;
            winnerData = result.champion.champion != null ? result.champion.champion.Clone() : null;
            lastResult = TournamentSnapshot.FromResult(result);
        }

        public void RecordSavedChampionPath(string path)
        {
            lastSavedChampionPath = path ?? string.Empty;
        }

        private static bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }
    }

    [Serializable]
    public sealed class TournamentSnapshot
    {
        public int seed;
        public int playerCount;
        public int totalMatches;
        public int totalFights;
        public float generationMilliseconds;
        public float simulationMilliseconds;
        public float totalMilliseconds;
        public string winnerName;
        public string winnerId;
        public int winnerSlot;
        public int winnerSeed;
        public string winnerWeapon;
        public List<TournamentRoundSnapshot> rounds = new List<TournamentRoundSnapshot>();

        public static TournamentSnapshot FromResult(TournamentSimulationResult result)
        {
            var snapshot = new TournamentSnapshot
            {
                seed = result.seed,
                playerCount = result.playerCount,
                totalMatches = result.totalMatches,
                totalFights = result.totalFights,
                generationMilliseconds = (float)result.generationMilliseconds,
                simulationMilliseconds = (float)result.simulationMilliseconds,
                totalMilliseconds = (float)result.totalMilliseconds,
                winnerName = GetChampionName(result.champion),
                winnerId = result.champion != null && result.champion.champion != null ? result.champion.champion.ChampionId : string.Empty,
                winnerSlot = result.champion != null ? result.champion.slot : 0,
                winnerSeed = result.champion != null ? result.champion.generationSeed : 0,
                winnerWeapon = result.champion != null && result.champion.champion != null ? result.champion.champion.EquippedWeapon.ToString() : string.Empty
            };

            foreach (TournamentRoundResult round in result.rounds)
            {
                TournamentRoundSnapshot roundSnapshot = TournamentRoundSnapshot.FromResult(round);
                snapshot.rounds.Add(roundSnapshot);
            }

            return snapshot;
        }

        private static string GetChampionName(TournamentEntry entry)
        {
            return entry != null && entry.champion != null ? entry.champion.ChampionName : string.Empty;
        }
    }

    [Serializable]
    public sealed class TournamentRoundSnapshot
    {
        public int round;
        public int entrants;
        public List<TournamentMatchSnapshot> matches = new List<TournamentMatchSnapshot>();

        public static TournamentRoundSnapshot FromResult(TournamentRoundResult result)
        {
            var snapshot = new TournamentRoundSnapshot
            {
                round = result.round,
                entrants = result.matches.Count * 2
            };

            foreach (TournamentMatchResult match in result.matches)
            {
                snapshot.matches.Add(TournamentMatchSnapshot.FromResult(match));
            }

            return snapshot;
        }
    }

    [Serializable]
    public sealed class TournamentMatchSnapshot
    {
        public int round;
        public int match;
        public string leftName;
        public string rightName;
        public string winnerName;
        public int leftSlot;
        public int rightSlot;
        public int winnerSlot;
        public int leftWins;
        public int rightWins;
        public string fightSummary;
        public List<TournamentFightSnapshot> fights = new List<TournamentFightSnapshot>();

        public static TournamentMatchSnapshot FromResult(TournamentMatchResult result)
        {
            var snapshot = new TournamentMatchSnapshot
            {
                round = result.round,
                match = result.match,
                leftName = GetChampionName(result.left),
                rightName = GetChampionName(result.right),
                winnerName = GetChampionName(result.winner),
                leftSlot = result.left != null ? result.left.slot : 0,
                rightSlot = result.right != null ? result.right.slot : 0,
                winnerSlot = result.winner != null ? result.winner.slot : 0,
                leftWins = result.leftWins,
                rightWins = result.rightWins,
                fightSummary = result.fightSummary
            };

            for (int i = 0; i < result.fights.Count; i++)
            {
                snapshot.fights.Add(TournamentFightSnapshot.FromResult(i + 1, result.fights[i]));
            }

            return snapshot;
        }

        private static string GetChampionName(TournamentEntry entry)
        {
            return entry != null && entry.champion != null ? entry.champion.ChampionName : string.Empty;
        }
    }

    [Serializable]
    public sealed class TournamentFightSnapshot
    {
        public int fight;
        public int seed;
        public CombatSimulationSide winner;
        public float duration;
        public int leftHealth;
        public int rightHealth;

        public static TournamentFightSnapshot FromResult(int fightNumber, TournamentFightResult result)
        {
            return new TournamentFightSnapshot
            {
                fight = fightNumber,
                seed = result.fightSeed,
                winner = result.winner,
                duration = result.duration,
                leftHealth = result.leftHealth,
                rightHealth = result.rightHealth
            };
        }
    }
}
