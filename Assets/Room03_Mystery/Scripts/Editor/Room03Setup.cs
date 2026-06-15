#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using Room03Mystery;

// Room03 시나리오 셋업용 에디터 유틸 (Coplay execute_script 로 단계별 실행)
public static class Room03Setup
{
    const string SO_DIR = "Assets/Room03_Mystery/ScriptableObjects";

    static ClueData MakeClue(string id, string name, string desc,
                             string hover, bool collectable)
    {
        string path = $"{SO_DIR}/{id}.asset";
        var cd = AssetDatabase.LoadAssetAtPath<ClueData>(path);
        if (cd == null)
        {
            cd = ScriptableObject.CreateInstance<ClueData>();
            AssetDatabase.CreateAsset(cd, path);
        }
        cd.clueID = id;
        cd.displayName = name;
        cd.description = desc;
        cd.hoverLabel = hover;
        cd.isCollectable = collectable;
        EditorUtility.SetDirty(cd);
        return cd;
    }

    // Phase A: ClueData 에셋 + Room03PuzzleManager 생성
    public static string Execute()
    {
        if (!Directory.Exists(SO_DIR)) Directory.CreateDirectory(SO_DIR);
        AssetDatabase.Refresh();

        MakeClue("clue_report", "부검 보고서",
            "사인이 석연치 않다. 보고서 하단: \"검체는 보관함에 따로 안치됨.\"",
            "부검 보고서 읽기", false);

        MakeClue("clue_memo", "흩어진 메모",
            "급하게 휘갈긴 메모. 누군가 이 방을 빠져나가려 했던 흔적이다.",
            "메모 살펴보기", false);

        // 액자 → 보관함 번호 07 (서랍 퍼즐의 선행 단서)
        MakeClue("clue_locker_07", "액자 사진",
            "사진 속 남자... 액자를 뒤집자 뒷면에 적힌 글씨: \"보관함 No.07\"",
            "액자 뒤집어보기", false);

        // 서랍 열림 단서 (머리 조사의 선행 조건)
        MakeClue("clue_drawer_open", "07번 보관함",
            "잠금이 풀린다. 트레이가 슬라이드되며 시체가 모습을 드러낸다.",
            "07번 보관함 열기", false);

        // 머리 → 황동 열쇠 + 코드 앞 2자리(73)
        MakeClue("item_brass_key", "황동 열쇠",
            "엉킨 머리카락 사이에서 작은 황동 열쇠를 찾았다. 발가락 태그 번호: 73",
            "머리 살펴보기", true);

        // 캐비닛 → UV 펜라이트
        MakeClue("item_uv_light", "UV 펜라이트",
            "약품 캐비닛 안에서 자외선 펜라이트를 발견했다. 숨겨진 글씨를 비춰볼 수 있겠다.",
            "약품 캐비닛 열기", true);

        // 칠판 → 코드 뒤 2자리(91)
        MakeClue("clue_blackboard", "칠판",
            "UV를 비추자 지워졌던 숫자가 드러난다. 거울에 비춰 순서를 맞추면: 91",
            "칠판 살펴보기", false);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Room03PuzzleManager GameObject
        var existing = Object.FindObjectOfType<Room03PuzzleManager>();
        if (existing == null)
        {
            var go = new GameObject("Room03PuzzleManager");
            go.AddComponent<Room03PuzzleManager>();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(go.scene);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        string msg = "[Phase A] ClueData 7개 + Room03PuzzleManager 생성 완료";
        Debug.Log(msg);
        return msg;
    }

    // ─── 공통 헬퍼 ────────────────────────────────────────────────────

    static ClueData LoadClue(string id) =>
        AssetDatabase.LoadAssetAtPath<ClueData>($"{SO_DIR}/{id}.asset");

    static GameObject Find(string path)
    {
        var go = GameObject.Find(path);
        if (go == null) Debug.LogError($"[Room03Setup] 오브젝트 못 찾음: {path}");
        return go;
    }

    // 자식 Renderer 들의 월드 바운드를 BoxCollider 에 맞춤
    static void FitBox(GameObject go, BoxCollider bc)
    {
        var rends = go.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return;
        Bounds b = rends[0].bounds;
        foreach (var r in rends) b.Encapsulate(r.bounds);
        bc.center = go.transform.InverseTransformPoint(b.center);
        Vector3 s = go.transform.InverseTransformVector(b.size);
        bc.size = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    static InteractableObject SetupInteractable(
        string goPath, string clueId, string requiredClueId, string requiredItemId)
    {
        var go = Find(goPath);
        if (go == null) return null;

        if (go.GetComponent<Collider>() == null)
        {
            var bc = go.AddComponent<BoxCollider>();
            FitBox(go, bc);
        }

        var io = go.GetComponent<InteractableObject>();
        if (io == null) io = go.AddComponent<InteractableObject>();

        var so = new SerializedObject(io);
        so.FindProperty("clueData").objectReferenceValue = LoadClue(clueId);
        so.FindProperty("requiredClueID").stringValue = requiredClueId ?? "";
        so.FindProperty("requiredItemID").stringValue = requiredItemId ?? "";
        so.ApplyModifiedProperties();
        return io;
    }

    // Phase B: 시체 머리 배치 + 상호작용 컴포넌트 부착/할당 + 서랍 슬라이드 + 이벤트 연결
    public static string Execute_PhaseB()
    {
        var pm = Object.FindObjectOfType<Room03PuzzleManager>();

        // 1) 책상 단서들 (선행조건 없음)
        SetupInteractable("props/table",       "clue_report",     null, null);
        SetupInteractable("props/notebook",    "clue_memo",       null, null);
        SetupInteractable("props/photo_frame", "clue_locker_07",  null, null);

        // 2) 07번 보관함 문 — 액자에서 번호(07) 확인해야 열림
        var drawerDoorPath = "props/corpse_drawer 7/corpse_drawer_door";
        var drawerIO = SetupInteractable(drawerDoorPath, "clue_drawer_open", "clue_locker_07", null);

        // 3) 서랍 트레이에 DrawerSlide 부착
        var trayPath = "props/corpse_drawer 7/corpse_drawer_table";
        var tray = Find(trayPath);
        DrawerSlide slide = null;
        if (tray != null)
        {
            slide = tray.GetComponent<DrawerSlide>();
            if (slide == null) slide = tray.AddComponent<DrawerSlide>();
        }

        // 4) 시체 머리(man_head) 배치 — 트레이 자식으로 두어 함께 슬라이드
        GameObject head = GameObject.Find("man_head");
        if (head == null)
        {
            var headAsset = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Room03_Mystery/Models/ManHead/man_head.glb");
            if (headAsset != null && tray != null)
            {
                head = (GameObject)PrefabUtility.InstantiatePrefab(headAsset);
                head.name = "man_head";
                head.transform.SetParent(tray.transform, false);
                // 트레이 윗면 중앙에 올림
                var rends = tray.GetComponentsInChildren<Renderer>();
                if (rends.Length > 0)
                {
                    Bounds b = rends[0].bounds;
                    foreach (var r in rends) b.Encapsulate(r.bounds);
                    head.transform.position = new Vector3(b.center.x, b.max.y, b.center.z);
                }
            }
            else Debug.LogError("[Room03Setup] man_head.glb 로드 실패 또는 트레이 없음");
        }

        // 5) 머리 조사 → 황동 열쇠 + 코드 73. 서랍 열린 뒤에만 가능.
        InteractableObject headIO = null;
        if (head != null)
        {
            if (head.GetComponent<Collider>() == null)
            {
                var bc = head.AddComponent<BoxCollider>();
                FitBox(head, bc);
            }
            headIO = head.GetComponent<InteractableObject>();
            if (headIO == null) headIO = head.AddComponent<InteractableObject>();
            var so = new SerializedObject(headIO);
            so.FindProperty("clueData").objectReferenceValue = LoadClue("item_brass_key");
            so.FindProperty("requiredClueID").stringValue = "clue_drawer_open";
            so.ApplyModifiedProperties();
        }

        // 6) 약품 캐비닛 — 황동 열쇠 있어야 열림 → UV 펜라이트 획득
        SetupInteractable("props/cabinet", "item_uv_light", null, "item_brass_key");

        // 7) 칠판 — UV 펜라이트 있어야 해독 → 코드 91
        var boardIO = SetupInteractable("props/writing_board", "clue_blackboard", null, "item_uv_light");

        // ─── UnityEvent 연결 ─────────────────────────────────────────
        // 서랍 문 조사 → 트레이 슬라이드 Open
        if (drawerIO != null && slide != null)
            UnityEventTools.AddPersistentListener(drawerIO.onInspected, slide.Open);

        // 머리 조사 → SolvePuzzle("drawer")
        if (headIO != null && pm != null)
            UnityEventTools.AddStringPersistentListener(
                headIO.onInspected, new UnityAction<string>(pm.SolvePuzzle), "drawer");

        // 칠판 해독 → SolvePuzzle("blackboard")
        if (boardIO != null && pm != null)
            UnityEventTools.AddStringPersistentListener(
                boardIO.onInspected, new UnityAction<string>(pm.SolvePuzzle), "blackboard");

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        string msg = "[Phase B] 머리 배치 + 상호작용 7개 + 서랍 슬라이드 + 이벤트 연결 완료";
        Debug.Log(msg);
        return msg;
    }

    // ─── UI 빌드 헬퍼 ─────────────────────────────────────────────────

    static RectTransform Rect(GameObject go) => go.GetComponent<RectTransform>();

    static void SetRect(GameObject go, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size)
    {
        var rt = Rect(go);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos; rt.sizeDelta = size;
    }

    static TMP_FontAsset _font;
    static TMP_FontAsset GetFont()
    {
        if (_font != null) return _font;
        try { if (TMP_Settings.defaultFontAsset != null) { _font = TMP_Settings.defaultFontAsset; return _font; } }
        catch { /* TMP 미설정 — 아래 fallback */ }
        var guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        if (guids.Length > 0)
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        return _font;
    }

    static TextMeshProUGUI MkText(Transform parent, string name, string content,
                                  float size, TextAlignmentOptions align)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = content; t.fontSize = size; t.alignment = align; t.color = Color.white;
        t.enableWordWrapping = true;
        var f = GetFont();
        if (f != null) t.font = f;
        return t;
    }

    // TMP 필수 리소스(폰트/설정) 임포트
    public static string Execute_ImportTMP()
    {
        var found = Directory.GetFiles("Library/PackageCache",
            "TMP Essential Resources.unitypackage", SearchOption.AllDirectories);
        if (found.Length == 0) { Debug.LogError("TMP Essentials 패키지 못 찾음"); return "FAIL"; }
        AssetDatabase.ImportPackage(found[0], false);
        AssetDatabase.Refresh();
        string msg = "[TMP] Essential Resources 임포트 요청: " + found[0];
        Debug.Log(msg);
        return msg;
    }

    static Image MkImage(Transform parent, string name, Color c)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        return img;
    }

