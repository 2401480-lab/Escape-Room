using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace EscapeRoom
{
    public static class HudRuntimeBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            bool isOperatingRoom = SceneManager.GetActiveScene().name == "Scene_OperatingRoom";

            EnsureEventSystem();

            if (!isOperatingRoom)
            {
                return;
            }

            EnsureHudCanvas();
            EnsurePlayerSetup();
            EnsureRuntimeObject<ClueJournalManager>("ClueJournalManager");
            EnsureRuntimeObject<StoryProgressManager>("StoryProgressManager");
            EnsureRuntimeObject<EndingUI>("EndingUI");
            EnsureRuntimeObject<ClueJournalUI>("ClueJournalUI");
            EnsureRuntimeObject<TimerUI>("TimerUI");
            EnsureRuntimeObject<SettingsUI>("SettingsUI");
            EnsureRuntimeObject<CluePickupPopupUI>("CluePickupPopupUI");
            EnsureRuntimeObject<ClueBoxRuntimeAdapter>("ClueBoxRuntimeAdapter");
            EnsureRuntimeObject<Room02FlashlightController>("Room02_FlashlightController");
            EnsureRuntimeObject<Room02BgmPlayer>("Room02_BGM");
        }

        private static void EnsurePlayerSetup()
        {
            Time.timeScale = 1f;

            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                player = new GameObject("Player");
            }

            Camera mainCamera = Camera.main;
            GameObject playerStart = GameObject.Find("PlayerStart");

            if (playerStart != null)
            {
                player.transform.position = playerStart.transform.position;
                player.transform.rotation = playerStart.transform.rotation;
            }
            else if (mainCamera != null)
            {
                Vector3 cameraPosition = mainCamera.transform.position;
                player.transform.position = new Vector3(cameraPosition.x, Mathf.Max(0f, cameraPosition.y - 1.65f), cameraPosition.z);
                player.transform.rotation = Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f);
            }

            TrySetPlayerTag(player);

            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller == null)
            {
                controller = player.AddComponent<CharacterController>();
            }

            controller.radius = 0.5f;
            controller.height = 2f;
            controller.center = new Vector3(0f, 1f, 0f);

            if (player.GetComponent<global::PlayerMove>() == null)
            {
                player.AddComponent<global::PlayerMove>();
            }

            if (player.GetComponent<EscapeGame.DoorInteractor>() == null)
            {
                player.AddComponent<EscapeGame.DoorInteractor>();
            }

            if (mainCamera != null)
            {
                mainCamera.transform.SetParent(player.transform, false);
                mainCamera.transform.localPosition = new Vector3(0f, 1.65f, 0f);
                mainCamera.transform.localRotation = Quaternion.identity;
            }
        }

        private static void TrySetPlayerTag(GameObject player)
        {
            try
            {
                player.tag = "Player";
            }
            catch
            {
                // Some project copies do not define a Player tag yet. Name lookup still works.
            }
        }

        private static void EnsureHudCanvas()
        {
            GameObject canvasObject = GameObject.Find("HUD_Canvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("HUD_Canvas");
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (canvasObject.GetComponent<CanvasScaler>() == null)
            {
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();
        }

        private static void EnsureRuntimeObject<T>(string objectName) where T : Component
        {
            if (Object.FindObjectOfType<T>() != null)
            {
                return;
            }

            GameObject go = new GameObject(objectName);
            go.AddComponent<T>();
        }
    }
}
