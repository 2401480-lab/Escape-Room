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
                TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(ttf);
                fontAsset.name = "MalgunGothic_TMP";

                // HideAndDontSave 플래그 제거 — 이게 없으면 도메인 리로드 시 텍스처 소멸
                fontAsset.hideFlags = HideFlags.None;
                if (fontAsset.atlasTextures != null)
                {
                    foreach (Texture2D tex in fontAsset.atlasTextures)
                    {
                        if (tex != null)
                        {
                            tex.hideFlags = HideFlags.None;
                            tex.name = "Atlas";
                        }
                    }
                }
                if (fontAsset.material != null)
                {
                    fontAsset.material.hideFlags = HideFlags.None;
                    fontAsset.material.name = "Material";
                }

                // 메인 에셋 저장
                AssetDatabase.CreateAsset(fontAsset, AssetPath);

                // 텍스처·머티리얼을 서브에셋으로 등록
                if (fontAsset.atlasTextures != null)
                    foreach (Texture2D tex in fontAsset.atlasTextures)
                        if (tex != null) AssetDatabase.AddObjectToAsset(tex, fontAsset);

                if (fontAsset.material != null)
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

                EditorUtility.SetDirty(fontAsset);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
                Debug.Log("[Room02] 맑은 고딕 TMP 폰트 에셋 생성 완료.");
            }
            else
            {
                Debug.Log("[Room02] 기존 맑은 고딕 TMP 폰트 에셋 재사용.");
            }

            // ── 3. 기본 폰트(LiberationSans)의 Fallback에 맑은 고딕 추가 ──────
            //    기본 폰트를 교체하면 기존 UI가 깨지므로 Fallback 방식 사용
            //    영문·기호 → LiberationSans, 한글 → MalgunGothic(fallback)
            TMP_Settings settings = Resources.Load<TMP_Settings>("TMP Settings");
            if (settings != null)
            {
                SerializedObject settingsSO = new SerializedObject(settings);
                SerializedProperty defaultFontProp = settingsSO.FindProperty("m_defaultFontAsset");
                TMP_FontAsset defaultFont = defaultFontProp?.objectReferenceValue as TMP_FontAsset;

                if (defaultFont != null)
                {
                    SerializedObject fontSO = new SerializedObject(defaultFont);
                    SerializedProperty fallbackList = fontSO.FindProperty("m_FallbackFontAssetTable");
                    if (fallbackList != null)
                    {
                        // 이미 추가됐는지 확인
                        bool alreadyAdded = false;
                        for (int i = 0; i < fallbackList.arraySize; i++)
                        {
                            if (fallbackList.GetArrayElementAtIndex(i).objectReferenceValue == existing)
                            {
                                alreadyAdded = true;
                                break;
                            }
                        }

                        if (!alreadyAdded)
                        {
                            fallbackList.arraySize++;
                            fallbackList.GetArrayElementAtIndex(fallbackList.arraySize - 1).objectReferenceValue = existing;
                            fontSO.ApplyModifiedProperties();
                            EditorUtility.SetDirty(defaultFont);
                            AssetDatabase.SaveAssets();
                            Debug.Log("[Room02] LiberationSans Fallback에 맑은 고딕 추가 완료.");
                        }
                    }
                }
            }

            EditorUtility.DisplayDialog("완료",
                "한글 폰트 세팅 완료!\n\n" +
                "• 폰트: 맑은 고딕 (malgun.ttf)\n" +
                "• 방식: LiberationSans Fallback 추가\n" +
                "  (영문은 기존 폰트, 한글은 자동 전환)\n\n" +
                "플레이하면 한글이 정상 출력됩니다.", "확인");
        }
    }
}
