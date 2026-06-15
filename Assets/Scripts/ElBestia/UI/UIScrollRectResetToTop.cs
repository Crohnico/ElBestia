using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public sealed class UIScrollRectResetToTop : MonoBehaviour
    {
        [SerializeField] private bool resetOnEnable = true;

        private ScrollRect scrollRect;
        private Coroutine resetCoroutine;
        private ScrollRect ScrollRect => scrollRect != null ? scrollRect : scrollRect = GetComponent<ScrollRect>();

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        private void OnEnable()
        {
            if (resetOnEnable && Application.isPlaying)
            {
                ResetToTopNextFrame();
            }
        }

        public void ResetToTop()
        {
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
            ScrollRect.StopMovement();
            ScrollRect.verticalNormalizedPosition = 1f;
            Canvas.ForceUpdateCanvases();
        }

        public void ResetToTopNextFrame()
        {
            if (resetCoroutine != null)
            {
                StopCoroutine(resetCoroutine);
            }

            resetCoroutine = StartCoroutine(ResetToTopAtEndOfFrame());
        }

        private IEnumerator ResetToTopAtEndOfFrame()
        {
            yield return null;
            ResetToTop();
            resetCoroutine = null;
        }
    }
}
