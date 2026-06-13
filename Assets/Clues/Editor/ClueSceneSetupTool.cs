using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EscapeRoom.Editor
{
    public static class ClueSceneSetupTool
    {
        private static readonly HashSet<string> CorridorZones = new HashSet<string> { "Lobby", "Hallway", "Ward" };
        private static readonly HashSet<string> DressingRoomZones = new HashSet<string> { "Storage", "DressingRoom" };
        private static readonly HashSet<string> OperatingRoomZones = new HashSet<string> { "OperatingRoom" };

        [MenuItem("Tools/Clues/Setup Current Stage Clues")]
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

        [MenuItem("Tools/Clues/Setup All Stage Clues")]
        public static void SetupAllStageClues()
        {
            ClueAssetGenerator.GenerateStoryClueAssets();

            foreach (string scenePath in new[]
            {
                "Assets/Scenes/Scene_Corridor.unity",
                "Assets/Scenes/Scene_DressingRoom.unity",
                "Assets/Scenes/Scene_OperatingRoom.unity"
            })
            {
                Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                int placed = SetupScene(scene.name);
                Debug.Log($"[Clues] {scene.name} clue scene wiring complete. Placed/updated: {placed}");
                EditorSceneManager.SaveScene(scene);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayDialog("단서 전체 세팅 완료", "3개 스테이지 씬 단서 세팅 완료", "확인");
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

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            return placed;
        }

        private static HashSet<string> GetZonesForScene(string sceneName)
        {
            switch (sceneName)
            {
                case "Scene_Corridor":
                    return CorridorZones;
                case "Scene_DressingRoom":
                    return DressingRoomZones;
                case "Scene_OperatingRoom":
                    return OperatingRoomZones;
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
                clueObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                clueObject.name = objectName;
                clueObject.transform.SetParent(cluesRoot.transform, false);
                Undo.RegisterCreatedObjectUndo(clueObject, $"Create {objectName}");
            }
            else
            {
                clueObject = existing.gameObject;
            }

            clueObject.transform.localPosition = GetLocalPosition(entry.zone, index);
            clueObject.transform.localScale = new Vector3(0.28f, 0.08f, 0.2f);

            BoxCollider collider = clueObject.GetComponent<BoxCollider>();
            if (collider == null)
            {
                collider = clueObject.AddComponent<BoxCollider>();
            }

            collider.isTrigger = false;

            MeshRenderer renderer = clueObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.enabled = true;
            }

            ClueInteractable interactable = clueObject.GetComponent<ClueInteractable>();
            if (interactable == null)
            {
                interactable = clueObject.AddComponent<ClueInteractable>();
            }

            SerializedObject serializedObject = new SerializedObject(interactable);
            serializedObject.FindProperty("clueData").objectReferenceValue = LoadClueAsset(entry);
            serializedObject.ApplyModifiedProperties();

            EditorUtility.SetDirty(clueObject);
        }

        private static ClueData LoadClueAsset(ClueAssetGenerator.ClueEntry entry)
        {
            string folder = entry.category == ClueCategory.KeyClue ? "Assets/Clues/KeyClue" : "Assets/Clues/Normal";
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
