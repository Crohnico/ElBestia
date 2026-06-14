namespace ElBestia.UI
{
    public readonly struct CameraMoveCompletedSignal
    {
        public readonly CameraSnapshotId Target;

        public CameraMoveCompletedSignal(CameraSnapshotId target)
        {
            Target = target;
        }
    }
}
