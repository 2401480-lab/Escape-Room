using TMPro;
using UnityEditor;
using UnityEngine;

namespace EscapeRoom
{
    public static class KoreanFontSetup
    {
        private const string RegularFontPath = "Assets/Fonts/Galmuri11.ttf";
        private const string BoldFontPath = "Assets/Fonts/Galmuri11-Bold.ttf";
        private const string RegularAssetPath = "Assets/Resources/Fonts/Galmuri11_TMP.asset";
        private const string BoldAssetPath = "Assets/Resources/Fonts/Galmuri11-Bold_TMP.asset";

        [MenuItem("Tools/Room02/Setup Galmuri Font")]
        public static void SetupKoreanFont()
        {
            TMP_FontAsset regularFontAsset = EnsureFontAsset(
                RegularFontPath,
                RegularAssetPath,
                "Galmuri11_TMP");

            if (regularFontAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "폰트 설정 오류",
                    $"Galmuri 폰트 파일을 찾을 수 없습니다:\n{RegularFontPath}",
                    "확인");
                return;
            }

            EnsureFontAsset(BoldFontPath, BoldAssetPath, "Galmuri11-Bold_TMP");
            SetDefaultTmpFont(regularFontAsset);

            EditorUtility.DisplayDialog(
                "완료",
                "Galmuri11 폰트 설정이 완료되었습니다.\nTMP 기본 폰트와 Room02 런타임 UI가 Galmuri를 사용합니다.",
                "확인");
        }

        private static TMP_FontAsset EnsureFontAsset(string fontPath, string assetPath, string assetName)
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null)
            {
                return existing;
            }

            Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (sourceFont == null)
            {
                return null;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            fontAsset.name = assetName;
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.hideFlags = HideFlags.None;

            if (fontAsset.atlasTextures != null)
            {
                foreach (Texture2D texture in fontAsset.atlasTextures)
                {
                    if (texture != null)
                    {
                        texture.hideFlags = HideFlags.None;
                        texture.name = $"{assetName}_Atlas";
                    }
                }
            }

            if (fontAsset.material != null)
            {
                fontAsset.material.hideFlags = HideFlags.None;
                fontAsset.material.name = $"{assetName}_Material";
            }

            AssetDatabase.CreateAsset(fontAsset, assetPath);

            if (fontAsset.atlasTextures != null)
            {
                foreach (Texture2D texture in fontAsset.atlasTextures)
                {
                    if (texture != null)
                    {
                        AssetDatabase.AddObjectToAsset(texture, fontAsset);
                    }
                }
            }

            if (fontAsset.material != null)
            {
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
            }

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        }

        private static void SetDefaultTmpFont(TMP_FontAsset fontAsset)
        {
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings == null || fontAsset == null)
            {
                return;
            }

            SerializedObject settingsObject = new SerializedObject(settings);
            SerializedProperty defaultFontProperty = settingsObject.FindProperty("m_defaultFontAsset");
            if (defaultFontProperty == null)
            {
                return;
            }

            defaultFontProperty.objectReferenceValue = fontAsset;
            settingsObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
        }
    }
}
