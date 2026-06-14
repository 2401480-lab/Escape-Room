using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom.Editor
{
    public static class ClueSceneSetupTool
    {
        private static readonly HashSet<string> IntegratedZones = new HashSet<string>
        {
            "Lobby",
            "Hallway",
            "Ward",
            "Storage",
            "DressingRoom",
            "OperatingRoom"
        };

        private const string BoxPrefabPath = "Assets/Abandoned_Asylum/Prefabs/Box_V1.prefab";

        [MenuItem("Tools/Room02/Clues/Place Single Test Clue Box")]
        public static void PlaceSingleTestClueBox()
        {
            // Clues 루트 아래 기존 큐브 전부 제거
            GameObject cluesRoot = GameObject.Find("Clues");
            if (cluesRoot != null)
            {
                var children = new System.Collections.Generic.List<GameObject>();
                foreach (Transform t in cluesRoot.transform)
                    children.Add(t.gameObject);
                foreach (var c in children)
                    Undo.DestroyObjectImmediate(c);
            }
            else
            {
                cluesRoot = new GameObject("Clues");
                Undo.RegisterCreatedObjectUndo(cluesRoot, "Create Clues Root");
            }

            // 런타임 매니저 확보
            EnsureRuntimeObject<ClueJournalManager>("ClueJournalManager");
            EnsureRuntimeObject<CluePickupPopupUI>("CluePickupPopupUI");
            EnsureRuntimeObject<TimerUI>("TimerUI");
            EnsureRuntimeObject<SettingsUI>("SettingsUI");

            // PlayerStart 앞 2m 위치 계산
            GameObject ps = GameObject.Find("PlayerStart");
            Vector3 spawnPos = ps != null
                ? ps.transform.position + ps.transform.forward * 2f + Vector3.up * 0.15f
                : new Vector3(0f, 0.15f, 2f);

            GameObject box = CreateBoxObject("TestClueBox_cast_notice", cluesRoot.transform);
            box.transform.position = spawnPos;
            box.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            Undo.RegisterCreatedObjectUndo(box, "Create TestClueBox");

            // ClueInteractable + ClueData 연결 (첫 번째 에셋 자동 사용)
            ClueAssetGenerator.GenerateStoryClueAssets();
            AssetDatabase.Refresh();

            ClueBoxInteractable interactable = box.GetComponent<ClueBoxInteractable>();
            if (interactable == null)
            {
                interactable = box.AddComponent<ClueBoxInteractable>();
            }

            // GetEntries()로 첫 번째 일반 단서 에셋 경로를 정확히 가져옴
            ClueData asset = null;
            foreach (ClueAssetGenerator.ClueEntry e in ClueAssetGenerator.GetEntries())
            {
                if (e.category != ClueCategory.KeyClue)
                {
                    asset = AssetDatabase.LoadAssetAtPath<ClueData>($"Assets/Room02_Operating/Clues/Normal/{e.fileName}.asset");
                    if (asset != null) break;
                }
            }

            if (asset != null)
            {
                var so = new SerializedObject(interactable);
                so.FindProperty("clueData").objectReferenceValue = asset;
                so.ApplyModifiedProperties();
                Debug.Log($"[Clues] 테스트 박스에 ClueData 연결: {asset.clueName}");
            }
            else
            {
                Debug.LogError("[Clues] ClueData 에셋을 찾지 못했습니다. Tools > Room02 > Clues > Generate Story Clue Assets 먼저 실행하세요.");
            }

            EditorUtility.SetDirty(box);

            EditorUtility.DisplayDialog("완료",
                $"테스트 단서 박스 1개 배치 완료!\n\n위치: {spawnPos}\n단서: cast_notice\n\nCtrl+S 저장 후 플레이해서\nF키로 박스 조사 테스트!", "확인");
        }

        [MenuItem("Tools/Room02/Clues/Place Single Test Clue")]
        public static void PlaceSingleTestClue()
        {
            PlaceSingleTestClueBox();
        }

        [MenuItem("Tools/Room02/Clues/Setup Current Stage Clues")]
        public static void SetupCurrentStageClues()
        {
            ClueAssetGenerator.GenerateStoryClueAssets();

            Scene scene = SceneManager.GetActiveScene();
            int placed = SetupScene(scene.name);
            if (placed < 0)
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("단서 세팅 불가", $"지원하지 않는 씬입니다: {scene.name}", "확인");
                }

                return;
            }

            Debug.Log($"[Clues] {scene.name} clue scene wiring complete. Placed/updated: {placed}");
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("단서 씬 세팅 완료", $"{scene.name} 단서 {placed}개 세팅 완료", "확인");
            }
        }

        private static int SetupScene(string sceneName)
        {
            HashSet<string> zones = GetZonesForScene(sceneName);
            if (zones == null)
            {
                return -1;
            }

            EnsureRuntimeObject<ClueJournalManager>("ClueJournalManager");
            EnsureRuntimeObject<ClueJournalUI>("ClueJournalUI");
            EnsureRuntimeObject<CluePickupPopupUI>("CluePickupPopupUI");
            EnsureRuntimeObject<TimerUI>("TimerUI");
            EnsureRuntimeObject<SettingsUI>("SettingsUI");

            GameObject cluesRoot = EnsureRoot("Clues");
            int placed = 0;
            foreach (ClueAssetGenerator.ClueEntry entry in ClueAssetGenerator.GetEntries())
            {
                if (!zones.Contains(entry.zone))
                {
                    continue;
                }

                CreateOrUpdateClueObject(cluesRoot, entry, placed);
                placed++;
            }

            return placed;
        }

        private static HashSet<string> GetZonesForScene(string sceneName)
        {
            switch (sceneName)
            {
                case "Scene_OperatingRoom":
                    return IntegratedZones;
                default:
                    return null;
            }
        }

        private static GameObject EnsureRoot(string name)
        {
            GameObject root = GameObject.Find(name);
            if (root != null)
            {
                return root;
            }

            root = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(root, $"Create {name}");
            EditorUtility.SetDirty(root);
            return root;
        }

        private static void EnsureRuntimeObject<T>(string objectName) where T : Component
        {
            T existing = Object.FindObjectOfType<T>();
            if (existing != null)
            {
                return;
            }

            GameObject go = new GameObject(objectName);
            Undo.RegisterCreatedObjectUndo(go, $"Create {objectName}");
            go.AddComponent<T>();
            EditorUtility.SetDirty(go);
        }

        private static void CreateOrUpdateClueObject(GameObject cluesRoot, ClueAssetGenerator.ClueEntry entry, int index)
        {
            string objectName = $"Clue_{entry.fileName}";
            Transform existing = cluesRoot.transform.Find(objectName);
            GameObject clueObject;
            if (existing == null)
            {
                clueObject = CreateBoxObject(objectName, cluesRoot.transform);
                clueObject.name = objectName;
                Undo.RegisterCreatedObjectUndo(clueObject, $"Create {objectName}");
            }
            else
            {
                clueObject = existing.gameObject;
            }

            clueObject.transform.localPosition = GetLocalPosition(entry.zone, index);
            clueObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);

            BoxCollider collider = clueObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = clueObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = false;

            foreach (Renderer renderer in clueObject.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }

            ClueInteractable oldInteractable = clueObject.GetComponent<ClueInteractable>();
            if (oldInteractable != null)
            {
                Undo.DestroyObjectImmediate(oldInteractable);
            }

            ClueBoxInteractable interactable = clueObject.GetComponent<ClueBoxInteractable>();
            if (interactable == null)
            {
                interactable = clueObject.AddComponent<ClueBoxInteractable>();
            }

            SerializedObject serializedObject = new SerializedObject(interactable);
            serializedObject.FindProperty("clueData").objectReferenceValue = LoadClueAsset(entry);
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(clueObject);
        }

        private static GameObject CreateBoxObject(string objectName, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(BoxPrefabPath);
            GameObject box;
            if (prefab != null)
            {
                box = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                box = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Debug.LogWarning($"[Clues] 박스 프리팹을 찾지 못해 임시 큐브를 사용합니다: {BoxPrefabPath}");
            }

            box.name = objectName;
            box.transform.SetParent(parent, false);
            if (box.GetComponentInChildren<Collider>() == null)
            {
                box.AddComponent<BoxCollider>();
            }

            return box;
        }

        private static ClueData LoadClueAsset(ClueAssetGenerator.ClueEntry entry)
        {
            string folder = entry.category == ClueCategory.KeyClue ? "Assets/Room02_Operating/Clues/KeyClue" : "Assets/Room02_Operating/Clues/Normal";
            return AssetDatabase.LoadAssetAtPath<ClueData>($"{folder}/{entry.fileName}.asset");
        }

        private static Vector3 GetLocalPosition(string zone, int index)
        {
            float row = index % 8;
            switch (zone)
            {
                case "Lobby":
                    return new Vector3(-3f + row * 0.75f, 1.1f, -1.5f);
                case "Hallway":
                    return new Vector3(-3f + row * 0.75f, 1.1f, 1.5f);
                case "Ward":
                    return new Vector3(-3f + row * 0.75f, 1.1f, 4.5f);
                case "Storage":
                    return new Vector3(-2.5f + row * 0.75f, 1.1f, -1f);
                case "DressingRoom":
                    return new Vector3(-2f + row * 0.75f, 1.1f, 2f);
                case "OperatingRoom":
                    return new Vector3(-1.5f + row * 0.75f, 1.1f, 0.5f);
                default:
                    return new Vector3(row * 0.75f, 1.1f, 0f);
            }
        }
    }
}
