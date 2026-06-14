namespace ElBestia.Menu
{
    public readonly struct OpenMenuWindowSignal
    {
        public readonly MenuWindowId WindowId;

        public OpenMenuWindowSignal(MenuWindowId windowId)
        {
            WindowId = windowId;
        }
    }
}
