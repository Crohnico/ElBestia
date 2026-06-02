using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Feet SO", fileName = "FeetSO")]
    public sealed class FeetSO : BodyPartCatalogSO
    {
        public static FeetSO Instance => LoadSingleton<FeetSO>("CharacterParts/FeetSO");
    }
}

