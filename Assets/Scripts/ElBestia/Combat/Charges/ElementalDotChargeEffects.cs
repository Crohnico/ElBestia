using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal sealed class PoisonChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Poison;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Poison;
        public override ChargeType Opposite => ChargeType.PoisonFortified;
    }

    internal sealed class BurnChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Burn;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Fire;
        public override ChargeType Opposite => ChargeType.FireFortified;
    }

    internal sealed class DrowningChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Drowning;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Water;
        public override ChargeType Opposite => ChargeType.WaterFortified;
    }

    internal sealed class ShockChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Shock;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Electricity;
        public override ChargeType Opposite => ChargeType.ElectricityFortified;
    }

    internal sealed class SplinterChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Splinter;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Wood;
        public override ChargeType Opposite => ChargeType.WoodFortified;
    }

    internal sealed class CrushChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.Crush;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Earth;
        public override ChargeType Opposite => ChargeType.EarthFortified;
    }

    internal sealed class WindShearChargeEffect : ChargeEffectBase
    {
        public override ChargeType Charge => ChargeType.WindShear;
        public override bool TicksOnTurnStart => true;
        public override SkillElement DamageElement => SkillElement.Air;
        public override ChargeType Opposite => ChargeType.AirFortified;
    }
}
