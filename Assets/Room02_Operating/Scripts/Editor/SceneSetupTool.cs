using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Room02Operating
{
    public static class SceneSetupTool
    {
        private const string CluePath = "Assets/Room02_Operating/ScriptableObjects";

        [MenuItem("Tools/Room02/Setup Corridor Scene")]
        public static void SetupCorridor()
        {
            SetupManagers();
            SetupUI();
            SetupLobbyClues();
            SetupCorridorWaitingClues();
            SetupWardClues();
            Debug.Log("[Room02] 복도/입구/병실 구역 단서 세팅 완료.");
            EditorUtility.DisplayDialog("완료", "복도 씬에 입구 로비, 대기실, 병실 구역 단서 세팅 완료!", "확인");
        }

        [MenuItem("Tools/Room02/Setup DressingRoom Scene")]
        public static void SetupDressingRoom()
        {
            SetupManagers();
            SetupUI();
            SetupStorageClues();
            SetupMakeupClues();
            Debug.Log("[Room02] 보관실/분장실 구역 단서 세팅 완료.");
            EditorUtility.DisplayDialog("완료", "분장실 씬에 보관실, 분장실 구역 단서 세팅 완료!", "확인");
        }

        [MenuItem("Tools/Room02/Setup OperatingRoom Scene")]
        public static void SetupOperatingRoom()
        {
            SetupManagers();
            SetupUI();
            SetupOperatingZoneClues();
            Debug.Log("[Room02] 수술실 구역 단서 세팅 완료.");
            EditorUtility.DisplayDialog("완료", "수술실 씬에 수술 전실, 수술실 본실, 관찰실 단서 세팅 완료!", "확인");
        }

        private static void SetupManagers()
        {
            EnsureGameObject("RoomGameManager", go =>
            {
                if (go.GetComponent<RoomGameManager>() == null)
                {
                    go.AddComponent<RoomGameManager>();
                }
            });
        }

        private static void SetupUI()
        {
            GameObject canvas = EnsureGameObject("UI_Canvas", go =>
            {
                if (go.GetComponent<Canvas>() == null)
                {
                    Canvas c = go.AddComponent<Canvas>();
                    c.renderMode = RenderMode.ScreenSpaceOverlay;
                    go.AddComponent<CanvasScaler>();
                    go.AddComponent<GraphicRaycaster>();
                }
            });

            EnsureChild(canvas, "HoverText", go =>
            {
                if (go.GetComponent<TextMeshProUGUI>() == null)
                {
                    TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
                    tmp.text = "";
                    tmp.fontSize = 24;
                    tmp.alignment = TextAlignmentOptions.Center;

                    RectTransform rect = go.GetComponent<RectTransform>();
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.sizeDelta = new Vector2(400, 50);
                    rect.anchoredPosition = new Vector2(0, -80);
                }

                go.SetActive(false);
            });

            EnsureChild(canvas, "CluePanel", go =>
            {
                if (go.GetComponent<CluePanel>() == null)
                {
                    go.AddComponent<CluePanel>();
                }

                go.SetActive(false);
            });

            EnsureChild(canvas, "DeductionPopup", go =>
            {
                if (go.GetComponent<DeductionPopup>() == null)
                {
                    go.AddComponent<DeductionPopup>();
                }

                go.SetActive(false);
            });

            EnsureChild(canvas, "NotebookUI", go =>
            {
                if (go.GetComponent<NotebookUI>() == null)
                {
                    go.AddComponent<NotebookUI>();
                }

                go.SetActive(false);
            });

            EnsureChild(canvas, "SuspectSelection", go =>
            {
                if (go.GetComponent<SuspectSelection>() == null)
                {
                    go.AddComponent<SuspectSelection>();
                }

                go.SetActive(false);
            });

            EnsureChild(canvas, "InventoryBar", go =>
            {
                if (go.GetComponent<InventoryManager>() == null)
                {
                    go.AddComponent<InventoryManager>();
                }
            });

            Camera cam = Camera.main;
            if (cam != null && cam.GetComponent<HoverDetector>() == null)
            {
                cam.gameObject.AddComponent<HoverDetector>();
                Debug.Log("[Room02] HoverDetector를 Main Camera에 부착했습니다. Inspector에서 HoverText 연결 필요.");
            }
        }

        private static void SetupLobbyClues()
        {
            GameObject root = EnsureClueGroup("01_Lobby_Reception");
            CreateClueObject(root, "Clue_Cast_Notice", "clue_cast_notice", new Vector3(-4f, 1.2f, 2f));
            CreateClueObject(root, "Clue_Memorial_Frame", "clue_memorial_frame", new Vector3(-2.5f, 1.4f, 2f));
            CreateClueObject(root, "Clue_Visitor_Log", "clue_visitor_log", new Vector3(-1f, 1f, 1.5f));
            CreateClueObject(root, "Clue_Security_Log", "clue_security_log", new Vector3(0.5f, 1f, 1.5f));
        }

        private static void SetupCorridorWaitingClues()
        {
            GameObject root = EnsureClueGroup("02_Corridor_Waiting_Nurse");
            CreateClueObject(root, "Clue_Torn_Letter_Piece_A", "clue_torn_letter_piece_a", new Vector3(-3f, 1f, 5f));
            CreateClueObject(root, "Clue_Torn_Letter_Piece_B", "clue_torn_letter_piece_b", new Vector3(3f, 1f, 5f));
            CreateClueObject(root, "Clue_Yoanna_Note", "clue_yoanna_note", new Vector3(-1.5f, 1f, 6.5f));
            CreateClueObject(root, "Clue_Nurse_Log", "clue_nurse_log", new Vector3(0f, 1f, 6.5f));
            CreateClueObject(root, "Clue_CCTV_Memo", "clue_cctv_memo", new Vector3(1.5f, 1.2f, 6.5f));
            CreateClueObject(root, "Clue_Phone_Memo", "clue_phone_memo", new Vector3(3f, 1f, 6.5f));
        }

        private static void SetupWardClues()
        {
            GameObject root = EnsureClueGroup("03_Ward_Isolation_Bathroom");
            CreateClueObject(root, "Clue_Hasho_Will", "clue_hasho_will", new Vector3(-4f, 1f, 10f));
            CreateClueObject(root, "Clue_Medical_Certificate", "clue_medical_certificate", new Vector3(-2.5f, 1f, 10f));
            CreateClueObject(root, "Clue_Conversation_Memo_A", "clue_conversation_memo_a", new Vector3(-1f, 1f, 10f));
            CreateClueObject(root, "Clue_Isolation_Bloodstain", "clue_isolation_bloodstain", new Vector3(0.5f, 0.05f, 10f));
            CreateClueObject(root, "Clue_Sumi_Memo", "clue_sumi_memo", new Vector3(2f, 1f, 10f));
            CreateClueObject(root, "Clue_Bong_Rebuttal", "clue_bong_rebuttal", new Vector3(3.5f, 1f, 10f));
            CreateClueObject(root, "Clue_Ward_Calendar", "clue_ward_calendar", new Vector3(5f, 1.4f, 10f));
        }

        private static void SetupStorageClues()
        {
            GameObject root = EnsureClueGroup("04_Storage_Device_Locker");
            CreateClueObject(root, "Clue_Poison_Ampoule", "clue_poison_ampoule", new Vector3(-3f, 1f, 2f));
            CreateClueObject(root, "Clue_Hidden_Camera", "clue_hidden_camera", new Vector3(-1.5f, 1f, 2f));
            CreateClueObject(root, "Clue_Jin_Sneakers", "clue_jin_sneakers", new Vector3(0f, 0.2f, 2f));
            CreateClueObject(root, "Clue_Gloves", "clue_gloves", new Vector3(1.5f, 1f, 2f));
            CreateClueObject(root, "Clue_Locked_Locker", "clue_locked_locker", new Vector3(3f, 1f, 2f));
        }

        private static void SetupMakeupClues()
        {
            GameObject root = EnsureClueGroup("05_Makeup_PropRoom");
            CreateClueObject(root, "Clue_Paint_Footprints", "clue_paint_footprints", new Vector3(-2f, 0.05f, 5f));
            CreateClueObject(root, "Clue_Makeup_Diary", "clue_makeup_diary", new Vector3(-0.5f, 1f, 5f));
            CreateClueObject(root, "Clue_Mirror_Message", "clue_mirror_message", new Vector3(1f, 1.4f, 5f));
            CreateClueObject(root, "Clue_Paint_Toolbox", "clue_paint_toolbox", new Vector3(2.5f, 0.5f, 5f));
        }

        private static void SetupOperatingZoneClues()
        {
            GameObject root = EnsureClueGroup("06_PreOp_Operating_Observation");
            CreateClueObject(root, "Clue_Under_Table_Space", "clue_under_table_space", new Vector3(0f, 0.05f, 0f));
            CreateClueObject(root, "Clue_Yoanna_Relic", "clue_yoanna_relic", new Vector3(1f, 1f, 0.5f));
            CreateFinalTrigger(root, "EndingUI_Trigger", new Vector3(0f, 0.5f, 2f));
        }

        private static GameObject EnsureClueGroup(string groupName)
        {
            GameObject cluesRoot = GameObject.Find("Clues");
            if (cluesRoot == null)
            {
                cluesRoot = new GameObject("Clues");
                Undo.RegisterCreatedObjectUndo(cluesRoot, "Create Clues Root");
            }

            return EnsureChild(cluesRoot, groupName, _ => { });
        }

        private static void CreateClueObject(GameObject parent, string goName, string clueID, Vector3 localPos)
        {
            GameObject existing = GameObject.Find(goName);
            if (existing != null)
            {
                return;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = goName;
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = localPos;
            go.transform.localScale = Vector3.one * 0.2f;

            InteractableObject interactable = go.AddComponent<InteractableObject>();
            ClueData clueData = AssetDatabase.LoadAssetAtPath<ClueData>($"{CluePath}/{clueID}.asset");
            if (clueData != null)
            {
                SerializedObject so = new SerializedObject(interactable);
                so.FindProperty("clueData").objectReferenceValue = clueData;
                so.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning($"[Room02] ClueData 에셋 없음: {clueID}. 먼저 Generate All Clue Assets 실행 필요.");
            }

            int layer = LayerMask.NameToLayer("Interactable");
            if (layer >= 0)
            {
                go.layer = layer;
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
            EditorUtility.SetDirty(go);
        }

        private static void CreateFinalTrigger(GameObject parent, string goName, Vector3 localPos)
        {
            GameObject existing = GameObject.Find(goName);
            if (existing != null)
            {
                return;
            }

            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = goName;
            go.transform.SetParent(parent.transform);
            go.transform.localPosition = localPos;
            go.transform.localScale = new Vector3(1.5f, 1f, 1.5f);
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }

            Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
            EditorUtility.SetDirty(go);
        }

        private static GameObject EnsureGameObject(string name, System.Action<GameObject> setup)
        {
            GameObject go = GameObject.Find(name);
            if (go == null)
            {
                go = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            }

            setup(go);
            EditorUtility.SetDirty(go);
            return go;
        }

        private static GameObject EnsureChild(GameObject parent, string name, System.Action<GameObject> setup)
        {
            Transform t = parent.transform.Find(name);
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
