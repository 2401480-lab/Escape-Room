using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using EscapeRoom.Editor;

namespace EscapeRoom
{
    /// <summary>
    /// Show.unity (에셋팩 데모씬)을 게임 플레이 가능한 씬으로 원클릭 변환.
    /// 기존 라이팅·건물·Light Probe는 그대로 유지, 플레이어·UI·단서만 추가.
    /// </summary>
    public static class ShowSceneSetup
    {
        // Show.unity 의 Capsule 스폰 위치와 동일 (Y는 PositionFixer가 보정)
        private static readonly Vector3 DefaultSpawnPos = new Vector3(-1.5f, 0f, -13f);

        [MenuItem("Tools/Room02/Convert Show Scene to Game")]
        public static void ConvertShowScene()
        {
            // ── 1. 기존 Capsule(데모 플레이어) 제거 ───────────────────────────
            RemoveOldCapsule();

            // ── 2. Player 생성 (CharacterController + PlayerMove + Camera) ───
            GameObject player = SetupPlayer();

            // ── 3. HUD_Canvas (UI 전체) ──────────────────────────────────────
            SetupHUD();

            // ── 4. 매니저들 ──────────────────────────────────────────────────
            EnsureComponent<ClueJournalManager>("ClueJournalManager");

            // ── 5. 테스트 큐브 1개 (플레이어 앞 2m) ─────────────────────────
            PlaceTestClue(player.transform.position);

            // ── 6. 씬 저장 표시 ──────────────────────────────────────────────
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("완료",
                "Show.unity → 게임씬 변환 완료!\n\n" +
                "① Ctrl+S 저장\n" +
                "② Tools > Room02 > Fix Player & Clue Positions\n" +
                "③ Ctrl+S 저장\n" +
                "④ 플레이 버튼!\n\n" +
                "F키로 앞 큐브 수집, E키로 문 열기 테스트", "확인");
        }

        // ── 기존 Capsule 제거 ─────────────────────────────────────────────────
        static void RemoveOldCapsule()
        {
            // "Capsule", "capsule" 이름의 오브젝트 삭제 (PlayerMove가 붙어있는 것)
            foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
            {
                if (go.name == "Capsule" || go.name == "capsule")
                {
                    Debug.Log($"[Room02] 기존 데모 플레이어 제거: {go.name}");
                    Undo.DestroyObjectImmediate(go);
                }
            }

            // 씬 루트에 떠있는 Main Camera(플레이어와 무관한 것) 정리
            Camera[] cams = Object.FindObjectsOfType<Camera>();
            foreach (Camera cam in cams)
            {
                // 부모 없이 떠있는 카메라 = 데모 카메라 → 제거
                if (cam.transform.parent == null)
                {
                    Debug.Log($"[Room02] 독립 카메라 제거: {cam.gameObject.name}");
                    Undo.DestroyObjectImmediate(cam.gameObject);
                }
            }
        }

        // ── Player 생성 ───────────────────────────────────────────────────────
        static GameObject SetupPlayer()
        {
            // 이미 있으면 재사용
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                player.name = "Player";
                Undo.RegisterCreatedObjectUndo(player, "Create Player");
            }

            // 위치: 건물 내부 (Show.unity 기존 Capsule 위치)
            player.transform.position = DefaultSpawnPos;

            // MeshRenderer 끄기 (FPS이므로 몸통 불필요)
            MeshRenderer mr = player.GetComponent<MeshRenderer>();
            if (mr != null) mr.enabled = false;

            // CapsuleCollider 제거 → CharacterController로 교체
            CapsuleCollider cc2 = player.GetComponent<CapsuleCollider>();
            if (cc2 != null) Undo.DestroyObjectImmediate(cc2);

            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc == null) cc = Undo.AddComponent<CharacterController>(player);
            cc.center    = new Vector3(0f, 1f, 0f);
            cc.height    = 2f;
            cc.radius    = 0.5f;
            cc.skinWidth = 0.08f;

            // PlayerMove
            if (player.GetComponent<PlayerMove>() == null)
                Undo.AddComponent<PlayerMove>(player);

