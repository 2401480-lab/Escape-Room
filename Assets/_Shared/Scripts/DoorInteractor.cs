using System.Collections.Generic;
using UnityEngine;

namespace EscapeGame
{
    public class DoorInteractor : MonoBehaviour
    {
        public KeyCode interactKey = KeyCode.E;
        public float interactDistance = 3f;
        public float openAngle = 90f;

        private Camera playerCamera;
        private readonly HashSet<Transform> openedDoors = new HashSet<Transform>();

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

            OpenDoor(door, hit.point);
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

        private void OpenDoor(Transform door, Vector3 hitPoint)
        {
            if (openedDoors.Contains(door))
            {
                return;
            }

            Bounds doorBounds = GetDoorBounds(door);
            Vector3 widthAxis = GetDoorWidthAxis(door, doorBounds);
            Vector3 hingePoint = GetHingePoint(doorBounds, widthAxis, hitPoint);
            float openDirection = GetOpenDirection(doorBounds, hingePoint);

            door.RotateAround(hingePoint, Vector3.up, openAngle * openDirection);
            openedDoors.Add(door);
            SetCollidersEnabled(door, false);
        }

        private Bounds GetDoorBounds(Transform door)
        {
            Bounds bounds = new Bounds(door.position, Vector3.zero);
            bool hasBounds = false;

            foreach (Renderer renderer in door.GetComponentsInChildren<Renderer>())
            {
                if (!renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            if (!hasBounds)
            {
                foreach (Collider collider in door.GetComponentsInChildren<Collider>())
                {
                    if (!collider.enabled)
                    {
                        continue;
                    }

                    if (!hasBounds)
                    {
                        bounds = collider.bounds;
                        hasBounds = true;
                    }
                    else
                    {
                        bounds.Encapsulate(collider.bounds);
                    }
                }
            }

            return hasBounds ? bounds : new Bounds(door.position, Vector3.one);
        }

        private Vector3 GetDoorWidthAxis(Transform door, Bounds doorBounds)
        {
            Vector3 right = Vector3.ProjectOnPlane(door.right, Vector3.up).normalized;
            Vector3 forward = Vector3.ProjectOnPlane(door.forward, Vector3.up).normalized;

            if (right.sqrMagnitude < 0.001f)
            {
                right = Vector3.right;
            }

            if (forward.sqrMagnitude < 0.001f)
            {
                forward = Vector3.forward;
            }

            float rightSpan = GetProjectedSpan(doorBounds, right);
            float forwardSpan = GetProjectedSpan(doorBounds, forward);
            return rightSpan >= forwardSpan ? right : forward;
        }

        private Vector3 GetHingePoint(Bounds doorBounds, Vector3 widthAxis, Vector3 hitPoint)
        {
            Vector3 normalizedWidth = widthAxis.normalized;
            float centerProjection = Vector3.Dot(doorBounds.center, normalizedWidth);
            float hitProjection = Vector3.Dot(hitPoint, normalizedWidth);
            float hingeSign = hitProjection >= centerProjection ? -1f : 1f;
            float halfWidth = GetProjectedSpan(doorBounds, normalizedWidth) * 0.5f;

            Vector3 hingePoint = doorBounds.center + normalizedWidth * hingeSign * halfWidth;
            hingePoint.y = doorBounds.center.y;
            return hingePoint;
        }

        private float GetOpenDirection(Bounds doorBounds, Vector3 hingePoint)
        {
            if (playerCamera == null)
            {
                return 1f;
            }

            Vector3 hingeToCenter = doorBounds.center - hingePoint;
            Vector3 playerPosition = playerCamera.transform.position;
            Vector3 positiveCenter = hingePoint + Quaternion.AngleAxis(openAngle, Vector3.up) * hingeToCenter;
            Vector3 negativeCenter = hingePoint + Quaternion.AngleAxis(-openAngle, Vector3.up) * hingeToCenter;

            float positiveDistance = (positiveCenter - playerPosition).sqrMagnitude;
            float negativeDistance = (negativeCenter - playerPosition).sqrMagnitude;
            return positiveDistance >= negativeDistance ? 1f : -1f;
        }

        private float GetProjectedSpan(Bounds bounds, Vector3 axis)
        {
            Vector3 normalizedAxis = axis.normalized;
            Vector3[] corners = GetBoundsCorners(bounds);
            float min = Vector3.Dot(corners[0], normalizedAxis);
            float max = min;

            for (int i = 1; i < corners.Length; i++)
            {
                float projection = Vector3.Dot(corners[i], normalizedAxis);
                min = Mathf.Min(min, projection);
                max = Mathf.Max(max, projection);
            }

            return max - min;
        }

        private Vector3[] GetBoundsCorners(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            return new[]
            {
                new Vector3(min.x, min.y, min.z),
                new Vector3(min.x, min.y, max.z),
                new Vector3(min.x, max.y, min.z),
                new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z),
                new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z)
            };
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
