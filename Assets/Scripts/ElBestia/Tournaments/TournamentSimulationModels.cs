using System;
using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Combat;

namespace ElBestia.Tournaments
{
    [Serializable]
    public sealed class TournamentSimulationOptions
    {
        public int seed = 260602;
        public int playerCount = 64;
        public int winsNeeded = 2;
        public int maxFightsPerMatch = 7;
        public float matchDurationSeconds = 60f;
        public int maxEventsPerFight = 20000;
        public int championLevel = 1;
        public int totalBaseStatPoints = 50;
    }

    [Serializable]
    public sealed class TournamentEntry
    {
        public int slot;
        public int generationSeed;
        public ChampionData champion;
    }

    [Serializable]
    public sealed class TournamentFightResult
    {
        public int fightSeed;
        public CombatSimulationSide winner;
        public float duration;
        public int leftHealth;
        public int rightHealth;
    }

    [Serializable]
    public sealed class TournamentMatchResult
    {
        public int round;
        public int match;
        public TournamentEntry left;
        public TournamentEntry right;
        public TournamentEntry winner;
        public int leftWins;
        public int rightWins;
        public string fightSummary;
        public readonly List<TournamentFightResult> fights = new List<TournamentFightResult>();
    }

    [Serializable]
    public sealed class TournamentRoundResult
    {
        public int round;
        public readonly List<TournamentMatchResult> matches = new List<TournamentMatchResult>();
    }

    [Serializable]
    public sealed class TournamentSimulationResult
    {
        public int seed;
        public int playerCount;
        public int totalMatches;
        public int totalFights;
        public double generationMilliseconds;
        public double simulationMilliseconds;
        public double totalMilliseconds;
        public TournamentEntry champion;
        public readonly List<TournamentEntry> entrants = new List<TournamentEntry>();
        public readonly List<TournamentRoundResult> rounds = new List<TournamentRoundResult>();
    }
}
