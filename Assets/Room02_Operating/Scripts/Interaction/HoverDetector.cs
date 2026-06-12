using UnityEngine;
using TMPro;

namespace Room02Operating
{
    [RequireComponent(typeof(Camera))]
    public class HoverDetector : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hoverText;
        [SerializeField] private float rayDistance = 20f;
        [SerializeField] private LayerMask interactableLayer;

        private Camera _cam;
        private InteractableObject _currentTarget;

        void Awake() => _cam = GetComponent<Camera>();

        void Update()
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactableLayer))
            {
                var interactable = hit.collider.GetComponent<InteractableObject>();
                if (interactable != null && interactable.IsInteractable())
                {
                    SetHover(interactable);
                    if (Input.GetMouseButtonDown(0))
                        interactable.OnClick();
                    return;
                }
            }

            ClearHover();
        }

        void SetHover(InteractableObject target)
        {
            if (_currentTarget == target) return;
            _currentTarget = target;
            hoverText.text = $"[ {target.GetHoverLabel()} ]";
            hoverText.gameObject.SetActive(true);
        }

        void ClearHover()
        {
            if (_currentTarget == null) return;
            _currentTarget = null;
            hoverText.gameObject.SetActive(false);
        }
    }
}
