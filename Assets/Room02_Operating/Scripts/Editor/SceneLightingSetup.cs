using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace EscapeRoom
{
    public static class SceneLightingSetup
    {
        [MenuItem("Tools/Room02/Setup Scene Lighting")]
        public static void SetupLighting()
        {
            // ── 앰비언트: Show.unity와 동일하게 Flat + 거의 검정 ──────────────
            RenderSettings.ambientMode  = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.04f); // 아주 어두운 파란빛
            RenderSettings.skybox       = null;
            RenderSettings.fog          = false;

            // ── 기존 Directional Light 어둡게 ─────────────────────────────────
            Light[] lights = Object.FindObjectsOfType<Light>();
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    l.intensity = 0.15f;
                    l.color     = new Color(0.6f, 0.65f, 0.8f); // 차가운 파란빛
                    EditorUtility.SetDirty(l.gameObject);
                    Debug.Log($"[Room02] Directional Light 어둡게 조정: {l.gameObject.name}");
                }
            }

            // ── 복도용 Point Light 배치 ────────────────────────────────────────
            // 씬에 "_CorridorLights" 루트가 없으면 새로 만들기
            GameObject lightsRoot = GameObject.Find("_CorridorLights");
            if (lightsRoot == null)
            {
                lightsRoot = new GameObject("_CorridorLights");
                Undo.RegisterCreatedObjectUndo(lightsRoot, "Create CorridorLights");

                // 복도 따라 4개 배치 (위치는 씬 상황에 따라 Move 툴로 조정)
                Vector3[] positions = {
                    new Vector3( 0f, 3f,  0f),
                    new Vector3( 0f, 3f,  8f),
                    new Vector3( 0f, 3f, 16f),
                    new Vector3( 0f, 3f, 24f),
                };
                float[] intensities = { 1.5f, 1.2f, 1.2f, 1.0f };

                for (int i = 0; i < positions.Length; i++)
                {
                    GameObject lightGo = new GameObject($"PointLight_{i + 1:00}");
                    Undo.RegisterCreatedObjectUndo(lightGo, $"Create PointLight_{i}");
                    lightGo.transform.SetParent(lightsRoot.transform);
                    lightGo.transform.position = positions[i];

                    Light pl = lightGo.AddComponent<Light>();
                    pl.type      = LightType.Point;
                    pl.range     = 12f;
                    pl.intensity = intensities[i];
                    pl.color     = new Color(1.0f, 0.92f, 0.8f); // 따뜻한 형광등색
                    pl.shadows   = LightShadows.None;

                    EditorUtility.SetDirty(lightGo);
                }

                Debug.Log("[Room02] 복도 PointLight 4개 생성 완료. Scene View에서 위치 조정 가능.");
            }
            else
            {
                Debug.Log("[Room02] _CorridorLights 이미 존재 — 라이팅 설정만 변경.");
            }

            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("완료",
                "씬 라이팅 세팅 완료!\n\n" +
                "• Ambient: 거의 검정 (Show.unity 동일)\n" +
                "• Directional Light 강도: 0.15\n" +
                "• 복도 PointLight 4개 생성\n\n" +
                "Hierarchy의 _CorridorLights 자식 선택 후\n" +
                "Move 툴(W)로 실내 위치 맞춰줘.\n\nCtrl+S 저장", "확인");
        }
    }
}
