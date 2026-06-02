using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionDamageResolver
    {
        private const float DirectDamageMinMultiplier = 0.85f;
        private const float DirectDamageMaxMultiplier = 1f;

        private readonly ChampionBehaviour owner;
        private readonly ChampionDamageCalculator calculator;

        public ChampionDamageResolver(ChampionBehaviour owner)
        {
            this.owner = owner;
            calculator = new ChampionDamageCalculator(owner);
        }

        public void ExecuteDamageAction(SkillAction skillAction, SkillExecutionContext context, ChampionBehaviour target, bool allowCounterattack = true)
        {
            int amount = skillAction.amount;
            if (amount < 0)
            {
                target.Heal(CalculateOutgoingAmount(-amount));
                return;
            }

            int damage = Mathf.RoundToInt((calculator.CalculateOutgoingSkillDamage(amount, context.skill) + context.damageBonus) * Mathf.Max(0f, context.damageMultiplier));
            damage = ApplyDirectDamageVariance(damage);
            DealDamageTo(target, damage, context.skill != null ? context.skill.element : SkillElement.None, allowCounterattack, true, CombatStatUtility.GetSkillWeapon(context.skill), true);
        }

        public int DealDamageTo(
            ChampionBehaviour target,
            int amount,
            SkillElement element,
            bool allowCounterattack,
            bool allowAvoidance,
            WeaponType criticalWeapon = WeaponType.None,
            bool triggerReactiveEffects = false)
        {
            if (target == null || amount <= 0)
            {
                return 0;
            }

            int damageAmount = Mathf.Max(0, amount);
            bool canAvoid = allowAvoidance && target != owner;
            WeaponAvoidanceResult avoidance = canAvoid && target.Avoidance != null ? target.Avoidance.RollAgainst(owner) : WeaponAvoidanceResult.Hit;
            int ghostAttempts = 0;
            while (avoidance == WeaponAvoidanceResult.Dodged && owner.Avoidance.TryGetGhostStrikeRetryAmount(damageAmount, out int retryAmount) && ghostAttempts < 16)
            {
                owner.ConsumeChargeForCombat(ChargeType.Dizzle, 1);
                damageAmount = retryAmount;
                ghostAttempts++;
                avoidance = target.Avoidance != null ? target.Avoidance.RollAgainst(owner) : WeaponAvoidanceResult.Hit;
            }

            if (avoidance == WeaponAvoidanceResult.Dodged)
            {
                owner.ConsumeChargeForCombat(ChargeType.Dizzle, 1);
                return 0;
            }

            if (avoidance == WeaponAvoidanceResult.Blocked)
            {
                damageAmount = owner.Avoidance.ApplyBlockReduction(damageAmount);
            }

            damageAmount = calculator.ApplyOutgoingElementalDamage(damageAmount, element);
            damageAmount = owner.Avoidance.RollCriticalDamage(damageAmount, criticalWeapon);
            int finalDamage = target.TakeDamage(damageAmount, element, owner);
            owner.DebugCombatFlow(nameof(ChampionDamageResolver), "Damage", $"target={ChampionLabel(target)} raw={amount} final={finalDamage} element={element} allowCounter={allowCounterattack}");
            ApplyLifeStealAndRecoil(finalDamage);
            if (triggerReactiveEffects && finalDamage > 0 && target != owner)
            {
                target.Damage.TryThorns(owner);
            }

            if (allowCounterattack && finalDamage > 0)
            {
                owner.QueueCounterattackForCombat(target);
            }

            return finalDamage;
        }

        public int TakeDamage(int amount, SkillElement element, ChampionBehaviour source)
        {
            int finalDamage = Mathf.Max(0, calculator.CalculateIncomingDamage(amount, element));
            return owner.ApplyResolvedDamage(finalDamage);
        }

        public int CalculateOutgoingAmount(float amount)
        {
            return calculator.CalculateOutgoingAmount(amount);
        }

        public int GetIntelligence()
        {
            return calculator.GetIntelligence();
        }

        public void TryThorns(ChampionBehaviour attacker)
        {
            if (attacker == null || !owner.IsAlive || !owner.HasChargeForCombat(ChargeType.Thorns))
            {
                return;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(owner.MaxHealth * 0.1f));
            DealDamageTo(attacker, damage, SkillElement.None, false, false);
        }

        private void ApplyLifeStealAndRecoil(int damage)
        {
            if (damage <= 0)
            {
                return;
            }

            float lifeStealPercent = calculator.GetLifeStealPercent();
            if (lifeStealPercent > 0f)
            {
                owner.Heal(Mathf.RoundToInt(damage * lifeStealPercent));
            }

            if (owner.HasChargeForCombat(ChargeType.Recoil))
            {
                int recoilDamage = Mathf.Max(1, Mathf.RoundToInt(damage * 0.2f));
                owner.TakeDamage(recoilDamage, SkillElement.None, owner);
            }
        }

        private int ApplyDirectDamageVariance(int amount)
        {
            float multiplier = Mathf.Lerp(DirectDamageMinMultiplier, DirectDamageMaxMultiplier, owner.Roll01ForCombat());
            return Mathf.Max(1, Mathf.RoundToInt(amount * multiplier));
        }

        private static string ChampionLabel(ChampionBehaviour behaviour)
        {
            if (behaviour == null)
            {
                return "null";
            }

            return behaviour.Champion != null && !string.IsNullOrEmpty(behaviour.Champion.ChampionName)
                ? behaviour.Champion.ChampionName
                : behaviour.name;
        }
    }
}
