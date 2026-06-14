using TMPro;
using UnityEngine;

namespace EscapeRoom
{
    public class ClueInteractable : MonoBehaviour
    {
        [SerializeField] public ClueData clueData;
        [SerializeField] private float interactDistance = 4f;
        [SerializeField] private TextMeshProUGUI promptText;

        private const string PromptMessage = "[F] 조사하기";
        private static ClueInteractable currentTarget; // 현재 Raycast가 가리키는 단서

        private void Awake()
        {
            EnsurePromptText();
            HidePrompt();
        }

        private void Start()
        {
            RegisterDefinition();
        }

        private void OnDisable()
        {
            if (currentTarget == this) currentTarget = null;
            HidePrompt();
        }

        private void Update()
        {
            // clueData 없으면 콘솔에 경고
            if (clueData == null)
            {
                if (Input.GetKeyDown(KeyCode.F))
                    Debug.LogWarning($"[ClueInteractable] {name}: clueData가 null입니다. Inspector에서 ClueData 연결 확인!");
                return;
            }

            // ClueJournalManager 없으면 자동 생성
            if (ClueJournalManager.Instance == null)
            {
                Debug.LogWarning("[ClueInteractable] ClueJournalManager 없음 — 자동 생성");
                new GameObject("ClueJournalManager").AddComponent<ClueJournalManager>();
                RegisterDefinition();
            }

            if (ClueJournalManager.Instance.HasClue(clueData))
            {
                gameObject.SetActive(false);
                return;
            }

            // Raycast: 카메라 정면에 이 오브젝트가 있는지 확인 (문 열기와 동일 방식)
            Camera cam = Camera.main;
            bool aimed = false;
            if (cam != null)
            {
                Ray ray = new Ray(cam.transform.position, cam.transform.forward);
                if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
                    aimed = hit.collider != null && hit.collider.gameObject == gameObject;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.F))
                    Debug.LogWarning("[ClueInteractable] Camera.main이 null입니다!");
            }

            if (aimed)
            {
                currentTarget = this;
                SetPromptVisible(true);
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Debug.Log($"[ClueInteractable] F키 수집 시도: {clueData.clueName}");
                    CollectClue();
                }
            }
            else
            {
                if (currentTarget == this)
                {
                    currentTarget = null;
                    SetPromptVisible(false);
                }
            }
        }

        private void CollectClue()
        {
            if (ClueJournalManager.Instance != null && ClueJournalManager.Instance.AddClue(clueData))
            {
                Debug.Log($"[ClueInteractable] 수집 완료: {clueData.clueName}");
                HidePrompt();
                gameObject.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[ClueInteractable] AddClue 실패: {clueData?.clueName}");
            }
        }

        private void RegisterDefinition()
        {
            if (clueData != null && ClueJournalManager.Instance != null)
                ClueJournalManager.Instance.RegisterClueDefinition(clueData);
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

            GameObject canvasObject = GameObject.Find("HUD_Canvas");
            Canvas canvas;
            if (canvasObject == null)
            {
                canvasObject = new GameObject("HUD_Canvas");
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
            HorrorUITheme.ApplyText(promptText, 28f, HorrorUITheme.TextMain);
            promptText.text = PromptMessage;
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
