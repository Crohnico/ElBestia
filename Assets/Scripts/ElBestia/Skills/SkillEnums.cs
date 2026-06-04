namespace ElBestia.Skills
{
    public enum SkillElement
    {
        None,
        Water,
        Electricity,
        Fire,
        Poison,
        Earth,
        Air,
        Wood
    }

    public enum WeaponType
    {
        None,
        Fists,
        Sword,
        Axe,
        Spear,
        Staff
    }

    public enum SkillTarget
    {
        Self,
        Enemy
    }

    public enum SkillActionType
    {
        DoDamage,
        IncreaseDamage,
        WeakenEnemy,
        GainCharges,
        ApplyCharges
    }

    public enum ChargeType
    {
        None,
        Poison,
        Burn,
        Drowning,
        Shock,
        Splinter,
        Crush,
        WindShear,
        Slow,
        Haste,
        LifeSteal,
        Counterattack,
        Empowered,
        Weakened,
        Regeneration,
        Fortified,
        Vulnerable,
        Bleed,
        Thorns,
        Caltrops,
        Recoil,
        FireFortified,
        WaterFortified,
        ElectricityFortified,
        PoisonFortified,
        EarthFortified,
        AirFortified,
        WoodFortified,
        Dizzle
    }
}
