using System.Reflection;
using UnityEngine;

namespace EscapeRoom
{
    public class ClueBoxRuntimeAdapter : MonoBehaviour
    {
        [SerializeField] private string cluesRootName = "Clues";
        [SerializeField] private Vector3 boxScale = new Vector3(0.6f, 0.6f, 0.6f);
        [SerializeField] private Vector3 boxOffset = new Vector3(0f, 0.15f, 0f);

        private void Start()
        {
            ConvertExistingCluesToBoxes();
        }

        private void ConvertExistingCluesToBoxes()
        {
            if (FindObjectOfType<ClueBoxInteractable>() != null)
            {
                return;
            }

            GameObject cluesRoot = GameObject.Find(cluesRootName);
            if (cluesRoot == null)
            {
                return;
            }

            GameObject boxPrefab = Resources.Load<GameObject>("Room02_ClueBox");
            if (boxPrefab == null)
            {
                Debug.LogWarning("[ClueBoxRuntimeAdapter] Resources/Room02_ClueBox 프리팹을 찾지 못했습니다.");
                return;
            }

            int childCount = cluesRoot.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform oldClue = cluesRoot.transform.GetChild(i);
                if (oldClue == null || !oldClue.gameObject.activeSelf)
                {
                    continue;
                }

                ClueData clueData = ExtractClueData(oldClue.gameObject);
                if (clueData == null)
                {
                    continue;
                }

                GameObject box = Instantiate(boxPrefab, oldClue.position + boxOffset, oldClue.rotation, cluesRoot.transform);
                box.name = $"Box_{oldClue.name}";
                box.transform.localScale = boxScale;

                ClueBoxInteractable interactable = box.GetComponent<ClueBoxInteractable>();
                if (interactable == null)
                {
                    interactable = box.AddComponent<ClueBoxInteractable>();
                }

                interactable.clueData = clueData;
                oldClue.gameObject.SetActive(false);
            }
        }

        private static ClueData ExtractClueData(GameObject source)
        {
            foreach (MonoBehaviour behaviour in source.GetComponents<MonoBehaviour>())
            {
                if (behaviour == null)
                {
                    continue;
                }

                FieldInfo field = behaviour.GetType().GetField(
                    "clueData",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null && field.GetValue(behaviour) is ClueData clueData)
                {
                    return clueData;
                }
            }

            return null;
        }
    }
}