            // Main Camera를 자식으로
            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                GameObject camGo = new GameObject("Main Camera");
                camGo.tag = "MainCamera";
                Undo.RegisterCreatedObjectUndo(camGo, "Create Main Camera");
                mainCam = camGo.AddComponent<Camera>();
                camGo.AddComponent<AudioListener>();
            }
            Undo.SetTransformParent(mainCam.transform, player.transform, "Parent Camera");
            mainCam.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            mainCam.transform.localRotation = Quaternion.identity;

            EditorUtility.SetDirty(player);
            Debug.Log($"[Room02] Player 세팅 완료 @ {player.transform.position}");
            return player;
        }

        // ── HUD_Canvas ────────────────────────────────────────────────────────
        static void SetupHUD()
        {
            if (GameObject.Find("HUD_Canvas") != null) return;

            // Canvas
            GameObject canvas = new GameObject("HUD_Canvas");
            Undo.RegisterCreatedObjectUndo(canvas, "Create HUD_Canvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler cs = canvas.AddComponent<CanvasScaler>();
            cs.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cs.referenceResolution = new Vector2(1920f, 1080f);
            canvas.AddComponent<GraphicRaycaster>();

            // TimerUI (우상단)
            CreateTMPPanel(canvas, "TimerUI",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20f, -20f), new Vector2(200f, 60f),
                "00:00", 36f, TextAlignmentOptions.TopRight);

            // InteractionPromptUI (하단 중앙, 비활성)
            GameObject prompt = CreateTMPPanel(canvas, "InteractionPromptUI",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 140f), new Vector2(400f, 60f),
                "[F] 증거 수집", 26f, TextAlignmentOptions.Center);
            prompt.SetActive(false);

            // CluePickupPopupUI (하단 중앙)
            CreateTMPPanel(canvas, "CluePickupPopupUI",
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0f, 60f), new Vector2(600f, 80f),
                "", 28f, TextAlignmentOptions.Center);

            EditorUtility.SetDirty(canvas);
            Debug.Log("[Room02] HUD_Canvas 생성 완료.");
        }

        static GameObject CreateTMPPanel(GameObject parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 sizeDelta,
            string text, float fontSize, TextAlignmentOptions align)
        {
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent.transform, false);

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.anchorMin        = anchorMin;
            rt.anchorMax        = anchorMax;
            rt.pivot            = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta        = sizeDelta;

            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text      = text;
            tmp.fontSize  = fontSize;
            tmp.alignment = align;
            tmp.color     = Color.white;

            return go;
        }

        // ── 테스트 단서 큐브 ──────────────────────────────────────────────────
        static void PlaceTestClue(Vector3 playerPos)
        {
            // 기존 Clues 루트 정리
            GameObject cluesRoot = GameObject.Find("Clues");
            if (cluesRoot != null)
            {
                var old = new System.Collections.Generic.List<GameObject>();
                foreach (Transform t in cluesRoot.transform) old.Add(t.gameObject);
                foreach (var o in old) Undo.DestroyObjectImmediate(o);
            }
            else
            {
                cluesRoot = new GameObject("Clues");
                Undo.RegisterCreatedObjectUndo(cluesRoot, "Create Clues");
            }

            // 플레이어 앞 2m
            Vector3 cluePos = playerPos + new Vector3(0f, 0.15f, 2f);

            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = "TestClue_cast_notice";
            cube.transform.SetParent(cluesRoot.transform);
            cube.transform.position   = cluePos;
            cube.transform.localScale = new Vector3(0.3f, 0.1f, 0.2f);
            Undo.RegisterCreatedObjectUndo(cube, "Create TestClue");

            // Renderer 색상: 노란색으로 눈에 띄게
            MeshRenderer mr = cube.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(1f, 0.9f, 0.2f); // 노란색
                mr.sharedMaterial = mat;
            }

            ClueInteractable interactable = cube.AddComponent<ClueInteractable>();

            // ClueData 로드 (없으면 생성)
            ClueData asset = AssetDatabase.LoadAssetAtPath<ClueData>("Assets/Clues/Normal/cast_notice.asset");
            if (asset == null)
            {
                ClueAssetGenerator.GenerateStoryClueAssets();
                asset = AssetDatabase.LoadAssetAtPath<ClueData>("Assets/Clues/Normal/cast_notice.asset");
            }
            if (asset != null)
            {
                var so = new SerializedObject(interactable);
                so.FindProperty("clueData").objectReferenceValue = asset;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(cube);
            Debug.Log($"[Room02] 테스트 단서 배치: {cluePos}");
        }

        static void EnsureComponent<T>(string goName) where T : Component
        {
            if (Object.FindObjectOfType<T>() != null) return;
            GameObject go = new GameObject(goName);
            Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
            go.AddComponent<T>();
        }
    }
}
