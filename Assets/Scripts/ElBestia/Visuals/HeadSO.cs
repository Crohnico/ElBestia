using UnityEngine;

namespace ElBestia.Visuals
{
    [CreateAssetMenu(menuName = "El Bestia/Visuals/Head SO", fileName = "HeadSO")]
    public sealed class HeadSO : BodyPartCatalogSO
    {
        public static HeadSO Instance => LoadSingleton<HeadSO>("CharacterParts/HeadSO");
    }
}

