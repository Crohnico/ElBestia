using DG.Tweening;
using ElBestia.Core;
using UnityEngine;
using UnityEngine.Events;

namespace ElBestia.Menu
{
    public sealed class Building : MonoBehaviour, IInteractable
    {
        [SerializeField] private MenuWindowId targetWindow;
        [SerializeField] private Transform animatedTransform;
        [SerializeField] private float clickShrinkScale = 0.92f;
        [SerializeField] private float clickGrowScale = 1.06f;
        [SerializeField] private float clickStepDuration = 0.08f;
        [SerializeField] private Ease clickEase = Ease.OutQuad;
        [SerializeField] private UnityEvent onClickAction;

        private Vector3 originalScale;
        private Tween clickTween;

        private void Awake()
        {
            if (animatedTransform == null)
            {
                animatedTransform = transform;
            }

            originalScale = animatedTransform.localScale;
        }

        public void OnHover(bool isHovering)
        {
        }

        public void OnClick()
        {
            Debug.Log($"click en {targetWindow}");
            PlayClickFeedback();
            onClickAction?.Invoke();
            SignalBus.Fire(new OpenMenuWindowSignal(targetWindow));
        }

        private void PlayClickFeedback()
        {
            clickTween?.Kill();
            animatedTransform.localScale = originalScale;

            clickTween = DOTween.Sequence()
                .Append(animatedTransform.DOScale(originalScale * clickShrinkScale, clickStepDuration).SetEase(clickEase))
                .Append(animatedTransform.DOScale(originalScale * clickGrowScale, clickStepDuration).SetEase(clickEase))
                .Append(animatedTransform.DOScale(originalScale, clickStepDuration).SetEase(clickEase))
                .SetTarget(animatedTransform);
        }

        private void OnDisable()
        {
            clickTween?.Kill();

            if (animatedTransform != null)
            {
                animatedTransform.localScale = originalScale;
            }
        }
    }
}
