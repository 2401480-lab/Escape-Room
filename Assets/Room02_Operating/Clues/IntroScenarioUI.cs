using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class IntroScenarioUI : MonoBehaviour
    {
        [SerializeField] private Canvas introCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI hintText;

        private bool isOpen;

        private void Awake()
        {
            EnsureUI();
            SetOpen(true);
        }

        private void Update()
        {
            if (!isOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                SetOpen(false);
            }
        }

        private void SetOpen(bool open)
        {
            isOpen = open;
            if (panelRoot != null)
            {
                panelRoot.SetActive(open);
            }
        }

        private void EnsureUI()
        {
            introCanvas = EnsureHudCanvas();
            if (panelRoot != null)
            {
                return;
            }

            panelRoot = CreatePanel("IntroScenarioPanel", introCanvas.transform, new Color(0f, 0f, 0f, 0.88f)).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            RectTransform textBox = CreatePanel("IntroTextBox", panelRect, HorrorUITheme.PanelBlack);
            textBox.anchorMin = new Vector2(0.5f, 0.5f);
            textBox.anchorMax = new Vector2(0.5f, 0.5f);
            textBox.pivot = new Vector2(0.5f, 0.5f);
            textBox.sizeDelta = new Vector2(920f, 420f);
            textBox.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = textBox.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(56, 56, 48, 42);
            layout.spacing = 22f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            titleText = CreateText("IntroTitle", textBox, "절규의 수술실", 38f, TextAlignmentOptions.Center, HorrorUITheme.BloodRed);
            bodyText = CreateText(
                "IntroBody",
                textBox,
                "눈을 떠보니 차가운 병원 복도였다.\n\n문은 잠겨 있고, 불빛은 불안하게 깜빡인다.\n나는 갇혔다.\n\n누가 나를 여기로 불렀는지, 왜 이 수술실이 아직도 피 냄새를 품고 있는지 알아내야 한다.",
                24f,
                TextAlignmentOptions.Center,
                HorrorUITheme.TextMain);
            hintText = CreateText("IntroHint", textBox, "Space / F / 클릭: 정신 차리기", 19f, TextAlignmentOptions.Center, HorrorUITheme.TextDim);

            AddLayoutHeight(titleText, 60f);
            AddLayoutHeight(bodyText, 230f);
            AddLayoutHeight(hintText, 44f);
        }

        private static void AddLayoutHeight(TextMeshProUGUI text, float height)
        {
            LayoutElement element = text.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = height;
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
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            return canvas;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            HorrorUITheme.ApplyPanel(image, color);
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            HorrorUITheme.ApplyText(tmp, fontSize, color);
            return tmp;
        }
    }
}
