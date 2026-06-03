using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Beard SO", fileName = "BeardSO")]
    public sealed class BeardSO : BodyPartCatalogSO
    {
        public static BeardSO Instance => LoadSingleton<BeardSO>("CharacterParts/BeardSO");
    }
}
