using System;

namespace ElBestia.UI
{
    public readonly struct UIScreenToggleSignal
    {
        public readonly UIScreenId ScreenId;
        public readonly bool Show;
        public readonly Action OnComplete;
        public readonly int Token;

        public UIScreenToggleSignal(UIScreenId screenId, bool show, Action onComplete, int token)
        {
            ScreenId = screenId;
            Show = show;
            OnComplete = onComplete;
            Token = token;
        }
    }
}
