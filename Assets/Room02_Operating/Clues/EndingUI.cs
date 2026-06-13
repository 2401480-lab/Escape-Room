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

        public UnityEvent OnCorrectSuspectSelected;
        public UnityEvent OnWrongSuspectSelected;
        public UnityEvent OnBlackoutRequested;
        public UnityEvent OnJinMaterialized;

        private SuspectChoice pendingSuspect;
        private bool wrongAnswerUsed;

        private void Awake()
        {
            EnsureUI();
            Hide();
        }

        public void Show()
        {
            EnsureUI();
            panelRoot.SetActive(true);
            StoryProgressManager.Instance?.BeginSuspectSelection();
        }

        public void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
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
            StoryProgressManager.Instance?.BeginChase();
            StartChase();
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

            CreateSuspectButton(panelRect, "진세웅", new Vector2(0f, 80f), ChooseJinSewoong);
            CreateSuspectButton(panelRect, "봉태현", new Vector2(0f, 20f), ChooseBongTaehyeon);
            CreateSuspectButton(panelRect, "문수미", new Vector2(0f, -40f), ChooseMoonSumi);
            CreateSuspectButton(panelRect, "오세진", new Vector2(0f, -100f), ChooseOhSejin);
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
