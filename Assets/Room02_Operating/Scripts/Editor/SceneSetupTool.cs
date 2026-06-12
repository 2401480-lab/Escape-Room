using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace Room02Operating
{
    public static class SceneSetupTool
    {
        private const string CLUE_PATH = "Assets/Room02_Operating/ScriptableObjects";

        // ── 복도 씬 세팅 ──────────────────────────────────────────────────
        [MenuItem("Tools/Room02/Setup Corridor Scene")]
        public static void SetupCorridor()
        {
            SetupManagers();
            SetupUI();
            SetupCorridorClues();
            Debug.Log("[Room02] 복도 씬 세팅 완료.");
            EditorUtility.DisplayDialog("완료", "복도 씬 세팅 완료!\n\n남은 작업:\n- UI 오브젝트 Inspector 연결\n- Clue 오브젝트 위치 조정\n- HoverDetector Layer 설정", "확인");
        }

        // ── 수술실 씬 세팅 ────────────────────────────────────────────────
        [MenuItem("Tools/Room02/Setup OperatingRoom Scene")]
        public static void SetupOperatingRoom()
        {
            SetupManagers();
            SetupUI();
            SetupOperatingRoomClues();
            Debug.Log("[Room02] 수술실 씬 세팅 완료.");
            EditorUtility.DisplayDialog("완료", "수술실 씬 세팅 완료!", "확인");
        }

        // ── 분장실 씬 세팅 ────────────────────────────────────────────────
        [MenuItem("Tools/Room02/Setup DressingRoom Scene")]
        public static void SetupDressingRoom()
        {
            SetupManagers();
            SetupUI();
            SetupDressingRoomClues();
            Debug.Log("[Room02] 분장실 씬 세팅 완료.");
            EditorUtility.DisplayDialog("완료", "분장실 씬 세팅 완료!", "확인");
        }

        // ═════════════════════════════════════════════════════════════════
        // 공통: 매니저 오브젝트 생성
        // ═════════════════════════════════════════════════════════════════
        static void SetupManagers()
        {
            EnsureGameObject("RoomGameManager", go =>
            {
                if (go.GetComponent<RoomGameManager>() == null)
                    go.AddComponent<RoomGameManager>();
            });
        }

        // ═════════════════════════════════════════════════════════════════
        // 공통: UI Canvas + 각 패널 생성
        // ═════════════════════════════════════════════════════════════════
        static void SetupUI()
        {
            // ── Canvas ─────────────────────────────────────────────────
            var canvas = EnsureGameObject("UI_Canvas", go =>
            {
                if (go.GetComponent<Canvas>() == null)
                {
                    var c = go.AddComponent<Canvas>();
                    c.renderMode = RenderMode.ScreenSpaceOverlay;
                    go.AddComponent<CanvasScaler>();
                    go.AddComponent<GraphicRaycaster>();
                }
            });

            // ── HoverText ──────────────────────────────────────────────
            EnsureChild(canvas, "HoverText", go =>
            {
                if (go.GetComponent<TextMeshProUGUI>() == null)
                {
                    var tmp = go.AddComponent<TextMeshProUGUI>();
                    tmp.text = "";
                    tmp.fontSize = 24;
                    tmp.alignment = TextAlignmentOptions.Center;
                    var rect = go.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(400, 50);
                    rect.anchoredPosition = new Vector2(0, -80);
                }
                go.SetActive(false);
            });

            // ── CluePanel ──────────────────────────────────────────────
            EnsureChild(canvas, "CluePanel", go =>
            {
                if (go.GetComponent<CluePanel>() == null)
                    go.AddComponent<CluePanel>();
                go.SetActive(false);
            });

            // ── DeductionPopup ─────────────────────────────────────────
            EnsureChild(canvas, "DeductionPopup", go =>
            {
                if (go.GetComponent<DeductionPopup>() == null)
                    go.AddComponent<DeductionPopup>();
                go.SetActive(false);
            });

            // ── NotebookUI ─────────────────────────────────────────────
            EnsureChild(canvas, "NotebookUI", go =>
            {
                if (go.GetComponent<NotebookUI>() == null)
                    go.AddComponent<NotebookUI>();
                go.SetActive(false);
            });

            // ── SuspectSelection ───────────────────────────────────────
            EnsureChild(canvas, "SuspectSelection", go =>
            {
                if (go.GetComponent<SuspectSelection>() == null)
                    go.AddComponent<SuspectSelection>();
                go.SetActive(false);
            });

            // ── InventoryBar ───────────────────────────────────────────
            EnsureChild(canvas, "InventoryBar", go =>
            {
                if (go.GetComponent<InventoryManager>() == null)
                    go.AddComponent<InventoryManager>();
            });

            // ── HoverDetector를 Main Camera에 부착 ────────────────────
            var cam = Camera.main;
            if (cam != null && cam.GetComponent<HoverDetector>() == null)
            {
                var hd = cam.gameObject.AddComponent<HoverDetector>();
                Debug.Log("[Room02] HoverDetector → Main Camera 부착 완료. Inspector에서 HoverText 연결 필요.");
            }
        }

        // ═════════════════════════════════════════════════════════════════
        // 복도 단서 오브젝트 (구역 2)
        // ═════════════════════════════════════════════════════════════════
        static void SetupCorridorClues()
        {
            var cluesRoot = GameObject.Find("Clues");
            if (cluesRoot == null)
            {
                cluesRoot = new GameObject("Clues");
                Undo.RegisterCreatedObjectUndo(cluesRoot, "Create Clues Root");
            }

            CreateClueObject(cluesRoot, "Clue_CCTV_Memo",       "clue_cctv_memo",      new Vector3(0, 1, 3));
            CreateClueObject(cluesRoot, "Clue_Staff_Schedule",   "clue_staff_schedule", new Vector3(2, 1, 3));
            CreateClueObject(cluesRoot, "Clue_Broken_Locker",    "clue_broken_locker",  new Vector3(-2, 1, 5));
        }

        // ═════════════════════════════════════════════════════════════════
        // 수술실 단서 오브젝트 (구역 5 — 결정적 증거)
        // ═════════════════════════════════════════════════════════════════
        static void SetupOperatingRoomClues()
        {
            var cluesRoot = GameObject.Find("Clues");
            if (cluesRoot == null)
            {
                cluesRoot = new GameObject("Clues");
                Undo.RegisterCreatedObjectUndo(cluesRoot, "Create Clues Root");
            }

            CreateClueObject(cluesRoot, "Clue_Shoe_Print",        "clue_shoe_print",        new Vector3(0,   0.05f, 0));
            CreateClueObject(cluesRoot, "Clue_Toe_Print",         "clue_toe_print",         new Vector3(0.3f,0.05f, 0.2f));
            CreateClueObject(cluesRoot, "Clue_Under_Table_Dust",  "clue_under_table_dust",  new Vector3(0,   0.05f, -0.5f));
            CreateClueObject(cluesRoot, "Clue_Poison_Glass",      "clue_poison_glass",      new Vector3(1,   1,    2));
            CreateClueObject(cluesRoot, "Clue_Yoanna_Body",       "clue_yoanna_body",       new Vector3(0,   1,    0));
        }

        // ═════════════════════════════════════════════════════════════════
        // 분장실 단서 오브젝트 (구역 4 일부)
        // ═════════════════════════════════════════════════════════════════
        static void SetupDressingRoomClues()
        {
            var cluesRoot = GameObject.Find("Clues");
            if (cluesRoot == null)
            {
                cluesRoot = new GameObject("Clues");
                Undo.RegisterCreatedObjectUndo(cluesRoot, "Create Clues Root");
            }

            CreateClueObject(cluesRoot, "Clue_Paint_Can",   "clue_paint_can",   new Vector3(1,  0, 2));
            CreateClueObject(cluesRoot, "Clue_Gloves",      "clue_gloves",      new Vector3(-1, 1, 2));
            CreateClueObject(cluesRoot, "Clue_Poison_Bottle","clue_poison_bottle",new Vector3(0, 1, 3));
            CreateClueObject(cluesRoot, "Clue_Storage_Log", "clue_storage_log", new Vector3(2,  1, 1));
        }

        // ═════════════════════════════════════════════════════════════════
        // 유틸
        // ═════════════════════════════════════════════════════════════════
        static void CreateClueObject(GameObject parent, string goName, string clueID, Vector3 localPos)
        {
            // 이미 있으면 스킵
            var existing = GameObject.Find(goName);
            if (existing != null) return;

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = goName;
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * 0.2f;

            // InteractableObject + ClueData 연결
            var interactable = go.AddComponent<InteractableObject>();
            var clueData = AssetDatabase.LoadAssetAtPath<ClueData>($"{CLUE_PATH}/{clueID}.asset");
            if (clueData != null)
            {
                var so = new SerializedObject(interactable);
                so.FindProperty("clueData").objectReferenceValue = clueData;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[Room02] ClueData 에셋 없음: {clueID}. 먼저 Generate All Clue Assets 실행 필요.");
            }

            // Interactable 레이어 설정 (레이어 이름 "Interactable" 사전 생성 필요)
            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0) go.layer = layer;

            Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
            EditorUtility.SetDirty(go);
        }

        static GameObject EnsureGameObject(string name, System.Action<GameObject> setup)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            }
            setup(go);
            EditorUtility.SetDirty(go);
            return go;
        }

        static GameObject EnsureChild(GameObject parent, string name, System.Action<GameObject> setup)
        {
            var t = parent.transform.Find(name);
            GameObject go = t != null ? t.gameObject : null;
            if (go == null)
            {
                go = new GameObject(name);
                go.transform.SetParent(parent.transform, false);
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            }
            setup(go);
            EditorUtility.SetDirty(go);
            return go;
        }
    }
}
