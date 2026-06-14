using System;
using UnityEngine;

namespace ElBestia.UI
{
    [Serializable]
    public sealed class CameraSnapshot
    {
        public CameraSnapshotId id;
        public Transform transform;
    }
}
