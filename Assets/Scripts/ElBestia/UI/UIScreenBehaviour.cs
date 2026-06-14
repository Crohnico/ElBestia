using System;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class UIScreenBehaviour : MonoBehaviour
    {
        [SerializeField] private UIActionBase[] uiActions = new UIActionBase[0];

        private bool[] completedActions;
        private Action onComplete;

        public bool IsActive { get; private set; }

        private void Update()
        {
            if (IsActive)
            {
                Tick();
            }
        }

        public void Execute(Action onCompleteCallback)
        {
            Kill();

            onComplete = onCompleteCallback;
            completedActions = new bool[uiActions.Length];

            for (int i = 0; i < uiActions.Length; i++)
            {
                if (uiActions[i] != null)
                {
                    uiActions[i].ResetActionState();
                }
            }

            IsActive = true;

            if (uiActions.Length == 0)
            {
                Complete();
            }
        }

        public void Kill()
        {
            IsActive = false;
            completedActions = new bool[uiActions.Length];
            onComplete = null;
        }

        public void InstantExecution()
        {
            for (int i = 0; i < uiActions.Length; i++)
            {
                if (uiActions[i] != null)
                {
                    uiActions[i].InstantExecute();
                }
            }

            completedActions = new bool[uiActions.Length];
            for (int i = 0; i < completedActions.Length; i++)
            {
                completedActions[i] = true;
            }
        }

        private void Tick()
        {
            bool allCompleted = true;

            for (int i = 0; i < uiActions.Length; i++)
            {
                if (completedActions[i])
                {
                    continue;
                }

                UIActionBase action = uiActions[i];
                completedActions[i] = action == null || action.Execute();
                if (!completedActions[i])
                {
                    allCompleted = false;
                }
            }

            if (allCompleted)
            {
                Complete();
            }
        }

        private void Complete()
        {
            Action callback = onComplete;
            Kill();
            callback?.Invoke();
        }
    }
}
