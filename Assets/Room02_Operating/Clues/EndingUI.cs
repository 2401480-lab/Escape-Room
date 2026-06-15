using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class EndingUI : MonoBehaviour
    {
        [SerializeField] private Canvas endingCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private ChaseController chaseController;
        [SerializeField] private SilhouetteController silhouetteController;
        [SerializeField] private GameOverUI gameOverUI;
        [SerializeField] private SuspectConfirmUI suspectConfirmUI;
        [SerializeField] private Button culpritChaseButton;

        public UnityEvent OnCorrectSuspectSelected = new UnityEvent();
        public UnityEvent OnWrongSuspectSelected = new UnityEvent();
        public UnityEvent OnBlackoutRequested = new UnityEvent();
        public UnityEvent OnJinMaterialized = new UnityEvent();

        private SuspectChoice pendingSuspect;
        private bool wrongAnswerUsed;
        private StoryProgressManager observedStoryManager;

        private void Awake()
        {
            EnsureUI();
            Hide();
        }

        private void OnEnable()
        {
            SubscribeToStoryProgress();
        }

        private void Start()
        {
            SubscribeToStoryProgress();
            RefreshCulpritChaseButton();
        }

        private void Update()
        {
            HandleSuspectNumberShortcuts();

            if (Input.GetKeyDown(KeyCode.G))
            {
                TryShowCulpritSelectionShortcut();
            }

            if (observedStoryManager != StoryProgressManager.Instance)
            {
                RefreshCulpritChaseButton();
            }
        }

        private void OnDisable()
        {
            if (observedStoryManager != null)
            {
                observedStoryManager.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
                observedStoryManager = null;
            }
        }

        public void Show()
        {
            EnsureUI();
            SetCulpritChaseButtonVisible(false);
            panelRoot.SetActive(true);
            StoryProgressManager.Instance?.BeginSuspectSelection();
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            RefreshCulpritChaseButton();
        }

        public void ChooseJinSewoong()
        {
            SelectSuspect(SuspectChoice.JinSewoong, "진세웅");
        }

        public void ChooseBongTaehyeon()
        {
            SelectSuspect(SuspectChoice.BongTaehyeon, "봉태현");
        }

        public void ChooseMoonSumi()
        {
            SelectSuspect(SuspectChoice.MoonSumi, "문수미");
        }

        public void ChooseOhSejin()
        {
            SelectSuspect(SuspectChoice.OhSejin, "오세진");
        }

        public void ConfirmSuspect(SuspectChoice suspect)
        {
            pendingSuspect = suspect;
            if (pendingSuspect == SuspectChoice.JinSewoong)
            {
                CorrectAnswer();
            }
            else
            {
                WrongAnswer();
            }
        }

        private void SelectSuspect(SuspectChoice suspect, string suspectName)
        {
            pendingSuspect = suspect;
            SuspectConfirmUI targetConfirmUI = GetSuspectConfirmUI();
            if (targetConfirmUI != null)
            {
                targetConfirmUI.Show(this, pendingSuspect, suspectName);
            }
            else
            {
                ConfirmSuspect(pendingSuspect);
            }
        }

        private void CorrectAnswer()
        {
            Hide();
            OnCorrectSuspectSelected?.Invoke();
            OnBlackoutRequested?.Invoke();
            silhouetteController?.MaterializeAsJin();
            OnJinMaterialized?.Invoke();
            StoryProgressManager storyManager = StoryProgressManager.Instance;
            storyManager?.GrantEscapeKeyFromCorrectSuspect();
            storyManager?.BeginChase();
            StartChase();
            EscapeChaseQTE.StartOrCreate();
        }

        private void WrongAnswer()
        {
            if (wrongAnswerUsed)
            {
                return;
            }

            wrongAnswerUsed = true;
            Hide();
            OnWrongSuspectSelected?.Invoke();
            silhouetteController?.PlayJumpscare();
            GameOverUI targetGameOver = gameOverUI != null ? gameOverUI : GameOverUI.Instance;
            targetGameOver?.PlayGameOver(GameOverReason.WrongAnswer);
            StoryProgressManager.Instance?.MarkGameOver();
        }

        private void StartChase()
        {
            ChaseController targetChase = chaseController != null ? chaseController : FindObjectOfType<ChaseController>();
            targetChase?.StartChase();
        }

        private SuspectConfirmUI GetSuspectConfirmUI()
        {
            if (suspectConfirmUI != null)
            {
                return suspectConfirmUI;
            }

            suspectConfirmUI = FindObjectOfType<SuspectConfirmUI>();
            if (suspectConfirmUI != null)
            {
                return suspectConfirmUI;
            }

            GameObject confirmObject = new GameObject("SuspectConfirmUI");
            suspectConfirmUI = confirmObject.AddComponent<SuspectConfirmUI>();
            return suspectConfirmUI;
        }

        private void EnsureUI()
        {
            if (endingCanvas == null)
            {
                GameObject canvasObject = new GameObject("EndingCanvas");
                endingCanvas = canvasObject.AddComponent<Canvas>();
                endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                endingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            EnsureCulpritChaseButton();

            if (panelRoot != null)
            {
                return;
            }

            panelRoot = CreatePanel("EndingPanel", endingCanvas.transform, new Color(0f, 0f, 0f, 0.88f)).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI title = CreateText("Title", panelRect, "범인을 선택하라", 34f);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.anchoredPosition = new Vector2(0f, -120f);
            title.rectTransform.sizeDelta = new Vector2(600f, 60f);

            CreateSuspectButton(panelRect, "1. 진세웅", new Vector2(0f, 80f), ChooseJinSewoong);
            CreateSuspectButton(panelRect, "2. 봉태현", new Vector2(0f, 20f), ChooseBongTaehyeon);
            CreateSuspectButton(panelRect, "3. 문수미", new Vector2(0f, -40f), ChooseMoonSumi);
            CreateSuspectButton(panelRect, "4. 오세진", new Vector2(0f, -100f), ChooseOhSejin);
        }

        private void EnsureCulpritChaseButton()
        {
            if (culpritChaseButton == null)
            {
                RectTransform rect = CreatePanel("CulpritChaseButton", endingCanvas.transform, new Color(0.18f, 0.015f, 0.025f, 0.96f));
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.anchoredPosition = new Vector2(-24f, -74f);
                rect.sizeDelta = new Vector2(210f, 50f);

                culpritChaseButton = rect.gameObject.AddComponent<Button>();
                HorrorUITheme.ApplyButton(culpritChaseButton, rect.GetComponent<Image>());

                TextMeshProUGUI text = CreateText("Label", rect, "범인찾기 (G)", 21f);
                text.color = HorrorUITheme.TextMain;
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = new Vector2(8f, 0f);
                text.rectTransform.offsetMax = new Vector2(-8f, 0f);
            }

            culpritChaseButton.onClick.RemoveListener(Show);
            culpritChaseButton.onClick.AddListener(Show);
            culpritChaseButton.gameObject.SetActive(false);
        }

        private void SubscribeToStoryProgress()
        {
            StoryProgressManager manager = StoryProgressManager.Instance;
            if (manager == null || observedStoryManager == manager)
            {
                return;
            }

            if (observedStoryManager != null)
            {
                observedStoryManager.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
            }

            observedStoryManager = manager;
            observedStoryManager.OnPhaseChanged.AddListener(HandlePhaseChanged);
        }

        private void HandlePhaseChanged(StoryPhase phase)
        {
            SetCulpritChaseButtonVisible(phase == StoryPhase.SuspectSelection);
        }

        private void RefreshCulpritChaseButton()
        {
            SubscribeToStoryProgress();
            SetCulpritChaseButtonVisible(StoryProgressManager.Instance != null &&
                                         StoryProgressManager.Instance.CurrentPhase == StoryPhase.SuspectSelection);
        }

        private void SetCulpritChaseButtonVisible(bool isVisible)
        {
            if (culpritChaseButton == null)
            {
                return;
            }

            bool panelIsOpen = panelRoot != null && panelRoot.activeSelf;
            culpritChaseButton.gameObject.SetActive(isVisible && !panelIsOpen);
        }

        private void HandleSuspectNumberShortcuts()
        {
            bool panelIsOpen = panelRoot != null && panelRoot.activeSelf;
            if (!panelIsOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                ConfirmSuspect(SuspectChoice.JinSewoong);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                ConfirmSuspect(SuspectChoice.BongTaehyeon);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                ConfirmSuspect(SuspectChoice.MoonSumi);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                ConfirmSuspect(SuspectChoice.OhSejin);
            }
        }

        private void TryShowCulpritSelectionShortcut()
        {
            StoryProgressManager manager = StoryProgressManager.Instance;
            if (manager == null || !manager.CanSelectSuspect)
            {
                return;
            }

            bool panelIsOpen = panelRoot != null && panelRoot.activeSelf;
            if (!panelIsOpen)
            {
                Show();
            }
        }

        private static void CreateSuspectButton(RectTransform parent, string label, Vector2 anchoredPosition, UnityAction action)
        {
            RectTransform rect = CreatePanel($"Button_{label}", parent, new Color(0.14f, 0.14f, 0.16f, 0.95f));
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(260f, 44f);

            Button button = rect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(action);

            TextMeshProUGUI text = CreateText("Label", rect, label, 22f);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            FontHelper.Apply(tmp);
            return tmp;
        }
    }

    public enum SuspectChoice
    {
        JinSewoong,
        BongTaehyeon,
        MoonSumi,
        OhSejin
    }
}
