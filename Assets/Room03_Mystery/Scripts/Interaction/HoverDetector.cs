using UnityEngine;
using TMPro;

namespace Room03Mystery
{
    // 메인 카메라에 부착
    // 마우스 위치에서 Raycast → InteractableObject 의 hoverLabel 표시, 클릭 시 OnClick 호출
    [RequireComponent(typeof(Camera))]
    public class HoverDetector : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI hoverText;
        [SerializeField] private float rayDistance = 20f;
        [SerializeField] private LayerMask interactableLayer = ~0;
        [Tooltip("1인칭: 화면 중앙(조준점) 기준 / 끄면 마우스 위치 기준")]
        [SerializeField] private bool useScreenCenter = true;

        private Camera _cam;
        private InteractableObject _currentTarget;

        void Awake() => _cam = GetComponent<Camera>();

        void Update()
        {
            // 커서가 풀려 있으면(팝업/Tab UI 모드) 상호작용 중지
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                ClearHover();
                return;
            }

            Ray ray = useScreenCenter
                ? _cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
                : _cam.ScreenPointToRay(Input.mousePosition);

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
            if (hoverText != null)
            {
                hoverText.text = $"[ {target.GetHoverLabel()} ]";
                hoverText.gameObject.SetActive(true);
            }
        }

        void ClearHover()
        {
            if (_currentTarget == null) return;
            _currentTarget = null;
            if (hoverText != null)
                hoverText.gameObject.SetActive(false);
        }
    }
}
