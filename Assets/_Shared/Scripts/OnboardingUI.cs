using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EscapeGame
{
    public class OnboardingUI : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string roomSceneName = "Show";

        [Header("Main Menu")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Button directStartButton;
        [SerializeField] private Button descriptionButton;

        [Header("Description")]
        [SerializeField] private GameObject descriptionPanel;
        [SerializeField] private Button descriptionStartButton;

        private GameObject menuBackdrop;
        private GameObject descriptionRoot;
        private GameObject generatedDescriptionPanel;
        private GameObject generatedIntroPanel;
        private GameObject roomIntroObject;
        private GameObject roomIntroPanel;
        private bool roomIntroWasActivated;
        private int roomIntroShownFrame;

        private static readonly Color Blood = new Color(0.66f, 0.02f, 0.02f, 1f);
        private static readonly Color ButtonIdle = new Color(0.12f, 0.014f, 0.016f, 0.98f);
        private static readonly Color ButtonHover = new Color(0.30f, 0.018f, 0.022f, 1f);
        private static readonly Color ButtonPressed = new Color(0.045f, 0.004f, 0.006f, 1f);
        private static readonly Color TextMain = new Color(0.92f, 0.88f, 0.80f, 1f);
        private static readonly Color TextDim = new Color(0.72f, 0.67f, 0.60f, 1f);

        private void Awake()
        {
            ResolveReferences();
            BuildMenuBackdrop();
            WireButtons();
            ApplyInitialLayout();
            ApplyHorrorStyle();
            ShowMainMenu();
        }

        private void Start()
        {
            ShowMainMenu();
        }

        private void LateUpdate()
        {
            if (!roomIntroWasActivated)
            {
                HideRoomIntro();
                return;
            }

            if (Time.frameCount == roomIntroShownFrame)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || Input.GetMouseButtonDown(0))
            {
                LoadRoom();
            }
        }

        public void OnDescriptionButtonClicked()
        {
            SetButtonVisible(directStartButton, false);
            SetButtonVisible(descriptionButton, false);

            if (titleText != null)
            {
                titleText.gameObject.SetActive(false);
            }

            SetDescriptionVisible(false);

            if (generatedDescriptionPanel == null)
            {
                BuildDescriptionPanel();
            }

            if (generatedDescriptionPanel != null)
            {
                generatedDescriptionPanel.SetActive(true);
                generatedDescriptionPanel.transform.SetAsLastSibling();
            }
        }

        public void OnStartButtonClicked()
        {
            ShowStoryIntro();
        }

        public void LoadRoom()
        {
            SceneManager.LoadScene(roomSceneName);
        }

        private void ShowMainMenu()
        {
            SetButtonVisible(directStartButton, true);
            SetButtonVisible(descriptionButton, true);

            if (titleText != null)
            {
                titleText.gameObject.SetActive(true);
            }

            if (descriptionPanel != null)
            {
                SetDescriptionVisible(false);
            }

            if (generatedDescriptionPanel != null)
            {
                generatedDescriptionPanel.SetActive(false);
            }

            if (generatedIntroPanel != null)
            {
                generatedIntroPanel.SetActive(false);
            }

            HideRoomIntro();

            if (menuBackdrop != null)
            {
                menuBackdrop.SetActive(true);
                menuBackdrop.transform.SetAsFirstSibling();
            }
        }

        private void ShowStoryIntro()
        {
            if (menuBackdrop != null)
            {
                menuBackdrop.SetActive(false);
            }

            SetButtonVisible(directStartButton, false);
            SetButtonVisible(descriptionButton, false);

            if (titleText != null)
            {
                titleText.gameObject.SetActive(false);
            }

            if (descriptionPanel != null)
            {
                SetDescriptionVisible(false);
            }

            if (generatedDescriptionPanel != null)
            {
                generatedDescriptionPanel.SetActive(false);
            }

            ShowGeneratedIntro();
        }

        private void ResolveReferences()
        {
            directStartButton ??= FindButton("Button_DirectStart");
            descriptionButton ??= FindButton("Button_Description");
            descriptionPanel ??= FindObjectByName("Scroll View");
            descriptionRoot ??= descriptionPanel != null && descriptionPanel.transform.parent != null
                ? descriptionPanel.transform.parent.gameObject
                : descriptionPanel;
            descriptionStartButton ??= FindButton("StartGameButton");
            titleText ??= FindTitleText();
            roomIntroObject ??= FindObjectByName("IntroScenarioUI");
            roomIntroPanel ??= FindObjectByName("IntroScenarioPanel");
        }

        private void WireButtons()
        {
            ReplaceClick(directStartButton, OnStartButtonClicked);
            ReplaceClick(descriptionButton, OnDescriptionButtonClicked);
            ReplaceClick(descriptionStartButton, OnStartButtonClicked);
        }

        private void ApplyInitialLayout()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            CanvasScaler scaler = GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280f, 720f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            if (titleText != null)
            {
                RectTransform rect = titleText.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 125f);
                rect.sizeDelta = new Vector2(520f, 72f);
                titleText.text = "\uC808\uADDC\uC758 \uC218\uC220\uC2E4";
                titleText.fontSize = 42f;
                titleText.alignment = TextAlignmentOptions.Center;
            }

            PositionButton(directStartButton, new Vector2(0f, 20f));
            PositionButton(descriptionButton, new Vector2(0f, -62f));

            if (descriptionPanel != null)
            {
                RectTransform rect = descriptionPanel.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = Vector2.zero;
                    rect.sizeDelta = new Vector2(640f, 520f);
                }
            }
        }

        private void ApplyHorrorStyle()
        {
            if (Camera.main != null)
            {
                Camera.main.backgroundColor = new Color(0.02f, 0.024f, 0.03f, 1f);
            }

            StyleButton(directStartButton, "\uAC8C\uC784 \uC2DC\uC791");
            StyleButton(descriptionButton, "\uAC8C\uC784 \uC124\uBA85");
            StyleButton(descriptionStartButton, "\uAC8C\uC784 \uC2DC\uC791");

            if (titleText != null)
            {
                titleText.color = Blood;
            }

            StylePanel(descriptionPanel, new Color(0.018f, 0.016f, 0.016f, 0.96f));

            foreach (TextMeshProUGUI text in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text.transform.GetComponentInParent<Button>(true) != null)
                {
                    continue;
                }

                text.enableWordWrapping = true;
                if (text != titleText)
                {
                    text.color = text.fontSize >= 28f ? TextMain : TextDim;
                }
            }
        }

        private void HideRoomIntro()
        {
            roomIntroPanel ??= FindObjectByName("IntroScenarioPanel");
            if (roomIntroPanel != null)
            {
                roomIntroPanel.SetActive(false);
            }

            roomIntroObject ??= FindObjectByName("IntroScenarioUI");
            GameObject hudCanvas = FindObjectByName("HUD_Canvas");
            if (hudCanvas != null)
            {
                hudCanvas.SetActive(false);
            }
        }

        private void SetDescriptionVisible(bool visible)
        {
            if (descriptionRoot != null)
            {
                descriptionRoot.SetActive(false);
            }

            if (descriptionPanel != null)
            {
                descriptionPanel.SetActive(false);
            }
        }

        private void ShowGeneratedIntro()
        {
            roomIntroWasActivated = true;
            roomIntroShownFrame = Time.frameCount;

            if (generatedIntroPanel == null)
            {
                BuildGeneratedIntroPanel();
            }

            if (generatedIntroPanel != null)
            {
                generatedIntroPanel.SetActive(true);
                generatedIntroPanel.transform.SetAsLastSibling();
            }
        }

        private void BuildGeneratedIntroPanel()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            generatedIntroPanel = new GameObject("GeneratedIntroPanel");
            generatedIntroPanel.transform.SetParent(canvas.transform, false);
            Image backdrop = generatedIntroPanel.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.72f);

            RectTransform rootRect = generatedIntroPanel.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            GameObject box = new GameObject("IntroStoryBox");
            box.transform.SetParent(generatedIntroPanel.transform, false);
            Image boxImage = box.AddComponent<Image>();
            boxImage.color = new Color(0.015f, 0.012f, 0.012f, 0.96f);

            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            boxRect.sizeDelta = new Vector2(640f, 250f);

            VerticalLayoutGroup layout = box.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(46, 46, 28, 24);
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateText(box.transform, "IntroTitle", "\uC808\uADDC\uC758 \uC218\uC220\uC2E4", 28f, Blood);
            title.fontStyle = FontStyles.Bold;
            AddPreferredHeight(title.gameObject, 38f);

            TextMeshProUGUI body = CreateText(
                box.transform,
                "IntroBody",
                "\uB208\uC744 \uB5A0\uBCF4\uB2C8 \uCC28\uAC00\uC6B4 \uBCD1\uC6D0 \uBCF5\uB3C4\uC600\uB2E4.\n\n\uBB38\uC740 \uC7A0\uACA8 \uC788\uACE0, \uBD88\uB7C9\uD55C \uC870\uBA85\uB9CC\uC774 \uB108\uB97C \uBC18\uACA8\uC900\uB2E4.\n\n\uD0C8\uCD9C\uAD6C \uC5F4\uC1E0\uB294 \uC774 \uC548 \uC5B4\uB518\uAC00\uC5D0 \uC788\uB2E4. \uB2E8\uC11C\uB97C \uCC3E\uC544\uB77C. \uBC94\uC778\uC744 \uBC1D\uD600\uB77C.\n\uADF8\uB9AC\uACE0 20\uBD84 \uC548\uC5D0 \uC5EC\uAE30\uC11C \uB098\uAC00\uB77C.",
                18f,
                TextMain);
            AddPreferredHeight(body.gameObject, 118f);

            TextMeshProUGUI hint = CreateText(box.transform, "IntroHint", "Space / F / \uD074\uB9AD: \uC815\uC2E0 \uCC28\uB9AC\uAE30", 15f, TextDim);
            AddPreferredHeight(hint.gameObject, 28f);

            generatedIntroPanel.SetActive(false);
        }

        private void BuildDescriptionPanel()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            generatedDescriptionPanel = new GameObject("GeneratedDescriptionPanel");
            generatedDescriptionPanel.transform.SetParent(canvas.transform, false);
            Image backdrop = generatedDescriptionPanel.AddComponent<Image>();
            backdrop.color = new Color(0f, 0f, 0f, 0.76f);

            RectTransform rootRect = generatedDescriptionPanel.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;

            GameObject box = new GameObject("DescriptionBox");
            box.transform.SetParent(generatedDescriptionPanel.transform, false);
            Image boxImage = box.AddComponent<Image>();
            boxImage.color = new Color(0.018f, 0.015f, 0.015f, 0.97f);

            RectTransform boxRect = box.GetComponent<RectTransform>();
            boxRect.anchorMin = new Vector2(0.5f, 0.5f);
            boxRect.anchorMax = new Vector2(0.5f, 0.5f);
            boxRect.pivot = new Vector2(0.5f, 0.5f);
            boxRect.anchoredPosition = Vector2.zero;
            boxRect.sizeDelta = new Vector2(650f, 520f);

            VerticalLayoutGroup layout = box.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(44, 44, 30, 30);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateText(box.transform, "DescriptionTitle", "\uAC8C\uC784 \uC124\uBA85", 34f, Blood);
            title.fontStyle = FontStyles.Bold;
            AddPreferredHeight(title.gameObject, 48f);

            TextMeshProUGUI body = CreateText(
                box.transform,
                "DescriptionBody",
                "\uCC28\uAC00\uC6B4 \uBA54\uC2A4\uC640 \uD53C\uBE44\uB9B0\uB0B4\uB85C \uAC00\uB4DD\uD55C \uC758\uBB38\uC758 \uC218\uC220\uC2E4.\n\uB2F9\uC2E0\uC740 \uC774 \uC5B4\uB450\uC6B4 \uBC29 \uC548\uC5D0 \uAC07\uD614\uC2B5\uB2C8\uB2E4.\n\n<color=#b00000><b>\uAC8C\uC784 \uBC29\uBC95</b></color>\n\uBC29 \uC548\uC744 \uC790\uC720\uB86D\uAC8C \uB3CC\uC544\uB2E4\uB2C8\uBA70\n\uC758\uC2EC\uC2A4\uB7EC\uC6B4 \uACF3\uC744 \uC870\uC0AC\uD558\uC138\uC694.\n\uC228\uACA8\uC9C4 \uACB0\uC815\uC801 \uB2E8\uC11C\uB4E4\uC744 \uC218\uC9D1\uD574\uC57C \uD569\uB2C8\uB2E4.\n\n<color=#b00000><b>\uACBD\uACE0</b></color>\n\uBC18\uB4DC\uC2DC \uC2E0\uC911\uD558\uAC8C \uCD94\uB9AC\uD558\uC138\uC694.\n\uD2C0\uB9B0 \uCD94\uB9AC\uB294 \uC704\uD5D8\uD55C \uACB0\uB9D0\uB85C \uC774\uC5B4\uC9C8 \uC218 \uC788\uC2B5\uB2C8\uB2E4.",
                20f,
                TextMain);
            body.alignment = TextAlignmentOptions.Center;
            body.richText = true;
            body.overflowMode = TextOverflowModes.Overflow;
            AddPreferredHeight(body.gameObject, 330f);

            Button startButton = CreateButton(box.transform, "\uAC8C\uC784 \uC2DC\uC791");
            AddPreferredHeight(startButton.gameObject, 54f);
            ReplaceClick(startButton, OnStartButtonClicked);
            StyleButton(startButton, "\uAC8C\uC784 \uC2DC\uC791");

            generatedDescriptionPanel.SetActive(false);
        }

        private void BuildMenuBackdrop()
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null || menuBackdrop != null)
            {
                return;
            }

            menuBackdrop = new GameObject("MainMenuBackdrop");
            menuBackdrop.transform.SetParent(canvas.transform, false);
            Image image = menuBackdrop.AddComponent<Image>();
            image.color = new Color(0.01f, 0.012f, 0.014f, 0.78f);
            image.raycastTarget = false;

            RectTransform rect = menuBackdrop.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            menuBackdrop.transform.SetAsFirstSibling();
        }

        private static void AddPreferredHeight(GameObject go, float height)
        {
            LayoutElement element = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            element.preferredHeight = height;
        }

        private static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            GameObject go = new GameObject("GeneratedStartButton");
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = ButtonIdle;
            image.raycastTarget = true;
            Button button = go.AddComponent<Button>();

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.04f, 0.04f, 0.95f);
            outline.effectDistance = new Vector2(2f, -2f);

            GameObject textObject = new GameObject("Text (TMP)");
            textObject.transform.SetParent(go.transform, false);
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 24f;
            text.color = TextMain;
            text.alignment = TextAlignmentOptions.Center;

            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }

        private static void PositionButton(Button button, Vector2 position)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(220f, 44f);
        }

        private static void ReplaceClick(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        private static void SetButtonVisible(Button button, bool visible)
        {
            if (button != null)
            {
                button.gameObject.SetActive(visible);
            }
        }

        private static Button FindButton(string name)
        {
            foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (button.gameObject.name == name)
                {
                    return button;
                }
            }

            return null;
        }

        private static GameObject FindObjectByName(string name)
        {
            foreach (Transform transform in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (transform.gameObject.name == name)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static TextMeshProUGUI FindTitleText()
        {
            foreach (TextMeshProUGUI text in FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (text.gameObject.name == "\uC808\uADDC\uC758 \uC218\uC220\uC2E4" || text.text.Contains("\uC808\uADDC\uC758 \uC218\uC220\uC2E4"))
                {
                    return text;
                }
            }

            return null;
        }

        private static void StylePanel(GameObject panel, Color color)
        {
            if (panel == null)
            {
                return;
            }

            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        private static void StyleButton(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = ButtonIdle;
            }

            ColorBlock colors = button.colors;
            colors.normalColor = ButtonIdle;
            colors.highlightedColor = ButtonHover;
            colors.pressedColor = ButtonPressed;
            colors.selectedColor = ButtonHover;
            colors.disabledColor = new Color(0.08f, 0.08f, 0.08f, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            TextMeshProUGUI text = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (text != null)
            {
                text.text = label;
                text.color = TextMain;
                text.fontSize = 24f;
                text.fontStyle = FontStyles.Bold;
                text.alignment = TextAlignmentOptions.Center;
            }
        }
    }
}
