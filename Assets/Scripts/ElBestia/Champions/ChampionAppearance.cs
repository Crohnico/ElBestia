using System;
using ElBestia.Skills;
using ElBestia.Visuals;
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
        public int hairAsset = -1;
        public int beardAsset = -1;
        public int earAsset = -1;
        public int weaponAsset = -1;

        public static ChampionAppearance CreateRandom(System.Random rng)
        {
            var appearance = new ChampionAppearance
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

            appearance.hairAsset = RollPart(rng, HairSO.Instance, appearance.sex);
            appearance.beardAsset = RollPart(rng, BeardSO.Instance, appearance.sex);
            appearance.earAsset = RollPart(rng, EarSO.Instance, appearance.sex);
            return appearance;
        }

        public void RollWeaponPart(System.Random rng, WeaponType weaponType)
        {
            WeaponPartCatalogSO catalog = WeaponPartCatalogSO.GetCatalog(weaponType);
            weaponAsset = RollPart(rng, catalog, sex);
        }

        private static float Random01(System.Random rng)
        {
            return (float)rng.NextDouble();
        }

        private static Color RandomBodyColor(System.Random rng)
        {
            float hue = Random01(rng);
            float saturation = Mathf.Lerp(0.68f, 0.95f, Random01(rng));
            float value = Mathf.Lerp(0.72f, 1f, Random01(rng));
            return Color.HSVToRGB(hue, saturation, value);
        }

        private static Color RandomHairColor(System.Random rng)
        {
            float hue = Random01(rng);
            float saturation = Mathf.Lerp(0.5f, 0.95f, Random01(rng));
            float value = Mathf.Lerp(0.18f, 0.85f, Random01(rng));
            return Color.HSVToRGB(hue, saturation, value);
        }

        private static int RollPart(System.Random rng, BodyPartCatalogSO catalog, ChampionSex sex)
        {
            return catalog != null ? catalog.RollId(rng, sex) : -1;
        }
    }
}
