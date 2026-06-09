using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeGame
{
    // 온보딩 → 방 선택 → 방 진행 흐름의 씬 로드 담당
    // RoomGameManager.OnRoomCleared 이벤트에 ReturnToRoomSelect() 연결
    public class RoomLoader : MonoBehaviour
    {
        public static RoomLoader Instance { get; private set; }

        // ── Build Settings 씬 이름 ──────────────────────────────────
        // 팀 합칠 때 실제 씬 이름으로 수정할 것
        private const string ONBOARDING_SCENE  = "Onboarding";     // 게임 온보딩 화면
        private const string ROOM_SELECT_SCENE = "RoomSelect";     // 테마(방) 선택 화면
        private const string ROOM01_SCENE      = "Room01_Conan";   // 코난 외교관 사건
        private const string ROOM02_SCENE      = "Room02";         // 팀원2 확정 후 수정
        private const string ROOM03_SCENE      = "Room03";         // 팀원3 확정 후 수정

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadOnboarding()    => SceneManager.LoadScene(ONBOARDING_SCENE);
        public void LoadRoomSelect()    => SceneManager.LoadScene(ROOM_SELECT_SCENE);
        public void ReturnToRoomSelect()=> SceneManager.LoadScene(ROOM_SELECT_SCENE);

        public void LoadRoom(int roomNumber)
        {
            switch (roomNumber)
            {
                case 1: SceneManager.LoadScene(ROOM01_SCENE); break;
                case 2: SceneManager.LoadScene(ROOM02_SCENE); break;
                case 3: SceneManager.LoadScene(ROOM03_SCENE); break;
                default: Debug.LogWarning($"존재하지 않는 방 번호: {roomNumber}"); break;
            }
        }
    }
}
