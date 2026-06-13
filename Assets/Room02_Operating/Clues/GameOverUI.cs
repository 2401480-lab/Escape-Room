using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [SerializeField] private Canvas gameOverCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Animator jumpscareAnimator;
        [SerializeField] private string mainMenuSceneName = "RoomSelect";

        public UnityEvent OnJumpscareStarted;
        public UnityEvent OnSurvivalEndingShown;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureUI();
            panelRoot.SetActive(false);
        }

        public void PlayGameOver(GameOverReason reason)
        {
            EnsureUI();
            messageText.text = GetMessage(reason);
            panelRoot.SetActive(true);

            if (reason == GameOverReason.WrongAnswer || reason == GameOverReason.DeductionTimerExpired)
            {
                OnJumpscareStarted?.Invoke();
                if (jumpscareAnimator != null)
                {
                    jumpscareAnimator.SetTrigger("Jumpscare");
                }
            }
        }

        public void ShowSurvivalEnding()
        {
            EnsureUI();
            messageText.text = "당신은 살아남았다.\n\n진세웅은 유안나가 하시호의 죽음을 유도했다는 사실을 알고 복수를 계획했다. 독약, 페인트 자국, 수술대 아래 공간이 그를 범인으로 가리켰다.";
            panelRoot.SetActive(true);
            OnSurvivalEndingShown?.Invoke();
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void MainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private static string GetMessage(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.WrongAnswer:
                    return "틀렸다.\n검은 실루엣이 눈앞을 덮친다.";
                case GameOverReason.CaughtDuringChase:
                    return "진범을 알아챘지만, 너무 늦었다.";
                case GameOverReason.DeductionTimerExpired:
                    return "추리 시간이 끝났다.\n실루엣이 뒤에서 덮쳐온다.";
                case GameOverReason.ChaseTimerExpired:
                    return "진범을 알아챘지만, 너무 늦었다.";
                default:
                    return "GameOver";
            }
        }

        private void EnsureUI()
        {
            if (gameOverCanvas == null)
            {
                GameObject canvasObject = new GameObject("GameOverCanvas");
                gameOverCanvas = canvasObject.AddComponent<Canvas>();
                gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            if (panelRoot != null)
            {
                return;
            }

            panelRoot = CreatePanel("GameOverPanel", gameOverCanvas.transform, new Color(0f, 0f, 0f, 0.9f)).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            messageText = CreateText("Message", panelRect, "", 30f);
            messageText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.anchoredPosition = new Vector2(0f, 80f);
            messageText.rectTransform.sizeDelta = new Vector2(720f, 220f);

            restartButton = CreateButton("RestartButton", panelRect, "재시작", new Vector2(-90f, -120f));
            restartButton.onClick.AddListener(Restart);

            mainMenuButton = CreateButton("MainMenuButton", panelRect, "메인 메뉴", new Vector2(110f, -120f));
            mainMenuButton.onClick.AddListener(MainMenu);
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            RectTransform rect = CreatePanel(name, parent, new Color(0.16f, 0.16f, 0.18f, 0.96f));
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(160f, 44f);

            Button button = rect.gameObject.AddComponent<Button>();
            TextMeshProUGUI text = CreateText("Label", rect, label, 20f);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
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
            tmp.enableWordWrapping = true;
            return tmp;
        }
    }

    public enum GameOverReason
    {
        WrongAnswer,
        CaughtDuringChase,
        DeductionTimerExpired,
        ChaseTimerExpired
    }
}
