using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class SuspectConfirmUI : MonoBehaviour
    {
        [SerializeField] private Canvas confirmCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button yesButton;
        [SerializeField] private Button noButton;

        private EndingUI owner;
        private SuspectChoice selectedSuspect;

        private void Awake()
        {
            EnsureUI();
            Hide();
        }

        public void Show(EndingUI endingUI, SuspectChoice suspect, string suspectName)
        {
            owner = endingUI;
            selectedSuspect = suspect;
            EnsureUI();
            messageText.text = $"정말 {suspectName}이(가) 범인입니까?";
            panelRoot.SetActive(true);
        }

        public void Confirm()
        {
            Hide();
            owner?.ConfirmSuspect(selectedSuspect);
        }

        public void Cancel()
        {
            Hide();
        }

        private void Hide()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }
        }

        private void EnsureUI()
        {
            if (confirmCanvas == null)
            {
                GameObject canvasObject = GameObject.Find("SuspectConfirmCanvas");
                if (canvasObject == null)
                {
                    canvasObject = new GameObject("SuspectConfirmCanvas");
                    confirmCanvas = canvasObject.AddComponent<Canvas>();
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    confirmCanvas = canvasObject.GetComponent<Canvas>();
                    if (confirmCanvas == null)
                    {
                        confirmCanvas = canvasObject.AddComponent<Canvas>();
                    }
                }
            }

            confirmCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (panelRoot != null)
            {
                return;
            }

            panelRoot = CreatePanel("SuspectConfirmPanel", confirmCanvas.transform, new Color(0f, 0f, 0f, 0.9f)).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(560f, 220f);

            messageText = CreateText("Message", panelRect, "", 25f);
            messageText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            messageText.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            messageText.rectTransform.pivot = new Vector2(0.5f, 1f);
            messageText.rectTransform.anchoredPosition = new Vector2(0f, -42f);
            messageText.rectTransform.sizeDelta = new Vector2(500f, 90f);

            yesButton = CreateButton("YesButton", panelRect, "예", new Vector2(-80f, -62f));
            yesButton.onClick.AddListener(Confirm);

            noButton = CreateButton("NoButton", panelRect, "아니오", new Vector2(80f, -62f));
            noButton.onClick.AddListener(Cancel);
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            RectTransform rect = CreatePanel(name, parent, new Color(0.16f, 0.16f, 0.18f, 0.96f));
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(120f, 44f);

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
}
