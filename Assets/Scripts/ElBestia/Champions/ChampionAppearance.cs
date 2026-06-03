using System;
using UnityEngine;

namespace ElBestia.Champions
{
    [Serializable]
    public sealed class ChampionAppearance
    {
        public ChampionSex sex;
        public Color bodyColor;
        public Color hairColor;
        [Range(0f, 1f)] public float chest;
        [Range(0f, 1f)] public float belly;
        [Range(0f, 1f)] public float arms;
        [Range(0f, 1f)] public float forearms;
        [Range(0f, 1f)] public float upperLegs;
        [Range(0f, 1f)] public float lowerLegs;
        [Range(0f, 1f)] public float glutes;

        public static ChampionAppearance CreateRandom(System.Random rng)
        {
            return new ChampionAppearance
            {
                sex = rng.Next(0, 2) == 0 ? ChampionSex.H : ChampionSex.M,
                bodyColor = RandomBodyColor(rng),
                hairColor = RandomHairColor(rng),
                chest = Random01(rng),
                belly = Random01(rng),
                arms = Random01(rng),
                forearms = Random01(rng),
                upperLegs = Random01(rng),
                lowerLegs = Random01(rng),
                glutes = Random01(rng)
            };
        }

        private static float Random01(System.Random rng)
        {
            return (float)rng.NextDouble();
        }

        private static Color RandomBodyColor(System.Random rng)
        {
            float hue = Mathf.Lerp(0.01f, 0.13f, Random01(rng));
            float saturation = Mathf.Lerp(0.28f, 0.58f, Random01(rng));
            float value = Mathf.Lerp(0.62f, 0.95f, Random01(rng));
            return Color.HSVToRGB(hue, saturation, value);
        }

        private static Color RandomHairColor(System.Random rng)
        {
            float hue = Mathf.Lerp(0.02f, 0.12f, Random01(rng));
            float saturation = Mathf.Lerp(0.35f, 0.8f, Random01(rng));
            float value = Mathf.Lerp(0.12f, 0.55f, Random01(rng));
            return Color.HSVToRGB(hue, saturation, value);
        }
    }
}
