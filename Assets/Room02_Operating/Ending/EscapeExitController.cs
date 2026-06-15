using UnityEngine;

namespace EscapeRoom
{
    public class EscapeExitController : MonoBehaviour
    {
        [SerializeField] private string exitDoorName = "ExitDoor";
        [SerializeField] private float interactDistance = 5f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private KeyCode alternateInteractKey = KeyCode.F;

        private bool qteStarted;

        private void Update()
        {
            if (qteStarted || !EscapeKeyState.HasKey)
            {
                return;
            }

            if (!Input.GetKeyDown(interactKey) && !Input.GetKeyDown(alternateInteractKey))
            {
                return;
            }

            if (IsLookingAtExitDoor())
            {
                qteStarted = true;
                EscapeChaseQTE qte = EscapeChaseQTE.Instance ?? FindObjectOfType<EscapeChaseQTE>();
                if (qte == null)
                {
                    GameObject qteObject = new GameObject("EscapeChaseQTE");
                    qte = qteObject.AddComponent<EscapeChaseQTE>();
                }

                qte.StartQTE();
            }
#if UNITY_EDITOR
            else
            {
                Debug.Log("[EscapeExitController] E/F pressed, but exit door was not detected. Stand close to ExitDoor/MainDoor and look at it.");
            }
#endif
        }

        private bool IsLookingAtExitDoor()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                return false;
            }

            Ray ray = new Ray(camera.transform.position, camera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                return IsExitDoorNearCamera(camera);
            }

            Transform current = hit.collider.transform;
            while (current != null)
            {
                if (IsExitDoorName(current.name))
                {
                    return true;
                }

                current = current.parent;
            }

            return IsExitDoorNearCamera(camera);
        }

        private bool IsExitDoorNearCamera(Camera camera)
        {
            GameObject door = GameObject.Find(exitDoorName) ?? GameObject.Find("MainDoor");
            if (door == null)
            {
                return false;
            }

            Vector3 toDoor = door.transform.position - camera.transform.position;
            if (toDoor.magnitude > interactDistance)
            {
                return false;
            }

            return Vector3.Angle(camera.transform.forward, toDoor) <= 45f;
        }

        private bool IsExitDoorName(string objectName)
        {
            if (objectName == exitDoorName || objectName == "MainDoor")
            {
                return true;
            }

            string lowerName = objectName.ToLowerInvariant();
            return lowerName.Contains("exit");
        }
    }
}
