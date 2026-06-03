using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Ear SO", fileName = "EarSO")]
    public sealed class EarSO : BodyPartCatalogSO
    {
        public static EarSO Instance => LoadSingleton<EarSO>("CharacterParts/EarSO");
    }
}
