using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom
{
    public static class FontHelper
    {
        private const string GalmuriTmpResourcePath = "Fonts/Galmuri11_TMP";
        private const string GalmuriTtfResourcePath = "Fonts/Galmuri11";

        private static TMP_FontAsset cachedFont;

        public static TMP_FontAsset KoreanFont
        {
            get
            {
                if (cachedFont != null) return cachedFont;
                cachedFont = Resources.Load<TMP_FontAsset>(GalmuriTmpResourcePath);
                if (cachedFont != null)
                {
                    return cachedFont;
                }

                Font sourceFont = Resources.Load<Font>(GalmuriTtfResourcePath);
                if (sourceFont != null)
                {
                    cachedFont = TMP_FontAsset.CreateFontAsset(sourceFont);
                    cachedFont.name = "Galmuri11_Runtime_TMP";
                    cachedFont.atlasPopulationMode = AtlasPopulationMode.Dynamic;
                }

                return cachedFont;
            }
        }

        public static void Apply(TextMeshProUGUI tmp)
        {
            if (tmp == null) return;
            TMP_FontAsset font = KoreanFont;
            if (font != null) tmp.font = font;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RegisterSceneFontRefresh()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
            SceneManager.sceneLoaded += HandleSceneLoaded;
            ApplyToLoadedTextObjects();
        }

        private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ApplyToLoadedTextObjects();
        }

        private static void ApplyToLoadedTextObjects()
        {
            TextMeshProUGUI[] textObjects = Object.FindObjectsByType<TextMeshProUGUI>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (TextMeshProUGUI textObject in textObjects)
            {
                Apply(textObject);
            }
        }
    }
}
