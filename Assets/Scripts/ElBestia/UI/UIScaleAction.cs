using DG.Tweening;
using UnityEngine;

namespace ElBestia.UI
{
    public sealed class UIScaleAction : UIActionBase
    {
        private enum ScaleMode
        {
            PopIn,
            ScaleOut
        }

        [SerializeField] private Transform target;
        [SerializeField] private ScaleMode mode = ScaleMode.PopIn;
        [SerializeField] private float duration = 0.24f;
        [SerializeField] private Ease ease = Ease.OutQuad;

        // Intentional small bounce: readable pop without turning the panel elastic.
        [SerializeField] private float popOvershootScale = 1.08f;
        [SerializeField] private float popUndershootScale = 0.96f;

        private Tween scaleTween;
        private bool tweenStarted;
        private bool tweenCompleted;

        protected override bool ExecuteOwnAction()
        {
            if (!tweenStarted)
            {
                PlayScale();
                tweenStarted = true;
            }

            return tweenCompleted;
        }

        protected override void InstantExecuteOwnAction()
        {
            KillTween();
            target.localScale = GetFinalScale();
            tweenStarted = true;
            tweenCompleted = true;
        }

        public override void ResetActionState()
        {
            KillTween();
            tweenStarted = false;
            tweenCompleted = false;
            base.ResetActionState();
        }

        private void OnDisable()
        {
            KillTween();
        }

        private void PlayScale()
        {
            Transform scaleTarget = target;
            scaleTarget.localScale = GetInitialScale();

            switch (mode)
            {
                case ScaleMode.PopIn:
                    scaleTween = CreatePopInTween(scaleTarget);
                    break;
                case ScaleMode.ScaleOut:
                    scaleTween = scaleTarget.DOScale(Vector3.zero, duration).SetEase(ease);
                    break;
            }

            scaleTween.SetTarget(scaleTarget);
            scaleTween.OnComplete(() =>
            {
                tweenCompleted = true;
                scaleTween = null;
            });
        }

        private Tween CreatePopInTween(Transform scaleTarget)
        {
            float stepDuration = duration / 3f;
            return DOTween.Sequence()
                .Append(scaleTarget.DOScale(Vector3.one * popOvershootScale, stepDuration).SetEase(ease))
                .Append(scaleTarget.DOScale(Vector3.one * popUndershootScale, stepDuration).SetEase(ease))
                .Append(scaleTarget.DOScale(Vector3.one, stepDuration).SetEase(ease));
        }

        private Vector3 GetInitialScale()
        {
            return mode == ScaleMode.ScaleOut ? Vector3.one : Vector3.zero;
        }

        private Vector3 GetFinalScale()
        {
            return mode == ScaleMode.ScaleOut ? Vector3.zero : Vector3.one;
        }

        private void KillTween()
        {
            scaleTween?.Kill();
            scaleTween = null;
        }
    }
}
