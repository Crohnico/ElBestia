using UnityEngine;

namespace ElBestia.Combat
{
    public sealed class WeaponSfxOrigin : MonoBehaviour
    {
        [SerializeField] private Transform launchPoint;

        public Transform LaunchPoint => launchPoint != null ? launchPoint : transform;
    }
}
