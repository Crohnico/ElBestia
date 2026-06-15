using System.Collections.Generic;
using ElBestia.Champions;
using ElBestia.Generation;
using UnityEngine;

namespace ElBestia.Dojo
{
    [DefaultExecutionOrder(-100)]
    public sealed class ChampionAdmisionList : MonoBehaviour
    {
        public static ChampionAdmisionList Instance { get; private set; }

        public int payload = 5;
        public List<ChampionData> champions = new List<ChampionData>();

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            CreateList();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void CreateList()
        {
            champions.Clear();

            for (int i = 0; i < payload; i++)
            {
                int generationSeed = Random.Range(1, int.MaxValue);
                champions.Add(ChampionDataFactory.CreateRandom(generationSeed));
            }
        }
    }
}
