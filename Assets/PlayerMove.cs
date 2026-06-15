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

    void Start()
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
        pitch = 0f;
        ApplyLookRotation();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        characterController.Move(move * currentSpeed * Time.deltaTime);

        if (characterController.isGrounded && verticalVelocity.y < 0f)
        {
            verticalVelocity.y = -2f;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        characterController.Move(verticalVelocity * Time.deltaTime);

        float mouseX = Input.GetAxis("Mouse X") * mouseSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSpeed;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        ApplyLookRotation();
    }

    Transform FindPlayerCamera()
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

    void ApplyLookRotation()
    {
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
