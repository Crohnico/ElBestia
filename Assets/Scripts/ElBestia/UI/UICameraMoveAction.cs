using System;
using ElBestia.Core;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class UICameraMoveAction : UIActionBase
    {
        [SerializeField] private CameraSnapshotId targetSnapshot;

        private IDisposable subscription;
        private bool moveRequested;
        private bool moveCompleted;

        protected override bool ExecuteOwnAction()
        {
            if (!moveRequested)
            {
                subscription = SignalBus.Subscribe<CameraMoveCompletedSignal>(OnCameraMoveCompleted);
                SignalBus.Fire(new CameraMoveSignal(targetSnapshot));
                moveRequested = true;
            }

            return moveCompleted;
        }

        protected override void InstantExecuteOwnAction()
        {
            SignalBus.Fire(new CameraMoveSignal(targetSnapshot));
            moveCompleted = true;
            DisposeSubscription();
        }

        public override void ResetActionState()
        {
            DisposeSubscription();
            moveRequested = false;
            moveCompleted = false;
            base.ResetActionState();
        }

        private void OnDisable()
        {
            DisposeSubscription();
        }

        private void OnCameraMoveCompleted(CameraMoveCompletedSignal signal)
        {
            if (signal.Target != targetSnapshot)
            {
                return;
            }

            moveCompleted = true;
            DisposeSubscription();
        }

        private void DisposeSubscription()
        {
            subscription?.Dispose();
            subscription = null;
        }
    }
}
