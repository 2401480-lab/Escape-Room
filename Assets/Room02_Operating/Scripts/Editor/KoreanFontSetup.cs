using TMPro;
using UnityEditor;
using UnityEngine;

namespace EscapeRoom
{
    public static class KoreanFontSetup
    {
        private const string FontPath   = "Assets/Fonts/malgun.ttf";
        private const string AssetPath  = "Assets/Fonts/MalgunGothic_TMP.asset";

        [MenuItem("Tools/Room02/Setup Korean Font")]
        public static void SetupKoreanFont()
        {
            // ── 1. TTF 임포트 확인 ────────────────────────────────────────────
            Font ttf = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (ttf == null)
            {
                EditorUtility.DisplayDialog("오류",
                    $"폰트 파일을 찾을 수 없습니다:\n{FontPath}\n\n" +
                    "malgun.ttf를 Assets/Fonts/ 폴더에 복사하고 다시 실행하세요.", "확인");
                return;
            }

            // ── 2. TMP 다이나믹 폰트 에셋 생성 (한글 포함 모든 글자 런타임 렌더) ──
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            if (existing == null)
            {
                // Dynamic 아틀라스 방식 — 글자를 처음 사용할 때 자동 추가
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                    ttf,
                    samplingPointSize: 90,
                    padding: 9,
                    packingMode: UnityEngine.TextCore.Text.GlyphRenderMode.SDFAA,
                    atlasWidth: 1024,
                    atlasHeight: 1024,
                    AtlasPopulationMode.Dynamic,
                    enableMultiAtlasSupport: true);

                fontAsset.name = "MalgunGothic_TMP";
                AssetDatabase.CreateAsset(fontAsset, AssetPath);
                AssetDatabase.SaveAssets();
                existing = fontAsset;
                Debug.Log("[Room02] 맑은 고딕 TMP 폰트 에셋 생성 완료.");
            }
            else
            {
                Debug.Log("[Room02] 기존 맑은 고딕 TMP 폰트 에셋 재사용.");
            }

            // ── 3. TMP Settings 기본 폰트로 등록 ─────────────────────────────
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings != null)
            {
                SerializedObject so = new SerializedObject(settings);
                SerializedProperty prop = so.FindProperty("m_defaultFontAsset");
                if (prop != null)
                {
                    prop.objectReferenceValue = existing;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(settings);
                    AssetDatabase.SaveAssets();
                    Debug.Log("[Room02] TMP 기본 폰트를 맑은 고딕으로 변경 완료.");
                }
            }
            else
            {
                Debug.LogWarning("[Room02] TMP Settings를 찾을 수 없습니다. 수동으로 설정하세요.");
            }

            EditorUtility.DisplayDialog("완료",
                "한글 폰트 세팅 완료!\n\n" +
                "• 폰트: 맑은 고딕 (malgun.ttf)\n" +
                "• 방식: Dynamic Atlas (런타임 한글 자동 렌더)\n" +
                "• TMP 기본 폰트로 등록\n\n" +
                "기존 TextMeshPro 오브젝트는 Inspector에서\n" +
                "Font Asset → MalgunGothic_TMP 로 변경하면 됩니다.", "확인");
        }
    }
}
