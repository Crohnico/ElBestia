using System;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Combat
{
    [CreateAssetMenu(menuName = "El Bestia/Combat/Projectile Catalog", fileName = "CombatProjectileCatalog")]
    public sealed class CombatProjectileCatalogSO : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public WeaponType weapon;
            public SkillElement element;
            public GameObject prefab;
            [Min(0.1f)] public float speed = 12f;
        }

        [SerializeField] private Entry[] entries = Array.Empty<Entry>();
        [SerializeField, Min(0.1f)] private float defaultSpeed = 12f;

        public bool TryGet(WeaponType weapon, SkillElement element, out GameObject prefab, out float speed)
        {
            foreach (Entry entry in entries)
            {
                if (entry != null && entry.weapon == weapon && entry.element == element)
                {
                    prefab = entry.prefab;
                    speed = Mathf.Max(0.1f, entry.speed);
                    return prefab != null;
                }
            }

            prefab = null;
            speed = Mathf.Max(0.1f, defaultSpeed);
            return false;
        }
    }
}
