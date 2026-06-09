using UnityEngine;

namespace EscapeGame
{
    // 방 클리어 기록 전역 관리 — DontDestroyOnLoad
    // 팀원들이 클리어 조건 추가 시 여기에 작성
    public class GameData : MonoBehaviour
    {
        public static GameData Instance { get; private set; }

        public bool room01Cleared { get; private set; }
        public bool room02Cleared { get; private set; }
        public bool room03Cleared { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetRoomCleared(int roomNumber)
        {
            switch (roomNumber)
            {
                case 1: room01Cleared = true; break;
                case 2: room02Cleared = true; break;
                case 3: room03Cleared = true; break;
            }
        }

        public bool AllRoomsCleared() => room01Cleared && room02Cleared && room03Cleared;
    }
}
