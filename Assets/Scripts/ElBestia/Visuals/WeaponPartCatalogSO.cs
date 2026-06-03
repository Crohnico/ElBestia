using ElBestia.Skills;
using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Weapon Part Catalog", fileName = "WeaponPartCatalogSO")]
    public sealed class WeaponPartCatalogSO : BodyPartCatalogSO
    {
        [SerializeField] private WeaponType weaponType;

        public WeaponType WeaponType => weaponType;

        public static WeaponPartCatalogSO GetCatalog(WeaponType weaponType)
        {
            WeaponPartCatalogSO[] catalogs = Resources.LoadAll<WeaponPartCatalogSO>("CharacterParts/Weapons");
            for (int i = 0; i < catalogs.Length; i++)
            {
                if (catalogs[i] != null && catalogs[i].weaponType == weaponType)
                {
                    return catalogs[i];
                }
            }

            return null;
        }
    }
}
