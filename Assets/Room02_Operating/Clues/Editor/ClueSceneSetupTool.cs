using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
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

        private const string BoxPrefabPath = "Assets/Room02_Operating/Resources/Room02_ClueBox.prefab";
        private const string CulpritPrefabPath = "Assets/Room02_Operating/Models/char_shadow.fbx";
        private const string CulpritObjectName = "Culprit_StartPosition";
        private const string ShowScenePath = "Assets/Room02_Operating/Scenes/Show.unity";
        private static readonly Vector3 CluesRootPosition = Vector3.zero;

        [MenuItem("Tools/Room02/Clues/Setup Current Stage Clues")]
        public static void SetupCurrentStageClues()
        {
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
            EditorSceneManager.MarkSceneDirty(scene);
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("단서 씬 세팅 완료", $"{scene.name} 단서 {placed}개 세팅 완료", "확인");
            }
        }

        [MenuItem("Tools/Room02/Clues/Restore All Clue Boxes")]
        public static void RestoreAllClueBoxes()
        {
            SetupCurrentStageClues();
        }

        public static void SetupShowSceneForBatch()
        {
            Scene scene = EditorSceneManager.OpenScene(ShowScenePath, OpenSceneMode.Single);
            int placed = SetupScene(scene.name);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Clues] Batch scene apply complete. Scene: {scene.path}, Placed/updated: {placed}");
        }

        public static void SetupOperatingRoomSceneForBatch()
        {
            SetupShowSceneForBatch();
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
            cluesRoot.transform.position = CluesRootPosition;
            cluesRoot.transform.rotation = Quaternion.identity;
            cluesRoot.transform.localScale = Vector3.one;
            List<ClueAssetGenerator.ClueEntry> entriesToPlace = new List<ClueAssetGenerator.ClueEntry>();
            HashSet<string> expectedNames = new HashSet<string>();
            foreach (ClueAssetGenerator.ClueEntry entry in ClueAssetGenerator.GetEntries())
            {
                if (!zones.Contains(entry.zone))
                {
                    continue;
                }

                entriesToPlace.Add(entry);
                expectedNames.Add($"Clue_{entry.clueID}");
            }

            RemoveStaleClueObjects(cluesRoot, expectedNames);

            int placed = 0;
            foreach (ClueAssetGenerator.ClueEntry entry in entriesToPlace)
            {
                CreateOrUpdateClueObject(cluesRoot, entry, placed);
                placed++;
            }

            EnsureCulpritAtStart();
            return placed;
        }

        private static void RemoveStaleClueObjects(GameObject cluesRoot, HashSet<string> expectedNames)
        {
            List<GameObject> staleObjects = new List<GameObject>();
            foreach (Transform child in cluesRoot.transform)
            {
                if (!child.name.StartsWith("Clue_") && !child.name.StartsWith("TestClue"))
                {
                    continue;
                }

                bool isExpected = expectedNames.Contains(child.name);
                bool hasBrokenData = child.GetComponent<ClueBoxInteractable>() != null
                    && child.GetComponent<ClueBoxInteractable>().clueData == null;
                if (!isExpected || hasBrokenData)
                {
                    staleObjects.Add(child.gameObject);
                }
            }

            foreach (GameObject staleObject in staleObjects)
            {
                Undo.DestroyObjectImmediate(staleObject);
            }
        }

        private static HashSet<string> GetZonesForScene(string sceneName)
        {
            switch (sceneName)
            {
                case "Show":
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
            string objectName = $"Clue_{entry.clueID}";
            Transform existing = cluesRoot.transform.Find(objectName);
            if (existing != null && existing.GetComponent<ClueBoxInteractable>() == null)
            {
                Undo.DestroyObjectImmediate(existing.gameObject);
                existing = null;
            }

            GameObject clueObject;
            bool isNewClueObject = existing == null;
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

            if (isNewClueObject)
            {
                clueObject.transform.position = CluePlacementLayout.TryGetPosition(entry.clueID, out Vector3 position)
                    ? position
                    : CluePlacementLayout.GetFallbackPosition(index);
                clueObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
            }

            clueObject.SetActive(true);

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

        private static void EnsureCulpritAtStart()
        {
            GameObject culprit = GameObject.Find(CulpritObjectName);
            if (culprit == null)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CulpritPrefabPath);
                culprit = prefab != null
                    ? (GameObject)PrefabUtility.InstantiatePrefab(prefab)
                    : GameObject.CreatePrimitive(PrimitiveType.Capsule);
                culprit.name = CulpritObjectName;
                Undo.RegisterCreatedObjectUndo(culprit, $"Create {CulpritObjectName}");
            }

            culprit.transform.position = GetCameraVisibleCulpritWorldPosition();
            Vector3 lookDirection = GetPlacementCameraPosition() - culprit.transform.position;
            lookDirection.y = 0f;
            culprit.transform.rotation = lookDirection.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(lookDirection)
                : Quaternion.Euler(0f, 180f, 0f);
            culprit.transform.localScale = Vector3.one;
            EditorUtility.SetDirty(culprit);
        }

        private static Vector3 GetCameraVisibleCulpritWorldPosition()
        {
            return GetPlacementCameraPosition()
                + (GetPlacementCameraForward() * 5.4f)
                + (GetPlacementCameraRight() * 3.0f)
                - (GetPlacementCameraUp() * GetPlacementCameraPosition().y);
        }

        private static Transform GetPlacementCameraTransform()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera.transform;
            }

            GameObject cameraObject = GameObject.Find("Main Camera");
            return cameraObject != null ? cameraObject.transform : null;
        }

        private static Vector3 GetPlacementCameraPosition()
        {
            Transform cameraTransform = GetPlacementCameraTransform();
            return cameraTransform != null ? cameraTransform.position : new Vector3(0f, 1f, -10f);
        }

        private static Vector3 GetPlacementCameraForward()
        {
            Transform cameraTransform = GetPlacementCameraTransform();
            return cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        }

        private static Vector3 GetPlacementCameraRight()
        {
            Transform cameraTransform = GetPlacementCameraTransform();
            return cameraTransform != null ? cameraTransform.right : Vector3.right;
        }

        private static Vector3 GetPlacementCameraUp()
        {
            Transform cameraTransform = GetPlacementCameraTransform();
            return cameraTransform != null ? cameraTransform.up : Vector3.up;
        }
    }
}
