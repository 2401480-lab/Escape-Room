using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] private Canvas settingsCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private GameObject volumeSensitivityTabRoot;
        [SerializeField] private GameObject controlsTabRoot;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Slider sensitivitySlider;

        public float Volume => volumeSlider != null ? volumeSlider.value : 1f;
        public float Sensitivity => sensitivitySlider != null ? sensitivitySlider.value : 1f;

        private void Awake()
        {
            EnsureUI();
            SetOpen(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Toggle();
            }
        }

        public void Toggle()
        {
            SetOpen(panelRoot == null || !panelRoot.activeSelf);
        }

        public void SetOpen(bool isOpen)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(isOpen);
            }
        }

        private void ShowVolumeSensitivityTab()
        {
            if (volumeSensitivityTabRoot != null) volumeSensitivityTabRoot.SetActive(true);
            if (controlsTabRoot != null) controlsTabRoot.SetActive(false);
        }

        private void ShowControlsTab()
        {
            if (volumeSensitivityTabRoot != null) volumeSensitivityTabRoot.SetActive(false);
            if (controlsTabRoot != null) controlsTabRoot.SetActive(true);
        }

        private void EnsureUI()
        {
            settingsCanvas = EnsureHudCanvas();

            if (panelRoot != null)
            {
                return;
            }

            CreateSettingsButton(settingsCanvas.transform);
            CreateSettingsPanel(settingsCanvas.transform);
            ShowVolumeSensitivityTab();
        }

        private void CreateSettingsButton(Transform parent)
        {
            Button button = CreateButton("SettingsHudButton", parent, "설정 (ESC)", 108f, 38f);
            RectTransform rect = (RectTransform)button.transform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-24f, -24f);
            button.onClick.AddListener(Toggle);
        }

        private void CreateSettingsPanel(Transform parent)
        {
            panelRoot = CreatePanel("SettingsPanel", parent, HorrorUITheme.PanelBlack).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 420f);
            panelRect.anchoredPosition = Vector2.zero;

            TextMeshProUGUI title = CreateText("SettingsTitle", panelRect, "설정", 32f, TextAlignmentOptions.Left);
            title.color = HorrorUITheme.BloodRed;
            title.rectTransform.anchorMin = new Vector2(0f, 1f);
            title.rectTransform.anchorMax = new Vector2(1f, 1f);
            title.rectTransform.offsetMin = new Vector2(28f, -70f);
            title.rectTransform.offsetMax = new Vector2(-28f, -22f);

            RectTransform tabBar = CreatePanel("SettingsTabBar", panelRect, new Color(0f, 0f, 0f, 0f));
            tabBar.anchorMin = new Vector2(0f, 1f);
            tabBar.anchorMax = new Vector2(1f, 1f);
            tabBar.offsetMin = new Vector2(28f, -124f);
            tabBar.offsetMax = new Vector2(-28f, -82f);

            HorizontalLayoutGroup tabLayout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            tabLayout.childControlWidth = false;
            tabLayout.childControlHeight = true;
            tabLayout.spacing = 8f;

            Button volumeTab = CreateButton("VolumeSensitivityTabButton", tabBar, "소리 / 감도", 150f, 38f);
            volumeTab.onClick.AddListener(ShowVolumeSensitivityTab);
            Button controlsTab = CreateButton("ControlsTabButton", tabBar, "조작법", 120f, 38f);
            controlsTab.onClick.AddListener(ShowControlsTab);

            CreateVolumeSensitivityTab(panelRect);
            CreateControlsTab(panelRect);
        }

        private void CreateVolumeSensitivityTab(RectTransform parent)
        {
            volumeSensitivityTabRoot = CreatePanel("VolumeSensitivityTabRoot", parent, new Color(0f, 0f, 0f, 0f)).gameObject;
            RectTransform rect = (RectTransform)volumeSensitivityTabRoot.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(28f, 34f);
            rect.offsetMax = new Vector2(-28f, -140f);

            VerticalLayoutGroup layout = volumeSensitivityTabRoot.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 0, 12, 0);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;

            volumeSlider = CreateLabeledSlider("VolumeSlider", rect, "볼륨", 0f, 1f, 1f);
            sensitivitySlider = CreateLabeledSlider("SensitivitySlider", rect, "감도", 0.1f, 3f, 1f);
        }

        private void CreateControlsTab(RectTransform parent)
        {
            controlsTabRoot = CreatePanel("ControlsTabRoot", parent, new Color(0f, 0f, 0f, 0f)).gameObject;
            RectTransform rect = (RectTransform)controlsTabRoot.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(28f, 34f);
            rect.offsetMax = new Vector2(-28f, -140f);

            string controls = "WASD 이동\nShift 달리기\n마우스 시점 이동\nE 문 열기\nF 조사하기 / 박스 조사하기\nJ 수집 증거\nK 용의자 수첩\nESC 설정";
            TextMeshProUGUI text = CreateText("ControlsText", rect, controls, 22f, TextAlignmentOptions.Left);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
        }

        private Slider CreateLabeledSlider(string name, Transform parent, string label, float min, float max, float value)
        {
            RectTransform row = CreatePanel($"{name}Row", parent, HorrorUITheme.PanelDeep);
            LayoutElement rowElement = row.gameObject.AddComponent<LayoutElement>();
            rowElement.preferredHeight = 72f;

            TextMeshProUGUI labelText = CreateText($"{name}Label", row, label, 20f, TextAlignmentOptions.Left);
            labelText.rectTransform.anchorMin = new Vector2(0f, 0f);
            labelText.rectTransform.anchorMax = new Vector2(0f, 1f);
            labelText.rectTransform.offsetMin = new Vector2(16f, 0f);
            labelText.rectTransform.offsetMax = new Vector2(140f, 0f);

            GameObject sliderObject = new GameObject(name);
            sliderObject.transform.SetParent(row, false);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;

            RectTransform sliderRect = slider.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.5f);
            sliderRect.anchorMax = new Vector2(1f, 0.5f);
            sliderRect.pivot = new Vector2(0.5f, 0.5f);
            sliderRect.offsetMin = new Vector2(150f, -12f);
            sliderRect.offsetMax = new Vector2(-20f, 12f);

            Image background = CreatePanel("Background", sliderRect, HorrorUITheme.PanelBlack).GetComponent<Image>();
            background.rectTransform.anchorMin = new Vector2(0f, 0.35f);
            background.rectTransform.anchorMax = new Vector2(1f, 0.65f);
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            RectTransform fillArea = CreatePanel("Fill Area", sliderRect, new Color(0f, 0f, 0f, 0f));
            fillArea.anchorMin = new Vector2(0f, 0.25f);
            fillArea.anchorMax = new Vector2(1f, 0.75f);
            fillArea.offsetMin = new Vector2(8f, 0f);
            fillArea.offsetMax = new Vector2(-8f, 0f);

            Image fill = CreatePanel("Fill", fillArea, HorrorUITheme.BloodRed).GetComponent<Image>();
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            Image handle = CreatePanel("Handle", sliderRect, HorrorUITheme.TextMain).GetComponent<Image>();
            handle.rectTransform.sizeDelta = new Vector2(18f, 28f);

            slider.targetGraphic = handle;
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            return slider;
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

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            HorrorUITheme.ApplyPanel(image, color);
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.alignment = alignment;
            HorrorUITheme.ApplyText(tmp, fontSize);
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string text, float width, float height)
        {
            RectTransform rect = CreatePanel(name, parent, HorrorUITheme.PanelRed);
            LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = width;
            element.preferredHeight = height;

            Button button = rect.gameObject.AddComponent<Button>();
            HorrorUITheme.ApplyButton(button, rect.GetComponent<Image>());
            TextMeshProUGUI label = CreateText("Label", rect, text, 17f, TextAlignmentOptions.Center);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }
    }
}
