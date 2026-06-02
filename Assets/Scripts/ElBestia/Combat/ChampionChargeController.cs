using System;
using System.Collections.Generic;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    internal sealed class ChampionChargeController
    {
        private readonly Func<ActiveCharge[]> getCharges;
        private readonly Action<ActiveCharge[]> setCharges;
        private readonly Func<bool> isExecutingActionFlow;
        private readonly Func<bool> endTurnChargesConsumed;
        private readonly Func<int> completedActions;
        private readonly Func<float> getActionTimeMultiplier;
        private readonly Action<ActiveCharge[]> chargesChanged;
        private readonly Action<float> actionSpeedMayHaveChanged;

        public ChampionChargeController(
            Func<ActiveCharge[]> getCharges,
            Action<ActiveCharge[]> setCharges,
            Func<bool> isExecutingActionFlow,
            Func<bool> endTurnChargesConsumed,
            Func<int> completedActions,
            Func<float> getActionTimeMultiplier,
            Action<ActiveCharge[]> chargesChanged,
            Action<float> actionSpeedMayHaveChanged)
        {
            this.getCharges = getCharges;
            this.setCharges = setCharges;
            this.isExecutingActionFlow = isExecutingActionFlow;
            this.endTurnChargesConsumed = endTurnChargesConsumed;
            this.completedActions = completedActions;
            this.getActionTimeMultiplier = getActionTimeMultiplier;
            this.chargesChanged = chargesChanged;
            this.actionSpeedMayHaveChanged = actionSpeedMayHaveChanged;
        }

        public void Clear()
        {
            setCharges(Array.Empty<ActiveCharge>());
            chargesChanged?.Invoke(getCharges());
        }

        public void Add(ChargeType charge, int amount, ChampionBehaviour source)
        {
            if (charge == ChargeType.None || amount <= 0)
            {
                return;
            }

            float previousActionTimeMultiplier = getActionTimeMultiplier();
            var charges = new List<ActiveCharge>(getCharges() ?? Array.Empty<ActiveCharge>());
            ActiveCharge existing = charges.Find(entry => entry != null && entry.charge == charge);
            if (existing != null)
            {
                existing.amount += amount;
                existing.source = source;
                ProtectNewChargesIfEndTurnWasConsumed(existing, amount);
            }
            else
            {
                var newCharge = new ActiveCharge { charge = charge, amount = amount, source = source };
                ProtectNewChargesIfEndTurnWasConsumed(newCharge, amount);
                charges.Add(newCharge);
            }

            setCharges(charges.ToArray());
            chargesChanged?.Invoke(getCharges());
            actionSpeedMayHaveChanged?.Invoke(previousActionTimeMultiplier);
        }

        public void Consume(ChargeType chargeType, int amount, bool respectProtection = false)
        {
            ActiveCharge[] activeCharges = getCharges();
            if (activeCharges == null || activeCharges.Length == 0)
            {
                return;
            }

            float previousActionTimeMultiplier = getActionTimeMultiplier();
            var remaining = new List<ActiveCharge>();
            foreach (ActiveCharge charge in activeCharges)
            {
                if (charge == null)
                {
                    continue;
                }

                if (charge.charge == chargeType)
                {
                    int protectedAmount = respectProtection ? GetProtectedChargeAmount(charge) : 0;
                    int consumableAmount = Mathf.Max(0, charge.amount - protectedAmount);
                    int consumed = Mathf.Min(amount, consumableAmount);
                    charge.amount -= consumed;
                }

                if (charge.amount > 0)
                {
                    ClearExpiredChargeProtection(charge);
                    remaining.Add(charge);
                }
            }

            setCharges(remaining.ToArray());
            chargesChanged?.Invoke(getCharges());
            actionSpeedMayHaveChanged?.Invoke(previousActionTimeMultiplier);
        }

        public bool ConsumeIfAvailable(ChargeType chargeType)
        {
            if (!Has(chargeType))
            {
                return false;
            }

            Consume(chargeType, 1);
            return true;
        }

        public int GetAmount(ChargeType chargeType)
        {
            ActiveCharge[] activeCharges = getCharges();
            if (activeCharges == null)
            {
                return 0;
            }

            int total = 0;
            foreach (ActiveCharge charge in activeCharges)
            {
                if (charge != null && charge.charge == chargeType)
                {
                    total += Mathf.Max(0, charge.amount);
                }
            }

            return total;
        }

        public bool Has(ChargeType chargeType)
        {
            return GetAmount(chargeType) > 0;
        }

        private void ProtectNewChargesIfEndTurnWasConsumed(ActiveCharge charge, int amount)
        {
            if (!isExecutingActionFlow() || !endTurnChargesConsumed() || charge == null || amount <= 0)
            {
                return;
            }

            charge.protectedAmount += amount;
            charge.protectedUntilCompletedActions = Mathf.Max(charge.protectedUntilCompletedActions, completedActions() + 1);
        }

        private int GetProtectedChargeAmount(ActiveCharge charge)
        {
            if (charge == null || charge.protectedUntilCompletedActions <= completedActions())
            {
                return 0;
            }

            return Mathf.Clamp(charge.protectedAmount, 0, Mathf.Max(0, charge.amount));
        }

        private void ClearExpiredChargeProtection(ActiveCharge charge)
        {
            if (charge == null || charge.protectedUntilCompletedActions > completedActions())
            {
                return;
            }

            charge.protectedAmount = 0;
            charge.protectedUntilCompletedActions = 0;
        }
    }
}
