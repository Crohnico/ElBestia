using UnityEngine;
using UnityEngine.UI;

namespace ElBestia.Combat
{
    public sealed class TimelineMarkerUI : MonoBehaviour
    {
        [SerializeField] private Image avatarImage;

        private RectTransform rectTransform;
        private RectTransform end;
        private CombatCrono crono;
        private Vector3 startPosition;
        private float startTime;
        private float endTime;

        private void Awake()
        {
            rectTransform = transform as RectTransform;
            if (avatarImage == null)
            {
                avatarImage = GetComponentInChildren<Image>(true);
            }
        }

        private void Update()
        {
            if (crono == null || rectTransform == null || end == null)
            {
                return;
            }

            float duration = Mathf.Max(0.001f, endTime - startTime);
            float t = Mathf.Clamp01((crono.CurrentTime - startTime) / duration);
            rectTransform.position = Vector3.Lerp(startPosition, end.position, t);
        }

        public void SetAvatar(Sprite avatar)
        {
            if (avatarImage == null)
            {
                avatarImage = GetComponentInChildren<Image>(true);
            }

            if (avatarImage != null)
            {
                avatarImage.sprite = avatar;
                avatarImage.enabled = avatar != null;
            }
        }

        public void Travel(RectTransform from, RectTransform to, float fromTime, float toTime, CombatCrono timelineCrono)
        {
            end = to;
            startTime = fromTime;
            endTime = toTime;
            crono = timelineCrono;

            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (rectTransform != null && from != null)
            {
                startPosition = from.position;
                rectTransform.position = startPosition;
            }
        }

        public void Retarget(RectTransform to, float fromTime, float toTime)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (rectTransform == null)
            {
                return;
            }

            startPosition = rectTransform.position;
            end = to;
            startTime = fromTime;
            endTime = toTime;
        }

        public void SnapTo(RectTransform target)
        {
            if (rectTransform == null)
            {
                rectTransform = transform as RectTransform;
            }

            if (rectTransform != null && target != null)
            {
                rectTransform.position = target.position;
            }
        }
    }
}
