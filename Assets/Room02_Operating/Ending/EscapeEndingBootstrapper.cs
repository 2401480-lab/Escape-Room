using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom
{
    public static class EscapeEndingBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            string sceneName = SceneManager.GetActiveScene().name;
            if (sceneName != "Show" && sceneName != "Scene_OperatingRoom")
            {
                return;
            }

            EnsureComponent<EscapeChaseQTE>("EscapeChaseQTE");
            EnsureComponent<EscapeExitController>("EscapeExitController");

#if UNITY_EDITOR
            EnsureComponent<EscapeKeyDebugGrant>("EscapeKeyDebugGrant");
#endif
        }

        private static void EnsureComponent<T>(string objectName) where T : Component
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
