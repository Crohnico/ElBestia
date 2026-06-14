namespace ElBestia.UI
{
    public readonly struct RequestUIScreenSignal
    {
        public readonly UIScreenId ScreenId;
        public readonly bool Show;

        public RequestUIScreenSignal(UIScreenId screenId, bool show)
        {
            ScreenId = screenId;
            Show = show;
        }
    }
}
