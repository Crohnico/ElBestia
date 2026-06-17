using System.Text;
using ElBestia.Champions;
using TMPro;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class ChampionStatsVisualizer : MonoBehaviour
    {
        private const int LabelWidth = 13;
        private const int ValueWidth = 8;
        private const string DataStartTag = "<size=80%><mspace=0.55em>";
        private const string DataEndTag = "</mspace></size>";

        public TMP_Text content;

        public void SetUp(ChampionData champion)
        {
            if (content == null)
            {
                return;
            }

            ChampionStats stats = champion != null ? champion.Stats : null;
            if (stats == null)
            {
                content.text = string.Empty;
                return;
            }

            var builder = new StringBuilder();
            AppendCombat(builder, stats);
            builder.AppendLine();
            AppendResistance(builder, stats);
            builder.AppendLine();
            AppendElementDamage(builder, stats);
            content.text = builder.ToString();
        }

        private static void AppendCombat(StringBuilder builder, ChampionStats stats)
        {
            AppendTitle(builder, "COMBAT");
            builder.Append(DataStartTag);
            AppendPairLine(builder, "Life", stats.life, "Energy", stats.energy);
            AppendPairLine(builder, "Dodge", stats.dodge, "Block", stats.block);
            AppendPairLine(builder, "Critical", stats.critical, "Hit", stats.hit);
            AppendPairLine(builder, "Recovery", stats.recovery, "Speed", stats.speed);
            AppendPairLine(builder, "Fatal Resist", stats.fatalInjuryResistance, "Injury Resist", stats.injurySeverityReduction);
            builder.AppendLine(DataEndTag);
        }

        private static void AppendResistance(StringBuilder builder, ChampionStats stats)
        {
            AppendTitle(builder, "RESISTANCE");
            builder.Append(DataStartTag);
            AppendPairLine(builder, "Fire", stats.fireResistance, "Water", stats.waterResistance);
            AppendPairLine(builder, "Electricity", stats.electricityResistance, "Poison", stats.poisonResistance);
            AppendPairLine(builder, "Earth", stats.earthResistance, "Air", stats.airResistance);
            AppendSingleLine(builder, "Wood", stats.woodResistance);
            builder.AppendLine(DataEndTag);
        }

        private static void AppendElementDamage(StringBuilder builder, ChampionStats stats)
        {
            AppendTitle(builder, "ELEMENT DAMAGE");
            builder.Append(DataStartTag);
            AppendPairLine(builder, "Fire", FormatElementDamage(stats.fireDamageMultiplier), "Water", FormatElementDamage(stats.waterDamageMultiplier));
            AppendPairLine(builder, "Electricity", FormatElementDamage(stats.electricityDamageMultiplier), "Poison", FormatElementDamage(stats.poisonDamageMultiplier));
            AppendPairLine(builder, "Earth", FormatElementDamage(stats.earthDamageMultiplier), "Air", FormatElementDamage(stats.airDamageMultiplier));
            AppendSingleLine(builder, "Wood", FormatElementDamage(stats.woodDamageMultiplier));
            builder.AppendLine(DataEndTag);
        }

        private static void AppendPairLine(StringBuilder builder, string leftLabel, object leftValue, string rightLabel, object rightValue)
        {
            builder.Append(FormatCell(leftLabel, leftValue));
            builder.Append(FormatCell(rightLabel, rightValue));
            builder.AppendLine();
        }

        private static void AppendTitle(StringBuilder builder, string title)
        {
            builder.AppendLine(title);
        }

        private static void AppendSingleLine(StringBuilder builder, string label, object value)
        {
            builder.AppendLine(FormatCell(label, value));
        }

        private static string FormatCell(string label, object value)
        {
            return label.PadRight(LabelWidth) + value.ToString().PadRight(ValueWidth);
        }

        private static string FormatElementDamage(float multiplier)
        {
            return $"x{multiplier:0.##}";
        }
    }
}
