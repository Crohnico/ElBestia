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
            CharacterPartCatalogBuilder.RebuildCharacterPartCatalogs();
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

            for (int i = 0; i < anchors.Length && specs.Count < 200; i++)
            {
                string[] variants = GetLoreVariants(stage, anchors[i]);
                for (int j = 0; j < variants.Length && specs.Count < 200; j++)
                {
                    string id = $"{stage.ToString().ToLowerInvariant()}_{i:00}_{j:00}";
                    string title = variants[j];
                    string fragment = BuildLoreFragment(stage, anchors[i], title);
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

        private static string[] GetLoreVariants(ChampionLoreStage stage, string anchor)
        {
            switch (stage)
            {
                case ChampionLoreStage.Birth:
                    return GetBirthVariants(anchor);
                case ChampionLoreStage.Childhood:
                    return GetChildhoodVariants(anchor);
                default:
                    return GetYouthVariants(anchor);
            }
        }

        private static string[] GetBirthVariants(string anchor)
        {
            switch (anchor)
            {
                case "Sword Family":
                    return new[] { "Sword Tutor Family", "Arena Champion Family", "Dojo Owner Family", "Spear Guard Family", "Retired Duelist Family", "Temple Blade Family", "Village Militia Family", "Mercenary Parentage", "Weapon Merchant Family", "Pit Fighter Bloodline" };
                case "Poor Family":
                    return new[] { "Tenement Family", "Debtor Household", "Soup Kitchen Family", "Ragpicker Family", "Market Shack Family", "Farmhand Cottage", "Street Beggar Family", "Washerwoman Family", "Dockside Household", "Famine Alley Family" };
                case "Desert Birth":
                    return new[] { "Desert Oasis", "Dune Camp", "Salt Flat Camp", "Caravanserai", "Dry River Village", "Canyon Settlement", "Glass Desert Outpost", "Nomad Tent Circle", "Sunken Well Hamlet", "Scorpion Pass" };
                case "Mountain Clan":
                    return new[] { "Mountain Clan", "Goat Trail Hamlet", "Cliffside Village", "Avalanche Shelter", "High Pass Camp", "Stone Monastery Village", "Shepherd Ridge", "Peak Watchtower", "Miner Family Camp", "Snowline Household" };
                case "River House":
                    return new[] { "River House", "Ferry Keeper Family", "Floodplain Hamlet", "Canal Boat Family", "Reed Village", "Bridge Toll House", "Watermill Household", "Delta Fishing Home", "River Shrine Family", "Barge Worker Family" };
                case "Dojo Lineage":
                    return new[] { "Dojo Lineage", "Dojo Owner Household", "Tournament Dojo Family", "Village Dojo Family", "Temple Dojo Line", "Debt-Ridden Dojo", "Traveling Dojo Troupe", "Old Master Household", "Rival School Family", "Dojo Servant Family" };
                case "Mercenary Camp":
                    return new[] { "Mercenary Camp", "Warband Nursery", "Campaign Wagon Birth", "Siege Camp Family", "Hired Blade Camp", "Camp Surgeon Tent", "Quartermaster Family", "Banner Company Birth", "Roadside Barracks", "Spoils Camp" };
                case "Temple Step":
                    return new[] { "Temple Steps", "Bell Tower Birth", "Shrine Keeper Family", "Pilgrim Shelter", "Monk Gate Foundling", "Incense Hall Family", "Mountain Temple Birth", "River Temple Birth", "Temple Kitchen Family", "Sanctuary Courtyard" };
                case "Fishing Village":
                    return new[] { "Fishing Village", "Pearl Diver Family", "Net Maker Household", "Harpooner Family", "Tide Pool Hamlet", "Crab Boat Family", "Whaler Dock Family", "Lagoon Hut", "Salt Curer Family", "Storm Net Village" };
                case "Forge Quarter":
                    return new[] { "Forge Quarter", "Blacksmith Family", "Foundry Row Birth", "Charcoal Burner Family", "Armorer Household", "Bellows Room Birth", "Iron Market Family", "Smelter Alley", "Horseshoe Shop Family", "Steam Workshop Birth" };
                case "Nomad Caravan":
                    return new[] { "Nomad Caravan", "Camel Train Family", "Wagon Circle Birth", "Road Clan", "Map Seller Family", "Horse Trader Caravan", "Salt Caravan Birth", "Bannerless Caravan", "Desert Cart Family", "Festival Caravan" };
                case "Noble House":
                    return new[] { "Noble House", "Minor Noble Family", "Estate Servant Birth", "Courtier Household", "Exiled Noble Family", "Manor Nursery", "House Guard Family", "Tutor Wing Birth", "Silk Merchant Dynasty", "Inheritance Dispute Birth" };
                case "Prison Town":
                    return new[] { "Prison Town", "Jailer Family", "Convict Quarter Birth", "Gatehouse Household", "Chain Yard Family", "Watchtower Birth", "Execution Square Alley", "Prison Kitchen Family", "Warden Servant Family", "Parole Camp Birth" };
                case "Storm Coast":
                    return new[] { "Storm Coast", "Lighthouse Family", "Shipwreck Cove Birth", "Cliff Harbor Family", "Rain Barrel Hamlet", "Sea Wall Household", "Rescue Boat Family", "Tide Cave Birth", "Storm Shrine Village", "Coastal Watch Family" };
                case "Woodcutters":
                    return new[] { "Woodcutter Family", "Forest Cabin Birth", "Timber Camp Family", "Charcoal Kiln Household", "Pine Ridge Family", "River Log Driver Family", "Sawpit Hamlet", "Hunting Lodge Birth", "Bark Gatherer Family", "Forest Shrine Family" };
                case "Street Clinic":
                    return new[] { "Street Clinic", "Midwife Alley Birth", "Herbalist Backroom", "Charity Ward Birth", "Battlefield Clinic Family", "Plague Cart Birth", "Bone Setter Household", "Doctor Debt Family", "Canal Sickroom", "Night Clinic Birth" };
                case "Arena District":
                    return new[] { "Arena District", "Gladiator Barracks Birth", "Ticket Seller Family", "Pit Gate Household", "Beast Pen Birth", "Prize Fighter Family", "Arena Medic Family", "Training Yard Birth", "Crowd Vendor Family", "Blood Sand Quarter" };
                case "Monk Refuge":
                    return new[] { "Monk Refuge", "Silent Cloister Birth", "Pilgrim Refuge Family", "Mountain Hermitage", "Refuge Kitchen Birth", "Scriptorium Household", "Prayer Hall Foundling", "Monk Farm Family", "Cave Shrine Birth", "Disciples' Dormitory" };
                case "Sailor Blood":
                    return new[] { "Sailor Family", "Harbor Sailor Family", "River Barge Family", "Deckhand Household", "Navigator Family", "Shipwright Family", "Pearl Boat Family", "Smuggler Dock Birth", "Rope Maker Family", "Far Harbor Birth" };
                case "Border Farm":
                    return new[] { "Border Farm", "Watch Fence Family", "Militia Farmstead", "Raided Orchard Birth", "Goat Pen Household", "Frontier Wheat Farm", "Border Stable Family", "Truce Road Farm", "Signal Hill Family", "Checkpoint Farmhouse" };
                default:
                    return new[] { anchor };
            }
        }

        private static string[] GetChildhoodVariants(string anchor)
        {
            switch (anchor)
            {
                case "Orphanage":
                    return new[] { "City Orphanage", "Temple Orphanage", "Workhouse Dormitory", "War Orphan Ward", "Foundling Home", "Canal Orphanage", "Guild Foster Home", "Barracks Fosterage", "Poorhouse Dormitory", "Street Foundling Years" };
                case "Wild Child":
                    return new[] { "Forest Survival", "Rooftop Survival", "Cave Shelter Years", "Riverbank Survival", "Dump Yard Survival", "Abandoned Farm Years", "Mountain Foraging", "Canal Tunnel Survival", "Marsh Hut Childhood", "Wild Road Childhood" };
                case "Good School":
                    return new[] { "Charity School", "Guild School", "Temple School", "Merchant School", "Village Classroom", "Noble Tutor Lessons", "Scribe School", "Military Academy Prep", "Doctor Apprentice Lessons", "Dojo Scholarship" };
                case "Street Gang":
                    return new[] { "Pickpocket Crew", "Alley Gang", "Market Thief Circle", "Bridge Toll Kids", "Dockside Crew", "Knife Errand Gang", "Rooftop Lookout Crew", "Canal Smuggler Kids", "Dice Den Runners", "Backstreet Protection Crew" };
                case "Stable Work":
                    return new[] { "Trade Road Stables", "Noble Stables", "Caravan Stables", "Racehorse Yard", "Mule Train Work", "Courier Horse Yard", "Warhorse Grooming", "Market Stable Work", "Inn Yard Stables", "Border Post Stables" };
                case "Kitchen Yard":
                    return new[] { "Monastery Kitchen", "Noble Kitchen Yard", "Dockside Soup Kitchen", "Army Mess Tent", "Market Cook Stall", "Temple Kitchen Chores", "Inn Kitchen Work", "Butcher Kitchen Yard", "Festival Cook Crew", "Orphanage Kitchen" };
                case "Mine Tunnels":
                    return new[] { "Coal Mine Tunnels", "Silver Mine Tunnels", "Salt Mine Work", "Crystal Mine Tunnels", "Quarry Cart Runs", "Flooded Mine Shafts", "Lantern Boy Work", "Ore Sorting Yard", "Tunnel Brace Crew", "Mine Mule Work" };
                case "Library Dust":
                    return new[] { "Temple Library", "Noble Archive", "Map Room Errands", "Scribe Hall", "Medical Library", "Dojo Records Room", "Tax Archive", "Forbidden Shelf Chores", "Bookbinder Table", "Scholar House Lessons" };
                case "Fisher Nets":
                    return new[] { "Fisher Nets", "Crab Pot Work", "Pearl Diving Lessons", "Harpoon Practice", "Tide Pool Gathering", "Boat Repair Yard", "Salt Fish Drying", "River Net Work", "Lagoon Traps", "Night Fishing Runs" };
                case "Temple Bells":
                    return new[] { "Temple Bell Chores", "Shrine Sweeping", "Pilgrim Gate Duty", "Incense Hall Work", "Prayer Drum Lessons", "Monk Garden Chores", "Temple Laundry", "Offering Bowl Duty", "Meditation Hall Chores", "Festival Bell Duty" };
                case "Market Runner":
                    return new[] { "Market Runner", "Courier Child", "Spice Stall Errands", "Fish Market Runs", "Auction House Runner", "Debt Message Runner", "Medicine Delivery Child", "Gate Toll Runner", "Festival Errand Child", "Street Vendor Helper" };
                case "Butcher Block":
                    return new[] { "Butcher Block", "Slaughter Yard Work", "Tannery Yard", "Fish Cutting Table", "Meat Market Helper", "Bone Cart Duty", "Smokehouse Work", "Hide Scraper Table", "Knife Cleaning Chores", "Blood Drain Yard" };
                case "Rooftop Games":
                    return new[] { "Rooftop Games", "Chimney Climbing", "Tile Runner Childhood", "Bell Tower Races", "Laundry Line Jumps", "Rooftop Messenger Work", "Gutter Path Games", "Pigeon Loft Chases", "Roof Garden Hideouts", "Moonlit Roof Races" };
                case "Old Hospital":
                    return new[] { "Old Hospital", "Charity Ward", "Sickroom Helper", "Herbalist Ward", "Bone Setter Room", "Plague Ward Errands", "Recovery Hall", "Surgery Lamp Duty", "Medicine Shelf Work", "Ward Porter Job" };
                case "Burned Hamlet":
                    return new[] { "Burned Hamlet", "Ash Field Childhood", "Rebuilt Village", "Refugee Camp Years", "Ruined Mill Shelter", "Scavenger Field Work", "Collapsed Chapel Shelter", "Firebreak Camp", "Charcoal Ruins", "Evacuation Road Years" };
                case "Winter Road":
                    return new[] { "Winter Road", "Snow Caravan Childhood", "Frozen Pass Crossing", "Ice Fishing Camp", "Sled Runner Years", "Fur Trader Road", "Mountain Winter Shelter", "White Road March", "Cold Inn Yard", "Blizzard Refuge Years" };
                case "Debt House":
                    return new[] { "Debt House", "Pawnshop Childhood", "Collector Errands", "Ledger Room Chores", "Debt Kitchen Work", "Hostage Ward", "Contract Copying", "Loan Office Runner", "Auction Yard Childhood", "Indenture House" };
                case "Circus Tent":
                    return new[] { "Circus Tent", "Acrobat Ropes", "Knife Thrower Assistant", "Animal Cage Chores", "Juggler Cart", "Strongman Yard", "Ticket Booth Childhood", "Mask Painter Table", "Traveling Stage", "Fire Eater Lessons" };
                case "Hidden Valley":
                    return new[] { "Hidden Valley", "Mountain Valley Farm", "Secret Orchard", "Hermit Village", "Valley Watch Duty", "Cave Garden Childhood", "Sealed Pass Hamlet", "Hidden River Camp", "Goat Valley Childhood", "Forbidden Valley School" };
                case "Training Hall":
                    return new[] { "Training Hall", "Dojo Floor Chores", "Sparring Dummy Duty", "Weapon Rack Work", "Mat Sweeping Years", "Footwork Line Drills", "Morning Form Practice", "Grappling Yard", "Beginner Ring Duty", "Instructor Shadowing" };
                default:
                    return new[] { anchor };
            }
        }

        private static string[] GetYouthVariants(string anchor)
        {
            switch (anchor)
            {
                case "Army Service":
                    return new[] { "Army Recruit", "Army Scout", "Camp Guard", "Shield Line Soldier", "Supply Porter", "War Drummer", "Field Cook Soldier", "Siege Crew Hand", "Border Patrol", "Deserter Years" };
                case "Teacher Years":
                    return new[] { "Village Teacher", "Dojo Instructor", "Tutor Work", "Weapon Drill Teacher", "Literacy Teacher", "Monastery Tutor", "Street Fighting Coach", "Children's Form Teacher", "Mercenary Trainer", "Assistant Instructor" };
                case "Mechanic Shop":
                    return new[] { "Mechanic Shop", "Steam Engine Garage", "Cart Repair Yard", "Clockwork Bench", "Mill Gear Workshop", "Rail Depot Repairs", "Boiler Room Work", "Locksmith Bench", "Pump Repair Shop", "Scrapyard Mechanic" };
                case "Plumber Work":
                    return new[] { "Plumber Work", "Sewer Crew", "Canal Pipe Repair", "Bathhouse Maintenance", "Well Digging Crew", "Flood Drain Work", "Pump House Shift", "Aqueduct Repair", "Cellar Pipe Work", "Street Valve Crew" };
                case "Dock Labor":
                    return new[] { "Dock Labor", "Cargo Loader", "Rope Gang", "Crate Stacker", "Harbor Crane Crew", "Fish Dock Shift", "Smuggler Pier Work", "Customs Yard Labor", "Dry Dock Crew", "Night Wharf Work" };
                case "Arena Debut":
                    return new[] { "Arena Debut", "Pit Fighter Circuit", "Opening Bout Work", "Beast Gate Trial", "Prize Fight Circuit", "Undercard Fighter", "Sand Ring Debut", "Traveling Arena Bout", "Blood Sport Contract", "Crowd Favorite Run" };
                case "Monastery Trial":
                    return new[] { "Monastery Trial", "Fasting Trial", "Stone Stair Trial", "Bell Tower Trial", "Silent Yard Trial", "Water Carrying Trial", "Candle Vigil Trial", "Scripture Combat Test", "Mountain Run Trial", "Gatekeeper Trial" };
                case "Bandit Season":
                    return new[] { "Bandit Season", "Road Ambush Crew", "Forest Raider Work", "Caravan Robber Years", "Toll Bridge Gang", "Hideout Guard Work", "Smuggler Escort", "Raid Planner Work", "Fence Runner Job", "Outlaw Camp Years" };
                case "Merchant Guard":
                    return new[] { "Merchant Guard", "Caravan Escort", "Market House Guard", "Spice Road Guard", "Bank Wagon Escort", "Noble Cargo Guard", "Warehouse Watch", "Auction Hall Guard", "Trade Gate Guard", "Counting House Guard" };
                case "Courier Route":
                    return new[] { "Courier Route", "Mountain Messenger", "City Runner", "Battlefield Courier", "Medicine Runner", "Noble Letter Route", "Border Dispatch Work", "Canal Messenger", "Night Runner Route", "Tournament Notice Runner" };
                case "Blacksmith Helper":
                    return new[] { "Blacksmith Helper", "Armorer Apprentice", "Bellows Worker", "Horseshoe Forge", "Blade Polishing Work", "Chain Maker Bench", "Anvil Striker Job", "Foundry Assistant", "Toolmaker Shop", "Shield Rivet Work" };
                case "Hunter Lodge":
                    return new[] { "Hunter Lodge", "Trapline Work", "Hunter Crew", "Boar Hunt Escort", "Falconry Yard", "Monster Trackers", "Pelt Trader Work", "Forest Ranger Shift", "Night Watch Hunt", "Guide Work" };
                case "Sailor Contract":
                    return new[] { "Sailor Contract", "Deckhand Work", "Navigator Assistant", "Harpoon Boat Crew", "River Barge Contract", "Pearl Boat Work", "Ship Cook Duty", "Rope Master Helper", "Storm Sail Crew", "Far Harbor Contract" };
                case "Field Medic":
                    return new[] { "Field Medic", "Battlefield Stretcher", "Herbalist Assistant", "Surgery Tent Work", "Bone Setter Aide", "Camp Nurse Work", "Plague Cart Medic", "Tournament Medic Work", "Mercenary Medic", "Recovery Ward Job" };
                case "Quarry Crew":
                    return new[] { "Quarry Crew", "Stone Hauler", "Hammer Line Work", "Cart Track Crew", "Granite Yard Labor", "Slate Splitter Work", "Lime Kiln Shift", "Cliff Quarry Crew", "Road Stone Work", "Dust Pit Labor" };
                case "Duelist Circle":
                    return new[] { "Duelist Circle", "Salon Duelist", "Street Duel Circuit", "Noble Challenge Work", "First Blood Contract", "Blade Witness Work", "Duel Instructor Job", "Tournament Seconds", "Honor Bout Circuit", "Private Duel Work" };
                case "Scholar Job":
                    return new[] { "Scholar Job", "Map Copyist", "Anatomy Assistant", "Combat Notes Scribe", "Archive Clerk", "Library Guard", "Battle Historian Aide", "Monster Bestiary Work", "Engineering Scribe", "Medical Sketch Artist" };
                case "Street Performer":
                    return new[] { "Street Performer", "Acrobat Act", "Knife Juggler Work", "Fire Dance Act", "Mask Theater Work", "Strongman Show", "Escape Artist Act", "Market Stage Work", "Festival Performer", "Traveling Troupe" };
                case "Bodyguard Work":
                    return new[] { "Bodyguard Work", "Noble Bodyguard", "Merchant Bodyguard", "Debt Collector Guard", "Stage Performer Guard", "Doctor Escort Work", "Caravan VIP Guard", "Court Hall Guard", "Gang Boss Guard", "Witness Protection Work" };
                case "Dojo Assistant":
                    return new[] { "Dojo Assistant", "Mat Sweeper Job", "Weapon Rack Keeper", "Beginner Class Aide", "Sparring Partner Work", "Tournament Bracket Clerk", "Dojo Door Guard", "Master's Errand Runner", "Practice Dummy Handler", "Form Demonstrator" };
                default:
                    return new[] { anchor };
            }
        }

        private static string BuildLoreFragment(ChampionLoreStage stage, string anchor, string title)
        {
            switch (stage)
            {
                case ChampionLoreStage.Birth:
                    return BuildBirthLoreFragment(anchor, title);
                case ChampionLoreStage.Childhood:
                    return BuildChildhoodLoreFragment(anchor, title);
                default:
                    return BuildYouthLoreFragment(anchor, title);
            }
        }

        private static string BuildBirthLoreFragment(string anchor, string title)
        {
            return $"{{name}} was born {BirthOriginPhrase(anchor, title)}, {BirthContext(anchor)}.";
        }

        private static string BuildChildhoodLoreFragment(string anchor, string title)
        {
            return $"As a child, {{name}} lived through {ArticlePhrase(title)}, {ChildhoodContext(anchor)}.";
        }

        private static string BuildYouthLoreFragment(string anchor, string title)
        {
            return $"As a young adult, {{name}} worked through {ArticlePhrase(title)}, {YouthContext(anchor)}. They enter the dojo to {YouthMotive(anchor)}.";
        }

        private static string BirthOriginPhrase(string anchor, string title)
        {
            switch (anchor)
            {
                case "Sword Family":
                case "Poor Family":
                case "Dojo Lineage":
                case "Woodcutters":
                case "Noble House":
                case "Sailor Blood":
                    return $"into {ArticlePhrase(title)}";
                case "Temple Step":
                    return $"on {ArticlePhrase(title)}";
                case "Desert Birth":
                case "Mountain Clan":
                case "River House":
                case "Fishing Village":
                case "Forge Quarter":
                case "Prison Town":
                case "Storm Coast":
                case "Street Clinic":
                case "Arena District":
                case "Monk Refuge":
                case "Border Farm":
                    return $"in {ArticlePhrase(title)}";
                default:
                    return $"around {ArticlePhrase(title)}";
            }
        }

        private static string BirthContext(string anchor)
        {
            switch (anchor)
            {
                case "Sword Family":
                    return "where weapons, footwork, and bruised pride were part of ordinary family life";
                case "Poor Family":
                    return "where food, debt, and shelter were problems children learned to understand too early";
                case "Desert Birth":
                    return "where water, shade, and distance shaped every decision";
                case "Mountain Clan":
                    return "where balance, lungs, and stubborn legs mattered before formal schooling";
                case "River House":
                    return "where floods, ferries, and rope work taught respect for timing";
                case "Dojo Lineage":
                    return "where old forms and old debts were treated as inheritance";
                case "Mercenary Camp":
                    return "where paid violence, marching boots, and camp discipline surrounded the cradle";
                case "Temple Step":
                    return "where bells, chores, and ritual discipline marked the first memories";
                case "Fishing Village":
                    return "where nets, boats, and empty mornings taught patience";
                case "Forge Quarter":
                    return "where heat, hammer rhythm, and metalwork filled the streets";
                case "Nomad Caravan":
                    return "where packing quickly and reading roads mattered more than owning walls";
                case "Noble House":
                    return "where manners, inheritance, and hidden rivalries carried their own kind of danger";
                case "Prison Town":
                    return "where locks, guards, and suspicion were part of the local weather";
                case "Storm Coast":
                    return "where wind, waves, and rescue bells taught people to move before panic";
                case "Woodcutters":
                    return "where axes fed the family and falling timber punished carelessness";
                case "Street Clinic":
                    return "where blood, bandages, and unpaid favors were never far away";
                case "Arena District":
                    return "where crowds cheered pain before they learned the names of fighters";
                case "Monk Refuge":
                    return "where silence, breath, and repeated chores were treated as survival tools";
                case "Sailor Blood":
                    return "where ropes, tides, and returning alive mattered more than stories about courage";
                case "Border Farm":
                    return "where harvests, fences, raids, and militia drills shared the same seasons";
                default:
                    return "where survival became the first lesson";
            }
        }

        private static string ChildhoodContext(string anchor)
        {
            switch (anchor)
            {
                case "Orphanage":
                    return "learning to compete for space, food, attention, and the right to be remembered";
                case "Wild Child":
                    return "learning from weather, hunger, hiding places, and mistakes nobody corrected";
                case "Good School":
                    return "learning letters, rivalry, discipline, and how rules can be used as weapons";
                case "Street Gang":
                    return "learning shortcuts, loyalty, dirty angles, and the price of fear";
                case "Stable Work":
                    return "building strength and balance around animals, carts, mud, and long roads";
                case "Kitchen Yard":
                    return "learning timing, heat, patience, and how to watch fighters while pretending to work";
                case "Mine Tunnels":
                    return "learning breath control, endurance, and courage in cramped darkness";
                case "Library Dust":
                    return "learning memory, patterns, and the habit of studying before acting";
                case "Fisher Nets":
                    return "learning balance, cold patience, and hand strength before sunrise";
                case "Temple Bells":
                    return "learning ritual calm, chores, fasting, and the value of repetition";
                case "Market Runner":
                    return "learning speed through crowds and how to read danger while moving";
                case "Butcher Block":
                    return "learning anatomy, steady hands, and why blood should not freeze the mind";
                case "Rooftop Games":
                    return "learning balance above danger and confidence after every fall";
                case "Old Hospital":
                    return "learning pain, recovery, herbs, and the fragile mechanics of bodies";
                case "Burned Hamlet":
                    return "learning salvage, grief, and how quickly ordinary life can vanish";
                case "Winter Road":
                    return "learning cold endurance and the cost of stopping too soon";
                case "Debt House":
                    return "learning what freedom costs and why promises can become chains";
                case "Circus Tent":
                    return "learning masks, timing, falls, applause, and public courage";
                case "Hidden Valley":
                    return "learning caution, secrecy, and suspicion of outsiders";
                case "Training Hall":
                    return "learning stances, bruises, repetition, and formal violence";
                default:
                    return "learning to turn childhood into survival";
            }
        }

        private static string YouthContext(string anchor)
        {
            switch (anchor)
            {
                case "Army Service":
                    return "turning drills, fear, formation work, and orders into practical violence";
                case "Teacher Years":
                    return "turning correction, repetition, and bruised students into a sharper understanding of technique";
                case "Mechanic Shop":
                    return "turning leverage, pressure, and heavy tools into a private fighting sense";
                case "Plumber Work":
                    return "learning angles, pressure, balance, and ugly work beneath the streets";
                case "Dock Labor":
                    return "hardening the body through ropes, crates, cargo, and cheap fights";
                case "Arena Debut":
                    return "discovering that pain becomes easier to bear when a crowd gives it a price";
                case "Monastery Trial":
                    return "stripping vanity away through fasting, chores, forms, and punishment";
                case "Bandit Season":
                    return "learning ambushes, bad roads, guilt, and the cost of trusting desperate people";
                case "Merchant Guard":
                    return "turning caravan routes and roadside fights into a working education";
                case "Courier Route":
                    return "learning speed, timing, breath, and how to keep moving while chased";
                case "Blacksmith Helper":
                    return "hardening hands through heat, rhythm, metal, and repeated impact";
                case "Hunter Lodge":
                    return "learning tracking, patience, distance, and how predators choose angles";
                case "Sailor Contract":
                    return "trading comfort for balance, rope-burned hands, and distance from shore";
                case "Field Medic":
                    return "learning where bodies fail and how panic moves through a fight";
                case "Quarry Crew":
                    return "turning stone dust, repetition, and impact into stubborn strength";
                case "Duelist Circle":
                    return "learning distance, witnesses, pride, and the price of public mistakes";
                case "Scholar Job":
                    return "copying maps, wounds, and combat notes until patterns became weapons";
                case "Street Performer":
                    return "turning timing, balance, masks, and public risk into survival";
                case "Bodyguard Work":
                    return "reading doorways, hands, threats, and the cowardice of richer people";
                case "Dojo Assistant":
                    return "sweeping floors, correcting stances, and stealing lessons from the edge of the mat";
                default:
                    return "turning ordinary work into a reason to fight";
            }
        }

        private static string YouthMotive(string anchor)
        {
            switch (anchor)
            {
                case "Army Service":
                    return "win coin without taking orders";
                case "Teacher Years":
                    return "prove their lessons survive opponents who hit back";
                case "Mechanic Shop":
                    return "save the workshop that gave them a trade";
                case "Plumber Work":
                    return "buy a life above the cellars";
                case "Dock Labor":
                    return "escape dock wages before the docks break them";
                case "Arena Debut":
                    return "turn a first taste of applause into real prize money";
                case "Monastery Trial":
                    return "test discipline against bloodier truths";
                case "Bandit Season":
                    return "buy distance from old crimes";
                case "Merchant Guard":
                    return "earn more than a season of escort work";
                case "Courier Route":
                    return "turn speed into fame instead of errands";
                case "Blacksmith Helper":
                    return "repay the forge that fed them";
                case "Hunter Lodge":
                    return "hunt opponents instead of tracks";
                case "Sailor Contract":
                    return "choose the arena before the sea chooses for them";
                case "Field Medic":
                    return "pay for medicine, debts, or mercy";
                case "Quarry Crew":
                    return "escape the quarry with tournament coin";
                case "Duelist Circle":
                    return "make their name public";
                case "Scholar Job":
                    return "test theory against pain";
                case "Street Performer":
                    return "trade applause for prize money";
                case "Bodyguard Work":
                    return "stop bleeding for someone else's purse";
                case "Dojo Assistant":
                    return "stand on the mat as a fighter";
                default:
                    return "turn survival into a future";
            }
        }

        private static string Article(string word)
        {
            if (string.IsNullOrEmpty(word))
            {
                return "a";
            }

            char first = char.ToLowerInvariant(word[0]);
            return first == 'a' || first == 'e' || first == 'i' || first == 'o' || first == 'u' ? "an" : "a";
        }

        private static string ArticlePhrase(string title)
        {
            string value = title.ToLowerInvariant();
            if (UsesDefiniteArticle(value))
            {
                return $"the {value}";
            }

            return $"{Article(value)} {value}";
        }

        private static bool UsesDefiniteArticle(string value)
        {
            if (value.EndsWith("ss"))
            {
                return false;
            }

            return value.EndsWith("s");
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
            Add(specs, "life_touched", "Life Touched", PerkRarity.Common, "A little more meat on the frame. Effect: +50 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 50);
            Add(specs, "life_trained", "Life Trained", PerkRarity.Rare, "Regular conditioning makes the champion harder to drop. Effect: +100 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 100);
            Add(specs, "life_hardened", "Life Hardened", PerkRarity.VeryRare, "Punishment has made the body stubborn. Effect: +150 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 150);
            Add(specs, "life_perfected", "Life Perfected", PerkRarity.Epic, "A serious reserve of blood and breath. Effect: +200 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 200);
            Add(specs, "life_myth", "Life Myth", PerkRarity.Legendary, "The champion feels unfairly alive. Effect: +250 Life.", PerkEffectType.CombatStatBonus, combatStat: PerkCombatStatType.Life, value: 250);
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

            if (IsDamageDotCharge(charge))
            {
                Add(specs, $"{id}_aftertouch", $"{shortName} Aftertouch", PerkRarity.VeryRare, $"After each action, {verb} 1 {label} stack on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 1);
                Add(specs, $"{id}_rhythm", $"{shortName} Rhythm", PerkRarity.Epic, $"After each action, {verb} 2 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 2);
                Add(specs, $"{id}_legend", legendaryName, PerkRarity.Legendary, $"After each action, {verb} 3 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 3);
                return;
            }

            if (charge == ChargeType.Caltrops)
            {
                Add(specs, $"{id}_aftertouch", $"{shortName} Aftertouch", PerkRarity.Rare, $"After each action, {verb} 1 {label} stack on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 1);
                Add(specs, $"{id}_rhythm", $"{shortName} Rhythm", PerkRarity.VeryRare, $"After each action, {verb} 2 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 2);
                Add(specs, $"{id}_engine", $"{shortName} Engine", PerkRarity.Epic, $"After each action, {verb} 3 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 3);
                Add(specs, $"{id}_legend", legendaryName, PerkRarity.Legendary, $"After each action, {verb} 4 {label} stacks on {target}.", PerkEffectType.ApplyChargeOnAction, charge: charge, value: 4);
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

        private static bool IsDamageDotCharge(ChargeType charge)
        {
            switch (charge)
            {
                case ChargeType.Burn:
                case ChargeType.Poison:
                case ChargeType.Drowning:
                case ChargeType.Shock:
                case ChargeType.Splinter:
                case ChargeType.Crush:
                case ChargeType.WindShear:
                case ChargeType.Bleed:
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
                Add(specs, $"{element.id}_sting", $"{element.dotName} Sting", PerkRarity.VeryRare, $"Every action leaves a trace of {element.dotName}. Effect: apply 1 {element.charge} stack after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 1);
                Add(specs, $"{element.id}_sentence", $"{element.dotName} Sentence", PerkRarity.Epic, $"The element keeps working after the strike. Effect: apply 2 {element.charge} stacks after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 2);
                Add(specs, $"{element.id}_doom", $"{element.doomName}", PerkRarity.Legendary, $"The arena slowly becomes {element.dotName}. Effect: apply 3 {element.charge} stacks after each action.", PerkEffectType.ApplyChargeOnAction, element: element.element, charge: element.charge, value: 3);
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
