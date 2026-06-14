using System;
using ElBestia.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ElBestia.UI
{
    public sealed class UIScreenManager : MonoBehaviour
    {
        [SerializeField] private bool openInitialScreen;
        [SerializeField] private UIScreenId initialScreenId = UIScreenId.MainMenu;

        private UIScreenId currentScreenId;
        private bool hasCurrentScreen;
        private int lastCancelToken;
        private IDisposable requestSubscription;
        private IDisposable closeAllSubscription;

        private void OnEnable()
        {
            requestSubscription = SignalBus.Subscribe<RequestUIScreenSignal>(OnRequestUIScreen);
            closeAllSubscription = SignalBus.Subscribe<UICloseAllScreensSignal>(OnCloseAllScreens);
        }

        private void Start()
        {
            if (openInitialScreen)
            {
                ShowScreen(initialScreenId);
            }
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ShowScreen(initialScreenId);
            }
        }

        private void OnDisable()
        {
            requestSubscription?.Dispose();
            closeAllSubscription?.Dispose();
            requestSubscription = null;
            closeAllSubscription = null;
        }

        private void OnRequestUIScreen(RequestUIScreenSignal signal)
        {
            if (signal.Show)
            {
                ShowScreen(signal.ScreenId);
            }
            else
            {
                HideScreen(signal.ScreenId);
            }
        }

        private void OnCloseAllScreens(UICloseAllScreensSignal signal)
        {
            CloseCurrent();
        }

        private void ShowScreen(UIScreenId screenId)
        {
            if (hasCurrentScreen && currentScreenId == screenId)
            {
                return;
            }

            int token = ++lastCancelToken;

            if (hasCurrentScreen)
            {
                UIScreenId closingScreenId = currentScreenId;
                FireToggle(closingScreenId, false, () =>
                {
                    if (token != lastCancelToken)
                    {
                        return;
                    }

                    currentScreenId = screenId;
                    hasCurrentScreen = true;
                    FireToggle(screenId, true, null, token);
                }, token);

                return;
            }

            currentScreenId = screenId;
            hasCurrentScreen = true;
            FireToggle(screenId, true, null, token);
        }

        private void HideScreen(UIScreenId screenId)
        {
            if (!hasCurrentScreen || currentScreenId != screenId)
            {
                return;
            }

            int token = ++lastCancelToken;
            FireToggle(screenId, false, () =>
            {
                if (token != lastCancelToken)
                {
                    return;
                }

                hasCurrentScreen = false;
            }, token);
        }

        private void CloseCurrent()
        {
            if (!hasCurrentScreen)
            {
                return;
            }

            HideScreen(currentScreenId);
        }

        private void FireToggle(UIScreenId screenId, bool show, Action onComplete, int token)
        {
            SignalBus.Fire(new UIScreenToggleSignal(screenId, show, onComplete, token));
        }
    }
}
