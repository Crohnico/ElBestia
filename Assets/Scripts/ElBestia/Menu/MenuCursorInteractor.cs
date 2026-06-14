using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ElBestia.Menu
{
    public sealed class MenuCursorInteractor : MonoBehaviour
    {
        [SerializeField] private Camera raycastCamera;
        [SerializeField] private float maxRayDistance = 1000f;

        private IInteractable currentSelected;
        private IInteractable cachedSelected;

        private void Update()
        {
            if (Mouse.current == null)
            {
                return;
            }

            currentSelected = RaycastInteractableUnderPointer();
            RefreshHoverSelection();

            if (Mouse.current.leftButton.wasPressedThisFrame && currentSelected != null)
            {
                currentSelected.OnClick();
            }
        }

        private IInteractable RaycastInteractableUnderPointer()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return null;
            }

            Camera cameraToUse = raycastCamera != null ? raycastCamera : Camera.main;
            if (cameraToUse == null)
            {
                return null;
            }

            Ray ray = cameraToUse.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<IInteractable>();
        }

        private void RefreshHoverSelection()
        {
            if (currentSelected == cachedSelected)
            {
                return;
            }

            if (cachedSelected != null)
            {
                cachedSelected.OnHover(false);
            }

            cachedSelected = currentSelected;

            if (cachedSelected != null)
            {
                cachedSelected.OnHover(true);
            }
        }

        private void OnDisable()
        {
            if (cachedSelected != null)
            {
                cachedSelected.OnHover(false);
            }

            currentSelected = null;
            cachedSelected = null;
        }
    }
}
