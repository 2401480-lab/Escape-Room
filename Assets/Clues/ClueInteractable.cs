using TMPro;
using UnityEngine;

namespace EscapeRoom
{
    public class ClueInteractable : MonoBehaviour
    {
        [SerializeField] public ClueData clueData;
        [SerializeField] private float collectDistance = 2f;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Transform player;

        private const string PromptMessage = "[E] 증거 수집";

        private void Awake()
        {
            EnsurePromptText();
            HidePrompt();
        }

        private void OnDisable()
        {
            HidePrompt();
        }

        private void Update()
        {
            if (clueData == null || ClueJournalManager.Instance == null)
            {
                HidePrompt();
                return;
            }

            if (ClueJournalManager.Instance.HasClue(clueData))
            {
                gameObject.SetActive(false);
                return;
            }

            Transform target = GetPlayerTransform();
            bool inRange = target != null && Vector3.Distance(target.position, transform.position) <= collectDistance;
            SetPromptVisible(inRange);

            if (inRange && Input.GetKeyDown(KeyCode.E))
            {
                CollectClue();
            }
        }

        private void CollectClue()
        {
            if (ClueJournalManager.Instance != null && ClueJournalManager.Instance.AddClue(clueData))
            {
                HidePrompt();
                gameObject.SetActive(false);
            }
        }

        private Transform GetPlayerTransform()
        {
            if (player != null)
            {
                return player;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                player = mainCamera.transform;
                return player;
            }

            GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
            if (taggedPlayer != null)
            {
                player = taggedPlayer.transform;
            }

            return player;
        }

        private void EnsurePromptText()
        {
            if (promptText != null)
            {
                promptText.text = PromptMessage;
                return;
            }

            GameObject existing = GameObject.Find("ClueInteractionPrompt");
            if (existing != null)
            {
                promptText = existing.GetComponent<TextMeshProUGUI>();
                if (promptText != null)
                {
                    promptText.text = PromptMessage;
                    return;
                }
            }

            GameObject canvasObject = GameObject.Find("CluePromptCanvas");
            Canvas canvas;
            if (canvasObject == null)
            {
                canvasObject = new GameObject("CluePromptCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
            else
            {
                canvas = canvasObject.GetComponent<Canvas>();
                if (canvas == null)
                {
                    canvas = canvasObject.AddComponent<Canvas>();
                }

                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            GameObject textObject = new GameObject("ClueInteractionPrompt");
            textObject.transform.SetParent(canvas.transform, false);
            promptText = textObject.AddComponent<TextMeshProUGUI>();
            promptText.text = PromptMessage;
            promptText.fontSize = 28f;
            promptText.alignment = TextAlignmentOptions.Center;

            RectTransform rect = promptText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(500f, 60f);
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptText != null && promptText.gameObject.activeSelf != visible)
            {
                promptText.gameObject.SetActive(visible);
            }
        }

        private void HidePrompt()
        {
            SetPromptVisible(false);
        }
    }
}
