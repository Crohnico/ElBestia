using UnityEngine;

namespace ElBestia.Visuals
{
    [ExecuteAlways]
    public sealed class StickmanGluteScaler : MonoBehaviour
    {
        [SerializeField] private Transform leftGlute;
        [SerializeField] private Transform rightGlute;
        [SerializeField, Range(0f, 1f)] private float size;
        [SerializeField, Min(0.001f)] private float minScale = 0.15f;
        [SerializeField, Min(0.001f)] private float maxScale = 0.24f;
        [SerializeField] private Vector3 growthDirection = new Vector3(0f, -1f, 1f);
        [SerializeField] private float displacementFactor = 0.5f;
        [SerializeField] private bool capturedBase;
        [SerializeField] private Transform capturedLeftGlute;
        [SerializeField] private Transform capturedRightGlute;
        [SerializeField] private Vector3 leftBaseLocalPosition;
        [SerializeField] private Vector3 rightBaseLocalPosition;
        [SerializeField] private Vector3 selfBaseLocalPosition;
        [SerializeField] private int serializedVersion;

        public float Size
        {
            get => size;
            set
            {
                size = Mathf.Clamp01(value);
                Apply();
            }
        }

        public void CaptureBase()
        {
            if (leftGlute != null)
            {
                leftBaseLocalPosition = leftGlute.localPosition;
            }

            if (rightGlute != null)
            {
                rightBaseLocalPosition = rightGlute.localPosition;
            }

            selfBaseLocalPosition = transform.localPosition;
            capturedLeftGlute = leftGlute;
            capturedRightGlute = rightGlute;
            capturedBase = true;
            Apply();
        }

        public void Apply()
        {
            EnsureCapturedBase();

            float scale = Mathf.Lerp(minScale, Mathf.Max(minScale, maxScale), size);
            Vector3 offset = GetOffset(scale);

            bool hasReferences = leftGlute != null || rightGlute != null;
            if (leftGlute != null)
            {
                ApplyToTarget(leftGlute, leftBaseLocalPosition, scale, offset);
            }

            if (rightGlute != null)
            {
                ApplyToTarget(rightGlute, rightBaseLocalPosition, scale, offset);
            }

            if (!hasReferences)
            {
                ApplyToTarget(transform, selfBaseLocalPosition, scale, offset);
            }
        }

        private void Reset()
        {
            CaptureBase();
        }

        private void OnEnable()
        {
            EnsureCapturedBase();
            Apply();
        }

        private void OnValidate()
        {
            MigrateSerializedData();
            minScale = Mathf.Max(0.001f, minScale);
            maxScale = Mathf.Max(minScale, maxScale);
            size = Mathf.Clamp01(size);
            displacementFactor = Mathf.Max(0f, displacementFactor);
            Apply();
        }

        private void MigrateSerializedData()
        {
            if (serializedVersion >= 1)
            {
                return;
            }

            if (minScale >= 0.999f && maxScale <= 1.001f)
            {
                minScale = 0.15f;
                maxScale = 0.24f;
            }

            serializedVersion = 1;
        }

        private void EnsureCapturedBase()
        {
            if (!capturedBase || capturedLeftGlute != leftGlute || capturedRightGlute != rightGlute)
            {
                CaptureBase();
            }
        }

        private Vector3 GetOffset(float scale)
        {
            Vector3 direction = growthDirection.sqrMagnitude > 0.0001f ? growthDirection.normalized : Vector3.zero;
            float scaleGrowth = Mathf.Max(0f, scale - minScale);
            return direction * scaleGrowth * displacementFactor;
        }

        private static void ApplyToTarget(Transform target, Vector3 baseLocalPosition, float scale, Vector3 offset)
        {
            target.localScale = Vector3.one * scale;
            Vector3 position = baseLocalPosition + offset;
            position.x = target.localPosition.x;
            target.localPosition = position;
        }
    }
}
