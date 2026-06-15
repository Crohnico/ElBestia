using UnityEngine;
using UnityEngine.InputSystem;

namespace ElBestia.Dojo
{
    public sealed class ChampionShowcaseDragRotator : MonoBehaviour
    {
        public Collider dragCollider;
        public Transform targetToRotate;
        public Camera raycastCamera;
        public float rotationSpeed = 0.35f;
        public float maxRaycastDistance = 100f;

        private bool isDragging;
        private Vector2 lastMousePosition;

        private void Awake()
        {
            if (raycastCamera == null)
            {
                raycastCamera = Camera.main;
            }
        }

        private void Update()
        {
            Mouse mouse = Mouse.current;
            Vector2 mousePosition = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame && IsPointerOverCollider(mousePosition))
            {
                isDragging = true;
                lastMousePosition = mousePosition;
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                isDragging = false;
            }

            if (!isDragging || !mouse.leftButton.isPressed)
            {
                return;
            }

            Vector2 delta = mousePosition - lastMousePosition;
            targetToRotate.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
            lastMousePosition = mousePosition;
        }

        public void ResetRotation()
        {
            targetToRotate.localRotation = Quaternion.identity;
        }

        public void RotatePreviewStep()
        {
            targetToRotate.Rotate(Vector3.up, 45f, Space.World);
        }

        private bool IsPointerOverCollider(Vector2 mousePosition)
        {
            Ray ray = raycastCamera.ScreenPointToRay(mousePosition);
            return dragCollider.Raycast(ray, out _, maxRaycastDistance);
        }
    }
}
