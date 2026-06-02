using ElBestia.Skills;

namespace ElBestia.Combat.Charges
{
    internal interface IChargeEffect
    {
        ChargeType Charge { get; }
        bool IsPositive { get; }
        bool TicksOnTurnStart { get; }
        SkillElement DamageElement { get; }
        ChargeType Opposite { get; }
    }
}
