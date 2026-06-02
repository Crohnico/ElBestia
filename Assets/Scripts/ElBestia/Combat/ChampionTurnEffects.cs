using ElBestia.Combat.Charges;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionTurnEffects
    {
        private readonly ChampionBehaviour owner;

        public ChampionTurnEffects(ChampionBehaviour owner)
        {
            this.owner = owner;
        }

        public void ApplyOpeningCharges()
        {
            if (owner.Champion == null || owner.Champion.Perks == null)
            {
                return;
            }

            foreach (PerkSO perk in owner.Champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.OpeningCharges)
                {
                    owner.AddCharges(perk.Charge, perk.FlatValue, owner);
                }
            }
        }

        public void ApplyStartTurnEffects()
        {
            ActiveCharge[] activeCharges = owner.ActiveCharges;
            if (activeCharges == null || activeCharges.Length == 0)
            {
                return;
            }

            ActiveCharge[] snapshot = activeCharges;
            foreach (ActiveCharge charge in snapshot)
            {
                if (charge == null || charge.amount <= 0 || !ChargeEffectCatalog.TicksOnTurnStart(charge.charge))
                {
                    continue;
                }

                if (charge.charge == ChargeType.Regeneration)
                {
                    owner.Heal(Mathf.Max(1, Mathf.RoundToInt(owner.MaxHealth * 0.1f)));
                }
                else
                {
                    ChampionBehaviour source = charge.source != null ? charge.source : owner;
                    int damage = source.Damage.CalculateOutgoingAmount(source.Damage.GetIntelligence() * 2 * charge.amount);
                    source.Damage.DealDamageTo(owner, damage, ChargeEffectCatalog.GetDamageElement(charge.charge), false, false);
                }

                owner.ConsumeChargeForCombat(charge.charge, 1);
                if (!owner.IsAlive)
                {
                    break;
                }
            }
        }

        public void ConsumeEndTurnCharges()
        {
            owner.ConsumeChargeForCombat(ChargeType.Slow, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.Haste, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.Weakened, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.Empowered, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.LifeSteal, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.Recoil, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.Thorns, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.FireFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.WaterFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.ElectricityFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.PoisonFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.EarthFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.AirFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.WoodFortified, 1, true);
            owner.ConsumeChargeForCombat(ChargeType.Dizzle, 1, true);
            owner.MarkEndTurnChargesConsumedForCombat();
        }

        public void ApplyAfterActionPerks()
        {
            if (owner.Champion == null || owner.Champion.Perks == null)
            {
                return;
            }

            foreach (PerkSO perk in owner.Champion.Perks)
            {
                if (perk == null || perk.Charge == ChargeType.None || perk.FlatValue <= 0)
                {
                    continue;
                }

                if (perk.EffectType == PerkEffectType.ApplyChargeOnAction)
                {
                    ChampionBehaviour target = ChargeEffectCatalog.IsPositive(perk.Charge) ? owner : owner.RivalForCombat;
                    target?.AddCharges(perk.Charge, Mathf.Max(0, perk.FlatValue), owner);
                }
                else if (perk.EffectType == PerkEffectType.GainChargeOnAction)
                {
                    owner.AddCharges(perk.Charge, perk.FlatValue, owner);
                }
                else if (perk.EffectType == PerkEffectType.ElementalSelfDotOnAction)
                {
                    owner.AddCharges(perk.Charge, perk.FlatValue, owner);
                    owner.RivalForCombat?.AddCharges(perk.Charge, perk.FlatValue, owner);
                }
            }
        }

        public void ApplyCaltropsActionTax()
        {
            if (!owner.HasChargeForCombat(ChargeType.Caltrops))
            {
                return;
            }

            int damage = Mathf.Max(1, Mathf.RoundToInt(owner.MaxHealth * 0.1f));
            owner.Damage.DealDamageTo(owner, damage, SkillElement.None, false, false);
            owner.ConsumeChargeForCombat(ChargeType.Caltrops, 1);
        }
    }
}
