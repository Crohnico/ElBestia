using System;
using ElBestia.Visuals;

namespace ElBestia.Champions
{
    [Serializable]
    public sealed class ChampionAppearance
    {
        public ChampionSex sex;
        public int hair;
        public int head;
        public int torso;
        public int arms;
        public int legs;
        public int feet;

        public static ChampionAppearance CreateRandom(System.Random rng)
        {
            return new ChampionAppearance
            {
                sex = rng.Next(0, 2) == 0 ? ChampionSex.H : ChampionSex.M,
                hair = RandomPart(rng, HairSO.Instance),
                head = RandomPart(rng, HeadSO.Instance),
                torso = RandomPart(rng, TorsoSO.Instance),
                arms = RandomPart(rng, ArmsSO.Instance),
                legs = RandomPart(rng, LegsSO.Instance),
                feet = RandomPart(rng, FeetSO.Instance)
            };
        }

        private static int RandomPart(System.Random rng, BodyPartCatalogSO catalog)
        {
            int amount = catalog != null ? catalog.GetAmount() : 0;
            return amount > 0 ? rng.Next(0, amount) : -1;
        }
    }
}

