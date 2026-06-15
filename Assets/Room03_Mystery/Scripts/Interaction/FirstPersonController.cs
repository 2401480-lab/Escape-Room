using UnityEngine;

namespace Room03Mystery
{
    // 1인칭 이동/시점 컨트롤러 (일반 FPS 방식).
    // 이동: WASD / 시점: 마우스 이동(커서 고정). 화면 중앙 조준점으로 오브젝트를 보고 좌클릭해 상호작용.
    // Tab: 커서 잠금/해제 토글(키패드·인벤토리 등 화면 UI 클릭용).
    // 단서 팝업이 열리면 자동으로 커서가 풀리고 이동/시점이 멈춘다(UIFocus).
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [Header("이동")]
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float gravity = -9.81f;

        [Header("시점")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float lookSensitivity = 2.0f;
        [SerializeField] private float minPitch = -80f;
        [SerializeField] private float maxPitch = 80f;

        [Header("커서 토글 키")]
        [SerializeField] private KeyCode cursorToggleKey = KeyCode.Tab;

        private CharacterController _cc;
        private float _yaw, _pitch, _vSpeed;
        private bool _freeCursor;   // Tab 로 수동 해제한 상태

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _yaw = transform.eulerAngles.y;
            if (cameraTransform == null)
            {
                var cam = GetComponentInChildren<Camera>();
                if (cam != null) cameraTransform = cam.transform;
            }
            if (cameraTransform != null)
            {
                float p = cameraTransform.localEulerAngles.x;
                _pitch = (p > 180f) ? p - 360f : p;
            }
            UIFocus.Reset();
        }

        void OnDisable()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            if (Input.GetKeyDown(cursorToggleKey)) _freeCursor = !_freeCursor;

            // 팝업이 열려 있거나 Tab 으로 풀었으면 조작 정지 + 커서 표시
            bool uiMode = UIFocus.Active || _freeCursor;

            if (uiMode)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                return;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            HandleLook();
            HandleMove();
        }

        void HandleLook()
        {
            _yaw += Input.GetAxis("Mouse X") * lookSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, minPitch, maxPitch);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (cameraTransform != null)
                cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        void HandleMove()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            Vector3 dir = transform.right * h + transform.forward * v;
            if (dir.sqrMagnitude > 1f) dir.Normalize();

            if (_cc.isGrounded && _vSpeed < 0f) _vSpeed = -2f;
            _vSpeed += gravity * Time.deltaTime;

            Vector3 vel = dir * moveSpeed + Vector3.up * _vSpeed;
            _cc.Move(vel * Time.deltaTime);
        }
    }
}
