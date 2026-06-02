using System.Collections.Generic;
using UnityEngine;

namespace ElBestia.Visuals
{
    public abstract class BodyPartCatalogSO : ScriptableObject
    {
        [SerializeField] private List<Sprite> resources = new List<Sprite>();

        public Sprite GetResource(int id)
        {
            if (id < 0 || resources == null || resources.Count == 0)
            {
                return null;
            }

            int safeIndex = Mathf.Abs(id) % resources.Count;
            return resources[safeIndex];
        }

        public int GetAmount()
        {
            return resources != null ? resources.Count : 0;
        }

        protected static T LoadSingleton<T>(string resourcesPath) where T : BodyPartCatalogSO
        {
            return Resources.Load<T>(resourcesPath);
        }
    }
}
