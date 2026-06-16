using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EscapeRoom
{
    public static class HudRuntimeBootstrapper
    {
        private const string Room02ScenePath = "Assets/Room02_Operating/Scenes/Show.unity";
        private static bool subscribedToSceneLoaded;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            EnsureSceneLoadedSubscription();
            BootstrapRoom02Runtime(SceneManager.GetActiveScene());
        }

        private static void EnsureSceneLoadedSubscription()
        {
            if (subscribedToSceneLoaded)
            {
                return;
            }

            SceneManager.sceneLoaded += HandleSceneLoaded;
            subscribedToSceneLoaded = true;
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BootstrapRoom02Runtime(scene);
        }

        private static void BootstrapRoom02Runtime(Scene scene)
        {
            if (!IsRoom02Scene(scene))
            {
                return;
            }

            EnsureHudCanvas();
            EnsureEventSystem();
            EnsureRuntimeObject<ClueJournalManager>("ClueJournalManager");
            EnsureRuntimeObject<StoryProgressManager>("StoryProgressManager");
            EnsureRuntimeObject<EndingUI>("EndingUI");
            EnsureRuntimeObject<GameOverUI>("GameOverUI");
            EnsureRuntimeObject<ClueJournalUI>("ClueJournalUI");
            EnsureRuntimeObject<TimerUI>("TimerUI");
            EnsureRuntimeObject<ControlHintUI>("ControlHintUI");
            EnsureRuntimeObject<SettingsUI>("SettingsUI");
            EnsureRuntimeObject<CluePickupPopupUI>("CluePickupPopupUI");
            EnsureRuntimeObject<ClueBoxRuntimeAdapter>("ClueBoxRuntimeAdapter");
            EnsureRuntimeObject<IntroScenarioUI>("IntroScenarioUI");
            EnsureRuntimeObject<Room02FlashlightController>("Room02_FlashlightController");
            EnsureRuntimeObject<Room02BgmPlayer>("Room02_BGM");
        }

        private static bool IsRoom02Scene(Scene scene)
        {
            return scene.name == "Show" || scene.path == Room02ScenePath;
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
