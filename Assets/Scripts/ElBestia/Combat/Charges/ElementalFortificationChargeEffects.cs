using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal abstract class ElementalFortificationChargeEffect : ChargeEffectBase
    {
        public override bool IsPositive => true;
    }

    internal sealed class FireFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.FireFortified;
        public override SkillElement DamageElement => SkillElement.Fire;
        public override ChargeType Opposite => ChargeType.Burn;
    }

    internal sealed class WaterFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.WaterFortified;
        public override SkillElement DamageElement => SkillElement.Water;
        public override ChargeType Opposite => ChargeType.Drowning;
    }

    internal sealed class ElectricityFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.ElectricityFortified;
        public override SkillElement DamageElement => SkillElement.Electricity;
        public override ChargeType Opposite => ChargeType.Shock;
    }

    internal sealed class PoisonFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.PoisonFortified;
        public override SkillElement DamageElement => SkillElement.Poison;
        public override ChargeType Opposite => ChargeType.Poison;
    }

    internal sealed class EarthFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.EarthFortified;
        public override SkillElement DamageElement => SkillElement.Earth;
        public override ChargeType Opposite => ChargeType.Crush;
    }

    internal sealed class AirFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.AirFortified;
        public override SkillElement DamageElement => SkillElement.Air;
        public override ChargeType Opposite => ChargeType.WindShear;
    }

    internal sealed class WoodFortifiedChargeEffect : ElementalFortificationChargeEffect
    {
        public override ChargeType Charge => ChargeType.WoodFortified;
        public override SkillElement DamageElement => SkillElement.Wood;
        public override ChargeType Opposite => ChargeType.Splinter;
    }
}
