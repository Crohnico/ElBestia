using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Hair SO", fileName = "HairSO")]
    public sealed class HairSO : BodyPartCatalogSO
    {
        public static HairSO Instance => LoadSingleton<HairSO>("CharacterParts/HairSO");
    }
}

