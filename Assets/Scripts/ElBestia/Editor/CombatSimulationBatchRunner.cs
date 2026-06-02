using System.Text;
using ElBestia.Champions;
using ElBestia.Combat;
using ElBestia.Generation;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    public static class CombatSimulationBatchRunner
    {
        private const int LeftChampionSeed = 240601;
        private const int RightChampionSeed = 240602;
        private const int FirstCombatSeed = 900100;
        private const int CombatCount = 10;
        private const string LogPrefix = "[ElBestiaSim]";

        [MenuItem("El Bestia/Simulation/Run 10 Generated Fights")]
        public static void RunTenGeneratedFights()
        {
            ChampionData left = ChampionDataFactory.CreateRandom(LeftChampionSeed);
            ChampionData right = ChampionDataFactory.CreateRandom(RightChampionSeed);

            var report = new StringBuilder();
            report.AppendLine($"{LogPrefix} Generated champions");
            report.AppendLine($"{LogPrefix} LEFT  {DescribeChampion(left)}");
            report.AppendLine($"{LogPrefix} RIGHT {DescribeChampion(right)}");

            int leftWins = 0;
            int rightWins = 0;
            int draws = 0;

            for (int i = 0; i < CombatCount; i++)
            {
                int seed = FirstCombatSeed + i;
                CombatSimulationResult result = CombatSimulator.Run(left, right, new CombatSimulationOptions
                {
                    seed = seed,
                    maxDurationSeconds = 60f,
                    maxEvents = 20000
                });

                if (result.winner == CombatSimulationSide.Left)
                {
                    leftWins++;
                }
                else if (result.winner == CombatSimulationSide.Right)
                {
                    rightWins++;
                }
                else
                {
                    draws++;
                }

                report.AppendLine($"{LogPrefix} Fight {i + 1:00} seed={seed} winner={WinnerLabel(result, left, right)} duration={result.duration:0.00}s hp={left.ChampionName}:{result.leftHealth}/{left.Stats.life} {right.ChampionName}:{result.rightHealth}/{right.Stats.life} events={result.events.Count}");
            }

            report.AppendLine($"{LogPrefix} Summary {left.ChampionName} {leftWins} - {rightWins} {right.ChampionName} | draws={draws}");
            Debug.Log(report.ToString());
        }

        private static string WinnerLabel(CombatSimulationResult result, ChampionData left, ChampionData right)
        {
            switch (result.winner)
            {
                case CombatSimulationSide.Left:
                    return left.ChampionName;
                case CombatSimulationSide.Right:
                    return right.ChampionName;
                case CombatSimulationSide.Draw:
                    return "Draw";
                default:
                    return "None";
            }
        }

        private static string DescribeChampion(ChampionData champion)
        {
            return $"{champion.ChampionName} id={champion.ChampionId} weapon={champion.EquippedWeapon} hp={champion.Stats.life} energy={champion.Stats.energy} " +
                $"stats=[str:{champion.Stats.strength} agi:{champion.Stats.agility} con:{champion.Stats.constitution} int:{champion.Stats.intelligence} end:{champion.Stats.endurance}] " +
                $"combat=[speed:{champion.Stats.speed} dodge:{champion.Stats.dodge} block:{champion.Stats.block} hit:{champion.Stats.hit} crit:{champion.Stats.critical}] " +
                $"perks=[{DescribePerks(champion)}] skills=[base:{DescribeSkill(champion.BaseSkill)} extra:{DescribeSkills(champion.Skills)}]";
        }

        private static string DescribePerks(ChampionData champion)
        {
            if (champion.Perks == null || champion.Perks.Length == 0)
            {
                return "none";
            }

            var names = new string[champion.Perks.Length];
            for (int i = 0; i < champion.Perks.Length; i++)
            {
                PerkSO perk = champion.Perks[i];
                names[i] = perk != null ? perk.DisplayName : "null";
            }

            return string.Join(", ", names);
        }

        private static string DescribeSkills(SkillData[] skills)
        {
            if (skills == null || skills.Length == 0)
            {
                return "none";
            }

            var names = new string[skills.Length];
            for (int i = 0; i < skills.Length; i++)
            {
                names[i] = DescribeSkill(skills[i]);
            }

            return string.Join(", ", names);
        }

        private static string DescribeSkill(SkillData skill)
        {
            return skill != null ? $"{skill.skillName}({skill.element}, cd:{skill.cooldownSeconds:0.0})" : "none";
        }
    }
}
