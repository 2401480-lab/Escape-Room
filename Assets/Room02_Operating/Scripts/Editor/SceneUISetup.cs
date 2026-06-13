using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public static class SceneUISetup
    {
        [MenuItem("Tools/Room02/Setup Scene UI")]
        public static void SetupSceneUI()
        {
            // 중복 생성 방지
            if (GameObject.Find("HUD_Canvas") != null)
            {
                EditorUtility.DisplayDialog("알림", "HUD_Canvas가 이미 존재합니다.\n중복 생성을 건너뜁니다.", "확인");
                return;
            }

            GameObject canvas = CreateHUDCanvas();
            CreateTimerUI(canvas);
            CreateProximityVignetteUI(canvas);
            CreateCluePickupPopupUI(canvas);
            CreateSuspectConfirmUI(canvas);
            CreateInteractionPromptUI(canvas);

            ConnectExistingComponents();

            EditorUtility.SetDirty(canvas);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("[Room02] HUD_Canvas 및 UI 오브젝트 배치 완료.");
            EditorUtility.DisplayDialog("완료",
                "씬 UI 세팅 완료!\n\n생성된 오브젝트:\n" +
                "- HUD_Canvas\n  - TimerUI\n  - ProximityVignetteUI\n  - CluePickupPopupUI\n" +
                "  - SuspectConfirmUI\n  - InteractionPromptUI\n\n" +
                "Inspector에서 스크립트 참조를 확인해주세요.", "확인");
        }

        // ── HUD_Canvas ─────────────────────────────────────────────────────────
        private static GameObject CreateHUDCanvas()
        {
            GameObject go = new GameObject("HUD_Canvas");
            Undo.RegisterCreatedObjectUndo(go, "Create HUD_Canvas");

            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();

            return go;
        }

        // ── 1. TimerUI ─────────────────────────────────────────────────────────
        private static void CreateTimerUI(GameObject parent)
        {
            GameObject go = new GameObject("TimerUI");
            Undo.RegisterCreatedObjectUndo(go, "Create TimerUI");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            // 우측 상단 앵커
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot     = new Vector2(1f, 1f);
            rect.sizeDelta = new Vector2(200f, 60f);
            rect.anchoredPosition = new Vector2(-20f, -20f);

            // TimerUI 컴포넌트 (스크립트가 존재하면 자동 추가)
            TryAddComponent(go, "EscapeRoom.TimerUI");

            // 자식: TimerText
            GameObject textGo = new GameObject("TimerText");
            Undo.RegisterCreatedObjectUndo(textGo, "Create TimerText");
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            SetFullStretch(textRect);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = "00:00";
            tmp.fontSize  = 36f;
            tmp.alignment = TextAlignmentOptions.TopRight;
            tmp.color     = Color.white;
        }

        // ── 2. ProximityVignetteUI ─────────────────────────────────────────────
        private static void CreateProximityVignetteUI(GameObject parent)
        {
            GameObject go = new GameObject("ProximityVignetteUI");
            Undo.RegisterCreatedObjectUndo(go, "Create ProximityVignetteUI");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            SetFullStretch(rect);

            Image img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0f); // 알파 0
            img.raycastTarget = false;

            TryAddComponent(go, "EscapeRoom.ProximityVignetteUI");
        }

        // ── 3. CluePickupPopupUI ───────────────────────────────────────────────
        private static void CreateCluePickupPopupUI(GameObject parent)
        {
            GameObject go = new GameObject("CluePickupPopupUI");
            Undo.RegisterCreatedObjectUndo(go, "Create CluePickupPopupUI");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            // 하단 중앙
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot     = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(600f, 80f);
            rect.anchoredPosition = new Vector2(0f, 60f);

            TryAddComponent(go, "EscapeRoom.CluePickupPopupUI");

            // 자식: PopupText (알파 0)
            GameObject textGo = new GameObject("PopupText");
            Undo.RegisterCreatedObjectUndo(textGo, "Create PopupText");
            textGo.transform.SetParent(go.transform, false);
            RectTransform textRect = textGo.AddComponent<RectTransform>();
            SetFullStretch(textRect);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = "";
            tmp.fontSize  = 28f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = new Color(1f, 1f, 1f, 0f); // 알파 0
        }

        // ── 4. SuspectConfirmUI ────────────────────────────────────────────────
        private static void CreateSuspectConfirmUI(GameObject parent)
        {
            GameObject go = new GameObject("SuspectConfirmUI");
            Undo.RegisterCreatedObjectUndo(go, "Create SuspectConfirmUI");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            // 중앙
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(500f, 300f);
            rect.anchoredPosition = Vector2.zero;

            // 배경 패널
            Image bg = go.AddComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.85f);

            TryAddComponent(go, "EscapeRoom.SuspectConfirmUI");

            // 자식: ConfirmText
            CreateTMPChild(go, "ConfirmText",
                new Vector2(0f, 60f), new Vector2(460f, 100f),
                "이 인물이 범인이라고 확신합니까?", 28f, TextAlignmentOptions.Center);

            // 자식: YesButton
            CreateButton(go, "YesButton", new Vector2(-100f, -80f), new Vector2(160f, 60f), "예");

            // 자식: NoButton
            CreateButton(go, "NoButton", new Vector2(100f, -80f), new Vector2(160f, 60f), "아니오");

            go.SetActive(false);
        }

        // ── 5. InteractionPromptUI ─────────────────────────────────────────────
        private static void CreateInteractionPromptUI(GameObject parent)
        {
            GameObject go = new GameObject("InteractionPromptUI");
            Undo.RegisterCreatedObjectUndo(go, "Create InteractionPromptUI");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            // 하단 중앙
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot     = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(400f, 60f);
            rect.anchoredPosition = new Vector2(0f, 140f);

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = "[F] 증거 수집";
            tmp.fontSize  = 26f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            go.SetActive(false);
        }

        // ── 기존 오브젝트 참조 연결 ────────────────────────────────────────────
        private static void ConnectExistingComponents()
        {
            GameObject timerUI       = GameObject.Find("TimerUI");
            GameObject vignetteUI    = GameObject.Find("ProximityVignetteUI");
            GameObject popupUI       = GameObject.Find("CluePickupPopupUI");
            GameObject confirmUI     = GameObject.Find("SuspectConfirmUI");

            TryConnectReference("StoryProgressManager",  "timerUI",    timerUI);
            TryConnectReference("ChaseController",       "vignetteUI", vignetteUI);
            TryConnectReference("ClueJournalManager",    "pickupPopupUI", popupUI);
            TryConnectReference("EndingUI",              "suspectConfirmUI", confirmUI);
        }

        // ── 유틸 ───────────────────────────────────────────────────────────────

        private static void TryAddComponent(GameObject go, string fullTypeName)
        {
            System.Type type = System.Type.GetType(fullTypeName + ", Assembly-CSharp");
            if (type == null)
            {
                // 네임스페이스 없이 재시도
                string shortName = fullTypeName.Contains(".")
                    ? fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1)
                    : fullTypeName;
                type = System.Type.GetType(shortName + ", Assembly-CSharp");
            }

            if (type != null)
            {
                if (go.GetComponent(type) == null)
                    Undo.AddComponent(go, type);
            }
            else
            {
                Debug.LogWarning($"[Room02] 컴포넌트를 찾을 수 없음: {fullTypeName} — 스크립트 컴파일 후 다시 실행하거나 수동으로 추가하세요.");
            }
        }

        private static void TryConnectReference(string targetGoName, string fieldName, GameObject uiGo)
        {
            if (uiGo == null) return;

            GameObject target = GameObject.Find(targetGoName);
            if (target == null) return;

            foreach (var comp in target.GetComponents<MonoBehaviour>())
            {
                if (comp == null) continue;
                var so = new SerializedObject(comp);
                var prop = so.FindProperty(fieldName);
                if (prop != null)
                {
                    prop.objectReferenceValue = uiGo;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(comp);
                    Debug.Log($"[Room02] {targetGoName}.{fieldName} → {uiGo.name} 연결 완료");
                    return;
                }
            }

            Debug.LogWarning($"[Room02] {targetGoName}에서 필드 '{fieldName}'를 찾지 못했습니다. Inspector에서 수동 연결 필요.");
        }

        private static void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin        = Vector2.zero;
            rect.anchorMax        = Vector2.one;
            rect.offsetMin        = Vector2.zero;
            rect.offsetMax        = Vector2.zero;
        }

        private static void CreateTMPChild(GameObject parent, string name,
            Vector2 anchoredPos, Vector2 size, string text, float fontSize,
            TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.sizeDelta        = size;
            rect.anchoredPosition = anchoredPos;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.alignment = align;
            tmp.color     = Color.white;
        }

        private static void CreateButton(GameObject parent, string name,
            Vector2 anchoredPos, Vector2 size, string label)
        {
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent.transform, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(0.5f, 0.5f);
            rect.anchorMax        = new Vector2(0.5f, 0.5f);
            rect.pivot            = new Vector2(0.5f, 0.5f);
            rect.sizeDelta        = size;
            rect.anchoredPosition = anchoredPos;

            Image img = go.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            Button btn = go.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
            colors.pressedColor     = new Color(0.1f, 0.1f, 0.1f, 1f);
            btn.colors = colors;

            // 버튼 텍스트
            GameObject textGo = new GameObject("Text");
            Undo.RegisterCreatedObjectUndo(textGo, $"Create {name} Text");
            textGo.transform.SetParent(go.transform, false);

            RectTransform textRect = textGo.AddComponent<RectTransform>();
            SetFullStretch(textRect);

            TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = label;
            tmp.fontSize  = 24f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color     = Color.white;
        }
    }
}
