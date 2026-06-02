using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal sealed class SlowChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Slow;
        public override ChargeType Opposite => ChargeType.Haste;
    }

    internal sealed class HasteChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Haste;
        public override bool IsPositive => true;
        public override ChargeType Opposite => ChargeType.Slow;
    }

    internal sealed class LifeStealChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.LifeSteal;
        public override bool IsPositive => true;
        public override ChargeType Opposite => ChargeType.Recoil;
    }

    internal sealed class CounterattackChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Counterattack;
        public override bool IsPositive => true;
        public override ChargeType Opposite => ChargeType.Dizzle;
    }

    internal sealed class EmpoweredChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Empowered;
        public override bool IsPositive => true;
        public override ChargeType Opposite => ChargeType.Weakened;
    }

    internal sealed class WeakenedChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Weakened;
        public override ChargeType Opposite => ChargeType.Empowered;
    }

    internal sealed class RegenerationChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Regeneration;
        public override bool IsPositive => true;
        public override bool TicksOnTurnStart => true;
        public override ChargeType Opposite => ChargeType.Bleed;
    }

    internal sealed class FortifiedChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Fortified;
        public override bool IsPositive => true;
        public override ChargeType Opposite => ChargeType.Vulnerable;
    }

    internal sealed class VulnerableChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Vulnerable;
        public override ChargeType Opposite => ChargeType.Fortified;
    }

    internal sealed class BleedChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Bleed;
        public override bool TicksOnTurnStart => true;
        public override ChargeType Opposite => ChargeType.Regeneration;
    }
}
