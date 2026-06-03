using ElBestia.Generation;
using ElBestia.Lore;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Champions
{
    [CreateAssetMenu(menuName = "El Bestia/Champion", fileName = "Champion")]
    public sealed class ChampionSO : ScriptableObject
    {
        [Header("Generation")]
        [SerializeField] private NameGeneratorSO nameGenerator;
        [SerializeField] private int level = 1;
        [SerializeField] private int totalBaseStatPoints = 50;
        [SerializeField] private int seed;

        [Header("Generated Data")]
        [SerializeField] private string championId;
        [SerializeField] private string championName;
        [SerializeField] private string playerNickname;
        [SerializeField] private ChampionAppearance appearance;
        [SerializeField] private ChampionLoreProfile lore;
        [SerializeField] private ChampionStats baseStats;
        [SerializeField] private ChampionStats stats;
        [SerializeField] private ChampionGrowthProfile growth;
        [SerializeField] private int currentExperience;
        [SerializeField] private int experienceToNextLevel;
        [SerializeField] private WeaponType equippedWeapon;
        [SerializeField] private PerkSO[] perks = new PerkSO[0];
        [SerializeField] private SkillData baseSkill;
        [SerializeField] private SkillData[] skills = new SkillData[0];

        public string ChampionId => championId;
        public string ChampionName => championName;
        public string PlayerNickname => playerNickname;
        public int Level => level;
        public int CurrentExperience => currentExperience;
        public int ExperienceToNextLevel => experienceToNextLevel;
        public ChampionStats BaseStats => baseStats;
        public ChampionStats Stats => stats;
        public ChampionAppearance Appearance => appearance;
        public ChampionGrowthProfile Growth => growth;
        public ChampionLoreProfile Lore => lore;
        public GrowthGrade OverallGrowthGrade => growth != null ? growth.GetOverallGrade() : GrowthGrade.F;
        public WeaponType EquippedWeapon => equippedWeapon;
        public PerkSO[] Perks => perks;
        public SkillData BaseSkill => baseSkill;
        public SkillData[] Skills => skills;

        public void CreateRandomCharacter()
        {
            seed = Random.Range(1, int.MaxValue);
            CreateRandomCharacter(seed);
        }

        public void CreateRandomCharacter(int generationSeed)
        {
            seed = generationSeed;
            ApplyData(ChampionDataFactory.CreateRandom(seed, level, totalBaseStatPoints, nameGenerator));
        }

        public void ReapplyPerksAndProgression()
        {
            ChampionData data = ToData();
            ChampionDataFactory.RecalculateFinalStats(data, totalBaseStatPoints);
            ApplyData(data);
        }

        public static int CalculateExperienceToNextLevel(int championLevel, ChampionGrowthProfile growthProfile)
        {
            return ChampionDataFactory.CalculateExperienceToNextLevel(championLevel, growthProfile);
        }

        public ChampionStatFormula GetBaseStatFormula(ChampionStatType stat)
        {
            return ChampionDataFactory.GetBaseStatFormula(ToData(), stat);
        }

        public ChampionData ToData()
        {
            return new ChampionData
            {
                championId = championId,
                championName = championName,
                playerNickname = playerNickname,
                level = level,
                currentExperience = currentExperience,
                experienceToNextLevel = experienceToNextLevel,
                generationSeed = seed,
                appearance = appearance,
                lore = lore,
                baseStats = baseStats != null ? baseStats.Clone() : null,
                stats = stats != null ? stats.Clone() : null,
                growth = growth,
                equippedWeapon = equippedWeapon,
                perks = perks != null ? (PerkSO[])perks.Clone() : new PerkSO[0],
                baseSkill = baseSkill,
                skills = skills != null ? (SkillData[])skills.Clone() : new SkillData[0]
            };
        }

        public void ApplyData(ChampionData data)
        {
            if (data == null)
            {
                return;
            }

            championId = data.championId;
            championName = data.championName;
            playerNickname = data.playerNickname;
            level = Mathf.Max(1, data.level);
            currentExperience = data.currentExperience;
            experienceToNextLevel = data.experienceToNextLevel;
            seed = data.generationSeed;
            appearance = data.appearance;
            lore = data.lore;
            baseStats = data.baseStats != null ? data.baseStats.Clone() : null;
            stats = data.stats != null ? data.stats.Clone() : null;
            growth = data.growth;
            equippedWeapon = data.equippedWeapon;
            perks = data.perks != null ? (PerkSO[])data.perks.Clone() : new PerkSO[0];
            baseSkill = data.baseSkill;
            skills = data.skills != null ? (SkillData[])data.skills.Clone() : new SkillData[0];
        }
    }
}
