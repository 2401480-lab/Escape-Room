using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class ControlHintUI : MonoBehaviour
    {
        public static ControlHintUI Instance { get; private set; }

        [SerializeField] private Canvas hintCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject doorOpenHintRow;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureUI();
        }

        private void LateUpdate()
        {
            RefreshDoorPromptVisibility();
        }

        public static void SetDoorPromptVisible(bool visible)
        {
            GetOrCreateInstance()?.RefreshDoorPromptVisibility();
        }

        private static ControlHintUI GetOrCreateInstance()
        {
            if (Instance != null)
            {
                return Instance;
            }

            ControlHintUI existingUI = Object.FindFirstObjectByType<ControlHintUI>();
            if (existingUI != null)
            {
                Instance = existingUI;
                existingUI.EnsureUI();
                return existingUI;
            }

            GameObject hintObject = new GameObject("ControlHintUI");
            return hintObject.AddComponent<ControlHintUI>();
        }

        private void EnsureUI()
        {
            hintCanvas = EnsureHudCanvas();
            EnsureHintPanel();
            EnsureBaseHintRows();
            RefreshDoorPromptVisibility();
        }

        private void EnsureHintPanel()
        {
            if (panelRoot == null)
            {
                GameObject existingPanel = GameObject.Find("KeyboardControlHintPanel");
                if (existingPanel != null)
                {
                    panelRoot = existingPanel;
                }
            }

            RectTransform panel;
            if (panelRoot == null)
            {
                panel = CreatePanel("KeyboardControlHintPanel", hintCanvas.transform, new Color(0.02f, 0.018f, 0.02f, 0.82f));
                panelRoot = panel.gameObject;
            }
            else
            {
                panel = (RectTransform)panelRoot.transform;
            }

            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(0f, 0f);
            panel.pivot = new Vector2(0f, 0f);
            panel.anchoredPosition = new Vector2(24f, 24f);
            panel.sizeDelta = new Vector2(310f, 92f);

            VerticalLayoutGroup layout = panelRoot.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = panelRoot.AddComponent<VerticalLayoutGroup>();
            }

            layout.padding = new RectOffset(10, 10, 10, 10);
            layout.spacing = 7f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandHeight = false;
        }

        private void EnsureBaseHintRows()
        {
            if (panelRoot == null)
            {
                return;
            }

            Transform shiftRow = panelRoot.transform.Find("ControlHintRow_SHIFT");
            if (shiftRow == null)
            {
                shiftRow = CreateHintRow(panelRoot.transform, "SHIFT", "빨리 달리기", true);
            }

            ConfigureHintRow(shiftRow, "SHIFT", "빨리 달리기");
            EnsureDoorOpenHintRow();
        }

        private void EnsureDoorOpenHintRow()
        {
            if (panelRoot == null)
            {
                return;
            }

            Transform existingRow = panelRoot.transform.Find("DoorOpenHintRow");
            if (existingRow == null)
            {
                existingRow = panelRoot.transform.Find("ControlHintRow_E");
            }

            if (existingRow == null && doorOpenHintRow != null)
            {
                existingRow = doorOpenHintRow.transform;
            }

            if (existingRow != null)
            {
                doorOpenHintRow = existingRow.gameObject;
                doorOpenHintRow.name = "DoorOpenHintRow";
                ConfigureHintRow(doorOpenHintRow.transform, "E", "- 문열기");
                return;
            }

            doorOpenHintRow = CreateHintRow(panelRoot.transform, "E", "- 문열기", true).gameObject;
            doorOpenHintRow.name = "DoorOpenHintRow";
            ConfigureHintRow(doorOpenHintRow.transform, "E", "- 문열기");
        }

        private void RefreshDoorPromptVisibility()
        {
            if (doorOpenHintRow != null)
            {
                doorOpenHintRow.SetActive(true);
            }
        }

        private static void ConfigureHintRow(Transform row, string keyText, string actionText)
        {
            if (row == null)
            {
                return;
            }

            row.gameObject.SetActive(true);
            SetHintRowText(row.GetComponentsInChildren<TextMeshProUGUI>(true), keyText, actionText);
        }

        private static void SetHintRowText(TextMeshProUGUI[] labels, string keyText, string actionText)
        {
            if (labels == null || labels.Length == 0)
            {
                return;
            }

            foreach (TextMeshProUGUI label in labels)
            {
                FontHelper.Apply(label);
            }

            labels[0].text = keyText;
            if (labels.Length > 1)
            {
                labels[1].text = actionText;
            }
        }

        private static RectTransform CreateHintRow(Transform parent, string keyText, string actionText, bool active)
        {
            RectTransform row = CreatePanel($"ControlHintRow_{keyText}", parent, new Color(0f, 0f, 0f, 0f));
            row.gameObject.SetActive(active);

            LayoutElement rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 31f;
            rowElement.minHeight = 31f;

            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            RectTransform keycap = CreatePanel($"ControlKeycap_{keyText}", row, new Color(0.11f, 0.095f, 0.08f, 0.96f));
            LayoutElement keycapElement = keycap.gameObject.AddComponent<LayoutElement>();
            keycapElement.preferredWidth = keyText.Length > 1 ? 78f : 42f;
            keycapElement.preferredHeight = 31f;

            TextMeshProUGUI keyLabel = CreateText("KeyLabel", keycap, keyText, 16f, TextAlignmentOptions.Center);
            keyLabel.color = HorrorUITheme.SickYellow;

            TextMeshProUGUI action = CreateText("ActionLabel", row, actionText, 17f, TextAlignmentOptions.Left);
            LayoutElement actionElement = action.gameObject.AddComponent<LayoutElement>();
            actionElement.preferredWidth = 178f;
            actionElement.preferredHeight = 31f;
            action.color = HorrorUITheme.TextMain;

            return row;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            Image image = go.AddComponent<Image>();
            HorrorUITheme.ApplyPanel(image, color);
            return rect;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            tmp.fontSize = fontSize;
            tmp.enableWordWrapping = false;
            FontHelper.Apply(tmp);

            RectTransform rect = tmp.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return tmp;
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
    }
}
