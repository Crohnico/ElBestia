using System;
using System.Collections.Generic;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionSkillLoadout
    {
        private readonly ChampionBehaviour owner;
        private readonly Func<CombatCrono> getCrono;
        private readonly float[] skillReadyAt = new float[4];

        public ChampionSkillLoadout(ChampionBehaviour owner, Func<CombatCrono> getCrono)
        {
            this.owner = owner;
            this.getCrono = getCrono;
        }

        public void ClearCooldowns()
        {
            Array.Clear(skillReadyAt, 0, skillReadyAt.Length);
        }

        public int PickSkillIndex()
        {
            var available = new List<int> { 0 };
            if (owner.Champion != null && owner.Champion.Skills != null)
            {
                for (int i = 0; i < Mathf.Min(3, owner.Champion.Skills.Length); i++)
                {
                    int skillIndex = i + 1;
                    if (owner.Champion.Skills[i] != null && IsSkillReady(skillIndex))
                    {
                        available.Add(skillIndex);
                    }
                }
            }

            return available[owner.RollIntForCombat(0, available.Count)];
        }

        public SkillData GetSkill(int skillIndex)
        {
            if (owner.Champion == null)
            {
                return null;
            }

            if (skillIndex == 0)
            {
                return owner.Champion.BaseSkill;
            }

            int equippedIndex = skillIndex - 1;
            return owner.Champion.Skills != null && equippedIndex >= 0 && equippedIndex < owner.Champion.Skills.Length
                ? owner.Champion.Skills[equippedIndex]
                : null;
        }

        public void SetCooldown(int skillIndex, SkillData skill)
        {
            CombatCrono crono = getCrono();
            if (skillIndex <= 0 || skillIndex >= skillReadyAt.Length || crono == null || skill == null)
            {
                return;
            }

            skillReadyAt[skillIndex] = crono.CurrentTime + Mathf.Max(0f, skill.cooldownSeconds);
        }

        private bool IsSkillReady(int skillIndex)
        {
            CombatCrono crono = getCrono();
            if (skillIndex <= 0 || skillIndex >= skillReadyAt.Length || crono == null)
            {
                return true;
            }

            return crono.CurrentTime >= skillReadyAt[skillIndex];
        }
    }
}
