using System.Collections.Generic;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public static partial class CombatSimulator
    {
        private static int PickSkillIndex(SimChampion actor, float time, System.Random rng)
        {
            var available = new List<int> { 0 };
            if (actor.Champion != null && actor.Champion.Skills != null)
            {
                int count = Mathf.Min(3, actor.Champion.Skills.Length);
                for (int i = 0; i < count; i++)
                {
                    int skillIndex = i + 1;
                    if (actor.Champion.Skills[i] != null && time >= actor.SkillReadyAt[skillIndex])
                    {
                        available.Add(skillIndex);
                    }
                }
            }

            return available[rng.Next(0, available.Count)];
        }

        private static SkillData GetSkill(SimChampion actor, int skillIndex)
        {
            if (actor.Champion == null)
            {
                return null;
            }

            if (skillIndex == 0)
            {
                return actor.Champion.BaseSkill;
            }

            int equippedIndex = skillIndex - 1;
            return actor.Champion.Skills != null && equippedIndex >= 0 && equippedIndex < actor.Champion.Skills.Length
                ? actor.Champion.Skills[equippedIndex]
                : null;
        }

        private static void SetSkillCooldown(SimChampion actor, int skillIndex, SkillData skill, float time)
        {
            if (skillIndex <= 0 || skillIndex >= actor.SkillReadyAt.Length || skill == null)
            {
                return;
            }

            actor.SkillReadyAt[skillIndex] = time + Mathf.Max(0f, skill.cooldownSeconds);
        }
    }
}
