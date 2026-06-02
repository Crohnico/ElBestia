using System;
using ElBestia.Champions;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class WeaponAvoidanceResolver
    {
        private readonly ChampionBehaviour owner;
        private readonly Func<float> getCriticalDamageMultiplier;

        public WeaponAvoidanceResolver(ChampionBehaviour owner, Func<float> getCriticalDamageMultiplier)
        {
            this.owner = owner;
            this.getCriticalDamageMultiplier = getCriticalDamageMultiplier;
        }

        public WeaponAvoidanceResult RollAgainst(ChampionBehaviour attacker)
        {
            ChampionStats stats = owner.Champion != null ? owner.Champion.Stats : null;
            if (stats == null)
            {
                return WeaponAvoidanceResult.Hit;
            }

            int attackerHit = attacker != null && attacker.Avoidance != null ? attacker.Avoidance.GetHitRating() : 0;
            float dodgeChance = GetContestedChance(ApplyDizzleRatingPenalty(stats.dodge, 0.1f), attackerHit);
            if (owner.Roll01ForCombat() < dodgeChance)
            {
                return WeaponAvoidanceResult.Dodged;
            }

            float blockChance = GetContestedChance(ApplyDizzleRatingPenalty(stats.block, 0.1f), attackerHit);
            if (owner.Roll01ForCombat() < blockChance)
            {
                return WeaponAvoidanceResult.Blocked;
            }

            return WeaponAvoidanceResult.Hit;
        }

        public int ApplyBlockReduction(int amount)
        {
            const float blockedFraction = 0.5f;
            float pierce = Mathf.Clamp01(GetBlockPiercePercent());
            float damageMultiplier = (1f - blockedFraction) + blockedFraction * pierce;
            return Mathf.Max(1, Mathf.RoundToInt(amount * damageMultiplier));
        }

        public bool TryGetGhostStrikeRetryAmount(int currentAmount, out int retryAmount)
        {
            retryAmount = 0;
            PerkSO perk = ChampionPerkQuery.GetFirst(owner.Champion, PerkEffectType.GhostStrike);
            if (perk == null || currentAmount <= 5)
            {
                return false;
            }

            float percent = perk.FlatValue > 0 ? perk.FlatValue / 100f : 0.5f;
            retryAmount = Mathf.Max(5, Mathf.RoundToInt(currentAmount * percent));
            if (retryAmount >= currentAmount)
            {
                retryAmount = Mathf.Max(5, currentAmount - 1);
            }

            return retryAmount > 0;
        }

        public int RollCriticalDamage(int amount, WeaponType weapon)
        {
            if (amount <= 0 || !RollCritical(weapon))
            {
                return amount;
            }

            return Mathf.Max(1, Mathf.RoundToInt(amount * Mathf.Max(1f, getCriticalDamageMultiplier())));
        }

        public int GetHitRating()
        {
            int rating = owner.Champion != null && owner.Champion.Stats != null ? Mathf.Max(0, owner.Champion.Stats.hit) : 0;
            return ApplyDizzleRatingPenalty(rating, 0.2f);
        }

        private bool RollCritical(WeaponType weapon)
        {
            ChampionStats stats = owner.Champion != null ? owner.Champion.Stats : null;
            if (stats == null)
            {
                return false;
            }

            float chance = stats.GetCriticalChancePercent(weapon) / 100f;
            return owner.Roll01ForCombat() < chance;
        }

        private int ApplyDizzleRatingPenalty(int rating, float penaltyPerStack)
        {
            int stacks = owner.GetChargeAmountForCombat(ChargeType.Dizzle);
            if (stacks <= 0)
            {
                return Mathf.Max(0, rating);
            }

            float multiplier = Mathf.Max(0f, 1f - penaltyPerStack * stacks);
            return Mathf.Max(0, Mathf.RoundToInt(rating * multiplier));
        }

        private float GetBlockPiercePercent()
        {
            float percent = 0f;
            if (owner.Champion == null || owner.Champion.Perks == null)
            {
                return percent;
            }

            foreach (PerkSO perk in owner.Champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.BlockPierce)
                {
                    percent = Mathf.Max(percent, perk.FlatValue / 100f);
                }
            }

            return percent;
        }

        private static float GetContestedChance(int defenderRating, int attackerHit)
        {
            int effectiveRating = Mathf.RoundToInt(Mathf.Max(0, defenderRating) * 100f / (100f + Mathf.Max(0, attackerHit)));
            return ChampionStats.ConvertRatingToPercent(effectiveRating) / 100f;
        }
    }
}
