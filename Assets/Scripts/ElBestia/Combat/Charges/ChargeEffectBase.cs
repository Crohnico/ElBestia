using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal abstract class ChargeEffectBase : IChargeEffect
    {
        public abstract ChargeType Charge { get; }
        public virtual bool IsPositive => false;
        public virtual bool TicksOnTurnStart => false;
        public virtual SkillElement DamageElement => SkillElement.None;
        public virtual ChargeType Opposite => ChargeType.None;
    }
}
