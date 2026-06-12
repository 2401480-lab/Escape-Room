using UnityEngine;

namespace EscapeGame
{
    public class DoorInteractor : MonoBehaviour
    {
        public KeyCode interactKey = KeyCode.E;
        public float interactDistance = 3f;
        public float openAngle = 90f;

        private Camera playerCamera;

        private void Awake()
        {
            playerCamera = Camera.main;
        }

        private void Update()
        {
            if (Input.GetKeyDown(interactKey))
            {
                TryOpenDoor();
            }
        }

        public bool TryOpenDoor()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
            }

            if (playerCamera == null)
            {
                return false;
            }

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            Transform door = FindDoorTransform(hit.collider.transform);
            if (door == null)
            {
                return false;
            }

            OpenDoor(door);
            return true;
        }

        private Transform FindDoorTransform(Transform target)
        {
            Transform current = target;
            while (current != null)
            {
                if (IsDoorName(current.name))
                {
                    return current;
                }

                current = current.parent;
            }

            return null;
        }

        private bool IsDoorName(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
            {
                return false;
            }

            string normalized = objectName.ToLowerInvariant();
            if (normalized.Contains("frame"))
            {
                return false;
            }

            return normalized.Contains("door");
        }

        private void OpenDoor(Transform door)
        {
            door.localRotation *= Quaternion.Euler(0f, openAngle, 0f);
            SetCollidersEnabled(door, false);
        }

        private void SetCollidersEnabled(Transform door, bool enabled)
        {
            foreach (Collider collider in door.GetComponentsInChildren<Collider>())
            {
                collider.enabled = enabled;
            }
        }
    }
}
