namespace ElBestia.Perks
{
    public enum PerkRarity
    {
        Common,
        Rare,
        VeryRare,
        Epic,
        Legendary
    }

    public enum PerkEffectType
    {
        BaseStatMultiplier,
        WeaponProficiency,
        CombatStatBonus,
        LifeSteal,
        DoubleAppliedCharges,
        ApplyChargeOnAction,
        GainChargeOnAction,
        Counterattack,
        OpeningCharges,
        ExperienceGain,
        FatalInjuryResistance,
        InjurySeverityReduction,
        MaxLifeBonus,
        GhostStrike,
        BlockPierce,
        ElementalResistance,
        ElementalAbsorption,
        ElementalSelfDotOnAction,
        ElementalExplosionOnHit,
        LazyGenius,
        InvertedBuffs,
        LowLifeCombatPower,
        BaseStatBonus,
        OpeningActionSpeed,
        SkillEcho,
        ElementalDamageBonus,
        ElementalDamageMultiplier
    }

    public enum PerkCombatStatType
    {
        None,
        DodgeRating,
        BlockRating,
        CriticalRating,
        HitRating,
        Life,
        Energy,
        Recovery,
        ExperienceGain,
        FatalInjuryResistance,
        InjurySeverityReduction
    }
}
