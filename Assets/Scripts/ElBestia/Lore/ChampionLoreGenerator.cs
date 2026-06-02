using System;
using ElBestia.Champions;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Lore
{
    public static class ChampionLoreGenerator
    {
        public static ChampionLoreProfile CreateRandom(string championName, System.Random rng)
        {
            ChampionLoreEntrySO birth = Pick(ChampionLoreStage.Birth, rng);
            ChampionLoreEntrySO childhood = Pick(ChampionLoreStage.Childhood, rng);
            ChampionLoreEntrySO youth = Pick(ChampionLoreStage.Youth, rng);

            return new ChampionLoreProfile
            {
                birth = birth,
                childhood = childhood,
                youth = youth,
                story = BuildStory(championName, birth, childhood, youth)
            };
        }

        public static void ApplyToStats(ChampionLoreProfile profile, ChampionStats stats)
        {
            if (profile == null || stats == null)
            {
                return;
            }

            ChampionStats baseSnapshot = stats.Clone();
            int[] additive = new int[5];
            float[] multiplier = { 1f, 1f, 1f, 1f, 1f };
            CollectBaseModifiers(profile.birth, additive, multiplier);
            CollectBaseModifiers(profile.childhood, additive, multiplier);
            CollectBaseModifiers(profile.youth, additive, multiplier);

            for (int i = 0; i < 5; i++)
            {
                ChampionStatType stat = (ChampionStatType)i;
                int baseValue = GetBaseStat(baseSnapshot, stat);
                int value = Mathf.RoundToInt((baseValue + additive[i]) * multiplier[i]);
                SetBaseStat(stats, stat, Mathf.Max(0, value));
            }

            stats.RecalculateDerivedResources();

            ApplyDerived(profile.birth, stats);
            ApplyDerived(profile.childhood, stats);
            ApplyDerived(profile.youth, stats);
        }

        private static ChampionLoreEntrySO Pick(ChampionLoreStage stage, System.Random rng)
        {
            string folder = stage.ToString();
            ChampionLoreEntrySO[] entries = Resources.LoadAll<ChampionLoreEntrySO>($"Lore/{folder}");
            if (entries == null || entries.Length == 0)
            {
                entries = Resources.LoadAll<ChampionLoreEntrySO>("Lore");
            }

            if (entries != null && entries.Length > 0)
            {
                for (int attempts = 0; attempts < 20; attempts++)
                {
                    ChampionLoreEntrySO entry = entries[rng.Next(0, entries.Length)];
                    if (entry != null && entry.Stage == stage)
                    {
                        return entry;
                    }
                }
            }

            return CreateFallback(stage, rng);
        }

        private static ChampionLoreEntrySO CreateFallback(ChampionLoreStage stage, System.Random rng)
        {
            string[] anchors = GetFallbackAnchors(stage);
            string[] tones = { "Hard", "Quiet", "Lucky", "Hungry", "Bright", "Cruel", "Patient", "Restless", "Broken", "Proud" };
            int anchorIndex = rng.Next(0, anchors.Length);
            int toneIndex = rng.Next(0, tones.Length);
            string anchor = anchors[anchorIndex];
            string tone = tones[toneIndex];
            string id = $"fallback_{stage.ToString().ToLowerInvariant()}_{anchorIndex:00}_{toneIndex:00}";
            string title = $"{tone} {anchor}";
            return ChampionLoreEntrySO.CreateRuntime(id, stage, title, BuildFallbackFragment(stage, anchor, tone), BuildFallbackModifiers(anchorIndex, toneIndex));
        }

        private static string[] GetFallbackAnchors(ChampionLoreStage stage)
        {
            switch (stage)
            {
                case ChampionLoreStage.Birth:
                    return new[] { "Sword Family", "Poor Family", "Desert Birth", "Mountain Clan", "River House", "Dojo Lineage", "Mercenary Camp", "Temple Step", "Fishing Village", "Forge Quarter", "Nomad Caravan", "Noble House", "Prison Town", "Storm Coast", "Woodcutters", "Street Clinic", "Arena District", "Monk Refuge", "Sailor Blood", "Border Farm" };
                case ChampionLoreStage.Childhood:
                    return new[] { "Orphanage", "Wild Child", "Good School", "Street Gang", "Stable Work", "Kitchen Yard", "Mine Tunnels", "Library Dust", "Fisher Nets", "Temple Bells", "Market Runner", "Butcher Block", "Rooftop Games", "Old Hospital", "Burned Hamlet", "Winter Road", "Debt House", "Circus Tent", "Hidden Valley", "Training Hall" };
                default:
                    return new[] { "Army Service", "Teacher Years", "Mechanic Shop", "Plumber Work", "Dock Labor", "Arena Debut", "Monastery Trial", "Bandit Season", "Merchant Guard", "Courier Route", "Blacksmith Helper", "Hunter Lodge", "Sailor Contract", "Field Medic", "Quarry Crew", "Duelist Circle", "Scholar Job", "Street Performer", "Bodyguard Work", "Dojo Assistant" };
            }
        }

        private static string BuildFallbackFragment(ChampionLoreStage stage, string anchor, string tone)
        {
            switch (stage)
            {
                case ChampionLoreStage.Birth:
                    return $"{{name}} was born around {Readable(anchor)}, and the household was shaped by {TonePhrase(tone)}.";
                case ChampionLoreStage.Childhood:
                    return $"As a child, {{name}} passed through {Readable(anchor)}, learning to survive through {TonePhrase(tone)}.";
                default:
                    return $"When youth came, {{name}} found work in {Readable(anchor)}, and violence gave {TonePhrase(tone)} a purpose.";
            }
        }

        private static ChampionLoreModifier[] BuildFallbackModifiers(int anchorIndex, int toneIndex)
        {
            ChampionStatType stat = (ChampionStatType)(anchorIndex % 5);
            ChampionStatType secondStat = (ChampionStatType)((anchorIndex + toneIndex + 2) % 5);
            WeaponType weapon = (WeaponType)((anchorIndex % 6) + 1);
            SkillElement element = GetFallbackElement(anchorIndex + toneIndex);
            PerkCombatStatType combatStat = GetFallbackCombat(toneIndex);
            int primary = 1 + toneIndex % 3;
            int secondary = 1 + anchorIndex % 2;

            switch ((anchorIndex + toneIndex) % 4)
            {
                case 0:
                    return new[] { Base(stat, primary), Weapon(weapon, 2 + toneIndex % 3) };
                case 1:
                    return new[] { Base(stat, primary), Base(secondStat, secondary) };
                case 2:
                    return new[] { Element(element, 3 + toneIndex % 4), Base(stat, secondary) };
                default:
                    return new[] { Combat(combatStat, 2 + toneIndex % 4), Weapon(weapon, 1 + anchorIndex % 3) };
            }
        }

        private static SkillElement GetFallbackElement(int index)
        {
            SkillElement[] elements = { SkillElement.Fire, SkillElement.Water, SkillElement.Electricity, SkillElement.Poison, SkillElement.Earth, SkillElement.Air, SkillElement.Wood };
            return elements[Mathf.Abs(index) % elements.Length];
        }

        private static PerkCombatStatType GetFallbackCombat(int index)
        {
            PerkCombatStatType[] stats = { PerkCombatStatType.DodgeRating, PerkCombatStatType.BlockRating, PerkCombatStatType.CriticalRating, PerkCombatStatType.HitRating, PerkCombatStatType.Recovery, PerkCombatStatType.Life, PerkCombatStatType.Energy };
            return stats[Mathf.Abs(index) % stats.Length];
        }

        private static string BuildStory(string championName, ChampionLoreEntrySO birth, ChampionLoreEntrySO childhood, ChampionLoreEntrySO youth)
        {
            string name = string.IsNullOrEmpty(championName) ? "This champion" : championName;
            return $"{Fragment(birth, name)} {Fragment(childhood, name)} {Fragment(youth, name)}";
        }

        private static string Fragment(ChampionLoreEntrySO entry, string championName)
        {
            string text = entry != null && !string.IsNullOrEmpty(entry.StoryFragment)
                ? entry.StoryFragment
                : "{name} survived without leaving many records behind.";

            return text.Replace("{name}", championName);
        }

        private static string Readable(string value)
        {
            return value.ToLowerInvariant().Replace("  ", " ");
        }

        private static string TonePhrase(string tone)
        {
            switch (tone)
            {
                case "Hard":
                    return "a hard lesson about endurance";
                case "Quiet":
                    return "a habit of silence";
                case "Lucky":
                    return "an early reputation for impossible luck";
                case "Hungry":
                    return "the pressure of hunger";
                case "Bright":
                    return "a stubborn belief in a better future";
                case "Cruel":
                    return "a cruel edge learned too young";
                case "Patient":
                    return "the patience to wait for an opening";
                case "Restless":
                    return "a restless need to leave";
                case "Broken":
                    return "a broken beginning that never fully healed";
                case "Proud":
                    return "a pride that refused to bow";
                default:
                    return $"a {tone.ToLowerInvariant()} temperament";
            }
        }

        private static void CollectBaseModifiers(ChampionLoreEntrySO entry, int[] additive, float[] multiplier)
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
                    additive[index] += modifier.value;
                }
                else if (modifier.type == ChampionLoreModifierType.BaseStatMultiplier)
                {
                    multiplier[index] *= modifier.multiplier > 0f ? modifier.multiplier : 1f;
                }
            }
        }

        private static void ApplyDerived(ChampionLoreEntrySO entry, ChampionStats stats)
        {
            Apply(entry, stats, false);
        }

        private static void Apply(ChampionLoreEntrySO entry, ChampionStats stats, bool basePhase)
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

                if (!basePhase && modifier.type == ChampionLoreModifierType.CombatStat)
                {
                    ApplyCombat(stats, modifier.combatStat, modifier.value);
                }
                else if (!basePhase && modifier.type == ChampionLoreModifierType.WeaponProficiency)
                {
                    ApplyWeapon(stats, modifier.weapon, modifier.value);
                }
                else if (!basePhase && modifier.type == ChampionLoreModifierType.ElementalResistance)
                {
                    ApplyElement(stats, modifier.element, modifier.value);
                }
            }
        }

        public static ChampionLoreModifier Base(ChampionStatType stat, int value)
        {
            return new ChampionLoreModifier { type = ChampionLoreModifierType.BaseStat, stat = stat, value = value };
        }

        public static ChampionLoreModifier BaseMultiplier(ChampionStatType stat, float multiplier)
        {
            return new ChampionLoreModifier { type = ChampionLoreModifierType.BaseStatMultiplier, stat = stat, multiplier = multiplier };
        }

        public static ChampionLoreModifier Combat(PerkCombatStatType stat, int value)
        {
            return new ChampionLoreModifier { type = ChampionLoreModifierType.CombatStat, combatStat = stat, value = value };
        }

        public static ChampionLoreModifier Weapon(WeaponType weapon, int value)
        {
            return new ChampionLoreModifier { type = ChampionLoreModifierType.WeaponProficiency, weapon = weapon, value = value };
        }

        public static ChampionLoreModifier Element(SkillElement element, int value)
        {
            return new ChampionLoreModifier { type = ChampionLoreModifierType.ElementalResistance, element = element, value = value };
        }

        private static void ApplyCombat(ChampionStats stats, PerkCombatStatType combatStat, int value)
        {
            switch (combatStat)
            {
                case PerkCombatStatType.DodgeRating:
                    stats.dodge += value;
                    break;
                case PerkCombatStatType.BlockRating:
                    stats.block += value;
                    break;
                case PerkCombatStatType.CriticalRating:
                    stats.critical += value;
                    break;
                case PerkCombatStatType.HitRating:
                    stats.hit += value;
                    break;
                case PerkCombatStatType.Recovery:
                    stats.recovery += value;
                    break;
                case PerkCombatStatType.Life:
                    stats.life += value;
                    break;
                case PerkCombatStatType.Energy:
                    stats.energy += value;
                    break;
            }
        }

        private static void ApplyWeapon(ChampionStats stats, WeaponType weapon, int value)
        {
            switch (weapon)
            {
                case WeaponType.Fists:
                    stats.fistsProficiency += value;
                    break;
                case WeaponType.Sword:
                    stats.swordProficiency += value;
                    break;
                case WeaponType.Axe:
                    stats.axeProficiency += value;
                    break;
                case WeaponType.Spear:
                    stats.spearProficiency += value;
                    break;
                case WeaponType.Staff:
                    stats.staffProficiency += value;
                    break;
                case WeaponType.Bow:
                    stats.bowProficiency += value;
                    break;
            }
        }

        private static void ApplyElement(ChampionStats stats, SkillElement element, int value)
        {
            switch (element)
            {
                case SkillElement.Fire:
                    stats.fireResistance += value;
                    break;
                case SkillElement.Water:
                    stats.waterResistance += value;
                    break;
                case SkillElement.Electricity:
                    stats.electricityResistance += value;
                    break;
                case SkillElement.Poison:
                    stats.poisonResistance += value;
                    break;
                case SkillElement.Earth:
                    stats.earthResistance += value;
                    break;
                case SkillElement.Air:
                    stats.airResistance += value;
                    break;
                case SkillElement.Wood:
                    stats.woodResistance += value;
                    break;
            }
        }

        private static int GetBaseStat(ChampionStats stats, ChampionStatType stat)
        {
            switch (stat)
            {
                case ChampionStatType.Strength:
                    return stats.strength;
                case ChampionStatType.Agility:
                    return stats.agility;
                case ChampionStatType.Constitution:
                    return stats.constitution;
                case ChampionStatType.Intelligence:
                    return stats.intelligence;
                case ChampionStatType.Endurance:
                    return stats.endurance;
                default:
                    return 0;
            }
        }

        private static void SetBaseStat(ChampionStats stats, ChampionStatType stat, int value)
        {
            switch (stat)
            {
                case ChampionStatType.Strength:
                    stats.strength = value;
                    break;
                case ChampionStatType.Agility:
                    stats.agility = value;
                    break;
                case ChampionStatType.Constitution:
                    stats.constitution = value;
                    break;
                case ChampionStatType.Intelligence:
                    stats.intelligence = value;
                    break;
                case ChampionStatType.Endurance:
                    stats.endurance = value;
                    break;
            }
        }
    }
}
