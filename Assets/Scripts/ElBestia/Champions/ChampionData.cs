using System;
using ElBestia.Lore;
using ElBestia.Perks;
using ElBestia.Skills;

namespace ElBestia.Champions
{
    [Serializable]
    public sealed class ChampionData
    {
        public string championId;
        public string championName;
        public string playerNickname;
        public int level = 1;
        public int currentExperience;
        public int experienceToNextLevel;
        public int generationSeed;
        public ChampionAppearance appearance;
        public ChampionLoreProfile lore;
        public ChampionStats baseStats;
        public ChampionStats stats;
        public ChampionGrowthProfile growth;
        public WeaponType equippedWeapon;
        public PerkSO[] perks = Array.Empty<PerkSO>();
        public SkillData baseSkill;
        public SkillData[] skills = Array.Empty<SkillData>();

        public string ChampionId => championId;
        public string ChampionName => championName;
        public string PlayerNickname => playerNickname;
        public int Level => level;
        public int CurrentExperience => currentExperience;
        public int ExperienceToNextLevel => experienceToNextLevel;
        public ChampionAppearance Appearance => appearance;
        public ChampionLoreProfile Lore => lore;
        public ChampionStats BaseStats => baseStats;
        public ChampionStats Stats => stats;
        public ChampionGrowthProfile Growth => growth;
        public WeaponType EquippedWeapon => equippedWeapon;
        public PerkSO[] Perks => perks;
        public SkillData BaseSkill => baseSkill;
        public SkillData[] Skills => skills;
        public GrowthGrade OverallGrowthGrade => growth != null ? growth.GetOverallGrade() : GrowthGrade.F;

        public ChampionData Clone()
        {
            return new ChampionData
            {
                championId = championId,
                championName = championName,
                playerNickname = playerNickname,
                level = level,
                currentExperience = currentExperience,
                experienceToNextLevel = experienceToNextLevel,
                generationSeed = generationSeed,
                appearance = appearance,
                lore = lore,
                baseStats = baseStats != null ? baseStats.Clone() : null,
                stats = stats != null ? stats.Clone() : null,
                growth = growth,
                equippedWeapon = equippedWeapon,
                perks = perks != null ? (PerkSO[])perks.Clone() : Array.Empty<PerkSO>(),
                baseSkill = baseSkill,
                skills = skills != null ? (SkillData[])skills.Clone() : Array.Empty<SkillData>()
            };
        }
    }
}
