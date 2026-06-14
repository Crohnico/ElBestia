using UnityEngine;

namespace ElBestia.UI
{
    public class UIActionBase : MonoBehaviour, UIAction
    {
        [SerializeField] private UIActionBase[] onCompleteActions = new UIActionBase[0];

        private bool ownActionCompleted;
        private bool[] completedChildActions;

        public bool Execute()
        {
            if (!ownActionCompleted)
            {
                ownActionCompleted = ExecuteOwnAction();
                if (!ownActionCompleted)
                {
                    return false;
                }
            }

            return ExecuteChildActions();
        }

        public void InstantExecute()
        {
            InstantExecuteOwnAction();

            for (int i = 0; i < onCompleteActions.Length; i++)
            {
                if (onCompleteActions[i] != null)
                {
                    onCompleteActions[i].InstantExecute();
                }
            }

            MarkCompleted();
        }

        public virtual void ResetActionState()
        {
            ownActionCompleted = false;
            completedChildActions = new bool[onCompleteActions.Length];

            for (int i = 0; i < onCompleteActions.Length; i++)
            {
                if (onCompleteActions[i] != null)
                {
                    onCompleteActions[i].ResetActionState();
                }
            }
        }

        protected virtual bool ExecuteOwnAction()
        {
            return true;
        }

        protected virtual void InstantExecuteOwnAction()
        {
        }

        private bool ExecuteChildActions()
        {
            if (onCompleteActions.Length == 0)
            {
                return true;
            }

            if (completedChildActions == null || completedChildActions.Length != onCompleteActions.Length)
            {
                completedChildActions = new bool[onCompleteActions.Length];
            }

            bool allCompleted = true;
            for (int i = 0; i < onCompleteActions.Length; i++)
            {
                if (completedChildActions[i])
                {
                    continue;
                }

                UIActionBase childAction = onCompleteActions[i];
                completedChildActions[i] = childAction == null || childAction.Execute();
                if (!completedChildActions[i])
                {
                    allCompleted = false;
                }
            }

            return allCompleted;
        }

        private void MarkCompleted()
        {
            ownActionCompleted = true;
            completedChildActions = new bool[onCompleteActions.Length];

            for (int i = 0; i < completedChildActions.Length; i++)
            {
                completedChildActions[i] = true;
            }
        }
    }
}
