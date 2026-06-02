using System.Text;
using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Lore;
using ElBestia.Perks;
using ElBestia.Skills;
using UnityEditor;
using UnityEngine;

namespace ElBestia.Editor
{
    [CustomEditor(typeof(ChampionSO))]
    public sealed class ChampionSOEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<PerkRarity, Texture2D> PerkBackgrounds = new Dictionary<PerkRarity, Texture2D>();

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawGenerationControls();

            var champion = (ChampionSO)target;
            DrawChampionHeader(champion);
            DrawLore(champion);
            DrawStatsTable(champion);
            DrawPerks(champion);
            DrawSkills(champion);
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGenerationControls()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("nameGenerator"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("level"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("totalBaseStatPoints"));

                EditorGUILayout.Space(6);
                if (GUILayout.Button("CreateRandomCharacter", GUILayout.Height(32)))
                {
                    var champion = (ChampionSO)target;
                    Undo.RecordObject(champion, "Create Random Character");
                    champion.CreateRandomCharacter();
                    EditorUtility.SetDirty(champion);
                    AssetDatabase.SaveAssets();
                }

                if (GUILayout.Button("Reapply Perks / XP", GUILayout.Height(24)))
                {
                    var champion = (ChampionSO)target;
                    Undo.RecordObject(champion, "Reapply Perks / XP");
                    champion.ReapplyPerksAndProgression();
                    EditorUtility.SetDirty(champion);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static void DrawLore(ChampionSO champion)
        {
            if (champion.Lore == null)
            {
                return;
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Lore", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawLoreLine("Birth", champion.Lore.birth);
                DrawLoreLine("Childhood", champion.Lore.childhood);
                DrawLoreLine("Youth", champion.Lore.youth);
                EditorGUILayout.LabelField("Story Key", champion.Lore.storyCombinationKey.ToString());
                EditorGUILayout.LabelField("Story ID", champion.Lore.storyCombinationId, EditorStyles.miniLabel);

                if (!string.IsNullOrEmpty(champion.Lore.story))
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(champion.Lore.story, EditorStyles.wordWrappedLabel);
                }
            }
        }

        private static void DrawLoreLine(string label, ChampionLoreEntrySO entry)
        {
            GUIContent content = entry != null
                ? new GUIContent(entry.Title, BuildLoreTooltip(entry))
                : new GUIContent("-");

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(76));
                EditorGUILayout.LabelField(content, EditorStyles.boldLabel);
            }
        }

        private static string BuildLoreTooltip(ChampionLoreEntrySO entry)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(entry.StoryFragment))
            {
                builder.AppendLine(entry.StoryFragment.Replace("{name}", "This champion"));
                builder.AppendLine();
            }

            string modifiers = BuildLoreModifierSummary(entry.Modifiers);
            builder.Append(string.IsNullOrEmpty(modifiers) ? "No modifiers." : modifiers);
            return builder.ToString();
        }

        private static string BuildLoreModifierSummary(ChampionLoreModifier[] modifiers)
        {
            if (modifiers == null || modifiers.Length == 0)
            {
                return string.Empty;
            }

            var additiveTotals = new Dictionary<string, int>();
            var multiplierTotals = new Dictionary<string, float>();
            var builder = new StringBuilder("Effects: ");
            foreach (ChampionLoreModifier modifier in modifiers)
            {
                if (modifier == null)
                {
                    continue;
                }

                string key = GetLoreModifierKey(modifier);
                if (modifier.type == ChampionLoreModifierType.BaseStatMultiplier)
                {
                    if (!multiplierTotals.ContainsKey(key))
                    {
                        multiplierTotals[key] = 1f;
                    }

                    multiplierTotals[key] *= modifier.multiplier > 0f ? modifier.multiplier : 1f;
                    continue;
                }

                if (!additiveTotals.ContainsKey(key))
                {
                    additiveTotals[key] = 0;
                }

                additiveTotals[key] += modifier.value;
            }

            foreach (KeyValuePair<string, int> total in additiveTotals)
            {
                if (builder.Length > "Effects: ".Length)
                {
                    builder.Append(", ");
                }

                builder.Append(FormatLoreModifier(total.Key, total.Value));
            }

            foreach (KeyValuePair<string, float> total in multiplierTotals)
            {
                if (builder.Length > "Effects: ".Length)
                {
                    builder.Append(", ");
                }

                builder.Append($"{total.Key} x{total.Value:0.##}");
            }

            return builder.ToString();
        }

        private static string GetLoreModifierKey(ChampionLoreModifier modifier)
        {
            switch (modifier.type)
            {
                case ChampionLoreModifierType.BaseStat:
                    return modifier.stat.ToString();
                case ChampionLoreModifierType.BaseStatMultiplier:
                    return modifier.stat.ToString();
                case ChampionLoreModifierType.CombatStat:
                    return modifier.combatStat.ToString();
                case ChampionLoreModifierType.WeaponProficiency:
                    return $"{modifier.weapon} Proficiency";
                case ChampionLoreModifierType.ElementalResistance:
                    return $"{modifier.element} Resistance";
                default:
                    return "Unknown";
            }
        }

        private static string FormatLoreModifier(string label, int value)
        {
            string sign = value >= 0 ? "+" : string.Empty;
            return $"{sign}{value} {label}";
        }

        private static void DrawChampionHeader(ChampionSO champion)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Champion", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Name:", champion.ChampionName);
                EditorGUILayout.LabelField("Nickname:", string.IsNullOrEmpty(champion.PlayerNickname) ? "-" : champion.PlayerNickname);
                EditorGUILayout.LabelField("Level:", champion.Level.ToString());
                EditorGUILayout.LabelField("XP:", $"{champion.CurrentExperience} / {champion.ExperienceToNextLevel}");
                EditorGUILayout.LabelField("Weapon:", champion.EquippedWeapon.ToString());
                EditorGUILayout.LabelField("ID:", champion.ChampionId, EditorStyles.miniLabel);
            }
        }

        private static void DrawStatsTable(ChampionSO champion)
        {
            ChampionStats stats = champion.Stats;
            if (stats == null)
            {
                return;
            }

            ChampionStats baseStats = champion.BaseStats ?? stats;
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Stats", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    DrawBaseStatsColumn(champion, stats, baseStats, 160f);
                    GUILayout.Space(14f);
                    DrawCombatStatsColumn(champion, stats, baseStats, 170f);
                    GUILayout.FlexibleSpace();
                }

                EditorGUILayout.Space(8f);
                DrawResistanceStatsSection(stats, baseStats);
                EditorGUILayout.Space(8f);
                DrawElementalDamageStatsSection(stats, baseStats);
            }
        }

        private static void DrawBaseStatsColumn(ChampionSO champion, ChampionStats stats, ChampionStats baseStats, float width)
        {
            using (CreateColumnScope(width))
            {
                EditorGUILayout.LabelField("BASE", EditorStyles.boldLabel);
                DrawBaseStatFormulaRow("Strength", champion.GetBaseStatFormula(ChampionStatType.Strength));
                DrawBaseStatFormulaRow("Agility", champion.GetBaseStatFormula(ChampionStatType.Agility));
                DrawBaseStatFormulaRow("Constitution", champion.GetBaseStatFormula(ChampionStatType.Constitution));
                DrawBaseStatFormulaRow("Intelligence", champion.GetBaseStatFormula(ChampionStatType.Intelligence));
                DrawBaseStatFormulaRow("Endurance", champion.GetBaseStatFormula(ChampionStatType.Endurance));
            }
        }

        private static void DrawCombatStatsColumn(ChampionSO champion, ChampionStats stats, ChampionStats baseStats, float width)
        {
            using (CreateColumnScope(width))
            {
                EditorGUILayout.LabelField("COMBAT", EditorStyles.boldLabel);
                DrawStatRow("Life", stats.life, baseStats.life);
                DrawStatRow("Energy", stats.energy, baseStats.energy);
                DrawRatingRow("Dodge", stats.dodge, baseStats.dodge, stats.GetDodgeChancePercent());
                DrawRatingRow("Block", stats.block, baseStats.block, stats.GetBlockChancePercent());
                DrawRatingRow("Crit", stats.critical, baseStats.critical, stats.GetCriticalChancePercent(champion.EquippedWeapon));
                DrawRatingRow("Hit", stats.hit, baseStats.hit, stats.GetHitPressurePercent());
                DrawRatingRow("Recovery", stats.recovery, baseStats.recovery, ChampionStats.ConvertRatingToPercent(stats.recovery));
                DrawRatingRow("Speed", stats.GetActionSpeedRating(), baseStats.GetActionSpeedRating(), ChampionStats.ConvertRatingToPercent(stats.GetActionSpeedRating()));
                DrawPercentStatRow("XP Gain", stats.experienceGain, baseStats.experienceGain);
                DrawPercentStatRow("Fatal Resist", stats.fatalInjuryResistance, baseStats.fatalInjuryResistance);
                DrawPercentStatRow("Injury Red.", stats.injurySeverityReduction, baseStats.injurySeverityReduction);
                DrawStatRow("Prof", stats.GetProficiency(champion.EquippedWeapon), baseStats.GetProficiency(champion.EquippedWeapon));
            }
        }

        private static void DrawResistanceStatsSection(ChampionStats stats, ChampionStats baseStats)
        {
            DrawDivider();
            EditorGUILayout.LabelField("RESISTANCES", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(170f)))
                {
                    DrawResistanceRow("Fire", stats, baseStats, SkillElement.Fire);
                    DrawResistanceRow("Water", stats, baseStats, SkillElement.Water);
                    DrawResistanceRow("Elec", stats, baseStats, SkillElement.Electricity);
                    DrawResistanceRow("Poison", stats, baseStats, SkillElement.Poison);
                }

                GUILayout.Space(18f);

                using (new EditorGUILayout.VerticalScope(GUILayout.Width(170f)))
                {
                    DrawResistanceRow("Earth", stats, baseStats, SkillElement.Earth);
                    DrawResistanceRow("Air", stats, baseStats, SkillElement.Air);
                    DrawResistanceRow("Wood", stats, baseStats, SkillElement.Wood);
                }

                GUILayout.FlexibleSpace();
            }
        }

        private static void DrawElementalDamageStatsSection(ChampionStats stats, ChampionStats baseStats)
        {
            DrawDivider();
            EditorGUILayout.LabelField("ELEMENT DAMAGE", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(190f)))
                {
                    DrawElementalDamageRow("Fire", stats, baseStats, SkillElement.Fire);
                    DrawElementalDamageRow("Water", stats, baseStats, SkillElement.Water);
                    DrawElementalDamageRow("Elec", stats, baseStats, SkillElement.Electricity);
                    DrawElementalDamageRow("Poison", stats, baseStats, SkillElement.Poison);
                }

                GUILayout.Space(18f);

                using (new EditorGUILayout.VerticalScope(GUILayout.Width(190f)))
                {
                    DrawElementalDamageRow("Earth", stats, baseStats, SkillElement.Earth);
                    DrawElementalDamageRow("Air", stats, baseStats, SkillElement.Air);
                    DrawElementalDamageRow("Wood", stats, baseStats, SkillElement.Wood);
                }

                GUILayout.FlexibleSpace();
            }
        }

        private static EditorGUILayout.VerticalScope CreateColumnScope(float width)
        {
            return width > 0f
                ? new EditorGUILayout.VerticalScope(GUILayout.Width(width))
                : new EditorGUILayout.VerticalScope();
        }

        private static void DrawDivider()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 1f);
            EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));
        }

        private static void DrawBaseStatFormulaRow(string label, ChampionStatFormula formula)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(76));
                DrawModifiedValue(formula.finalValue.ToString(), formula.finalValue, formula.baseValue, BuildFormulaTooltip(formula), GUILayout.Width(44));
            }
        }

        private static void DrawStatRow(string label, int value, int baseValue)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(76));
                DrawModifiedValue(value.ToString(), value, baseValue, BuildValueTooltip(baseValue, value), GUILayout.Width(44));
            }
        }

        private static void DrawRatingRow(string label, int rating, int baseRating, float percent)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(42));
                string tooltip = BuildValueTooltip(baseRating, rating);
                DrawModifiedValue(rating.ToString(), rating, baseRating, tooltip, GUILayout.Width(30));
                DrawPercentLabel(percent, rating, baseRating, tooltip, GUILayout.Width(52));
            }
        }

        private static void DrawPercentStatRow(string label, int value, int baseValue)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(76));
                DrawModifiedValue($"{value}%", value, baseValue, BuildValueTooltip(baseValue, value), GUILayout.Width(52));
            }
        }

        private static void DrawResistanceRow(string label, ChampionStats stats, ChampionStats baseStats, SkillElement element)
        {
            int value = stats.GetElementalResistance(element);
            int baseValue = baseStats.GetElementalResistance(element);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(42));
                string tooltip = BuildValueTooltip(baseValue, value);
                DrawModifiedValue(value.ToString(), value, baseValue, tooltip, GUILayout.Width(30));
                DrawPercentLabel(stats.GetElementalResistancePercent(element), value, baseValue, tooltip, GUILayout.Width(52));
            }
        }

        private static void DrawElementalDamageRow(string label, ChampionStats stats, ChampionStats baseStats, SkillElement element)
        {
            int value = stats.GetElementalDamageBonus(element);
            int baseValue = baseStats.GetElementalDamageBonus(element);
            float multiplier = stats.GetElementalDamageMultiplier(element);
            float baseMultiplier = baseStats.GetElementalDamageMultiplier(element);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(42));
                string tooltip = $"Base flat: {baseValue}\nFinal flat: {value}\nBase multiplier: x{baseMultiplier:0.##}\nFinal multiplier: x{multiplier:0.##}";
                DrawModifiedValue($"+{value}", value, baseValue, tooltip, GUILayout.Width(42));
                DrawModifiedFloatValue($"x{multiplier:0.##}", multiplier, baseMultiplier, tooltip, GUILayout.Width(58));
            }
        }

        private static void DrawModifiedValue(string text, int value, int baseValue, string tooltip, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(text, tooltip);
            EditorGUILayout.LabelField(content, GetValueStyle(value, baseValue), options);
        }

        private static void DrawPercentLabel(float percent, int value, int baseValue, string tooltip, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent($"{percent:0.#}%", tooltip);
            EditorGUILayout.LabelField(content, GetValueStyle(value, baseValue), options);
        }

        private static void DrawModifiedFloatValue(string text, float value, float baseValue, string tooltip, params GUILayoutOption[] options)
        {
            GUIContent content = new GUIContent(text, tooltip);
            EditorGUILayout.LabelField(content, GetValueStyle(value, baseValue), options);
        }

        private static string BuildFormulaTooltip(ChampionStatFormula formula)
        {
            return $"Base roll: {formula.baseValue}\nAdditive total: {FormatDelta(formula.additive)}\nMultiplier total: x{formula.multiplier:0.##}\nFinal: {formula.finalValue}\nFormula: ({formula.baseValue} {FormatSignedForFormula(formula.additive)}) x {formula.multiplier:0.##} = {formula.finalValue}";
        }

        private static string BuildValueTooltip(int baseValue, int finalValue)
        {
            return $"Base value: {baseValue}\nFinal: {finalValue}\nTotal delta: {FormatDelta(finalValue - baseValue)}";
        }

        private static string FormatDelta(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static string FormatSignedForFormula(int value)
        {
            return value >= 0 ? $"+ {value}" : $"- {Mathf.Abs(value)}";
        }

        private static GUIStyle GetValueStyle(int value, int baseValue)
        {
            var style = new GUIStyle(EditorStyles.label);
            if (value > baseValue)
            {
                style.normal.textColor = new Color(0.05f, 0.42f, 0.12f);
            }
            else if (value < baseValue)
            {
                style.normal.textColor = new Color(0.5f, 0.08f, 0.08f);
            }

            return style;
        }

        private static GUIStyle GetValueStyle(float value, float baseValue)
        {
            var style = new GUIStyle(EditorStyles.label);
            if (value > baseValue + 0.001f)
            {
                style.normal.textColor = new Color(0.05f, 0.42f, 0.12f);
            }
            else if (value < baseValue - 0.001f)
            {
                style.normal.textColor = new Color(0.5f, 0.08f, 0.08f);
            }

            return style;
        }

        private static void DrawPerks(ChampionSO champion)
        {
            PerkSO[] perks = champion.Perks;
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Perks", EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                if (perks == null || perks.Length == 0)
                {
                    EditorGUILayout.LabelField("-");
                    return;
                }

                float lineWidth = EditorGUIUtility.currentViewWidth - 50f;
                float used = 0f;
                EditorGUILayout.BeginHorizontal();

                foreach (PerkSO perk in perks)
                {
                    if (perk == null)
                    {
                        continue;
                    }

                    float width = Mathf.Clamp(EditorStyles.boldLabel.CalcSize(new GUIContent(perk.DisplayName)).x + 24f, 90f, 220f);
                    if (used + width > lineWidth)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        used = 0f;
                    }

                    DrawPerkChip(perk, width);
                    used += width + 4f;
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        private static void DrawPerkChip(PerkSO perk, float width)
        {
            GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal =
                {
                    textColor = Color.white,
                    background = MakeColorTexture(perk.Rarity)
                },
                padding = new RectOffset(8, 8, 3, 3)
            };

            GUILayout.Label(new GUIContent(perk.DisplayName, perk.Description), style, GUILayout.Width(width), GUILayout.Height(24));
        }

        private static Color GetPerkColor(PerkRarity rarity)
        {
            switch (rarity)
            {
                case PerkRarity.Common:
                    return new Color(0.25f, 0.25f, 0.25f);
                case PerkRarity.Rare:
                    return new Color(0.08f, 0.42f, 0.18f);
                case PerkRarity.VeryRare:
                    return new Color(0.08f, 0.22f, 0.58f);
                case PerkRarity.Epic:
                    return new Color(0.35f, 0.12f, 0.55f);
                case PerkRarity.Legendary:
                    return new Color(0.78f, 0.52f, 0.08f);
                default:
                    return Color.gray;
            }
        }

        private static Texture2D MakeColorTexture(PerkRarity rarity)
        {
            if (PerkBackgrounds.TryGetValue(rarity, out Texture2D cached) && cached != null)
            {
                return cached;
            }

            Color color = GetPerkColor(rarity);
            var texture = new Texture2D(1, 1)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            PerkBackgrounds[rarity] = texture;
            return texture;
        }

        private static void DrawSkills(ChampionSO champion)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Skills", EditorStyles.boldLabel);

            DrawSkillCard("Base Skill", champion.BaseSkill);

            SkillData[] skills = champion.Skills;
            for (int i = 0; i < 3; i++)
            {
                SkillData skill = skills != null && i < skills.Length ? skills[i] : null;
                DrawSkillCard($"Skill {i + 2}", skill);
            }
        }

        private static void DrawSkillCard(string title, SkillData skill)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
                if (skill == null)
                {
                    EditorGUILayout.LabelField("Locked / Empty");
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Skill:", GUILayout.Width(38));
                    EditorGUILayout.LabelField(skill.skillName, EditorStyles.boldLabel);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField("CD:", GUILayout.Width(24));
                    EditorGUILayout.LabelField($"{skill.cooldownSeconds:0.#}s", GUILayout.Width(45));
                    EditorGUILayout.LabelField("Scaling:", GUILayout.Width(52));
                    EditorGUILayout.LabelField(GetScalingText(skill), GUILayout.Width(170));
                    EditorGUILayout.LabelField("Weapon:", GUILayout.Width(52));
                    EditorGUILayout.LabelField(GetWeaponText(skill), GUILayout.Width(100));
                }

                EditorGUILayout.LabelField("ID:", skill.skillNameId, EditorStyles.miniLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    Rect iconRect = GUILayoutUtility.GetRect(72, 72, GUILayout.Width(72), GUILayout.Height(72));
                    GUI.DrawTexture(iconRect, SkillIconCache.GetIcon(skill), ScaleMode.ScaleToFit);

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                        EditorGUILayout.LabelField(skill.description, EditorStyles.wordWrappedLabel);
                    }
                }
            }
        }

        private static string GetScalingText(SkillData skill)
        {
            if (skill.statScaling == null || skill.statScaling.Length == 0)
            {
                return "-";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < skill.statScaling.Length; i++)
            {
                if (skill.statScaling[i] == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(skill.statScaling[i].stat);
                if (skill.statScaling[i].usesGradeScaling)
                {
                    builder.Append(" ");
                    builder.Append(skill.statScaling[i].scaling);
                }
            }

            return builder.Length > 0 ? builder.ToString() : "-";
        }

        private static string GetWeaponText(SkillData skill)
        {
            if (skill.weapons == null || skill.weapons.Length == 0)
            {
                return "-";
            }

            var builder = new StringBuilder();
            for (int i = 0; i < skill.weapons.Length; i++)
            {
                if (builder.Length > 0)
                {
                    builder.Append(", ");
                }

                builder.Append(skill.weapons[i]);
            }

            return builder.ToString();
        }
    }
}
