using ElBestia.Champions;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Generation
{
    public static class SkillIconGenerator
    {
        public const int IconSize = 64;

        public static Texture2D CreateTexture(SkillData skill)
        {
            var texture = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color background = ElementColor(skill.element);
            Color accent = GradeColor(skill.quality);
            var rng = new System.Random(skill.iconSeed);

            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    float noise = (float)rng.NextDouble() * 0.08f;
                    texture.SetPixel(x, y, Color.Lerp(background, Color.white, noise));
                }
            }

            DrawBorder(texture, accent, GetBorderWidth(skill.quality));
            DrawMainShape(texture, skill, accent);
            texture.Apply();
            return texture;
        }

        private static void DrawBorder(Texture2D texture, Color color, int width)
        {
            for (int y = 0; y < IconSize; y++)
            {
                for (int x = 0; x < IconSize; x++)
                {
                    if (x < width || y < width || x >= IconSize - width || y >= IconSize - width)
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
        }

        private static void DrawMainShape(Texture2D texture, SkillData skill, Color color)
        {
            ChampionStatType stat = skill.statScaling != null && skill.statScaling.Length > 0
                ? skill.statScaling[0].stat
                : ChampionStatType.Strength;

            switch (stat)
            {
                case ChampionStatType.Strength:
                    DrawCircle(texture, 32, 32, 16, color);
                    break;
                case ChampionStatType.Agility:
                    DrawDiamond(texture, color);
                    break;
                case ChampionStatType.Constitution:
                    DrawSquare(texture, 18, 18, 28, color);
                    break;
                case ChampionStatType.Intelligence:
                    DrawTriangle(texture, color);
                    break;
                case ChampionStatType.Endurance:
                    DrawCross(texture, color);
                    break;
            }
        }

        private static void DrawCircle(Texture2D texture, int centerX, int centerY, int radius, Color color)
        {
            int radiusSquared = radius * radius;
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    int dx = x - centerX;
                    int dy = y - centerY;
                    if (dx * dx + dy * dy <= radiusSquared)
                    {
                        SetSafe(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawDiamond(Texture2D texture, Color color)
        {
            for (int y = 12; y <= 52; y++)
            {
                for (int x = 12; x <= 52; x++)
                {
                    if (Mathf.Abs(x - 32) + Mathf.Abs(y - 32) <= 20)
                    {
                        SetSafe(texture, x, y, color);
                    }
                }
            }
        }

        private static void DrawSquare(Texture2D texture, int startX, int startY, int size, Color color)
        {
            for (int y = startY; y < startY + size; y++)
            {
                for (int x = startX; x < startX + size; x++)
                {
                    SetSafe(texture, x, y, color);
                }
            }
        }

        private static void DrawTriangle(Texture2D texture, Color color)
        {
            for (int y = 14; y <= 50; y++)
            {
                float t = (y - 14) / 36f;
                int halfWidth = Mathf.RoundToInt(t * 20f);
                for (int x = 32 - halfWidth; x <= 32 + halfWidth; x++)
                {
                    SetSafe(texture, x, y, color);
                }
            }
        }

        private static void DrawCross(Texture2D texture, Color color)
        {
            DrawSquare(texture, 28, 14, 8, color);
            DrawSquare(texture, 28, 42, 8, color);
            DrawSquare(texture, 14, 28, 8, color);
            DrawSquare(texture, 42, 28, 8, color);
            DrawSquare(texture, 26, 26, 12, color);
        }

        private static void SetSafe(Texture2D texture, int x, int y, Color color)
        {
            if (x >= 0 && x < IconSize && y >= 0 && y < IconSize)
            {
                texture.SetPixel(x, y, color);
            }
        }

        private static Color ElementColor(SkillElement element)
        {
            switch (element)
            {
                case SkillElement.Water:
                    return new Color(0.16f, 0.42f, 0.82f);
                case SkillElement.Electricity:
                    return new Color(0.95f, 0.85f, 0.2f);
                case SkillElement.Fire:
                    return new Color(0.9f, 0.22f, 0.12f);
                case SkillElement.Poison:
                    return new Color(0.36f, 0.68f, 0.18f);
                case SkillElement.Earth:
                    return new Color(0.48f, 0.34f, 0.18f);
                case SkillElement.Air:
                    return new Color(0.72f, 0.86f, 0.92f);
                case SkillElement.Wood:
                    return new Color(0.22f, 0.48f, 0.16f);
                default:
                    return new Color(0.18f, 0.18f, 0.2f);
            }
        }

        private static Color GradeColor(GrowthGrade grade)
        {
            switch (grade)
            {
                case GrowthGrade.F:
                case GrowthGrade.E:
                    return new Color(0.55f, 0.55f, 0.55f);
                case GrowthGrade.D:
                case GrowthGrade.C:
                    return new Color(0.3f, 0.75f, 0.35f);
                case GrowthGrade.B:
                case GrowthGrade.A:
                    return new Color(0.25f, 0.45f, 1f);
                default:
                    return new Color(1f, 0.82f, 0.2f);
            }
        }

        private static int GetBorderWidth(GrowthGrade grade)
        {
            int value = (int)grade;
            return value >= (int)GrowthGrade.S ? 5 : value >= (int)GrowthGrade.B ? 4 : 3;
        }
    }
}
