using ElBestia.Combat.Charges;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    public static partial class CombatSimulator
    {
        private static void ApplyOpeningCharges(SimChampion actor, CombatSimulationResult result, float time)
        {
            if (actor.Champion == null || actor.Champion.Perks == null)
            {
                return;
            }

            foreach (PerkSO perk in actor.Champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.OpeningCharges)
                {
                    AddCharge(actor, perk.Charge, perk.FlatValue, actor, time, result);
                }
            }
        }

        private static void ApplyStartTurnEffects(SimChampion actor, CombatSimulationResult result, float time, CombatSimulationOptions options, System.Random rng)
        {
            SimCharge[] snapshot = actor.Charges.ToArray();
            foreach (SimCharge charge in snapshot)
            {
                if (charge == null || charge.amount <= 0 || !ChargeEffectCatalog.TicksOnTurnStart(charge.charge))
                {
                    continue;
                }

                if (charge.charge == ChargeType.Regeneration)
                {
                    Heal(actor, Mathf.Max(1, Mathf.RoundToInt(actor.MaxHealth * 0.1f)), time, result);
                }
                else
                {
                    SimChampion source = charge.source ?? actor;
                    int damage = CalculateOutgoingAmount(source, GetIntelligence(source) * 2 * charge.amount);
                    DealDamage(source, actor, damage, ChargeEffectCatalog.GetDamageElement(charge.charge), WeaponType.None, false, false, false, time, options, rng, result);
                }

                ConsumeCharge(actor, charge.charge, 1, time, result);
                if (!actor.IsAlive)
                {
                    break;
                }
            }
        }

        private static void ConsumeEndTurnCharges(SimChampion actor, float time, CombatSimulationResult result)
        {
            ConsumeCharge(actor, ChargeType.Slow, 1, time, result);
            ConsumeCharge(actor, ChargeType.Haste, 1, time, result);
            ConsumeCharge(actor, ChargeType.Weakened, 1, time, result);
            ConsumeCharge(actor, ChargeType.Empowered, 1, time, result);
            ConsumeCharge(actor, ChargeType.LifeSteal, 1, time, result);
            ConsumeCharge(actor, ChargeType.Recoil, 1, time, result);
            ConsumeCharge(actor, ChargeType.Thorns, 1, time, result);
            ConsumeCharge(actor, ChargeType.FireFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.WaterFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.ElectricityFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.PoisonFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.EarthFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.AirFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.WoodFortified, 1, time, result);
            ConsumeCharge(actor, ChargeType.Dizzle, 1, time, result);
        }

        private static void ApplyAfterActionPerks(SimChampion actor, float time, CombatSimulationResult result)
        {
            if (actor.Champion == null || actor.Champion.Perks == null)
            {
                return;
            }

            foreach (PerkSO perk in actor.Champion.Perks)
            {
                if (perk == null || perk.Charge == ChargeType.None || perk.FlatValue <= 0)
                {
                    continue;
                }

                if (perk.EffectType == PerkEffectType.ApplyChargeOnAction)
                {
                    SimChampion target = ChargeEffectCatalog.IsPositive(perk.Charge) ? actor : actor.Enemy;
                    AddCharge(target, perk.Charge, perk.FlatValue, actor, time, result);
                }
                else if (perk.EffectType == PerkEffectType.GainChargeOnAction)
                {
                    AddCharge(actor, perk.Charge, perk.FlatValue, actor, time, result);
                }
                else if (perk.EffectType == PerkEffectType.ElementalSelfDotOnAction)
                {
                    AddCharge(actor, perk.Charge, perk.FlatValue, actor, time, result);
                    AddCharge(actor.Enemy, perk.Charge, perk.FlatValue, actor, time, result);
                }
            }
        }

        private static void AddCharge(SimChampion target, ChargeType charge, int amount, SimChampion source, float time, CombatSimulationResult result)
        {
            if (target == null || charge == ChargeType.None || amount <= 0)
            {
                return;
            }

            charge = GetIncomingChargeAfterInversion(target, charge);
            if (charge == ChargeType.None)
            {
                return;
            }

            int finalAmount = GetIncomingChargeAmount(target, amount);
            float speedBefore = target.GetActionTimeMultiplier();
            SimCharge existing = target.Charges.Find(entry => entry.charge == charge);
            if (existing != null)
            {
                existing.amount += finalAmount;
                existing.source = source;
            }
            else
            {
                target.Charges.Add(new SimCharge { charge = charge, amount = finalAmount, source = source });
            }

            RetargetIfSpeedChanged(target, speedBefore, time);
            AddEvent(result, time, CombatSimulationEventType.ChargeAdded, source != null ? source.Side : CombatSimulationSide.None, target.Side, null, SkillElement.None, charge, finalAmount, target.CurrentHealth, null);
        }

        private static void ConsumeCharge(SimChampion actor, ChargeType charge, int amount, float time, CombatSimulationResult result)
        {
            SimCharge existing = actor.Charges.Find(entry => entry.charge == charge);
            if (existing == null || amount <= 0)
            {
                return;
            }

            int consumed = Mathf.Min(existing.amount, amount);
            existing.amount -= consumed;
            if (existing.amount <= 0)
            {
                actor.Charges.Remove(existing);
            }

            AddEvent(result, time, CombatSimulationEventType.ChargeConsumed, actor.Side, actor.Side, null, SkillElement.None, charge, consumed, actor.CurrentHealth, null);
        }

        private static ChargeType GetIncomingChargeAfterInversion(SimChampion target, ChargeType charge)
        {
            if (!HasPerk(target, PerkEffectType.InvertedBuffs) || !ChargeEffectCatalog.TryGetOpposite(charge, out ChargeType opposite))
            {
                return charge;
            }

            return opposite;
        }

        private static int GetIncomingChargeAmount(SimChampion target, int amount)
        {
            int multiplier = 1;
            if (target.Champion != null && target.Champion.Perks != null)
            {
                foreach (PerkSO perk in target.Champion.Perks)
                {
                    if (perk != null && perk.EffectType == PerkEffectType.DoubleAppliedCharges)
                    {
                        multiplier = Mathf.Max(multiplier, perk.FlatValue);
                    }
                }
            }

            return Mathf.Max(0, amount) * Mathf.Max(1, multiplier);
        }

        private static bool HasPerk(SimChampion actor, PerkEffectType effectType)
        {
            return ChampionPerkQuery.Has(actor.Champion, effectType);
        }
    }
}
