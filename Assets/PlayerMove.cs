using UnityEngine;
using EscapeGame;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 5f;
    public float mouseSpeed = 2f;
    public float gravity = -9.81f;

    CharacterController characterController;
    Transform playerCamera;
    Vector3 verticalVelocity;
    float yaw;
    float pitch;

    void OnEnable()
    {
        SetupControllerAndCamera();
    }

    void Start()
    {
        SetupControllerAndCamera();
    }

    void Update()
    {
        if (characterController == null)
        {
            SetupControllerAndCamera();
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }

        Vector2 moveInput = GetMoveInput();
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);

        float mouseX = SafeGetAxis("Mouse X") * mouseSpeed;
        float mouseY = SafeGetAxis("Mouse Y") * mouseSpeed;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        ApplyLookRotation();
    }

    private void SetupControllerAndCamera()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.radius = 0.5f;
            characterController.height = 2f;
        }

        characterController.center = new Vector3(0f, 1f, 0f);

        if (GetComponent<DoorInteractor>() == null)
        {
            gameObject.AddComponent<DoorInteractor>();
        }

        playerCamera = FindPlayerCamera();
        yaw = transform.eulerAngles.y;
        pitch = playerCamera != null ? playerCamera.localEulerAngles.x : 0f;
        if (pitch > 180f)
        {
            pitch -= 360f;
        }

        TrySetPlayerTag();
        ApplyLookRotation();
        LockCursor();
    }

    private Transform FindPlayerCamera()
    {
        Camera camera = GetComponentInChildren<Camera>(true);
        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            return null;
        }

        if (camera.transform.parent != transform)
        {
            camera.transform.SetParent(transform, false);
        }

        camera.transform.localPosition = new Vector3(0f, 1.7f, 0f);
        return camera.transform;
    }

    private void ApplyLookRotation()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    private static Vector2 GetMoveInput()
    {
        float x = 0f;
        float y = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            x -= 1f;
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            x += 1f;
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            y -= 1f;
        }
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            y += 1f;
        }

        Vector2 input = new Vector2(x, y);
        return input.sqrMagnitude > 1f ? input.normalized : input;
    }

    private static float SafeGetAxis(string axisName)
    {
        try
        {
            return Input.GetAxis(axisName);
        }
        catch
        {
            return 0f;
        }
    }

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void TrySetPlayerTag()
    {
        try
        {
            gameObject.tag = "Player";
        }
        catch
        {
            // Name lookup still works in project copies where the Player tag is missing.
        }
    }
}
