using ElBestia.Generation;
using ElBestia.Visuals;
using ElBestia.Champions;
using ElBestia.Lore;
using ElBestia.Perks;
using ElBestia.Skills;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    public static class DefaultDataAssetCreator
    {
        [MenuItem("Tools/El Bestia/Create Default Data Assets")]
        public static void CreateDefaultDataAssets()
        {
            EnsureAsset<NameGeneratorSO>("Assets/Resources/Generators/ChampionNameGenerator.asset");
            EnsureAsset<HairSO>("Assets/Resources/CharacterParts/HairSO.asset");
            EnsureAsset<HeadSO>("Assets/Resources/CharacterParts/HeadSO.asset");
            EnsureAsset<TorsoSO>("Assets/Resources/CharacterParts/TorsoSO.asset");
            EnsureAsset<ArmsSO>("Assets/Resources/CharacterParts/ArmsSO.asset");
            EnsureAsset<LegsSO>("Assets/Resources/CharacterParts/LegsSO.asset");
            EnsureAsset<FeetSO>("Assets/Resources/CharacterParts/FeetSO.asset");
            EnsureDefaultPerks();
            EnsureDefaultLoreEntries();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("El Bestia default data assets are ready.");
        }

        [MenuItem("Tools/El Bestia/Rebuild Default Perks")]
        public static void RebuildDefaultPerks()
        {
            EnsureDefaultPerks();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("El Bestia default perks are ready.");
        }

        [MenuItem("Tools/El Bestia/Validate Default Perks")]
        public static void ValidateDefaultPerks()
        {
            PerkSO[] perks = Resources.LoadAll<PerkSO>("Perks");
            int issues = 0;

            foreach (PerkSO perk in perks)
            {
                if (perk == null)
                {
                    continue;
                }

                if (perk.EffectType == PerkEffectType.WeaponProficiency && perk.Weapon == WeaponType.None)
                {
                    Debug.LogError($"Perk {perk.PerkId} is WeaponProficiency but has no weapon.", perk);
                    issues++;
                }

                if (perk.EffectType == PerkEffectType.CombatStatBonus && perk.CombatStat == PerkCombatStatType.None)
                {
                    Debug.LogError($"Perk {perk.PerkId} is CombatStatBonus but has no combat stat.", perk);
                    issues++;
                }

                if ((perk.EffectType == PerkEffectType.ElementalResistance
                        || perk.EffectType == PerkEffectType.ElementalDamageBonus
                        || perk.EffectType == PerkEffectType.ElementalDamageMultiplier)
                    && perk.Element == SkillElement.None)
                {
                    Debug.LogError($"Perk {perk.PerkId} is {perk.EffectType} but has no element.", perk);
                    issues++;
                }

                if (perk.IsUnique && string.IsNullOrEmpty(perk.UniqueGroup))
                {
                    Debug.LogError($"Perk {perk.PerkId} is unique but has no unique group.", perk);
                    issues++;
                }
            }

            if (issues == 0)
            {
                Debug.Log($"El Bestia perk validation passed. Checked {perks.Length} perks.");
            }
            else
            {
                Debug.LogError($"El Bestia perk validation found {issues} issue(s).");
            }
        }

        [MenuItem("Tools/El Bestia/Rebuild Default Lore")]
        public static void RebuildDefaultLore()
        {
            EnsureDefaultLoreEntries();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("El Bestia default lore entries are ready.");
        }

        [MenuItem("Tools/El Bestia/Create Sample Champion")]
        public static void CreateSampleChampion()
        {
            CreateDefaultDataAssets();

            const string path = "Assets/Generated/SampleChampion.asset";
            AssetDatabase.DeleteAsset(path);
            var champion = ScriptableObject.CreateInstance<ChampionSO>();
            AssetDatabase.CreateAsset(champion, path);

            champion.CreateRandomCharacter();
            EditorUtility.SetDirty(champion);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Sample champion generated at {path}.");
        }

        private static void EnsureAsset<T>(string path) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing != null)
            {
                if (existing is NameGeneratorSO nameGenerator)
                {
                    nameGenerator.EnsureMinimumDefaults();
                    EditorUtility.SetDirty(nameGenerator);
                }

                return;
            }

            var asset = ScriptableObject.CreateInstance<T>();
            if (asset is NameGeneratorSO createdNameGenerator)
            {
                createdNameGenerator.EnsureMinimumDefaults();
            }

            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
        }

        private static void EnsureDefaultPerks()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Perks");

            var specs = BuildDefaultPerkSpecs();
            foreach (PerkSpec spec in specs)
            {
                string path = $"Assets/Resources/Perks/{Sanitize(spec.id)}.asset";
                PerkSO perk = AssetDatabase.LoadAssetAtPath<PerkSO>(path);
                if (perk == null)
                {
                    perk = ScriptableObject.CreateInstance<PerkSO>();
                    AssetDatabase.CreateAsset(perk, path);
                }

                perk.Configure(
                    spec.id,
                    spec.name,
                    spec.rarity,
                    spec.description,
                    spec.effectType,
                    spec.combatStat,
                    spec.stat,
                    spec.weapon,
                    spec.element,
                    spec.charge,
                    spec.flatValue,
                    spec.multiplier,
                    spec.chance,
                    spec.isUnique,
                    spec.uniqueGroup);

                EditorUtility.SetDirty(perk);
            }
        }

        private static void EnsureDefaultLoreEntries()
        {
            EnsureFolder("Assets/Resources");
            EnsureFolder("Assets/Resources/Lore");
            EnsureFolder("Assets/Resources/Lore/Birth");
            EnsureFolder("Assets/Resources/Lore/Childhood");
            EnsureFolder("Assets/Resources/Lore/Youth");

            CreateLoreEntries(BuildLoreSpecs(ChampionLoreStage.Birth), "Assets/Resources/Lore/Birth");
            CreateLoreEntries(BuildLoreSpecs(ChampionLoreStage.Childhood), "Assets/Resources/Lore/Childhood");
            CreateLoreEntries(BuildLoreSpecs(ChampionLoreStage.Youth), "Assets/Resources/Lore/Youth");
        }

        private static void CreateLoreEntries(List<LoreSpec> specs, string folder)
        {
            foreach (LoreSpec spec in specs)
            {
                string path = $"{folder}/{Sanitize(spec.id)}.asset";
                ChampionLoreEntrySO entry = AssetDatabase.LoadAssetAtPath<ChampionLoreEntrySO>(path);
                if (entry == null)
                {
                    entry = ScriptableObject.CreateInstance<ChampionLoreEntrySO>();
                    AssetDatabase.CreateAsset(entry, path);
                }

                entry.Configure(spec.id, spec.stage, spec.title, spec.fragment, spec.modifiers);
                EditorUtility.SetDirty(entry);
            }
        }

        private static List<LoreSpec> BuildLoreSpecs(ChampionLoreStage stage)
        {
            var specs = new List<LoreSpec>(200);
            string[] anchors = GetLoreAnchors(stage);
            string[] tones = GetLoreTones(stage);

            for (int i = 0; i < anchors.Length && specs.Count < 200; i++)
            {
                for (int j = 0; j < tones.Length && specs.Count < 200; j++)
                {
                    string id = $"{stage.ToString().ToLowerInvariant()}_{i:00}_{j:00}";
                    string title = $"{tones[j]} {anchors[i]}";
                    string fragment = BuildLoreFragment(stage, anchors[i], tones[j]);
                    ChampionLoreModifier[] modifiers = BuildLoreModifiers(i, j);
                    specs.Add(new LoreSpec(id, stage, title, fragment, modifiers));
                }
            }

            return specs;
        }

        private static string[] GetLoreAnchors(ChampionLoreStage stage)
        {
            switch (stage)
            {
                case ChampionLoreStage.Birth:
                    return new[]
                    {
                        "Sword Family", "Poor Family", "Desert Birth", "Mountain Clan", "River House",
                        "Dojo Lineage", "Mercenary Camp", "Temple Step", "Fishing Village", "Forge Quarter",
                        "Nomad Caravan", "Noble House", "Prison Town", "Storm Coast", "Woodcutters",
                        "Street Clinic", "Arena District", "Monk Refuge", "Sailor Blood", "Border Farm"
                    };
                case ChampionLoreStage.Childhood:
                    return new[]
                    {
                        "Orphanage", "Wild Child", "Good School", "Street Gang", "Stable Work",
                        "Kitchen Yard", "Mine Tunnels", "Library Dust", "Fisher Nets", "Temple Bells",
                        "Market Runner", "Butcher Block", "Rooftop Games", "Old Hospital", "Burned Hamlet",
                        "Winter Road", "Debt House", "Circus Tent", "Hidden Valley", "Training Hall"
                    };
                default:
                    return new[]
                    {
                        "Army Service", "Teacher Years", "Mechanic Shop", "Plumber Work", "Dock Labor",
                        "Arena Debut", "Monastery Trial", "Bandit Season", "Merchant Guard", "Courier Route",
                        "Blacksmith Helper", "Hunter Lodge", "Sailor Contract", "Field Medic", "Quarry Crew",
                        "Duelist Circle", "Scholar Job", "Street Performer", "Bodyguard Work", "Dojo Assistant"
                    };
            }
        }

        private static string[] GetLoreTones(ChampionLoreStage stage)
        {
            return new[]
            {
                "Hard", "Quiet", "Lucky", "Hungry", "Bright", "Cruel", "Patient", "Restless", "Broken", "Proud"
            };
        }

        private static string BuildLoreFragment(ChampionLoreStage stage, string anchor, string tone)
        {
            string place = DescribeLoreAnchor(anchor);
            string tonePhrase = DescribeLoreTone(tone);
            int variant = Mathf.Abs((anchor.GetHashCode() * 31 + tone.GetHashCode() + (int)stage * 17) % 8);

            switch (stage)
            {
                case ChampionLoreStage.Birth:
                    switch (variant)
                    {
                        case 0:
                            return $"{{name}} was born in {place}, and {tonePhrase} shaped ordinary life there.";
                        case 1:
                            return $"The first stories about {{name}} came from {place}, remembered locally for {tonePhrase}.";
                        case 2:
                            return $"Before anyone called {{name}} a fighter, life in {place} had already taught the family the meaning of {tonePhrase}.";
                        case 3:
                            return $"{{name}} entered the world in {place}, surrounded by people who measured children through {tonePhrase}.";
                        case 4:
                            return $"The household that raised {{name}} belonged to {place}, and its oldest inheritance was {tonePhrase}.";
                        case 5:
                            return $"In {place}, {{name}}'s birth was not considered special until rumors of {tonePhrase} began to follow it.";
                        case 6:
                            return $"{{name}} was born under the customs of {place}, where even a cradle could carry {tonePhrase}.";
                        default:
                            return $"The name {{name}} first appeared in the records of {place}, beside a note about {tonePhrase}.";
                    }
                case ChampionLoreStage.Childhood:
                    switch (variant)
                    {
                        case 0:
                            return $"As a child, {{name}} spent long years around {place}, learning to turn {tonePhrase} into a survival tool.";
                        case 1:
                            return $"By twelve, {{name}} had drifted into {place}, a life where {tonePhrase} mattered more than comfort.";
                        case 2:
                            return $"Childhood pulled {{name}} through {place}; the lesson that stayed was {tonePhrase}.";
                        case 3:
                            return $"The people of {place} remember a young {{name}} as someone shaped less by play than by {tonePhrase}.";
                        case 4:
                            return $"When other children learned games, {{name}} learned the rules of {place} and the cost of {tonePhrase}.";
                        case 5:
                            return $"{{name}}'s childhood turned around {place}, a rough education that made {tonePhrase} feel natural.";
                        case 6:
                            return $"The safest corner of {{name}}'s youth was {place}, though even there {tonePhrase} kept knocking.";
                        default:
                            return $"Years in {place} gave {{name}} habits that looked strange to outsiders and useful to anyone who understood {tonePhrase}.";
                    }
                default:
                    switch (variant)
                    {
                        case 0:
                            return $"At adulthood, {{name}} entered {place}, trading old fears for work, violence, and {tonePhrase}.";
                        case 1:
                            return $"Youth ended when {{name}} joined {place}, where duty sharpened {tonePhrase} into a practical weapon.";
                        case 2:
                            return $"In {place}, {{name}} learned that a future could be bought with bruises, discipline, and {tonePhrase}.";
                        case 3:
                            return $"The next chapter took {{name}} to {place}, a place that rewarded {tonePhrase} as long as it could win fights.";
                        case 4:
                            return $"When {{name}} came of age, {place} offered food, coin, and enough danger to give {tonePhrase} direction.";
                        case 5:
                            return $"The work at {place} did not make {{name}} gentle; it made {tonePhrase} easier to justify.";
                        case 6:
                            return $"Among the people of {place}, {{name}} learned a trade, a stance, and the value of {tonePhrase}.";
                        default:
                            return $"By the end of youth, {{name}} had passed through {place} and carried {tonePhrase} like a second name.";
                    }
            }
        }

        private static string DescribeLoreAnchor(string anchor)
        {
            switch (anchor)
            {
                case "Sword Family":
                    return "a family of sword tutors";
                case "Poor Family":
                    return "a poor household that counted every meal";
                case "Desert Birth":
                    return "a settlement pressed against the desert";
                case "Mountain Clan":
                    return "a mountain clan of stubborn walkers";
                case "River House":
                    return "a river house that lived by floods and ferries";
                case "Dojo Lineage":
                    return "an old dojo lineage with more pride than money";
                case "Mercenary Camp":
                    return "a mercenary camp where lullabies sounded like marching songs";
                case "Temple Step":
                    return "the steps of a severe hill temple";
                case "Fishing Village":
                    return "a humble fishing village where hunger arrived with the tide";
                case "Forge Quarter":
                    return "a forge quarter blackened by smoke and debt";
                case "Nomad Caravan":
                    return "a nomad caravan that never slept under the same roof twice";
                case "Noble House":
                    return "a noble house full of manners and hidden knives";
                case "Prison Town":
                    return "a prison town built around locked gates";
                case "Storm Coast":
                    return "a storm coast that taught children to fear the sky";
                case "Woodcutters":
                    return "a family of woodcutters who trusted axes before promises";
                case "Street Clinic":
                    return "a street clinic where pain was ordinary";
                case "Arena District":
                    return "an arena district that cheered before it pitied";
                case "Monk Refuge":
                    return "a monk refuge where silence was a rule";
                case "Sailor Blood":
                    return "a sailor family with salt in every story";
                case "Border Farm":
                    return "a border farm where raids were part of the seasons";
                case "Orphanage":
                    return "an overcrowded orphanage";
                case "Wild Child":
                    return "the wild edges beyond the village";
                case "Good School":
                    return "a strict but generous school";
                case "Street Gang":
                    return "a street gang that rewarded quick hands";
                case "Stable Work":
                    return "the stables behind a trading road";
                case "Kitchen Yard":
                    return "a kitchen yard full of heat, scraps, and shouted orders";
                case "Mine Tunnels":
                    return "mine tunnels where daylight became a rumor";
                case "Library Dust":
                    return "dusty library rooms watched by tired scholars";
                case "Fisher Nets":
                    return "fisher nets, cold mornings, and torn palms";
                case "Temple Bells":
                    return "temple bells and endless chores";
                case "Market Runner":
                    return "market alleys where every errand became a race";
                case "Butcher Block":
                    return "a butcher's block where blood was just work";
                case "Rooftop Games":
                    return "rooftops where children ran above adult trouble";
                case "Old Hospital":
                    return "an old hospital that smelled of herbs and fear";
                case "Burned Hamlet":
                    return "a burned hamlet that never stopped counting losses";
                case "Winter Road":
                    return "a winter road with no patience for weakness";
                case "Debt House":
                    return "a debt house where promises had teeth";
                case "Circus Tent":
                    return "a circus tent full of tricks and bruises";
                case "Hidden Valley":
                    return "a hidden valley that distrusted outsiders";
                case "Training Hall":
                    return "a training hall where children swept before they struck";
                case "Army Service":
                    return "army service under officers who loved drills";
                case "Teacher Years":
                    return "years spent teaching slower hands";
                case "Mechanic Shop":
                    return "a mechanic shop that fixed wheels, hinges, and sometimes bones";
                case "Plumber Work":
                    return "plumber work in narrow streets and flooded cellars";
                case "Dock Labor":
                    return "dock labor among ropes, crates, and cheap fights";
                case "Arena Debut":
                    return "a first arena circuit with more blood than applause";
                case "Monastery Trial":
                    return "a monastery trial designed to break vanity";
                case "Bandit Season":
                    return "a bandit season where trust was expensive";
                case "Merchant Guard":
                    return "a merchant guard escorting caravans through bad roads";
                case "Courier Route":
                    return "a courier route where speed meant dinner";
                case "Blacksmith Helper":
                    return "a blacksmith's shop where strength was expected";
                case "Hunter Lodge":
                    return "a hunter lodge that respected quiet steps";
                case "Sailor Contract":
                    return "a sailor contract signed for food and distance";
                case "Field Medic":
                    return "field medic work beside screaming fighters";
                case "Quarry Crew":
                    return "a quarry crew that turned stone into punishment";
                case "Duelist Circle":
                    return "a duelist circle where pride had witnesses";
                case "Scholar Job":
                    return "a scholar's job copying maps and wounds alike";
                case "Street Performer":
                    return "street performance, applause, and thrown bottles";
                case "Bodyguard Work":
                    return "bodyguard work for people worth less than their enemies claimed";
                case "Dojo Assistant":
                    return "a dojo assistant's post cleaning floors and correcting stances";
                default:
                    return anchor.ToLowerInvariant();
            }
        }

        private static string DescribeLoreTone(string tone)
        {
            switch (tone)
            {
                case "Hard":
                    return "hard endurance";
                case "Quiet":
                    return "quiet restraint";
                case "Lucky":
                    return "uncomfortable luck";
                case "Hungry":
                    return "hunger that made every choice sharper";
                case "Bright":
                    return "a bright belief that tomorrow could be stolen back";
                case "Cruel":
                    return "cruelty learned as protection";
                case "Patient":
                    return "patience that waited longer than fear";
                case "Restless":
                    return "restlessness that kept looking for a door";
                case "Broken":
                    return "broken trust and careful habits";
                case "Proud":
                    return "pride too stubborn to die quietly";
                default:
                    return tone.ToLowerInvariant();
            }
        }

        private static ChampionLoreModifier[] BuildLoreModifiers(int anchorIndex, int toneIndex)
        {
            ChampionStatType stat = (ChampionStatType)(anchorIndex % 5);
            ChampionStatType secondStat = (ChampionStatType)((anchorIndex + toneIndex + 2) % 5);
            WeaponType weapon = (WeaponType)((anchorIndex % 6) + 1);
            SkillElement element = GetElementByIndex(anchorIndex + toneIndex);
            PerkCombatStatType combatStat = GetCombatByIndex(toneIndex);

            int primary = 1 + toneIndex % 3;
            int secondary = 1 + anchorIndex % 2;

            switch ((anchorIndex + toneIndex) % 4)
            {
                case 0:
                    return new[] { LoreBase(stat, primary), LoreWeapon(weapon, 2 + toneIndex % 3) };
                case 1:
                    return new[] { LoreBase(stat, primary), LoreBase(secondStat, secondary) };
                case 2:
                    return new[] { LoreElement(element, 3 + toneIndex % 4), LoreBase(stat, secondary) };
                default:
                    return new[] { LoreCombat(combatStat, 2 + toneIndex % 4), LoreWeapon(weapon, 1 + anchorIndex % 3) };
            }
        }

        private static SkillElement GetElementByIndex(int index)
        {
            SkillElement[] elements =
            {
                SkillElement.Fire,
                SkillElement.Water,
                SkillElement.Electricity,
                SkillElement.Poison,
                SkillElement.Earth,
                SkillElement.Air,
                SkillElement.Wood
            };

            return elements[Mathf.Abs(index) % elements.Length];
        }

        private static PerkCombatStatType GetCombatByIndex(int index)
        {
            PerkCombatStatType[] stats =
            {
                PerkCombatStatType.DodgeRating,
                PerkCombatStatType.BlockRating,
                PerkCombatStatType.CriticalRating,
                PerkCombatStatType.HitRating,
                PerkCombatStatType.Recovery,
                PerkCombatStatType.Life,
                PerkCombatStatType.Energy
            };

            return stats[Mathf.Abs(index) % stats.Length];
        }

        private static ChampionLoreModifier LoreBase(ChampionStatType stat, int value)
        {
            return ChampionLoreGenerator.Base(stat, value);
        }

        private static ChampionLoreModifier LoreCombat(PerkCombatStatType stat, int value)
        {
            return ChampionLoreGenerator.Combat(stat, value);
        }

        private static ChampionLoreModifier LoreWeapon(WeaponType weapon, int value)
        {
            return ChampionLoreGenerator.Weapon(weapon, value);
        }

        private static ChampionLoreModifier LoreElement(SkillElement element, int value)
        {
            return ChampionLoreGenerator.Element(element, value);
        }

        private static List<PerkSpec> BuildDefaultPerkSpecs()
        {
            var specs = new List<PerkSpec>();
            AddWeaponPerks(specs);
            AddBaseStatPerks(specs);
            AddFlatBaseStatPerks(specs);
            AddCombatStatPerks(specs);
            AddLegacyCombatStatAliases(specs);
            AddLifePerks(specs);
            AddExperiencePerks(specs);
            AddInjuryPerks(specs);
            AddOpeningActionSpeedPerks(specs);
            AddChargePerks(specs);
            AddElementalResistancePerks(specs);
            AddElementalDamagePerks(specs);
            AddElementalDotPerks(specs);
            AddElementalKamikazePerks(specs);
            AddNamedLegendaryPerks(specs);
            AddTrainingPerks(specs);
            return specs;
        }

        private static void AddWeaponPerks(List<PerkSpec> specs)
        {
            AddWeaponChain(specs, WeaponType.Sword, "Sword", "Swordsman", "Sword Saint");
            AddWeaponChain(specs, WeaponType.Axe, "Axe", "Axeman", "Axe Saint");
            AddWeaponChain(specs, WeaponType.Spear, "Spear", "Spearman", "Spear Saint");
            AddWeaponChain(specs, WeaponType.Bow, "Bow", "Archer", "Hawk Saint");
            AddWeaponChain(specs, WeaponType.Staff, "Staff", "Staff Fighter", "Staff Saint");
            AddWeaponChain(specs, WeaponType.Fists, "Fists", "Brawler", "Fist Saint");
        }

        private static void AddWeaponChain(List<PerkSpec> specs, WeaponType weapon, string weaponName, string baseName, string legendaryName)
        {
            string id = weapon.ToString().ToLowerInvariant();
            Add(specs, $"{id}_novice", $"Novice {baseName}", PerkRarity.Common, $"Knows the basic angles of {weaponName}. Effect: +5 {weaponName} proficiency.", PerkEffectType.WeaponProficiency, weapon: weapon, value: 5);
            Add(specs, $"{id}_skilled", $"Skilled {baseName}", PerkRarity.Rare, $"Moves cleanly with {weaponName}. Effect: +10 {weaponName} proficiency.", PerkEffectType.WeaponProficiency, weapon: weapon, value: 10);
            Add(specs, $"{id}_veteran", $"Veteran {baseName}", PerkRarity.VeryRare, $"Has survived real fights with {weaponName}. Effect: +15 {weaponName} proficiency.", PerkEffectType.WeaponProficiency, weapon: weapon, value: 15);
            Add(specs, $"{id}_master", $"Master {baseName}", PerkRarity.Epic, $"Controls the fight through {weaponName} technique. Effect: +20 {weaponName} proficiency.", PerkEffectType.WeaponProficiency, weapon: weapon, value: 20);
            Add(specs, $"{id}_saint", legendaryName, PerkRarity.Legendary, $"Makes {weaponName} look like fate. Effect: +25 {weaponName} proficiency.", PerkEffectType.WeaponProficiency, weapon: weapon, value: 25);
        }

        private static void AddBaseStatPerks(List<PerkSpec> specs)
        {
            AddStatChain(specs, ChampionStatType.Strength, "Strength", "Titan");
            AddStatChain(specs, ChampionStatType.Agility, "Agility", "Flash");
            AddStatChain(specs, ChampionStatType.Constitution, "Constitution", "Colossus");
            AddStatChain(specs, ChampionStatType.Intelligence, "Intelligence", "Oracle");
            AddStatChain(specs, ChampionStatType.Endurance, "Endurance", "Bulwark");
        }

        private static void AddStatChain(List<PerkSpec> specs, ChampionStatType stat, string statName, string legendaryName)
        {
            string id = stat.ToString().ToLowerInvariant();
            Add(specs, $"{id}_spark", $"{statName} Spark", PerkRarity.Common, $"A visible natural edge in {statName}. Effect: base {statName} x1.1.", PerkEffectType.BaseStatMultiplier, stat: stat, multiplier: 1.1f);
            Add(specs, $"{id}_gift", $"{statName} Gift", PerkRarity.Rare, $"The champion is clearly gifted in {statName}. Effect: base {statName} x1.2.", PerkEffectType.BaseStatMultiplier, stat: stat, multiplier: 1.2f);
            Add(specs, $"{id}_born", $"{statName} Born", PerkRarity.VeryRare, $"This body was born for {statName}. Effect: base {statName} x1.35.", PerkEffectType.BaseStatMultiplier, stat: stat, multiplier: 1.35f);
            Add(specs, $"{id}_avatar", $"{statName} Avatar", PerkRarity.Epic, $"Turns {statName} into identity. Effect: base {statName} x1.6.", PerkEffectType.BaseStatMultiplier, stat: stat, multiplier: 1.6f);
            Add(specs, $"{id}_legend", legendaryName, PerkRarity.Legendary, $"A mythic expression of {statName}. Effect: base {statName} x2.", PerkEffectType.BaseStatMultiplier, stat: stat, multiplier: 2f);
        }

        private static void AddFlatBaseStatPerks(List<PerkSpec> specs)
        {
            AddFlatStatChain(specs, ChampionStatType.Strength, "Strength", "Raw Power");
            AddFlatStatChain(specs, ChampionStatType.Agility, "Agility", "Fast Hands");
            AddFlatStatChain(specs, ChampionStatType.Constitution, "Constitution", "Hard Body");
            AddFlatStatChain(specs, ChampionStatType.Intelligence, "Intelligence", "Quick Study");
            AddFlatStatChain(specs, ChampionStatType.Endurance, "Endurance", "Deep Grit");
        }

        private static void AddFlatStatChain(List<PerkSpec> specs, ChampionStatType stat, string statName, string theme)
        {
            string id = stat.ToString().ToLowerInvariant();
            Add(specs, $"{id}_training", $"{theme} Training", PerkRarity.Common, $"Useful early discipline in {statName}. Effect: +2 base {statName}.", PerkEffectType.BaseStatBonus, stat: stat, value: 2);
            Add(specs, $"{id}_habit", $"{theme} Habit", PerkRarity.Rare, $"Daily practice leaves a clear mark on {statName}. Effect: +4 base {statName}.", PerkEffectType.BaseStatBonus, stat: stat, value: 4);
            Add(specs, $"{id}_edge", $"{theme} Edge", PerkRarity.VeryRare, $"A practical advantage that wins early fights. Effect: +6 base {statName}.", PerkEffectType.BaseStatBonus, stat: stat, value: 6);
            Add(specs, $"{id}_discipline", $"{theme} Discipline", PerkRarity.Epic, $"Turns early training into real pressure. Effect: +8 base {statName}.", PerkEffectType.BaseStatBonus, stat: stat, value: 8);
            Add(specs, $"{id}_foundation", $"{theme} Foundation", PerkRarity.Legendary, $"A huge starting foundation, even if scaling later matters more. Effect: +12 base {statName}.", PerkEffectType.BaseStatBonus, stat: stat, value: 12);
        }

        private static void AddCombatStatPerks(List<PerkSpec> specs)
        {
            string[] stats = { "Critical Rating", "Dodge Rating", "Block Rating", "Energy", "Recovery" };
            foreach (string stat in stats)
            {
                string id = stat.ToLowerInvariant().Replace(" ", "_");
                PerkCombatStatType combatStat = ToCombatStat(stat);
                Add(specs, $"{id}_touched", $"{stat} Touched", PerkRarity.Common, $"A small combat habit improves {stat}. Effect: +3 {stat}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 3);
                Add(specs, $"{id}_trained", $"{stat} Trained", PerkRarity.Rare, $"Regular drilling improves {stat}. Effect: +6 {stat}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 6);
                Add(specs, $"{id}_hardened", $"{stat} Hardened", PerkRarity.VeryRare, $"Battle pressure hardens {stat}. Effect: +9 {stat}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 9);
                Add(specs, $"{id}_perfected", $"{stat} Perfected", PerkRarity.Epic, $"Every movement supports {stat}. Effect: +12 {stat}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 12);
                Add(specs, $"{id}_myth", $"{stat} Myth", PerkRarity.Legendary, $"Stories are told about this champion's {stat}. Effect: +18 {stat}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 18);
            }

            AddLifeRatingPerks(specs);
            AddHitRatingPerks(specs);
        }

        private static void AddLifeRatingPerks(List<PerkSpec> specs)
        {
            Add(specs, "life_touched", "Life Touched", PerkRarity.Common, "A little more meat on the frame. Effect: +10 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 10);
            Add(specs, "life_trained", "Life Trained", PerkRarity.Rare, "Regular conditioning makes the champion harder to drop. Effect: +20 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 20);
            Add(specs, "life_hardened", "Life Hardened", PerkRarity.VeryRare, "Punishment has made the body stubborn. Effect: +35 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 35);
            Add(specs, "life_perfected", "Life Perfected", PerkRarity.Epic, "A serious reserve of blood and breath. Effect: +55 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 55);
            Add(specs, "life_myth", "Life Myth", PerkRarity.Legendary, "The champion feels unfairly alive. Effect: +85 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 85);
        }

        private static void AddHitRatingPerks(List<PerkSpec> specs)
        {
            Add(specs, "hit_rating_touched", "Hit Rating Touched", PerkRarity.Common, "Keeps attacks on line under pressure. Effect: +3 Hit Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.HitRating, value: 3);
            Add(specs, "hit_rating_trained", "Hit Rating Trained", PerkRarity.Rare, "Finds openings through movement. Effect: +6 Hit Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.HitRating, value: 6);
            Add(specs, "hit_rating_hardened", "Hit Rating Hardened", PerkRarity.VeryRare, "Forces defenders to work harder. Effect: +9 Hit Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.HitRating, value: 9);
            Add(specs, "hit_rating_perfected", "Hit Rating Perfected", PerkRarity.Epic, "Attacks arrive where the enemy is going. Effect: +12 Hit Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.HitRating, value: 12);
            Add(specs, "unerring_hand", "Unerring Hand", PerkRarity.Legendary, "The strike seems to correct itself mid-swing. Effect: +18 Hit Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.HitRating, value: 18);
        }

        private static void AddLegacyCombatStatAliases(List<PerkSpec> specs)
        {
            AddCombatStatAliasChain(specs, "critical", "Critical Rating", PerkCombatStatType.CriticalRating);
            AddCombatStatAliasChain(specs, "dodge", "Dodge Rating", PerkCombatStatType.DodgeRating);
            AddCombatStatAliasChain(specs, "block", "Block Rating", PerkCombatStatType.BlockRating);
            AddCombatStatAliasChain(specs, "energy", "Energy", PerkCombatStatType.Energy);
            AddCombatStatAliasChain(specs, "recovery", "Recovery", PerkCombatStatType.Recovery);
        }

        private static void AddCombatStatAliasChain(List<PerkSpec> specs, string id, string statName, PerkCombatStatType combatStat)
        {
            Add(specs, $"{id}_touched", $"{statName} Touched", PerkRarity.Common, $"A small combat habit improves {statName}. Effect: +3 {statName}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 3);
            Add(specs, $"{id}_trained", $"{statName} Trained", PerkRarity.Rare, $"Regular drilling improves {statName}. Effect: +6 {statName}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 6);
            Add(specs, $"{id}_hardened", $"{statName} Hardened", PerkRarity.VeryRare, $"Battle pressure hardens {statName}. Effect: +9 {statName}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 9);
            Add(specs, $"{id}_perfected", $"{statName} Perfected", PerkRarity.Epic, $"Every movement supports {statName}. Effect: +12 {statName}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 12);
            Add(specs, $"{id}_myth", $"{statName} Myth", PerkRarity.Legendary, $"Stories are told about this champion's {statName}. Effect: +18 {statName}.", PerkEffectType.CombatStatBonus, combatStat: combatStat, value: 18);
        }

        private static void AddLifePerks(List<PerkSpec> specs)
        {
            Add(specs, "thick_blood", "Thick Blood", PerkRarity.Common, "Bleeds slowly and stays upright longer. Effect: +20 Life.", PerkEffectType.MaxLifeBonus, value: 20);
            Add(specs, "hard_to_drop", "Hard to Drop", PerkRarity.Rare, "Takes punishment better than most recruits. Effect: +45 Life.", PerkEffectType.MaxLifeBonus, value: 45);
            Add(specs, "second_lungs", "Second Lungs", PerkRarity.VeryRare, "Keeps breathing after the arena thinks it is over. Effect: +75 Life.", PerkEffectType.MaxLifeBonus, value: 75);
            Add(specs, "meat_fortress", "Meat Fortress", PerkRarity.Epic, "A brutal amount of body to get through. Effect: +120 Life.", PerkEffectType.MaxLifeBonus, value: 120);
            Add(specs, "unkillable_frame", "Unkillable Frame", PerkRarity.Legendary, "The body refuses the simple logic of death. Effect: +200 Life.", PerkEffectType.MaxLifeBonus, value: 200);
        }

        private static void AddExperiencePerks(List<PerkSpec> specs)
        {
            Add(specs, "fast_learner", "Fast Learner", PerkRarity.Common, "Understands mistakes quickly. Effect: +5% XP gain.", PerkEffectType.ExperienceGain, value: 5);
            Add(specs, "hungry_student", "Hungry Student", PerkRarity.Rare, "Turns every fight into training. Effect: +10% XP gain.", PerkEffectType.ExperienceGain, value: 10);
            Add(specs, "scar_scholar", "Scar Scholar", PerkRarity.VeryRare, "Learns best from pain. Effect: +15% XP gain.", PerkEffectType.ExperienceGain, value: 15);
            Add(specs, "battle_savant", "Battle Savant", PerkRarity.Epic, "Reads combat like a second language. Effect: +25% XP gain.", PerkEffectType.ExperienceGain, value: 25);
            Add(specs, "living_manual", "Living Manual", PerkRarity.Legendary, "Every second becomes doctrine. Effect: +40% XP gain.", PerkEffectType.ExperienceGain, value: 40);
        }

        private static void AddInjuryPerks(List<PerkSpec> specs)
        {
            Add(specs, "lucky_bones", "Lucky Bones", PerkRarity.Common, "Bad falls miss the worst angles. Effect: -5% fatal injury chance.", PerkEffectType.FatalInjuryResistance, value: 5);
            Add(specs, "scar_tissue", "Scar Tissue", PerkRarity.Rare, "Old wounds teach the body where to bend. Effect: -10% fatal injury chance.", PerkEffectType.FatalInjuryResistance, value: 10);
            Add(specs, "death_ducker", "Death Ducker", PerkRarity.VeryRare, "Somehow avoids the blow that should end it. Effect: -15% fatal injury chance.", PerkEffectType.FatalInjuryResistance, value: 15);
            Add(specs, "nine_lives", "Nine Lives", PerkRarity.Epic, "The hospital keeps being surprised. Effect: -25% fatal injury chance.", PerkEffectType.FatalInjuryResistance, value: 25);
            Add(specs, "death_refuses", "Death Refuses", PerkRarity.Legendary, "Death looks away first. Effect: -40% fatal injury chance.", PerkEffectType.FatalInjuryResistance, value: 40);

            Add(specs, "good_fall", "Good Fall", PerkRarity.Common, "Knows how to hit the floor. Effect: -5% injury severity.", PerkEffectType.InjurySeverityReduction, value: 5);
            Add(specs, "loose_joints", "Loose Joints", PerkRarity.Rare, "Damage travels poorly through this body. Effect: -10% injury severity.", PerkEffectType.InjurySeverityReduction, value: 10);
            Add(specs, "rubber_soul", "Rubber Soul", PerkRarity.VeryRare, "Bends out of career-ending damage. Effect: -15% injury severity.", PerkEffectType.InjurySeverityReduction, value: 15);
            Add(specs, "pain_alchemist", "Pain Alchemist", PerkRarity.Epic, "Turns terrible impacts into manageable wounds. Effect: -25% injury severity.", PerkEffectType.InjurySeverityReduction, value: 25);
            Add(specs, "miracle_recovery", "Miracle Recovery", PerkRarity.Legendary, "What should maim becomes a story. Effect: -40% injury severity.", PerkEffectType.InjurySeverityReduction, value: 40);
        }

        private static void AddOpeningActionSpeedPerks(List<PerkSpec> specs)
        {
            Add(specs, "quick_draw", "Quick Draw", PerkRarity.Rare, "Starts the fight before doubt arrives. Effect: action timeline speed x2 during the first turn.", PerkEffectType.OpeningActionSpeed, value: 1, multiplier: 2f);
            Add(specs, "opening_blitz", "Opening Blitz", PerkRarity.VeryRare, "Chains the first exchanges before the enemy settles. Effect: action timeline speed x2 during the first 2 turns.", PerkEffectType.OpeningActionSpeed, value: 2, multiplier: 2f);
            Add(specs, "rushing_engine", "Rushing Engine", PerkRarity.Epic, "Turns the opening into a storm of decisions. Effect: action timeline speed x2 during the first 3 turns.", PerkEffectType.OpeningActionSpeed, value: 3, multiplier: 2f);
            Add(specs, "first_minute_demon", "First Minute Demon", PerkRarity.Legendary, "The first moments of combat belong to this champion. Effect: action timeline speed x2 during the first 4 turns.", PerkEffectType.OpeningActionSpeed, value: 4, multiplier: 2f);
        }

        private static void AddChargePerks(List<PerkSpec> specs)
        {
            AddChargeChain(specs, ChargeType.Burn, "Burn", "Cinder", "Inferno Heart");
            AddChargeChain(specs, ChargeType.Poison, "Poison", "Venom", "Plague Apostle");
            AddChargeChain(specs, ChargeType.Slow, "Slow", "Chain", "Time Anchor");
            AddChargeChain(specs, ChargeType.Haste, "Haste", "Speed", "Storm Engine");
            AddChargeChain(specs, ChargeType.LifeSteal, "Life Steal", "Leech", "Blood Cathedral");
            AddChargeChain(specs, ChargeType.Counterattack, "Counterattack", "Revenge", "Mirror Saint");
            AddChargeChain(specs, ChargeType.Fortified, "Fortified", "Guard", "Iron Chapel");
            AddChargeChain(specs, ChargeType.Vulnerable, "Vulnerable", "Expose", "Open Wound");
            AddChargeChain(specs, ChargeType.Bleed, "Bleed", "Bloodletting", "Red River");
            AddChargeChain(specs, ChargeType.Thorns, "Thorns", "Barbs", "Thorn Cathedral");
            AddChargeChain(specs, ChargeType.Caltrops, "Caltrops", "Caltrop", "Iron Floor");
            AddChargeChain(specs, ChargeType.Recoil, "Recoil", "Backlash", "Pain Mirror");
            AddChargeChain(specs, ChargeType.Dizzle, "Dizzle", "Dizzle", "Broken Focus");
            AddRegenerationPerks(specs);
            AddElementalFortificationPerks(specs);
            AddKamikazeChargePerks(specs);
        }

        private static void AddChargeChain(List<PerkSpec> specs, ChargeType charge, string label, string shortName, string legendaryName)
        {
            string id = charge.ToString().ToLowerInvariant();
            bool positive = IsPositiveCharge(charge);
            string verb = positive ? "gain" : "apply";
            string target = positive ? "self" : "enemy";
            if (charge == ChargeType.Haste || charge == ChargeType.Slow)
            {
                Add(specs, $"{id}_aftertouch", $"{shortName} Aftertouch", PerkRarity.VeryRare, $"After each action, {verb} 1 {label} stack on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 1);
                return;
            }

            bool counterattack = charge == ChargeType.Counterattack;
            Add(specs, $"{id}_aftertouch", $"{shortName} Aftertouch", counterattack ? PerkRarity.Epic : PerkRarity.Common, $"After each action, {verb} 1 {label} stack on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 1);
            Add(specs, $"{id}_rhythm", $"{shortName} Rhythm", counterattack ? PerkRarity.Epic : PerkRarity.Rare, $"After each action, {verb} 2 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 2);
            Add(specs, $"{id}_engine", $"{shortName} Engine", counterattack ? PerkRarity.Epic : PerkRarity.VeryRare, $"After each action, {verb} 3 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 3);
            Add(specs, $"{id}_crown", $"{shortName} Crown", PerkRarity.Epic, $"After each action, {verb} 4 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 4);
            Add(specs, $"{id}_legend", legendaryName, PerkRarity.Legendary, $"After each action, {verb} 5 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 5);
        }

        private static void AddRegenerationPerks(List<PerkSpec> specs)
        {
            Add(specs, "regeneration_aftertouch", "Green Pulse", PerkRarity.Rare, "The body starts repairing itself after acting. Effect: after each action, gain 1 Regeneration stack on self.", PerkEffectType.ApplyChargeOnAction, charge: ChargeType.Regeneration, value: 1);
            Add(specs, "regeneration_seed", "Regeneration Seed", PerkRarity.Common, "A small reserve of recovery waits under the skin. Effect: start combat with 1 Regeneration stack.", PerkEffectType.OpeningCharges, charge: ChargeType.Regeneration, value: 1);
            Add(specs, "regeneration_well", "Regeneration Well", PerkRarity.Rare, "The first wounds close with stubborn patience. Effect: start combat with 2 Regeneration stacks.", PerkEffectType.OpeningCharges, charge: ChargeType.Regeneration, value: 2);
            Add(specs, "regeneration_garden", "Regeneration Garden", PerkRarity.VeryRare, "Recovery blooms before the fight has properly begun. Effect: start combat with 3 Regeneration stacks.", PerkEffectType.OpeningCharges, charge: ChargeType.Regeneration, value: 3);
            Add(specs, "regeneration_font", "Regeneration Font", PerkRarity.Epic, "Blood remembers where it belongs. Effect: start combat with 4 Regeneration stacks.", PerkEffectType.OpeningCharges, charge: ChargeType.Regeneration, value: 4);
            Add(specs, "undying_spring", "Undying Spring", PerkRarity.Legendary, "Every opening wound meets a deeper life beneath it. Effect: start combat with 5 Regeneration stacks.", PerkEffectType.OpeningCharges, charge: ChargeType.Regeneration, value: 5);
        }

        private static void AddElementalFortificationPerks(List<PerkSpec> specs)
        {
            foreach (ElementSpec element in GetElementSpecs())
            {
                ChargeType guard = GetElementalFortifiedCharge(element.element);
                Add(specs, $"{element.id}_fortified_aftertouch", $"{element.name} Aegis", PerkRarity.Rare, $"After each action, gain 1 {element.name} Fortified stack. It reduces matching {element.name} damage by 30%.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: guard, value: 1);
                Add(specs, $"{element.id}_fortified_seed", $"{element.name} Guard Seed", PerkRarity.Common, $"Starts with a small {element.name} guard. Effect: start combat with 1 {element.name} Fortified stack.", PerkEffectType.OpeningCharges, element: element.element, charge: guard, value: 1);
                Add(specs, $"{element.id}_fortified_well", $"{element.name} Guard Well", PerkRarity.Rare, $"Starts with a practiced {element.name} guard. Effect: start combat with 2 {element.name} Fortified stacks.", PerkEffectType.OpeningCharges, element: element.element, charge: guard, value: 2);
                Add(specs, $"{element.id}_fortified_garden", $"{element.name} Guard Garden", PerkRarity.VeryRare, $"Starts wrapped in {element.name} discipline. Effect: start combat with 3 {element.name} Fortified stacks.", PerkEffectType.OpeningCharges, element: element.element, charge: guard, value: 3);
                Add(specs, $"{element.id}_fortified_font", $"{element.name} Guard Font", PerkRarity.Epic, $"The element loses bite on contact. Effect: start combat with 4 {element.name} Fortified stacks.", PerkEffectType.OpeningCharges, element: element.element, charge: guard, value: 4);
                Add(specs, $"{element.id}_fortified_sanctum", $"{element.name} Sanctum", PerkRarity.Legendary, $"The opening of the fight belongs to {element.name} defense. Effect: start combat with 5 {element.name} Fortified stacks.", PerkEffectType.OpeningCharges, element: element.element, charge: guard, value: 5);
            }
        }

        private static void AddKamikazeChargePerks(List<PerkSpec> specs)
        {
            Add(specs, "shared_bleeding", "Shared Bleeding", PerkRarity.Epic, "Every action opens both bodies. Effect: after each action, apply 1 Bleed stack to both champions.", PerkEffectType.ElementalSelfDotOnAction, charge: ChargeType.Bleed, value: 1);
            Add(specs, "shared_vulnerability", "Open Exchange", PerkRarity.Epic, "Every action makes everyone easier to hurt. Effect: after each action, apply 1 Vulnerable stack to both champions.", PerkEffectType.ElementalSelfDotOnAction, charge: ChargeType.Vulnerable, value: 1);
            Add(specs, "barbed_floor", "Barbed Floor", PerkRarity.Epic, "Every action scatters pain under both fighters. Effect: after each action, apply 1 Caltrops stack to both champions.", PerkEffectType.ElementalSelfDotOnAction, charge: ChargeType.Caltrops, value: 1);
            Add(specs, "pain_feedback", "Pain Feedback", PerkRarity.Legendary, "Every action makes power bite its owner. Effect: after each action, apply 1 Recoil stack to both champions.", PerkEffectType.ElementalSelfDotOnAction, charge: ChargeType.Recoil, value: 1);
            Add(specs, "thorn_bargain", "Thorn Bargain", PerkRarity.Epic, "Both fighters grow dangerous to touch. Effect: after each action, apply 1 Thorns stack to both champions.", PerkEffectType.ElementalSelfDotOnAction, charge: ChargeType.Thorns, value: 1);
        }

        private static bool IsPositiveCharge(ChargeType charge)
        {
            switch (charge)
            {
                case ChargeType.Haste:
                case ChargeType.LifeSteal:
                case ChargeType.Counterattack:
                case ChargeType.Empowered:
                case ChargeType.Regeneration:
                case ChargeType.Fortified:
                case ChargeType.Thorns:
                case ChargeType.FireFortified:
                case ChargeType.WaterFortified:
                case ChargeType.ElectricityFortified:
                case ChargeType.PoisonFortified:
                case ChargeType.EarthFortified:
                case ChargeType.AirFortified:
                case ChargeType.WoodFortified:
                    return true;
                default:
                    return false;
            }
        }

        private static void AddElementalResistancePerks(List<PerkSpec> specs)
        {
            foreach (ElementSpec element in GetElementSpecs())
            {
                string id = element.id;
                Add(specs, $"{id}_ward", $"{element.name} Ward", PerkRarity.Common, $"The body learns to soften {element.name} damage. Effect: +8 {element.name} Resistance Rating.", PerkEffectType.ElementalResistance, element: element.element, value: 8);
                Add(specs, $"{id}_guard", $"{element.name} Guard", PerkRarity.Rare, $"Keeps shape under {element.name} pressure. Effect: +16 {element.name} Resistance Rating.", PerkEffectType.ElementalResistance, element: element.element, value: 16);
                Add(specs, $"{id}_skin", $"{element.name} Skin", PerkRarity.VeryRare, $"The champion's skin remembers every {element.name} wound. Effect: +25 {element.name} Resistance Rating.", PerkEffectType.ElementalResistance, element: element.element, value: 25);
                Add(specs, $"{id}_vessel", $"{element.name} Vessel", PerkRarity.Epic, $"Most {element.name} attacks lose their bite on contact. Effect: +35 {element.name} Resistance Rating.", PerkEffectType.ElementalResistance, element: element.element, value: 35);
                Add(specs, $"{id}_avatar", $"{element.avatarName}", PerkRarity.Legendary, $"The element recognizes something familiar. Effect: +50 {element.name} Resistance Rating.", PerkEffectType.ElementalResistance, element: element.element, value: 50);
            }
        }

        private static void AddElementalDamagePerks(List<PerkSpec> specs)
        {
            foreach (ElementSpec element in GetElementSpecs())
            {
                AddElementalDamageFlatChain(specs, element);
                AddElementalDamageMultiplierChain(specs, element);
            }
        }

        private static void AddElementalDamageFlatChain(List<PerkSpec> specs, ElementSpec element)
        {
            Add(specs, $"{element.id}_damage_touched", $"{element.name} Damage Touched", PerkRarity.Common, $"A small talent for hurting with {element.name}. Effect: +5 {element.name} Damage.", PerkEffectType.ElementalDamageBonus, element: element.element, value: 5);
            Add(specs, $"{element.id}_damage_trained", $"{element.name} Damage Trained", PerkRarity.Rare, $"Practice sharpens {element.name} output. Effect: +15 {element.name} Damage.", PerkEffectType.ElementalDamageBonus, element: element.element, value: 15);
            Add(specs, $"{element.id}_damage_hardened", $"{element.name} Damage Hardened", PerkRarity.VeryRare, $"The champion forces more pain through {element.name}. Effect: +25 {element.name} Damage.", PerkEffectType.ElementalDamageBonus, element: element.element, value: 25);
            Add(specs, $"{element.id}_damage_perfected", $"{element.name} Damage Perfected", PerkRarity.Epic, $"Every {element.name} wound lands heavier. Effect: +35 {element.name} Damage.", PerkEffectType.ElementalDamageBonus, element: element.element, value: 35);
            Add(specs, $"{element.id}_damage_myth", $"{element.name} Damage Myth", PerkRarity.Legendary, $"The element arrives like a verdict. Effect: +55 {element.name} Damage.", PerkEffectType.ElementalDamageBonus, element: element.element, value: 55);
        }

        private static void AddElementalDamageMultiplierChain(List<PerkSpec> specs, ElementSpec element)
        {
            Add(specs, $"{element.id}_damage_spark", $"{element.name} Damage Spark", PerkRarity.Common, $"A visible natural edge in {element.name} damage. Effect: {element.name} Damage x1.1.", PerkEffectType.ElementalDamageMultiplier, element: element.element, multiplier: 1.1f);
            Add(specs, $"{element.id}_damage_gift", $"{element.name} Damage Gift", PerkRarity.Rare, $"The champion is clearly gifted with {element.name}. Effect: {element.name} Damage x1.25.", PerkEffectType.ElementalDamageMultiplier, element: element.element, multiplier: 1.25f);
            Add(specs, $"{element.id}_damage_born", $"{element.name} Damage Born", PerkRarity.VeryRare, $"This body was born to weaponize {element.name}. Effect: {element.name} Damage x1.4.", PerkEffectType.ElementalDamageMultiplier, element: element.element, multiplier: 1.4f);
            Add(specs, $"{element.id}_damage_avatar", $"{element.name} Damage Avatar", PerkRarity.Epic, $"Turns {element.name} harm into identity. Effect: {element.name} Damage x1.6.", PerkEffectType.ElementalDamageMultiplier, element: element.element, multiplier: 1.6f);
            Add(specs, $"{element.id}_damage_legend", $"{element.name} Damage Legend", PerkRarity.Legendary, $"A mythic expression of {element.name} violence. Effect: {element.name} Damage x2.", PerkEffectType.ElementalDamageMultiplier, element: element.element, multiplier: 2f);
        }

        private static void AddElementalDotPerks(List<PerkSpec> specs)
        {
            foreach (ElementSpec element in GetElementSpecs())
            {
                Add(specs, $"{element.id}_sting", $"{element.dotName} Sting", PerkRarity.Rare, $"Every action leaves a trace of {element.dotName}. Effect: apply 1 {element.charge} stack after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 1);
                Add(specs, $"{element.id}_brand", $"{element.dotName} Brand", PerkRarity.VeryRare, $"The element keeps working after the strike. Effect: apply 2 {element.charge} stacks after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 2);
                Add(specs, $"{element.id}_sentence", $"{element.dotName} Sentence", PerkRarity.Epic, $"A small curse rides every movement. Effect: apply 3 {element.charge} stacks after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 3);
                Add(specs, $"{element.id}_doom", $"{element.doomName}", PerkRarity.Legendary, $"The arena slowly becomes {element.dotName}. Effect: apply 5 {element.charge} stacks after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 5);
            }
        }

        private static void AddElementalKamikazePerks(List<PerkSpec> specs)
        {
            foreach (ElementSpec element in GetElementSpecs())
            {
                Add(specs, $"{element.id}_self_curse", element.kamikazeName, PerkRarity.Epic, $"A dangerous school that hurts both sides. Effect: after each action, apply 1 {element.charge} stack to both champions.", PerkEffectType.ElementalSelfDotOnAction, element: element.element, charge: element.charge, value: 1);
            }

            Add(specs, "elemental_detonation", "Elemental Detonation", PerkRarity.Legendary, "Every clean hit erupts around both fighters. Effect: on hit, create an elemental explosion that damages both champions.", PerkEffectType.ElementalExplosionOnHit, value: 35);
            Add(specs, "lazy_genius", "Lazy Genius", PerkRarity.Legendary, "Starts half-asleep, then becomes terrifying when hurt. Effect: starts with all base stats -50%; below 50% Life, remove the penalty and multiply all base stats x1.5.", PerkEffectType.LazyGenius, multiplier: 1.5f);
            Add(specs, "backtalker", "Backtalker", PerkRarity.Epic, "Answers every condition with its opposite. Effect: buffs applied to this champion become matching debuffs, and debuffs become matching buffs.", PerkEffectType.InvertedBuffs);
        }

        private static void AddNamedLegendaryPerks(List<PerkSpec> specs)
        {
            Add(specs, "vampirism", "Vampirism", PerkRarity.Legendary, "Drinks victory directly from wounds. Effect: heal for 25% of damage dealt.", PerkEffectType.LifeSteal, value: 25);
            Add(specs, "double_down", "Double Down", PerkRarity.Legendary, "Everything sticks twice as hard, blessing or curse. Effect: any stack this champion gains is doubled, positive or negative.", PerkEffectType.DoubleAppliedCharges, value: 2);
            Add(specs, "berserker", "Berserker", PerkRarity.Legendary, "Starts the fight already foaming with speed. Effect: gain 10 Haste stacks at combat start.", PerkEffectType.OpeningCharges, charge: ChargeType.Haste, value: 10);
            Add(specs, "thorn_reversal", "Thorn Reversal", PerkRarity.Legendary, "Punishes anyone brave enough to land a hit. Effect: 25% counterattack chance when hit.", PerkEffectType.Counterattack, chance: 0.25f);
            Add(specs, "saint_of_scars", "Saint of Scars", PerkRarity.Legendary, "Every wound becomes a sermon. Effect: +25 combat power while badly injured.", PerkEffectType.LowLifeCombatPower, value: 25);
            Add(specs, "ghost_strike", "Ghost Strike", PerkRarity.Legendary, "When dodged, instantly repeats the attack at 50% power, minimum 5 damage. This can repeat until the reduced hit also fails at 5 damage.", PerkEffectType.GhostStrike, chance: 1f, value: 50);
            Add(specs, "echoing_style", "Echoing Style", PerkRarity.Epic, "Every skill leaves a second impact behind it. Effect: after executing a skill, echo its actions at 50% damage.", PerkEffectType.SkillEcho, multiplier: 0.5f);
            Add(specs, "perfect_echo", "Perfect Echo", PerkRarity.Legendary, "The second motion is as real as the first. Effect: after executing a skill, echo its actions at 100% damage.", PerkEffectType.SkillEcho, multiplier: 1f);
            AddElementalAbsorptionPerks(specs);
            AddBlockPiercePerks(specs);
        }

        private static ChargeType GetElementalFortifiedCharge(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    return ChargeType.FireFortified;
                case SkillElement.Water:
                    return ChargeType.WaterFortified;
                case SkillElement.Electricity:
                    return ChargeType.ElectricityFortified;
                case SkillElement.Poison:
                    return ChargeType.PoisonFortified;
                case SkillElement.Earth:
                    return ChargeType.EarthFortified;
                case SkillElement.Air:
                    return ChargeType.AirFortified;
                case SkillElement.Wood:
                    return ChargeType.WoodFortified;
                default:
                    return ChargeType.None;
            }
        }

        private static void AddElementalAbsorptionPerks(List<PerkSpec> specs)
        {
            foreach (ElementSpec element in GetElementSpecs())
            {
                Add(specs, $"{element.id}_eater", element.absorptionName, PerkRarity.Legendary, $"The element heals instead of harms. Effect: {element.name} damage and {element.charge} stacks heal this champion. Unique Absorption: cannot learn another Elemental Absorption perk.", PerkEffectType.ElementalAbsorption, element: element.element, charge: element.charge, value: 100, unique: true, uniqueGroup: "elemental_absorption");
            }
        }

        private static void AddBlockPiercePerks(List<PerkSpec> specs)
        {
            Add(specs, "crack_the_guard", "Crack the Guard", PerkRarity.Rare, "Some force leaks through a successful block. Effect: 10% blocked damage pierces.", PerkEffectType.BlockPierce, value: 10);
            Add(specs, "splinter_guard", "Splinter Guard", PerkRarity.VeryRare, "Blocks stop the shape, not the impact. Effect: 18% blocked damage pierces.", PerkEffectType.BlockPierce, value: 18);
            Add(specs, "shatter_guard", "Shatter Guard", PerkRarity.Epic, "Even perfect guards leave bruises. Effect: 28% blocked damage pierces.", PerkEffectType.BlockPierce, value: 28);
            Add(specs, "worldbreaker", "Worldbreaker", PerkRarity.Legendary, "The guard is only a delay before pain arrives. Effect: 40% blocked damage pierces.", PerkEffectType.BlockPierce, value: 40);
        }

        private static void AddTrainingPerks(List<PerkSpec> specs)
        {
            string[] themes =
            {
                "Arena", "Dojo", "Mountain", "River", "Ash", "Iron", "Silent", "Red", "Black", "Golden",
                "Broken", "Lucky", "Grim", "Moon", "Sun", "Storm", "Dust", "Frost", "Bone", "Shadow",
                "Lion", "Wolf", "Viper", "Dragon", "Ghost", "Thunder", "Stone", "Sky", "Blood", "Void",
                "Razor", "Cinder", "Bitter", "Wild", "Quick", "Heavy", "Sharp", "Patient", "Cruel", "Bright"
            };

            foreach (string theme in themes)
            {
                string id = theme.ToLowerInvariant();
                AddPersonalityChain(specs, id, theme);
            }
        }

        private static void AddPersonalityChain(List<PerkSpec> specs, string id, string theme)
        {
            Add(specs, $"{id}_lesson", $"{theme} Footwork", PerkRarity.Common, $"{theme} instincts make the champion harder to line up. Effect: +2 Dodge Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.DodgeRating, value: 2);
            Add(specs, $"{id}_form", $"{theme} Grip", PerkRarity.Rare, $"{theme} pressure makes every weapon bite cleaner. Effect: +5 Critical Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.CriticalRating, value: 5);
            Add(specs, $"{id}_oath", $"{theme} Grit", PerkRarity.VeryRare, $"{theme} stubbornness keeps the body in the fight. Effect: +8 Block Rating.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.BlockRating, value: 8);
            Add(specs, $"{id}_mastery", $"{theme} Engine", PerkRarity.Epic, $"{theme} rhythm turns long fights into advantage. Effect: +12 Energy.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Energy, value: 12);
            Add(specs, $"{id}_miracle", $"{theme} Signature", PerkRarity.Legendary, $"{theme} presence defines the whole champion. Effect: +20 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 20);
        }

        private static void Add(
            List<PerkSpec> specs,
            string id,
            string name,
            PerkRarity rarity,
            string description,
            PerkEffectType effectType,
            PerkCombatStatType combatStat = PerkCombatStatType.None,
            ChampionStatType stat = ChampionStatType.Strength,
            WeaponType weapon = WeaponType.None,
            SkillElement element = SkillElement.None,
            ChargeType charge = ChargeType.None,
            int value = 0,
            float multiplier = 1f,
            float chance = 0f,
            bool unique = false,
            string uniqueGroup = "")
        {
            specs.Add(new PerkSpec
            {
                id = id,
                name = name,
                rarity = rarity,
                description = description,
                effectType = effectType,
                combatStat = combatStat,
                stat = stat,
                weapon = weapon,
                element = element,
                charge = charge,
                flatValue = value,
                multiplier = multiplier,
                chance = chance,
                isUnique = unique,
                uniqueGroup = uniqueGroup
            });
        }

        private static ElementSpec[] GetElementSpecs()
        {
            return new[]
            {
                new ElementSpec("fire", "Fire", SkillElement.Fire, ChargeType.Burn, "Cinder", "Inferno Doom", "Living Bonfire", "Fire Eater", "Ash Avatar"),
                new ElementSpec("water", "Water", SkillElement.Water, ChargeType.Drowning, "Drowning", "Deep Doom", "Flooded Veins", "Tide Drinker", "Tide Avatar"),
                new ElementSpec("electricity", "Electricity", SkillElement.Electricity, ChargeType.Shock, "Shock", "Storm Doom", "Static Heart", "Storm Drinker", "Storm Avatar"),
                new ElementSpec("poison", "Poison", SkillElement.Poison, ChargeType.Poison, "Venom", "Plague Doom", "Rot Garden", "Venom Saint", "Plague Avatar"),
                new ElementSpec("earth", "Earth", SkillElement.Earth, ChargeType.Crush, "Crush", "Grave Doom", "Gravel Lung", "Stone Eater", "Stone Avatar"),
                new ElementSpec("air", "Air", SkillElement.Air, ChargeType.WindShear, "Wind Shear", "Sky Doom", "Razor Breath", "Sky Lung", "Sky Avatar"),
                new ElementSpec("wood", "Wood", SkillElement.Wood, ChargeType.Splinter, "Splinter", "Root Doom", "Thorn Body", "Root Drinker", "Root Avatar")
            };
        }

        private static PerkCombatStatType ToCombatStat(string stat)
        {
            switch (stat)
            {
                case "Critical Rating":
                    return PerkCombatStatType.CriticalRating;
                case "Dodge Rating":
                    return PerkCombatStatType.DodgeRating;
                case "Block Rating":
                    return PerkCombatStatType.BlockRating;
                case "Hit Rating":
                    return PerkCombatStatType.HitRating;
                case "Life":
                    return PerkCombatStatType.Life;
                case "Energy":
                    return PerkCombatStatType.Energy;
                case "Recovery":
                    return PerkCombatStatType.Recovery;
                case "Experience Gain":
                    return PerkCombatStatType.ExperienceGain;
                case "Fatal Injury Resistance":
                    return PerkCombatStatType.FatalInjuryResistance;
                case "Injury Severity Reduction":
                    return PerkCombatStatType.InjurySeverityReduction;
                default:
                    return PerkCombatStatType.None;
            }
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folder = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static string Sanitize(string value)
        {
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }

            return value.Replace(' ', '_').Replace('"', '_');
        }

        private struct PerkSpec
        {
            public string id;
            public string name;
            public PerkRarity rarity;
            public string description;
            public PerkEffectType effectType;
            public PerkCombatStatType combatStat;
            public ChampionStatType stat;
            public WeaponType weapon;
            public SkillElement element;
            public ChargeType charge;
            public int flatValue;
            public float multiplier;
            public float chance;
            public bool isUnique;
            public string uniqueGroup;
        }

        private readonly struct LoreSpec
        {
            public readonly string id;
            public readonly ChampionLoreStage stage;
            public readonly string title;
            public readonly string fragment;
            public readonly ChampionLoreModifier[] modifiers;

            public LoreSpec(string id, ChampionLoreStage stage, string title, string fragment, ChampionLoreModifier[] modifiers)
            {
                this.id = id;
                this.stage = stage;
                this.title = title;
                this.fragment = fragment;
                this.modifiers = modifiers;
            }
        }

        private readonly struct ElementSpec
        {
            public readonly string id;
            public readonly string name;
            public readonly SkillElement element;
            public readonly ChargeType charge;
            public readonly string dotName;
            public readonly string doomName;
            public readonly string kamikazeName;
            public readonly string absorptionName;
            public readonly string avatarName;

            public ElementSpec(string id, string name, SkillElement element, ChargeType charge, string dotName, string doomName, string kamikazeName, string absorptionName, string avatarName)
            {
                this.id = id;
                this.name = name;
                this.element = element;
                this.charge = charge;
                this.dotName = dotName;
                this.doomName = doomName;
                this.kamikazeName = kamikazeName;
                this.absorptionName = absorptionName;
                this.avatarName = avatarName;
            }
        }
    }
}
