using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Arms SO", fileName = "ArmsSO")]
    public sealed class ArmsSO : BodyPartCatalogSO
    {
        public static ArmsSO Instance => LoadSingleton<ArmsSO>("CharacterParts/ArmsSO");
    }
}

