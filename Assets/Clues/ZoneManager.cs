using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom
{
    public class ZoneManager : MonoBehaviour
    {
        public static ZoneManager Instance { get; private set; }

        [SerializeField] private ZoneType initialZone = ZoneType.Lobby;

        public UnityEvent<string> OnZoneEntered;

        public ZoneType CurrentZone { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            ActivateZone(ZoneType.Lobby);
        }

        public void ActivateZone(ZoneType zone)
        {
            foreach (ZoneType zoneType in System.Enum.GetValues(typeof(ZoneType)))
            {
                GameObject zoneObject = GameObject.Find(GetZoneObjectName(zoneType));
                if (zoneObject != null)
                {
                    zoneObject.SetActive(zoneType == zone);
                }
            }

            CurrentZone = zone;
            OnZoneEntered?.Invoke(GetZoneDisplayName(zone));
        }

        private static string GetZoneObjectName(ZoneType zone)
        {
            switch (zone)
            {
                case ZoneType.Lobby:
                    return "Zone_Lobby";
                case ZoneType.Corridor:
                    return "Zone_Corridor";
                case ZoneType.Ward:
                    return "Zone_Ward";
                case ZoneType.Storage:
                    return "Zone_Storage";
                case ZoneType.DressingRoom:
                    return "Zone_DressingRoom";
                case ZoneType.OperatingRoom:
                    return "Zone_OperatingRoom";
                default:
                    return $"Zone_{zone}";
            }
        }

        private static string GetZoneDisplayName(ZoneType zone)
        {
            switch (zone)
            {
                case ZoneType.Lobby:
                    return "Lobby";
                case ZoneType.Corridor:
                    return "Corridor";
                case ZoneType.Ward:
                    return "Ward";
                case ZoneType.Storage:
                    return "Storage";
                case ZoneType.DressingRoom:
                    return "DressingRoom";
                case ZoneType.OperatingRoom:
                    return "OperatingRoom";
                default:
                    return zone.ToString();
            }
        }
    }

    public enum ZoneType
    {
        Lobby,
        Corridor,
        Ward,
        Storage,
        DressingRoom,
        OperatingRoom
    }
}
