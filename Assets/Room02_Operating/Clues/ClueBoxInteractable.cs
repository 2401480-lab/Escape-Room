using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class ClueBoxInteractable : MonoBehaviour
    {
        [SerializeField] public ClueData clueData;
        [SerializeField] private float interactDistance = 2.2f;
        [SerializeField] private float promptDistance = 2.2f;
        [SerializeField] private float inputBufferSeconds = 0.2f;
        [SerializeField] private TextMeshProUGUI promptText;
        [SerializeField] private Renderer[] highlightRenderers;
        [SerializeField] private Color searchedTint = new Color(0.25f, 0.22f, 0.2f, 1f);

        private const string SearchPrompt = "[F] 박스 조사하기";
        private const string SearchedPrompt = "이미 조사한 박스";

        private static ClueBoxInteractable currentTarget;
        private static readonly Collider[] nearbyColliders = new Collider[64];
        private static float lastInteractPressedAt = -999f;

        private bool isSearched;

        private void Awake()
        {
            EnsurePromptText();
            HidePrompt();
        }

        private void Start()
        {
            RegisterDefinition();
            if (ClueJournalManager.Instance != null && ClueJournalManager.Instance.HasClue(clueData))
            {
                MarkSearchedVisual();
            }
        }

        private void OnDisable()
        {
            if (currentTarget == this)
            {
                currentTarget = null;
            }

            HidePrompt();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                lastInteractPressedAt = Time.unscaledTime;
            }

            if (clueData == null)
            {
                if (Input.GetKeyDown(KeyCode.F))
                {
                    Debug.LogWarning($"[ClueBoxInteractable] {name}: clueData가 비어 있습니다.");
                }

                return;
            }

            EnsureManager();
            ClueBoxInteractable bestTarget = FindBestTarget();
            if (bestTarget != null)
            {
                currentTarget = bestTarget;
            }

            if (currentTarget != this)
            {
                HidePrompt();
                return;
            }

            SetPrompt(isSearched ? SearchedPrompt : SearchPrompt, true);

            if (!isSearched && HasBufferedInteractInput())
            {
                lastInteractPressedAt = -999f;
                SearchBox();
            }
        }

        private void SearchBox()
        {
            if (ClueJournalManager.Instance == null)
            {
                return;
            }

            bool added = ClueJournalManager.Instance.AddClue(clueData);
            if (added || ClueJournalManager.Instance.HasClue(clueData))
            {
                isSearched = true;
                MarkSearchedVisual();
                SetPrompt(SearchedPrompt, true);
            }
        }

        private bool HasBufferedInteractInput()
        {
            return Time.unscaledTime - lastInteractPressedAt <= inputBufferSeconds;
        }

        private ClueBoxInteractable FindBestTarget()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                return null;
            }

            int hitCount = Physics.OverlapSphereNonAlloc(cam.transform.position, interactDistance, nearbyColliders);
            ClueBoxInteractable best = null;
            float bestScore = float.MinValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = nearbyColliders[i];
                nearbyColliders[i] = null;
                if (hit == null)
                {
                    continue;
                }

                ClueBoxInteractable candidate = hit.GetComponentInParent<ClueBoxInteractable>();
                if (candidate == null || candidate.clueData == null)
                {
                    continue;
                }

                Vector3 toCandidate = candidate.transform.position - cam.transform.position;
                float distance = toCandidate.magnitude;
                if (distance > promptDistance)
                {
                    continue;
                }

                float directionScore = Vector3.Dot(cam.transform.forward, toCandidate.normalized);
                float distanceScore = 1f - Mathf.Clamp01(distance / promptDistance);
                float score = distanceScore * 1.4f + Mathf.Max(0f, directionScore);
                if (score > bestScore)
                {
                    best = candidate;
                    bestScore = score;
                }
            }

            return best;
        }

        private void EnsureManager()
        {
            if (ClueJournalManager.Instance != null)
            {
                RegisterDefinition();
                return;
            }

            new GameObject("ClueJournalManager").AddComponent<ClueJournalManager>();
            RegisterDefinition();
        }

        private void RegisterDefinition()
        {
            if (clueData != null && ClueJournalManager.Instance != null)
            {
                ClueJournalManager.Instance.RegisterClueDefinition(clueData);
            }
        }

        private void MarkSearchedVisual()
        {
            isSearched = true;
            if (highlightRenderers == null || highlightRenderers.Length == 0)
            {
                highlightRenderers = GetComponentsInChildren<Renderer>();
            }

            foreach (Renderer boxRenderer in highlightRenderers)
            {
                if (boxRenderer != null && boxRenderer.material != null)
                {
                    boxRenderer.material.color = searchedTint;
                }
            }
        }

        private void EnsurePromptText()
        {
            if (promptText != null)
            {
                return;
            }

            GameObject existing = GameObject.Find("ClueInteractionPrompt");
            if (existing != null)
            {
                promptText = existing.GetComponent<TextMeshProUGUI>();
                if (promptText != null)
                {
                    return;
                }
            }

            Canvas canvas = EnsureHudCanvas();
            GameObject textObject = new GameObject("ClueInteractionPrompt");
            textObject.transform.SetParent(canvas.transform, false);
            promptText = textObject.AddComponent<TextMeshProUGUI>();
            HorrorUITheme.ApplyText(promptText, 28f, HorrorUITheme.TextMain);
            promptText.alignment = TextAlignmentOptions.Center;

            RectTransform rect = promptText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 80f);
            rect.sizeDelta = new Vector2(560f, 64f);
        }

        private static Canvas EnsureHudCanvas()
        {
            GameObject canvasObject = GameObject.Find("HUD_Canvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("HUD_Canvas");
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = canvasObject.AddComponent<Canvas>();
            }

            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            if (canvasObject.GetComponent<CanvasScaler>() == null)
            {
                canvasObject.AddComponent<CanvasScaler>();
            }

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private void SetPrompt(string message, bool visible)
        {
            if (promptText == null)
            {
                return;
            }

            promptText.text = message;
            promptText.gameObject.SetActive(visible);
        }

        private void HidePrompt()
        {
            SetPrompt(SearchPrompt, false);
        }
    }
}
