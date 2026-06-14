using System;
using ElBestia.Core;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class UIScreen : MonoBehaviour
    {
        [SerializeField] private UIScreenId screenId;
        [SerializeField] private UIScreenBehaviour openBehaviour;
        [SerializeField] private UIScreenBehaviour baseBehaviour;
        [SerializeField] private UIScreenBehaviour closeBehaviour;

        private IDisposable subscription;

        public UIScreenId ScreenId => screenId;

        private void OnEnable()
        {
            subscription = SignalBus.Subscribe<UIScreenToggleSignal>(OnScreenToggle);
        }

        private void OnDisable()
        {
            subscription?.Dispose();
            subscription = null;

            openBehaviour?.Kill();
            baseBehaviour?.Kill();
            closeBehaviour?.Kill();
        }

        public void Open(Action onComplete)
        {
            closeBehaviour?.Kill();

            ExecuteBehaviour(openBehaviour, () =>
            {
                baseBehaviour?.Execute(null);
                onComplete?.Invoke();
            });
        }

        public void Close(Action onComplete)
        {
            openBehaviour?.Kill();
            baseBehaviour?.Kill();

            ExecuteBehaviour(closeBehaviour, onComplete);
        }

        private void OnScreenToggle(UIScreenToggleSignal signal)
        {
            if (signal.ScreenId != screenId)
            {
                return;
            }

            if (signal.Show)
            {
                Open(signal.OnComplete);
            }
            else
            {
                Close(signal.OnComplete);
            }
        }

        private static void ExecuteBehaviour(UIScreenBehaviour behaviour, Action onComplete)
        {
            if (behaviour == null)
            {
                onComplete?.Invoke();
                return;
            }

            behaviour.Execute(onComplete);
        }
    }
}
