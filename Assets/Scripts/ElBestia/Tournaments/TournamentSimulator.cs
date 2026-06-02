using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using ElBestia.Champions;
using ElBestia.Combat;
using ElBestia.Generation;
using UnityEngine;

namespace ElBestia.Tournaments
{
    public static class TournamentSimulator
    {
        public static TournamentSimulationResult RunGeneratedSingleElimination(TournamentSimulationOptions options)
        {
            options ??= new TournamentSimulationOptions();
            ValidateOptions(options);

            var totalWatch = Stopwatch.StartNew();
            var rng = new System.Random(options.seed);
            var result = new TournamentSimulationResult
            {
                seed = options.seed,
                playerCount = options.playerCount
            };

            var generationWatch = Stopwatch.StartNew();
            List<TournamentEntry> bracket = GenerateEntries(options, rng);
            generationWatch.Stop();
            result.entrants.AddRange(bracket);

            var simulationWatch = Stopwatch.StartNew();
            int roundNumber = 1;
            while (bracket.Count > 1)
            {
                TournamentRoundResult round = SimulateRound(bracket, roundNumber, options, rng, result);
                result.rounds.Add(round);

                var nextRound = new List<TournamentEntry>(round.matches.Count);
                foreach (TournamentMatchResult match in round.matches)
                {
                    nextRound.Add(match.winner);
                }

                bracket = nextRound;
                roundNumber++;
            }

            simulationWatch.Stop();
            totalWatch.Stop();

            result.champion = bracket[0];
            result.generationMilliseconds = generationWatch.Elapsed.TotalMilliseconds;
            result.simulationMilliseconds = simulationWatch.Elapsed.TotalMilliseconds;
            result.totalMilliseconds = totalWatch.Elapsed.TotalMilliseconds;
            return result;
        }

        private static TournamentRoundResult SimulateRound(
            IReadOnlyList<TournamentEntry> bracket,
            int roundNumber,
            TournamentSimulationOptions options,
            System.Random rng,
            TournamentSimulationResult result)
        {
            var round = new TournamentRoundResult { round = roundNumber };
            for (int i = 0; i < bracket.Count; i += 2)
            {
                TournamentMatchResult match = RunMatch(bracket[i], bracket[i + 1], roundNumber, (i / 2) + 1, options, rng);
                round.matches.Add(match);
                result.totalMatches++;
                result.totalFights += match.fights.Count;
            }

            return round;
        }

        private static TournamentMatchResult RunMatch(
            TournamentEntry left,
            TournamentEntry right,
            int round,
            int match,
            TournamentSimulationOptions options,
            System.Random rng)
        {
            int leftWins = 0;
            int rightWins = 0;
            int aggregateLeftHp = 0;
            int aggregateRightHp = 0;
            var fightSummary = new StringBuilder();
            var matchResult = new TournamentMatchResult
            {
                round = round,
                match = match,
                left = left,
                right = right
            };

            while (leftWins < options.winsNeeded
                && rightWins < options.winsNeeded
                && matchResult.fights.Count < options.maxFightsPerMatch)
            {
                int fightSeed = rng.Next(1, int.MaxValue);
                CombatSimulationResult fight = CombatSimulator.Run(left.champion, right.champion, new CombatSimulationOptions
                {
                    seed = fightSeed,
                    maxDurationSeconds = options.matchDurationSeconds,
                    maxEvents = options.maxEventsPerFight
                });

                aggregateLeftHp += fight.leftHealth;
                aggregateRightHp += fight.rightHealth;

                if (fight.winner == CombatSimulationSide.Left)
                {
                    leftWins++;
                    fightSummary.Append("L");
                }
                else if (fight.winner == CombatSimulationSide.Right)
                {
                    rightWins++;
                    fightSummary.Append("R");
                }
                else
                {
                    fightSummary.Append("D");
                }

                matchResult.fights.Add(new TournamentFightResult
                {
                    fightSeed = fightSeed,
                    winner = fight.winner,
                    duration = fight.duration,
                    leftHealth = fight.leftHealth,
                    rightHealth = fight.rightHealth
                });
            }

            matchResult.leftWins = leftWins;
            matchResult.rightWins = rightWins;
            if (leftWins == rightWins)
            {
                matchResult.winner = aggregateLeftHp >= aggregateRightHp ? left : right;
                fightSummary.Append("*");
            }
            else
            {
                matchResult.winner = leftWins > rightWins ? left : right;
            }

            matchResult.fightSummary = fightSummary.ToString();
            return matchResult;
        }

        private static List<TournamentEntry> GenerateEntries(TournamentSimulationOptions options, System.Random rng)
        {
            var entries = new List<TournamentEntry>(options.playerCount);
            for (int i = 0; i < options.playerCount; i++)
            {
                int seed = rng.Next(1, int.MaxValue);
                entries.Add(new TournamentEntry
                {
                    slot = i + 1,
                    generationSeed = seed,
                    champion = ChampionDataFactory.CreateRandom(seed, options.championLevel, options.totalBaseStatPoints)
                });
            }

            return entries;
        }

        private static void ValidateOptions(TournamentSimulationOptions options)
        {
            options.playerCount = Mathf.Max(2, options.playerCount);
            options.winsNeeded = Mathf.Max(1, options.winsNeeded);
            options.maxFightsPerMatch = Mathf.Max(options.winsNeeded * 2 - 1, options.maxFightsPerMatch);
            options.matchDurationSeconds = Mathf.Max(1f, options.matchDurationSeconds);
            options.maxEventsPerFight = Mathf.Max(100, options.maxEventsPerFight);
            options.championLevel = Mathf.Max(1, options.championLevel);
            options.totalBaseStatPoints = Mathf.Max(5, options.totalBaseStatPoints);

            if (!IsPowerOfTwo(options.playerCount))
            {
                throw new ArgumentException("Generated single elimination tournaments currently require a power-of-two player count.");
            }
        }

        private static bool IsPowerOfTwo(int value)
        {
            return value > 0 && (value & (value - 1)) == 0;
        }
    }
}
