using UnityEngine;

namespace EscapeRoom
{
    public class ZoneDoorActivator : MonoBehaviour
    {
        [SerializeField] private ZoneType nextZone = ZoneType.Corridor;

        public ZoneType NextZone => nextZone;

        public void ActivateNextZone()
        {
            if (ZoneManager.Instance != null)
            {
                ZoneManager.Instance.ActivateZone(nextZone);
            }
        }
    }
}
