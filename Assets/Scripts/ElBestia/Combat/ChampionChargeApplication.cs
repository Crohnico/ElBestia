using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionChargeApplication
    {
        private readonly ChampionBehaviour owner;

        public ChampionChargeApplication(ChampionBehaviour owner)
        {
            this.owner = owner;
        }

        public void Add(ChargeType charge, int amount, ChampionBehaviour source)
        {
            if (charge == ChargeType.None || amount <= 0)
            {
                return;
            }

            charge = GetIncomingChargeAfterInversion(charge);
            if (charge == ChargeType.None)
            {
                return;
            }

            amount = GetIncomingChargeAmount(amount);
            float speedBefore = owner.GetActionTimeMultiplier();
            owner.AddRawChargeForCombat(charge, amount, source);
            owner.DebugCombatFlow(nameof(ChampionChargeApplication), "AddCharges", $"{charge} +{amount} from={Label(source)} total={owner.GetChargeAmountForCombat(charge)} speedBefore={speedBefore:0.00} speedAfter={owner.GetActionTimeMultiplier():0.00}");
        }

        private ChargeType GetIncomingChargeAfterInversion(ChargeType charge)
        {
            if (!HasInvertedBuffsPerk() || !ChampionBehaviour.TryGetOppositeCharge(charge, out ChargeType opposite))
            {
                return charge;
            }

            return opposite;
        }

        private int GetIncomingChargeAmount(int amount)
        {
            int result = Mathf.Max(0, amount);
            if (owner.Champion == null || owner.Champion.Perks == null)
            {
                return result;
            }

            int multiplier = 1;
            foreach (PerkSO perk in owner.Champion.Perks)
            {
                if (perk != null && perk.EffectType == PerkEffectType.DoubleAppliedCharges)
                {
                    multiplier = Mathf.Max(multiplier, perk.FlatValue);
                }
            }

            return result * Mathf.Max(1, multiplier);
        }

        private bool HasInvertedBuffsPerk()
        {
            return ChampionPerkQuery.Has(owner.Champion, PerkEffectType.InvertedBuffs);
        }

        private static string Label(ChampionBehaviour behaviour)
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
