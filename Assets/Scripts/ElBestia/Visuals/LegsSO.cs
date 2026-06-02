using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Legs SO", fileName = "LegsSO")]
    public sealed class LegsSO : BodyPartCatalogSO
    {
        public static LegsSO Instance => LoadSingleton<LegsSO>("CharacterParts/LegsSO");
    }
}