    static Button MkButton(Transform parent, string name, string label, Vector2 size)
    {
        var img = MkImage(parent, name, new Color(0.18f, 0.18f, 0.22f, 0.95f));
        var btn = img.gameObject.AddComponent<Button>();
        SetRect(img.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, size);
        var t = MkText(img.transform, "Label", label, 28, TextAlignmentOptions.Center);
        SetRect(t.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return btn;
    }

    // Phase C: UI(단서 팝업/인벤토리/키패드) + HoverDetector + 문 열림 구성
    public static string Execute_PhaseC_UI()
    {
        var pm = Object.FindFirstObjectByType<Room03PuzzleManager>();

        // EventSystem
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Canvas
        var canvasGO = GameObject.Find("Room03_UI");
        if (canvasGO != null) Object.DestroyImmediate(canvasGO); // 재실행 시 깨끗이 다시
        canvasGO = new GameObject("Room03_UI", typeof(RectTransform));
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();
        var root = canvasGO.transform;

        // 1) Hover 텍스트 (하단 중앙)
        var hover = MkText(root, "HoverText", "", 34, TextAlignmentOptions.Center);
        SetRect(hover.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0, 120), new Vector2(900, 60));
        hover.gameObject.SetActive(false);

        // 2) 단서 팝업 (CluePanel)
        var panelRoot = MkImage(root, "CluePanelRoot", new Color(0, 0, 0, 0.75f));
        SetRect(panelRoot.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var box = MkImage(panelRoot.transform, "Box", new Color(0.1f, 0.1f, 0.12f, 0.98f));
        SetRect(box.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1000, 620));
        var detail = MkImage(box.transform, "DetailImage", Color.white);
        SetRect(detail.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -200), new Vector2(360, 360));
        detail.preserveAspect = true;
        var desc = MkText(box.transform, "Description", "", 30, TextAlignmentOptions.Top);
        SetRect(desc.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 130), new Vector2(900, 220));
        var closeBtn = MkButton(box.transform, "CloseButton", "닫기", new Vector2(200, 70));
        SetRect(closeBtn.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 50), new Vector2(200, 70));

        var clue = canvasGO.AddComponent<CluePanel>();
        var soClue = new SerializedObject(clue);
        soClue.FindProperty("panelRoot").objectReferenceValue = panelRoot.gameObject;
        soClue.FindProperty("detailImage").objectReferenceValue = detail;
        soClue.FindProperty("descriptionText").objectReferenceValue = desc;
        soClue.FindProperty("closeButton").objectReferenceValue = closeBtn;
        soClue.ApplyModifiedProperties();

        // 3) 인벤토리 바 (하단)
        var invBar = MkImage(root, "InventoryBar", new Color(0, 0, 0, 0.4f));
        SetRect(invBar.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 20), new Vector2(720, 140));
        var invMgr = canvasGO.AddComponent<InventoryManager>();
        var slotList = new List<InventorySlot>();
        for (int i = 0; i < 4; i++)
        {
            var slotImg = MkImage(invBar.transform, $"Slot{i}", new Color(0.15f, 0.15f, 0.18f, 0.95f));
            SetRect(slotImg.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-270 + i * 180, 0), new Vector2(160, 120));
            var btn = slotImg.gameObject.AddComponent<Button>();
            var icon = MkImage(slotImg.transform, "Icon", Color.white);
            SetRect(icon.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 12), new Vector2(96, 72));
            icon.preserveAspect = true; icon.gameObject.SetActive(false);
            var nameT = MkText(slotImg.transform, "Name", "", 20, TextAlignmentOptions.Bottom);
            SetRect(nameT.gameObject, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 8), new Vector2(150, 36));
            var slot = slotImg.gameObject.AddComponent<InventorySlot>();
            var soSlot = new SerializedObject(slot);
            soSlot.FindProperty("iconImage").objectReferenceValue = icon;
            soSlot.FindProperty("itemNameText").objectReferenceValue = nameT;
            soSlot.FindProperty("button").objectReferenceValue = btn;
            soSlot.ApplyModifiedProperties();
            slotList.Add(slot);
        }
        var soInv = new SerializedObject(invMgr);
        var slotsProp = soInv.FindProperty("slots");
        slotsProp.arraySize = slotList.Count;
        for (int i = 0; i < slotList.Count; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotList[i];
        soInv.ApplyModifiedProperties();

        // 4) 키패드 (우측 하단)
        var kpRoot = MkImage(root, "KeypadPanel", new Color(0.08f, 0.08f, 0.1f, 0.97f));
        SetRect(kpRoot.gameObject, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-220, 320), new Vector2(360, 520));
        var kpTitle = MkText(kpRoot.transform, "Title", "출구 키패드", 26, TextAlignmentOptions.Center);
        SetRect(kpTitle.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -30), new Vector2(320, 40));
        var display = MkText(kpRoot.transform, "Display", "____", 48, TextAlignmentOptions.Center);
        SetRect(display.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(320, 70));
        var keypad = kpRoot.gameObject.AddComponent<KeypadLock>();
        var soKp = new SerializedObject(keypad);
        soKp.FindProperty("display").objectReferenceValue = display;
        soKp.ApplyModifiedProperties();

        // 숫자/기능 버튼 3x4 그리드
        string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "C", "0", "OK" };
        for (int i = 0; i < keys.Length; i++)
        {
            int col = i % 3, rowIdx = i / 3;
            var b = MkButton(kpRoot.transform, $"Key_{keys[i]}", keys[i], new Vector2(90, 80));
            SetRect(b.gameObject, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2(-100 + col * 100, -180 - rowIdx * 90), new Vector2(90, 80));
            if (keys[i] == "C")
                UnityEventTools.AddPersistentListener(b.onClick, keypad.Clear);
            else if (keys[i] == "OK")
                UnityEventTools.AddPersistentListener(b.onClick, keypad.Submit);
            else
                UnityEventTools.AddStringPersistentListener(
                    b.onClick, new UnityAction<string>(keypad.AppendDigit), keys[i]);
        }

        // 키패드 해제 → SolvePuzzle("keypad") + 문 열림
        if (pm != null)
            UnityEventTools.AddStringPersistentListener(
                keypad.onUnlocked, new UnityAction<string>(pm.SolvePuzzle), "keypad");

        var doorGO = GameObject.Find("door/door_01") ?? GameObject.Find("door/door_02");
        if (doorGO != null)
        {
            var swing = doorGO.GetComponent<DoorSwing>() ?? doorGO.AddComponent<DoorSwing>();
            UnityEventTools.AddPersistentListener(keypad.onUnlocked, swing.Open);
        }

        // 5) HoverDetector on Main Camera
        var camGO = GameObject.Find("Main Camera");
        if (camGO != null && camGO.GetComponent<Camera>() != null)
        {
            var hd = camGO.GetComponent<HoverDetector>() ?? camGO.AddComponent<HoverDetector>();
            var soHd = new SerializedObject(hd);
            soHd.FindProperty("hoverText").objectReferenceValue = hover;
            soHd.FindProperty("rayDistance").floatValue = 30f;
            soHd.FindProperty("interactableLayer").intValue = ~0; // Everything
            soHd.ApplyModifiedProperties();
        }
        else Debug.LogError("[Room03Setup] Main Camera 를 찾지 못했거나 Camera 컴포넌트 없음");

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        string msg = "[Phase C] UI(팝업/인벤토리/키패드) + HoverDetector + 문 열림 구성 완료";
        Debug.Log(msg);
        return msg;
    }

    // Phase D: 1인칭 플레이어 리그 + 카메라 재배치 + 머리 스케일/위치 수정
    public static string Execute_PhaseD_Player()
    {
        var camGO = GameObject.Find("Main Camera");
        if (camGO == null) { Debug.LogError("Main Camera 없음"); return "FAIL"; }

        // 1) Player 리그
        var player = GameObject.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player");
        }
        player.transform.position = new Vector3(0.8f, 0f, -1.2f);
        player.transform.rotation = Quaternion.identity; // +Z(부검대/방 안쪽) 바라봄

        var cc = player.GetComponent<CharacterController>();
        if (cc == null) cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f; cc.radius = 0.3f; cc.center = new Vector3(0f, 0.9f, 0f);

        var fpc = player.GetComponent<FirstPersonController>();
        if (fpc == null) fpc = player.AddComponent<FirstPersonController>();

        // 2) 카메라를 Player 자식으로 (눈높이 1.6)
        camGO.transform.SetParent(player.transform, false);
        camGO.transform.localPosition = new Vector3(0f, 1.6f, 0f);
        camGO.transform.localRotation = Quaternion.identity;

        var soFpc = new SerializedObject(fpc);
        soFpc.FindProperty("cameraTransform").objectReferenceValue = camGO.transform;
        soFpc.ApplyModifiedProperties();

        // 3) 시체 머리 스케일/위치 수정 (트레이 scale 0.01 보정 → 자식이라 0.01배로 작아져 있음)
        var tray = GameObject.Find("props/corpse_drawer 7/corpse_drawer_table");
        var head = GameObject.Find("man_head");
        if (head == null && tray != null)
        {
            var t = tray.transform.Find("man_head");
            if (t != null) head = t.gameObject;
        }
        if (head != null && tray != null)
        {
            // 부모(트레이) 월드 스케일 0.01 → 머리를 월드 스케일 ~1로 만들기 위해 100배
            head.transform.localScale = new Vector3(100f, 100f, 100f);

            // 트레이 윗면 중앙에 올림 (트레이 자체 렌더러 기준)
            var trayRend = tray.GetComponent<Renderer>();
            if (trayRend != null)
            {
                Bounds tb = trayRend.bounds;
                head.transform.position = new Vector3(tb.center.x, tb.max.y, tb.center.z);
            }
            // 슬라브에 누운 느낌으로 정면이 위를 보도록 (gltf 변환 보정값 — 시각 확인 후 미세조정)
            head.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            // 콜라이더 재피팅
            var bc = head.GetComponent<BoxCollider>();
            if (bc != null) FitBox(head, bc);
        }
        else Debug.LogError("[Room03Setup] 머리/트레이 못 찾음");

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        string msg = "[Phase D] 1인칭 플레이어 리그 + 카메라 재배치 + 머리 보정 완료";
        Debug.Log(msg);
        return msg;
    }

    static Bounds ChildBounds(GameObject go)
    {
        var rs = go.GetComponentsInChildren<Renderer>();
        Bounds b = rs[0].bounds;
        foreach (var r in rs) b.Encapsulate(r.bounds);
        return b;
    }

    // Phase E: 머리를 트레이 위에 피벗 오프셋 보정해서 정확히 얹고 눕힘
    // 회전 미세조정이 필요하면 아래 HEAD_ROT 값을 바꿔 재실행
    static readonly Vector3 HEAD_ROT = new Vector3(180f, 0f, 0f);

    public static string Execute_PhaseE_Head()
    {
        var tray = GameObject.Find("props/corpse_drawer 7/corpse_drawer_table");
        var head = GameObject.Find("man_head");
        if (head == null && tray != null)
        {
            var t = tray.transform.Find("man_head");
            if (t != null) head = t.gameObject;
        }
        if (head == null || tray == null) { Debug.LogError("머리/트레이 없음"); return "FAIL"; }

        float rx = HEAD_ROT.x, ry = HEAD_ROT.y, rz = HEAD_ROT.z;
        head.transform.rotation = Quaternion.Euler(rx, ry, rz);

        var trayRend = tray.GetComponent<Renderer>();
        Bounds tb = trayRend.bounds;

        // x,z 중심 맞추기
        Bounds hb = ChildBounds(head);
        head.transform.position += new Vector3(tb.center.x - hb.center.x, 0f, tb.center.z - hb.center.z);
        // 트레이 윗면에 얹기
        hb = ChildBounds(head);
        head.transform.position += new Vector3(0f, tb.max.y - hb.min.y, 0f);

        var bc = head.GetComponent<BoxCollider>();
        if (bc != null) FitBox(head, bc);

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Bounds fin = ChildBounds(head);
        string msg = $"[Phase E] 머리 배치 완료. 회전=({rx},{ry},{rz}) 월드중심={fin.center} 크기={fin.size}";
        Debug.Log(msg);
        return msg;
    }

    // Phase F: 화면 중앙 조준점 + 조작 힌트 추가
    public static string Execute_PhaseF_Crosshair()
    {
        var canvasGO = GameObject.Find("Room03_UI");
        if (canvasGO == null) { Debug.LogError("Room03_UI 캔버스 없음"); return "FAIL"; }
        var root = canvasGO.transform;

        if (root.Find("Crosshair") == null)
        {
            var dot = MkImage(root, "Crosshair", new Color(1f, 1f, 1f, 0.7f));
            SetRect(dot.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(8, 8));
        }

        if (root.Find("ControlHint") == null)
        {
            var hint = MkText(root, "ControlHint",
                "WASD 이동 · 마우스 시점 · 좌클릭 조사 · Tab 커서", 24, TextAlignmentOptions.Left);
            SetRect(hint.gameObject, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(310, 40), new Vector2(600, 40));
            hint.color = new Color(1f, 1f, 1f, 0.6f);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        string msg = "[Phase F] 조준점 + 조작 힌트 추가 완료";
        Debug.Log(msg);
        return msg;
    }

    // 퍼즐 로직 진단 — 내부 상태 덤프
    public static string Execute_TestSolve()
    {
        var all = Object.FindObjectsByType<Room03PuzzleManager>(FindObjectsSortMode.None);
        if (all.Length == 0) return "FAIL: PuzzleManager 없음";
        var pm = all[0];
        var tp = typeof(Room03PuzzleManager);
        var fCleared = tp.GetField("_roomCleared", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fCount = tp.GetField("requiredPuzzleCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fSet = tp.GetField("_solvedPuzzles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // 에디트 모드 인스턴스에 남은 테스트 상태 초기화
        fCleared.SetValue(pm, false);
        ((System.Collections.Generic.HashSet<string>)fSet.GetValue(pm)).Clear();

        pm.SolvePuzzle("drawer");
        bool clearedAfter1 = (bool)fCleared.GetValue(pm);
        pm.SolvePuzzle("blackboard");
        bool clearedAfter2 = (bool)fCleared.GetValue(pm);
        pm.SolvePuzzle("keypad");
        bool clearedAfter3 = (bool)fCleared.GetValue(pm);

        var set = (System.Collections.Generic.HashSet<string>)fSet.GetValue(pm);
        // 다시 깨끗이 (저장된 씬에 영향 없도록)
        fCleared.SetValue(pm, false);
        set.Clear();

        return $"required={fCount.GetValue(pm)} | 1퍼즐후cleared={clearedAfter1} 2퍼즐후={clearedAfter2} 3퍼즐후={clearedAfter3} | 최종set수(리셋전)=3";
    }
}
#endif
