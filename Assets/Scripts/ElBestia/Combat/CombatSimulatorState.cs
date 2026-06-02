using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public static partial class CombatSimulator
    {
        private sealed class SimChampion
        {
            public readonly CombatSimulationSide Side;
            public readonly ChampionData Champion;
            public readonly ChampionStats Stats;
            public readonly int MaxHealth;
            public readonly float[] SkillReadyAt = new float[4];
            public readonly List<SimCharge> Charges = new List<SimCharge>();
            public SimChampion Enemy;
            public int CurrentHealth;
            public int CompletedActions;
            public float ScheduledAt;
            public float BaseActionTime;
            public float NextActionTime;

            public SimChampion(CombatSimulationSide side, ChampionData champion)
            {
                Side = side;
                Champion = champion;
                Stats = champion != null && champion.Stats != null ? champion.Stats.Clone() : new ChampionStats { life = 100 };
                MaxHealth = Mathf.Max(1, Stats.life);
                CurrentHealth = MaxHealth;
            }

            public bool IsAlive => CurrentHealth > 0;

            public bool HasCharge(ChargeType charge)
            {
                return GetChargeAmount(charge) > 0;
            }

            public int GetChargeAmount(ChargeType charge)
            {
                SimCharge existing = Charges.Find(entry => entry.charge == charge);
                return existing != null ? Mathf.Max(0, existing.amount) : 0;
            }

            public float GetActionTimeMultiplier()
            {
                float multiplier = 1f;
                if (HasCharge(ChargeType.Slow))
                {
                    multiplier *= 1.5f;
                }

                if (HasCharge(ChargeType.Haste))
                {
                    multiplier *= 1f / 1.5f;
                }

                return multiplier;
            }
        }

        private sealed class SimCharge
        {
            public ChargeType charge;
            public int amount;
            public SimChampion source;
        }
    }
}
