using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal sealed class ThornsChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Thorns;
        public override bool IsPositive => true;
        public override ChargeType Opposite => ChargeType.Caltrops;
    }

    internal sealed class CaltropsChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Caltrops;
        public override ChargeType Opposite => ChargeType.Thorns;
    }

    internal sealed class RecoilChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Recoil;
        public override ChargeType Opposite => ChargeType.LifeSteal;
    }

    internal sealed class DizzleChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Dizzle;
        public override ChargeType Opposite => ChargeType.Counterattack;
    }
}
