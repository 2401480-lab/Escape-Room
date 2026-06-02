using UnityEngine;
using UnityEngine.SceneManagement;

// 방 선택 화면에서 호출 — 씬 이름은 Build Settings 순서와 일치해야 함
// RoomGameManager.OnRoomCleared 이벤트에 ReturnToRoomSelect() 연결
public class RoomLoader : MonoBehaviour
{
    public static RoomLoader Instance { get; private set; }

    // Build Settings 씬 이름 (팀 합칠 때 실제 씬 이름으로 수정)
    private const string ROOM_SELECT_SCENE = "RoomSelect";
    private const string ROOM01_SCENE      = "Room01_Conan";
    private const string ROOM02_SCENE      = "Room02";          // 팀원 2 확정 후 수정
    private const string ROOM03_SCENE      = "Room03";          // 팀원 3 확정 후 수정

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

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

    public void ReturnToRoomSelect()
    {
        SceneManager.LoadScene(ROOM_SELECT_SCENE);
    }
}
