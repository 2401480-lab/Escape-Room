using UnityEditor;
using UnityEditor.SceneManagement;

namespace EscapeGame.Editor
{
    [InitializeOnLoad]
    public static class OnboardingPlayModeStartScene
    {
        private const string OnboardingScenePath = "Assets/Onboarding.unity";

        static OnboardingPlayModeStartScene()
        {
            EditorApplication.delayCall += ConfigureStartScene;
            EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
        }

        private static void HandlePlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                ConfigureStartScene();
            }
        }

        private static void ConfigureStartScene()
        {
            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(OnboardingScenePath);
            if (sceneAsset == null || EditorSceneManager.playModeStartScene == sceneAsset)
            {
                return;
            }

            EditorSceneManager.playModeStartScene = sceneAsset;
        }
    }
}
