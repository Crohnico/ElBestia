using ElBestia.Champions;
using ElBestia.Combat.Charges;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public static partial class CombatSimulator
    {
        private static int CalculateOutgoingAmount(SimChampion actor, float amount)
        {
            float modified = Mathf.Max(0f, amount);
            if (actor.HasCharge(ChargeType.Empowered))
            {
                modified *= 1.2f;
            }

            if (actor.HasCharge(ChargeType.Weakened))
            {
                modified *= 0.8f;
            }

            return Mathf.Max(1, Mathf.RoundToInt(modified));
        }

        private static int CalculateOutgoingSkillDamage(SimChampion actor, int actionAmount, SkillData skill)
        {
            float damage = Mathf.Max(0, actionAmount);
            if (actor.Stats != null && skill != null && skill.statScaling != null)
            {
                foreach (StatScaling scaling in skill.statScaling)
                {
                    if (scaling == null)
                    {
                        continue;
                    }

                    float gradeMultiplier = scaling.usesGradeScaling ? CombatStatUtility.GetScalingMultiplier(scaling.scaling) : 0.75f;
                    damage += CombatStatUtility.GetStatValue(actor.Stats, scaling.stat) * gradeMultiplier;
                }
            }

            WeaponType weapon = CombatStatUtility.GetSkillWeapon(skill);
            int proficiency = actor.Stats != null ? actor.Stats.GetProficiency(weapon) : 0;
            damage *= 1f + proficiency / 100f;
            return CalculateOutgoingAmount(actor, damage);
        }

        private static int DealDamage(
            SimChampion source,
            SimChampion target,
            int amount,
            SkillElement element,
            WeaponType weapon,
            bool allowCounterattack,
            bool allowAvoidance,
            bool directDamage,
            float time,
            CombatSimulationOptions options,
            System.Random rng,
            CombatSimulationResult result)
        {
            if (source == null || target == null || amount <= 0 || !source.IsAlive || !target.IsAlive)
            {
                return 0;
            }

            int damage = directDamage ? ApplyDirectDamageVariance(amount, options, rng) : amount;
            if (allowAvoidance && source != target)
            {
                WeaponAvoidanceResult avoidance = RollAvoidance(source, target, rng);
                int ghostAttempts = 0;
                while (avoidance == WeaponAvoidanceResult.Dodged && TryGetGhostStrikeRetryAmount(source, damage, out int retryAmount) && ghostAttempts < 16)
                {
                    ConsumeCharge(source, ChargeType.Dizzle, 1, time, result);
                    damage = retryAmount;
                    ghostAttempts++;
                    avoidance = RollAvoidance(source, target, rng);
                }

                if (avoidance == WeaponAvoidanceResult.Dodged)
                {
                    ConsumeCharge(source, ChargeType.Dizzle, 1, time, result);
                    AddEvent(result, time, CombatSimulationEventType.Dodged, source.Side, target.Side, null, element, ChargeType.None, 0, target.CurrentHealth, null);
                    return 0;
                }

                if (avoidance == WeaponAvoidanceResult.Blocked)
                {
                    damage = ApplyBlockReduction(source, damage);
                    AddEvent(result, time, CombatSimulationEventType.Blocked, source.Side, target.Side, null, element, ChargeType.None, damage, target.CurrentHealth, null);
                }
            }

            damage = ApplyOutgoingElementalDamage(source, damage, element);
            damage = RollCriticalDamage(source, damage, weapon, time, result, rng);
            int finalDamage = ApplyIncomingDamage(target, damage, element, time, result);
            target.CurrentHealth = Mathf.Max(0, target.CurrentHealth - finalDamage);
            AddEvent(result, time, directDamage ? CombatSimulationEventType.DirectDamage : CombatSimulationEventType.DotDamage, source.Side, target.Side, null, element, ChargeType.None, finalDamage, target.CurrentHealth, null);
            ApplyLifeStealAndRecoil(source, finalDamage, time, options, rng, result);

            if (directDamage && finalDamage > 0 && source != target)
            {
                TryThorns(target, source, time, options, rng, result);
                if (allowCounterattack)
                {
                    ResolveCounterattack(target, source, time, options, rng, result);
                }
            }

            return finalDamage;
        }

        private static void Heal(SimChampion target, int amount, float time, CombatSimulationResult result)
        {
            if (target == null || amount <= 0 || !target.IsAlive)
            {
                return;
            }

            int healed = Mathf.Min(amount, target.MaxHealth - target.CurrentHealth);
            target.CurrentHealth += healed;
            AddEvent(result, time, CombatSimulationEventType.Heal, target.Side, target.Side, null, SkillElement.None, ChargeType.None, healed, target.CurrentHealth, null);
        }

        private static void ApplyCaltrops(SimChampion actor, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            if (!actor.HasCharge(ChargeType.Caltrops))
            {
                return;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(actor.MaxHealth * 0.1f));
            DealDamage(actor, actor, damage, SkillElement.None, WeaponType.None, false, false, false, time, options, rng, result);
            ConsumeCharge(actor, ChargeType.Caltrops, 1, time, result);
        }

        private static void ExecuteSkillEcho(SimChampion actor, SkillData skill, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            float multiplier = GetSkillEchoDamageMultiplier(actor);
            if (multiplier <= 0f || skill == null || !actor.IsAlive || !actor.Enemy.IsAlive)
            {
                return;
            }

            ExecuteActions(actor, skill, skill.preCast, false, multiplier, time, options, rng, result);
            ExecuteActions(actor, skill, skill.cast, true, multiplier, time, options, rng, result);
            ExecuteActions(actor, skill, skill.postCast, false, multiplier, time, options, rng, result);
        }

        private static int GetIntelligence(SimChampion actor)
        {
            return actor.Stats != null ? Mathf.Max(0, actor.Stats.intelligence) : 0;
        }

        private static int ApplyIncomingDamage(SimChampion target, int amount, SkillElement element, float time, CombatSimulationResult result)
        {
            float modified = Mathf.Max(0, amount);
            if (ConsumeIncomingModifier(target, ChargeType.Fortified, time, result))
            {
                modified *= 0.8f;
            }

            if (ConsumeIncomingModifier(target, ChargeType.Vulnerable, time, result))
            {
                modified *= 1.2f;
            }

            ChargeType elementalFortification = ChargeEffectCatalog.GetElementalFortification(element);
            if (elementalFortification != ChargeType.None && ConsumeIncomingModifier(target, elementalFortification, time, result))
            {
                modified *= 0.7f;
            }

            float resistance = target.Stats != null ? target.Stats.GetElementalResistancePercent(element) : 0f;
            return Mathf.Max(0, Mathf.RoundToInt(modified * Mathf.Clamp01(1f - resistance / 100f)));
        }

        private static bool ConsumeIncomingModifier(SimChampion target, ChargeType charge, float time, CombatSimulationResult result)
        {
            if (!target.HasCharge(charge))
            {
                return false;
            }

            ConsumeCharge(target, charge, 1, time, result);
            return true;
        }

        private static int ApplyOutgoingElementalDamage(SimChampion source, int amount, SkillElement element)
        {
            if (source.Stats == null || element == SkillElement.None || amount <= 0)
            {
                return Mathf.Max(0, amount);
            }

            float modified = amount + source.Stats.GetElementalDamageBonus(element);
            modified *= source.Stats.GetElementalDamageMultiplier(element);
            return Mathf.Max(1, Mathf.RoundToInt(modified));
        }

        private static int ApplyDirectDamageVariance(int amount, CombatSimulationOptions options, System.Random rng)
        {
            float min = options != null ? options.directDamageMinMultiplier : 0.85f;
            float max = options != null ? options.directDamageMaxMultiplier : 1f;
            float multiplier = Mathf.Lerp(Mathf.Min(min, max), Mathf.Max(min, max), (float)rng.NextDouble());
            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }

        private static WeaponAvoidanceResult RollAvoidance(SimChampion source, SimChampion target, System.Random rng)
        {
            int attackerHit = GetHitRating(source);
            float dodgeChance = GetContestedChance(ApplyDizzleRatingPenalty(target, target.Stats != null ? target.Stats.dodge : 0, 0.1f), attackerHit);
            if ((float)rng.NextDouble() < dodgeChance)
            {
                return WeaponAvoidanceResult.Dodged;
            }

            float blockChance = GetContestedChance(ApplyDizzleRatingPenalty(target, target.Stats != null ? target.Stats.block : 0, 0.1f), attackerHit);
            return (float)rng.NextDouble() < blockChance ? WeaponAvoidanceResult.Blocked : WeaponAvoidanceResult.Hit;
        }

        private static int RollCriticalDamage(SimChampion source, int amount, WeaponType weapon, float time, CombatSimulationResult result, System.Random rng)
        {
            if (amount <= 0 || source.Stats == null)
            {
                return amount;
            }

            float chance = source.Stats.GetCriticalChancePercent(weapon) / 100f;
            if ((float)rng.NextDouble() >= chance)
            {
                return amount;
            }

            int criticalDamage = Mathf.Max(1, Mathf.RoundToInt(amount * 2f));
            AddEvent(result, time, CombatSimulationEventType.Critical, source.Side, source.Enemy != null ? source.Enemy.Side : CombatSimulationSide.None, null, SkillElement.None, ChargeType.None, criticalDamage, 0, null);
            return criticalDamage;
        }

        private static int GetHitRating(SimChampion source)
        {
            int rating = source.Stats != null ? Mathf.Max(0, source.Stats.hit) : 0;
            return ApplyDizzleRatingPenalty(source, rating, 0.2f);
        }

        private static int ApplyDizzleRatingPenalty(SimChampion actor, int rating, float penaltyPerStack)
        {
            int stacks = actor.GetChargeAmount(ChargeType.Dizzle);
            float multiplier = Mathf.Max(0f, 1f - penaltyPerStack * stacks);
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, rating) * multiplier));
        }

        private static float GetContestedChance(int defenderRating, int attackerHit)
        {
            int effectiveRating = Mathf.RoundToInt(Mathf.Max(0, defenderRating) * 100f / (100f + Mathf.Max(0, attackerHit)));
            return ChampionStats.ConvertRatingToPercent(effectiveRating) / 100f;
        }

        private static int ApplyBlockReduction(SimChampion source, int amount)
        {
            const float blockedFraction = 0.5f;
            float pierce = Mathf.Clamp01(GetMaxFlatValue(source, PerkEffectType.BlockPierce) / 100f);
            float damageMultiplier = (1f - blockedFraction) + blockedFraction * pierce;
            return Mathf.Max(1, Mathf.RoundToInt(amount * damageMultiplier));
        }

        private static bool TryGetGhostStrikeRetryAmount(SimChampion source, int currentAmount, out int retryAmount)
        {
            retryAmount = 0;
            int percent = GetMaxFlatValue(source, PerkEffectType.GhostStrike);
            if (percent <= 0 || currentAmount <= 5)
            {
                return false;
            }

            retryAmount = Mathf.Max(5, Mathf.RoundToInt(currentAmount * percent / 100f));
            if (retryAmount >= currentAmount)
            {
                retryAmount = Mathf.Max(5, currentAmount - 1);
            }

            return retryAmount > 0;
        }

        private static void ApplyLifeStealAndRecoil(SimChampion source, int damage, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            if (damage <= 0)
            {
                return;
            }

            float lifeStealPercent = source.HasCharge(ChargeType.LifeSteal) ? 0.2f : 0f;
            lifeStealPercent += GetMaxFlatValue(source, PerkEffectType.LifeSteal) / 100f;
            if (lifeStealPercent > 0f)
            {
                Heal(source, Mathf.RoundToInt(damage * lifeStealPercent), time, result);
            }

            if (source.HasCharge(ChargeType.Recoil))
            {
                int recoilDamage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.2f));
                int finalDamage = ApplyIncomingDamage(source, recoilDamage, SkillElement.None, time, result);
                source.CurrentHealth = Mathf.Max(0, source.CurrentHealth - finalDamage);
                AddEvent(result, time, CombatSimulationEventType.DirectDamage, source.Side, source.Side, null, SkillElement.None, ChargeType.Recoil, finalDamage, source.CurrentHealth, "recoil");
            }
        }

        private static void TryThorns(SimChampion defender, SimChampion attacker, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            if (!defender.HasCharge(ChargeType.Thorns))
            {
                return;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(defender.MaxHealth * 0.1f));
            DealDamage(defender, attacker, damage, SkillElement.None, WeaponType.None, false, false, false, time, options, rng, result);
        }

        private static void ResolveCounterattack(SimChampion counterattacker, SimChampion attacker, float time, CombatSimulationOptions options, System.Random rng, CombatSimulationResult result)
        {
            SkillData baseSkill = counterattacker.Champion != null ? counterattacker.Champion.BaseSkill : null;
            int safety = 64;
            while (counterattacker.IsAlive && attacker.IsAlive && counterattacker.HasCharge(ChargeType.Counterattack) && baseSkill != null && safety-- > 0)
            {
                ApplyCaltrops(counterattacker, time, options, rng, result);
                if (!counterattacker.IsAlive)
                {
                    break;
                }

                ExecuteActions(counterattacker, baseSkill, baseSkill.cast, false, 1f, time, options, rng, result);
                ConsumeCharge(counterattacker, ChargeType.Counterattack, 1, time, result);
            }
        }

        private static int GetMaxFlatValue(SimChampion actor, PerkEffectType effectType)
        {
            int value = 0;
            if (actor.Champion == null || actor.Champion.Perks == null)
            {
                return value;
            }

            foreach (PerkSO perk in actor.Champion.Perks)
            {
                if (perk != null && perk.EffectType == effectType)
                {
                    value = Mathf.Max(value, perk.FlatValue);
                }
            }

            return value;
        }
    }
}
