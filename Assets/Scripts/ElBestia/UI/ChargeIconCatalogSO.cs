using System;
using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.UI
{
    [CreateAssetMenu(menuName = "El Bestia/UI/Charge Icon Catalog", fileName = "ChargeIconCatalog")]
    public sealed class ChargeIconCatalogSO : ScriptableObject
    {
        [SerializeField] private ChargeIconEntry[] entries = Array.Empty<ChargeIconEntry>();

        public Sprite GetIcon(ChargeType charge)
        {
            if (entries == null)
            {
                return null;
            }

            foreach (ChargeIconEntry entry in entries)
            {
                if (entry != null && entry.charge == charge)
                {
                    return entry.icon;
                }
            }

            return null;
        }

        public static ChargeIconCatalogSO LoadDefault()
        {
            return Resources.Load<ChargeIconCatalogSO>("ChargeIconCatalog");
        }
    }

    [Serializable]
    public sealed class ChargeIconEntry
    {
        public ChargeType charge;
        public Sprite icon;
    }
}
