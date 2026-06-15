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
