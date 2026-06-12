using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeGame
{
    public class SceneLoader : MonoBehaviour
    {
        public const string CorridorScene = "Scene_Corridor";
        public const string DressingRoomScene = "Scene_DressingRoom";
        public const string OperatingRoomScene = "Scene_OperatingRoom";

        public static SceneLoader Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void LoadCorridor() => LoadSceneByName(CorridorScene);

        public void LoadDressingRoom() => LoadSceneByName(DressingRoomScene);

        public void LoadOperatingRoom() => LoadSceneByName(OperatingRoomScene);

        public void ReloadCurrentScene()
        {
            LoadSceneByName(SceneManager.GetActiveScene().name);
        }

        public void LoadSceneByName(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogWarning("Scene name is empty.");
                return;
            }

            SceneManager.LoadScene(sceneName);
        }
    }
}
