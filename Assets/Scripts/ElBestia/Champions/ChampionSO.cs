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
        public ChampionGrowthProfile Growth => growth;
        public ChampionLoreProfile Lore => lore;
        public GrowthGrade OverallGrowthGrade => growth != null ? growth.GetOverallGrade() : GrowthGrade.F;
        public WeaponType EquippedWeapon => equippedWeapon;
        public PerkSO[] Perks => perks;
        public SkillData BaseSkill => baseSkill;
        public SkillData[] Skills => skills;

        public void CreateRandomCharacter()
        {
            seed = UnityEngine.Random.Range(1, int.MaxValue);
            CreateRandomCharacter(seed);
        }

        public void CreateRandomCharacter(int generationSeed)
        {
            seed = generationSeed;
            level = Mathf.Max(1, level);

            var rng = new System.Random(seed);
            int baseSeed = rng.Next(1, int.MaxValue);
            int perksSeed = rng.Next(1, int.MaxValue);
            int skillsSeed = rng.Next(1, int.MaxValue);
            int equipmentSeed = rng.Next(1, int.MaxValue);

            championId = $"{ToBase36(baseSeed)}-{ToBase36(perksSeed)}-{ToBase36(skillsSeed)}-{ToBase36(equipmentSeed)}";
            appearance = ChampionAppearance.CreateRandom(rng);

            NameGeneratorSO generator = nameGenerator != null ? nameGenerator : NameGeneratorSO.LoadDefault();
            championName = generator != null
                ? generator.GenerateName(appearance.sex, rng)
                : CreateFallbackName(appearance.sex, rng);
            playerNickname = string.Empty;

            baseStats = ChampionStats.CreateLevelOne(rng, totalBaseStatPoints);
            lore = ChampionLoreGenerator.CreateRandom(championName, rng);
            growth = ChampionGrowthProfile.CreateRandom(rng);
            currentExperience = 0;
            experienceToNextLevel = CalculateExperienceToNextLevel(level, growth);
            equippedWeapon = RollStartingWeapon(rng);
            perks = PerkGenerator.RollStartingPerks(rng);
            stats = CalculateFinalStats(baseStats, lore, perks);
            baseSkill = RandomSkillFactory.CreateBaseSkill(equippedWeapon, rng);
            skills = RandomSkillFactory.CreateEquippedElaborateSkills(level, equippedWeapon, rng);
        }

        public void ReapplyPerksAndProgression()
        {
            level = Mathf.Max(1, level);
            if (baseStats == null)
            {
                baseStats = stats != null ? stats.Clone() : ChampionStats.CreateLevelOne(new System.Random(Mathf.Max(1, seed)), totalBaseStatPoints);
            }

            stats = CalculateFinalStats(baseStats, lore, perks);
            experienceToNextLevel = CalculateExperienceToNextLevel(level, growth);
            currentExperience = Mathf.Clamp(currentExperience, 0, Mathf.Max(0, experienceToNextLevel - 1));
        }

        public static int CalculateExperienceToNextLevel(int championLevel, ChampionGrowthProfile growthProfile)
        {
            int safeLevel = Mathf.Max(1, championLevel);
            GrowthGrade grade = growthProfile != null ? growthProfile.GetOverallGrade() : GrowthGrade.F;
            float baseCost = 80f + Mathf.Pow(safeLevel, 1.35f) * 34f;
            float latePressure = 1f - Mathf.Exp(-(safeLevel - 1f) / 35f);
            float gradeFactor = Mathf.Lerp(1f, GetLateGameXpFactor(grade), latePressure);
            return Mathf.Max(1, Mathf.RoundToInt(baseCost * gradeFactor));
        }

        public ChampionStatFormula GetBaseStatFormula(ChampionStatType stat)
        {
            ChampionStats safeBase = baseStats ?? stats;
            int baseValue = safeBase != null ? GetBaseStatValue(safeBase, stat) : 0;
            int finalValue = stats != null ? GetBaseStatValue(stats, stat) : baseValue;
            BuildBaseStatModifiers(lore, perks, out int[] additives, out float[] multipliers);
            int index = (int)stat;

            return new ChampionStatFormula
            {
                baseValue = baseValue,
                additive = additives[index],
                multiplier = multipliers[index],
                finalValue = finalValue
            };
        }

        private static ChampionStats CalculateFinalStats(ChampionStats rawBaseStats, ChampionLoreProfile championLore, PerkSO[] championPerks)
        {
            if (rawBaseStats == null)
            {
                return null;
            }

            ChampionStats finalStats = rawBaseStats.Clone();
            BuildBaseStatModifiers(championLore, championPerks, out int[] additives, out float[] multipliers);

            for (int i = 0; i < 5; i++)
            {
                ChampionStatType stat = (ChampionStatType)i;
                int baseValue = GetBaseStatValue(rawBaseStats, stat);
                int modifiedValue = Mathf.RoundToInt((baseValue + additives[i]) * multipliers[i]);
                SetBaseStatValue(finalStats, stat, Mathf.Max(0, modifiedValue));
            }

            finalStats.RecalculateDerivedResources();
            ApplyLoreDerivedModifiers(finalStats, championLore);
            ApplyPerkDerivedModifiers(finalStats, championPerks);
            finalStats.RefreshDerivedCombatTotals();
            return finalStats;
        }

        private static void BuildBaseStatModifiers(ChampionLoreProfile championLore, PerkSO[] championPerks, out int[] additives, out float[] multipliers)
        {
            additives = new int[5];
            multipliers = new[] { 1f, 1f, 1f, 1f, 1f };

            AddLoreBaseModifiers(championLore != null ? championLore.birth : null, additives, multipliers);
            AddLoreBaseModifiers(championLore != null ? championLore.childhood : null, additives, multipliers);
            AddLoreBaseModifiers(championLore != null ? championLore.youth : null, additives, multipliers);

            if (championPerks == null)
            {
                return;
            }

            foreach (PerkSO perk in championPerks)
            {
                if (perk == null)
                {
                    continue;
                }

                int index = (int)perk.Stat;
                if (perk.EffectType == PerkEffectType.BaseStatBonus)
                {
                    additives[index] += perk.FlatValue;
                }
                else if (perk.EffectType == PerkEffectType.BaseStatMultiplier)
                {
                    multipliers[index] *= SanitizeMultiplier(perk.Multiplier);
                }
            }
        }

        private static void AddLoreBaseModifiers(ChampionLoreEntrySO entry, int[] additives, float[] multipliers)
        {
            if (entry == null || entry.Modifiers == null)
            {
                return;
            }

            foreach (ChampionLoreModifier modifier in entry.Modifiers)
            {
                if (modifier == null)
                {
                    continue;
                }

                int index = (int)modifier.stat;
                if (modifier.type == ChampionLoreModifierType.BaseStat)
                {
                    additives[index] += modifier.value;
                }
                else if (modifier.type == ChampionLoreModifierType.BaseStatMultiplier)
                {
                    multipliers[index] *= SanitizeMultiplier(modifier.multiplier);
                }
            }
        }

        private static void ApplyLoreDerivedModifiers(ChampionStats championStats, ChampionLoreProfile championLore)
        {
            if (championStats == null || championLore == null)
            {
                return;
            }

            ApplyLoreDerivedModifiers(championStats, championLore.birth);
            ApplyLoreDerivedModifiers(championStats, championLore.childhood);
            ApplyLoreDerivedModifiers(championStats, championLore.youth);
        }

        private static void ApplyLoreDerivedModifiers(ChampionStats championStats, ChampionLoreEntrySO entry)
        {
            if (entry == null || entry.Modifiers == null)
            {
                return;
            }

            foreach (ChampionLoreModifier modifier in entry.Modifiers)
            {
                if (modifier == null)
                {
                    continue;
                }

                if (modifier.type == ChampionLoreModifierType.CombatStat)
                {
                    ApplyCombatStatBonus(championStats, modifier.combatStat, modifier.value);
                }
                else if (modifier.type == ChampionLoreModifierType.WeaponProficiency)
                {
                    ApplyWeaponProficiency(championStats, modifier.weapon, modifier.value);
                }
                else if (modifier.type == ChampionLoreModifierType.ElementalResistance)
                {
                    ApplyElementalResistanceBonus(championStats, modifier.element, modifier.value);
                }
            }
        }

        private static void ApplyPerkDerivedModifiers(ChampionStats championStats, PerkSO[] championPerks)
        {
            if (championStats == null || championPerks == null)
            {
                return;
            }

            foreach (PerkSO perk in championPerks)
            {
                if (perk == null)
                {
                    continue;
                }

                if (perk.EffectType == PerkEffectType.WeaponProficiency)
                {
                    ApplyWeaponProficiency(championStats, perk.Weapon, perk.FlatValue);
                }
                else if (perk.EffectType == PerkEffectType.MaxLifeBonus)
                {
                    championStats.life += perk.FlatValue;
                }
                else if (perk.EffectType == PerkEffectType.ExperienceGain)
                {
                    championStats.experienceGain += perk.FlatValue;
                }
                else if (perk.EffectType == PerkEffectType.FatalInjuryResistance)
                {
                    championStats.fatalInjuryResistance += perk.FlatValue;
                }
                else if (perk.EffectType == PerkEffectType.InjurySeverityReduction)
                {
                    championStats.injurySeverityReduction += perk.FlatValue;
                }
                else if (perk.EffectType == PerkEffectType.CombatStatBonus)
                {
                    ApplyCombatStatBonus(championStats, perk.CombatStat, perk.FlatValue);
                }
                else if (perk.EffectType == PerkEffectType.ElementalResistance)
                {
                    ApplyElementalResistanceBonus(championStats, perk.Element, perk.FlatValue);
                }
                else if (perk.EffectType == PerkEffectType.ElementalDamageBonus)
                {
                    championStats.AddElementalDamageBonus(perk.Element, perk.FlatValue);
                }
                else if (perk.EffectType == PerkEffectType.ElementalDamageMultiplier)
                {
                    championStats.MultiplyElementalDamage(perk.Element, perk.Multiplier);
                }
            }
        }

        private static int GetBaseStatValue(ChampionStats championStats, ChampionStatType stat)
        {
            switch (stat)
            {
                case ChampionStatType.Strength:
                    return championStats.strength;
                case ChampionStatType.Agility:
                    return championStats.agility;
                case ChampionStatType.Constitution:
                    return championStats.constitution;
                case ChampionStatType.Intelligence:
                    return championStats.intelligence;
                case ChampionStatType.Endurance:
                    return championStats.endurance;
                default:
                    return 0;
            }
        }

        private static void SetBaseStatValue(ChampionStats championStats, ChampionStatType stat, int value)
        {
            switch (stat)
            {
                case ChampionStatType.Strength:
                    championStats.strength = value;
                    break;
                case ChampionStatType.Agility:
                    championStats.agility = value;
                    break;
                case ChampionStatType.Constitution:
                    championStats.constitution = value;
                    break;
                case ChampionStatType.Intelligence:
                    championStats.intelligence = value;
                    break;
                case ChampionStatType.Endurance:
                    championStats.endurance = value;
                    break;
            }
        }

        private static float SanitizeMultiplier(float multiplier)
        {
            return multiplier > 0f ? multiplier : 1f;
        }

        private static void ApplyCombatStatBonus(ChampionStats championStats, PerkCombatStatType combatStat, int value)
        {
            switch (combatStat)
            {
                case PerkCombatStatType.DodgeRating:
                    championStats.dodge += value;
                    break;
                case PerkCombatStatType.BlockRating:
                    championStats.block += value;
                    break;
                case PerkCombatStatType.CriticalRating:
                    championStats.critical += value;
                    break;
                case PerkCombatStatType.HitRating:
                    championStats.hit += value;
                    break;
                case PerkCombatStatType.Recovery:
                    championStats.recovery += value;
                    break;
                case PerkCombatStatType.ExperienceGain:
                    championStats.experienceGain += value;
                    break;
                case PerkCombatStatType.FatalInjuryResistance:
                    championStats.fatalInjuryResistance += value;
                    break;
                case PerkCombatStatType.InjurySeverityReduction:
                    championStats.injurySeverityReduction += value;
                    break;
                case PerkCombatStatType.Life:
                    championStats.life += value;
                    break;
                case PerkCombatStatType.Energy:
                    championStats.energy += value;
                    break;
            }
        }

        private static void ApplyElementalResistanceBonus(ChampionStats championStats, SkillElement element, int value)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    championStats.fireResistance += value;
                    break;
                case SkillElement.Water:
                    championStats.waterResistance += value;
                    break;
                case SkillElement.Electricity:
                    championStats.electricityResistance += value;
                    break;
                case SkillElement.Poison:
                    championStats.poisonResistance += value;
                    break;
                case SkillElement.Earth:
                    championStats.earthResistance += value;
                    break;
                case SkillElement.Air:
                    championStats.airResistance += value;
                    break;
                case SkillElement.Wood:
                    championStats.woodResistance += value;
                    break;
            }
        }

        private static void ApplyWeaponProficiency(ChampionStats championStats, WeaponType weapon, int value)
        {
            switch (weapon)
            {
                case WeaponType.Fists:
                    championStats.fistsProficiency += value;
                    break;
                case WeaponType.Sword:
                    championStats.swordProficiency += value;
                    break;
                case WeaponType.Axe:
                    championStats.axeProficiency += value;
                    break;
                case WeaponType.Spear:
                    championStats.spearProficiency += value;
                    break;
                case WeaponType.Staff:
                    championStats.staffProficiency += value;
                    break;
                case WeaponType.Bow:
                    championStats.bowProficiency += value;
                    break;
            }
        }

        private static WeaponType RollStartingWeapon(System.Random rng)
        {
            WeaponType[] weapons =
            {
                WeaponType.Fists,
                WeaponType.Sword,
                WeaponType.Axe,
                WeaponType.Spear,
                WeaponType.Staff,
                WeaponType.Bow
            };

            return weapons[rng.Next(0, weapons.Length)];
        }

        private static float GetLateGameXpFactor(GrowthGrade grade)
        {
            switch (grade)
            {
                case GrowthGrade.F:
                    return 1.35f;
                case GrowthGrade.E:
                    return 1.2f;
                case GrowthGrade.D:
                    return 1.07f;
                case GrowthGrade.C:
                    return 1f;
                case GrowthGrade.B:
                    return 0.82f;
                case GrowthGrade.A:
                    return 0.58f;
                case GrowthGrade.S:
                    return 0.28f;
                case GrowthGrade.SS:
                    return 0.08f;
                default:
                    return 1f;
            }
        }

        private static string CreateFallbackName(ChampionSex sex, System.Random rng)
        {
            string[] firstNames = { "Rodrigo", "Daniel", "Dionisio", "Ruben", "Isa", "Nieves", "Ana", "Lucia", "Afro", "Guillermo" };
            string[] prefixes = { "shadow", "blind", "hand", "hearth", "fire", "iron", "blood", "storm" };
            string[] suffixes = { "force", "man", "less", "cock", "blade", "fist", "breaker", "runner" };

            string first = firstNames[rng.Next(0, firstNames.Length)];
            string last = Capitalize(prefixes[rng.Next(0, prefixes.Length)] + suffixes[rng.Next(0, suffixes.Length)]);
            return $"{first} {last}";
        }

        private static string Capitalize(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            if (value.Length == 1)
            {
                return value.ToUpperInvariant();
            }

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        private static string ToBase36(int value)
        {
            const string digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (value == 0)
            {
                return "0";
            }

            int current = Mathf.Abs(value);
            string result = string.Empty;
            while (current > 0)
            {
                result = digits[current % 36] + result;
                current /= 36;
            }

            return result;
        }
    }
}
