using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Torso SO", fileName = "TorsoSO")]
    public sealed class TorsoSO : BodyPartCatalogSO
    {
        public static TorsoSO Instance => LoadSingleton<TorsoSO>("CharacterParts/TorsoSO");
    }
}

