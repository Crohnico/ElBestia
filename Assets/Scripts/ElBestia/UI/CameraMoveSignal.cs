namespace ElBestia.UI
{
    public readonly struct CameraMoveSignal
    {
        public readonly CameraSnapshotId Target;

        public CameraMoveSignal(CameraSnapshotId target)
        {
            Target = target;
        }
    }
}
