using System.Collections.Generic;
using ElBestia.Champions;
using UnityEngine;

namespace ElBestia.Visuals
{
    public abstract class BodyPartCatalogSO : ScriptableObject
    {
        [SerializeField] private List<GameObject> male = new List<GameObject>();
        [SerializeField] private List<GameObject> female = new List<GameObject>();
        [SerializeField] private List<GameObject> both = new List<GameObject>();
        [SerializeField] private bool includeEmptyOption = true;

        public int RollId(System.Random rng, ChampionSex sex)
        {
            List<GameObject> pool = BuildPool(sex);
            int extraEmptyOption = includeEmptyOption ? 1 : 0;
            int rollCount = pool.Count + extraEmptyOption;
            if (rollCount <= 0)
            {
                return -1;
            }

            return rng.Next(0, rollCount);
        }

        public GameObject GetPrefab(ChampionSex sex, int id)
        {
            if (id < 0)
            {
                return null;
            }

            List<GameObject> pool = BuildPool(sex);
            if (includeEmptyOption)
            {
                if (id == 0)
                {
                    return null;
                }

                id--;
            }

            return id >= 0 && id < pool.Count ? pool[id] : null;
        }

        public GameObject GetRandomPrefab(ChampionSex sex)
        {
            return GetPrefab(sex, RollId(new System.Random(), sex));
        }

        public int GetAmount()
        {
            return GetListCount(male) + GetListCount(female) + GetListCount(both);
        }

        private List<GameObject> BuildPool(ChampionSex sex)
        {
            var pool = new List<GameObject>();
            AddValid(pool, both);
            AddValid(pool, sex == ChampionSex.H ? male : female);
            return pool;
        }

        protected static T LoadSingleton<T>(string resourcesPath) where T : BodyPartCatalogSO
        {
            return Resources.Load<T>(resourcesPath);
        }

        private static void AddValid(List<GameObject> target, List<GameObject> source)
        {
            if (source == null)
            {
                return;
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] != null)
                {
                    target.Add(source[i]);
                }
            }
        }

        private static int GetListCount(List<GameObject> list)
        {
            return list != null ? list.Count : 0;
        }
    }
}
