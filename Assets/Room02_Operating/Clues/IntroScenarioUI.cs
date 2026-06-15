using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class IntroScenarioUI : MonoBehaviour
    {
        private const string IntroSeparator = "─────────────────────────";
        private const string IntroSoundResourcePath = "Audio/Intro/guts_and_gore_19";
        private static readonly Vector2 BottomLeftIntroButtonAnchor = new Vector2(0f, 0f);
        private static readonly Vector2 BottomRightIntroButtonAnchor = new Vector2(1f, 0f);
        private static readonly Vector2 BottomLeftIntroButtonPosition = new Vector2(48f, 42f);
        private static readonly Vector2 BottomRightIntroButtonPosition = new Vector2(-48f, 42f);
        private static readonly string[] IntroPages =
        {
            "눈을 떠보니 차가운 병원 복도였다.\n\n" +
            IntroSeparator + "\n\n" +
            "문은 잠겨 있고, 불빛은 불안하게 깜빡인다.\n" +
            "나는 갇혔다.",

            "오늘 밤, 이 폐요양 병원에서 한 명이 죽었다.\n\n" +
            IntroSeparator + "\n\n" +
            "심령 동아리 부원 유안나.\n" +
            "수술실 수술대 위에서 싸늘하게 식은 채 발견됐다.\n" +
            "독살이었다.",

            "용의자는 세 명이다.\n\n" +
            IntroSeparator + "\n\n" +
            "진세웅\n" +
            "행사 기획자. 수술실 구역 담당.\n\n" +
            "봉태현\n" +
            "진세웅의 절친. 수술실 안내원.\n\n" +
            "문수미\n" +
            "동아리 3학년. 좀비 역 담당.",

            "이들 모두 유안나와 얽힌 사정이 있는 것 같다.\n" +
            "그리고 그 중 한 명은 아직 이 안에 있다.\n\n" +
            IntroSeparator + "\n\n" +
            "탈출구 열쇠는 이 안 어딘가에 있다.\n" +
            "단서를 찾아라. 범인을 밝혀라.\n" +
            "그리고 — 20분 안에 여기서 나가라."
        };

        [SerializeField] private Canvas introCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI pageText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private AudioClip introSoundClip;
        [SerializeField] private float introSoundVolume = 0.85f;

        private bool isOpen;
        private int currentPageIndex;
        private AudioSource introAudioSource;

        private void Awake()
        {
            EnsureUI();
            EnsureIntroAudio();
            SetOpen(true);
        }

        private void Update()
        {
            if (!isOpen)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || ShouldAdvanceFromMouseClick())
            {
                NextIntroPage();
            }
        }

        public void NextIntroPage()
        {
            if (currentPageIndex >= IntroPages.Length - 1)
            {
                SetOpen(false);
                return;
            }

            currentPageIndex++;
            UpdateIntroPage();
        }

        public void PreviousIntroPage()
        {
            currentPageIndex = Mathf.Max(0, currentPageIndex - 1);
            UpdateIntroPage();
        }

        private bool ShouldAdvanceFromMouseClick()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                return false;
            }

            return EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject();
        }

        private void SetOpen(bool open)
        {
            isOpen = open;
            if (panelRoot != null)
            {
                panelRoot.SetActive(open);
            }

            if (open)
            {
                currentPageIndex = 0;
                UpdateIntroPage();
                PlayIntroSound();
            }
            else
            {
                StopIntroSound();
            }
        }

        private void EnsureIntroAudio()
        {
            introAudioSource = GetComponent<AudioSource>();
            if (introAudioSource == null)
            {
                introAudioSource = gameObject.AddComponent<AudioSource>();
            }

            introAudioSource.playOnAwake = false;
            introAudioSource.loop = false;
            introAudioSource.spatialBlend = 0f;

            if (introSoundClip == null)
            {
                introSoundClip = Resources.Load<AudioClip>(IntroSoundResourcePath);
            }
        }

        private void PlayIntroSound()
        {
            if (introAudioSource == null || introSoundClip == null)
            {
                return;
            }

            introAudioSource.Stop();
            introAudioSource.PlayOneShot(introSoundClip, introSoundVolume);
        }

        private void StopIntroSound()
        {
            if (introAudioSource == null)
            {
                return;
            }

            introAudioSource.Stop();
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
            textBox.sizeDelta = new Vector2(980f, 560f);
            textBox.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup layout = textBox.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(56, 56, 42, 32);
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            titleText = CreateText("IntroTitle", textBox, "절규의 수술실", 34f, TextAlignmentOptions.Center, HorrorUITheme.BloodRed);
            bodyText = CreateText(
                "IntroBody",
                textBox,
                string.Empty,
                24f,
                TextAlignmentOptions.Center,
                HorrorUITheme.TextMain);
            bodyText.enableAutoSizing = true;
            bodyText.fontSizeMin = 18f;
            bodyText.fontSizeMax = 24f;
            bodyText.lineSpacing = 8f;
            pageText = CreateText("IntroPageIndicator", textBox, "1 / 4", 18f, TextAlignmentOptions.Center, HorrorUITheme.SickYellow);
            hintText = CreateText("IntroHint", textBox, "Space / F / 클릭: 다음", 18f, TextAlignmentOptions.Center, HorrorUITheme.TextDim);

            AddLayoutHeight(titleText, 48f);
            AddLayoutHeight(bodyText, 340f);
            AddLayoutHeight(pageText, 28f);
            AddLayoutHeight(hintText, 36f);

            previousButton = CreateIntroButton("PreviousIntroPageButton", panelRect, "←", BottomLeftIntroButtonAnchor, BottomLeftIntroButtonPosition, PreviousIntroPage);
            nextButton = CreateIntroButton("NextIntroPageButton", panelRect, "→", BottomRightIntroButtonAnchor, BottomRightIntroButtonPosition, NextIntroPage);
            UpdateIntroPage();
        }

        private void UpdateIntroPage()
        {
            int pageCount = IntroPages.Length;
            currentPageIndex = Mathf.Clamp(currentPageIndex, 0, pageCount - 1);

            if (bodyText != null)
            {
                bodyText.text = IntroPages[currentPageIndex];
            }

            if (pageText != null)
            {
                pageText.text = $"{currentPageIndex + 1} / {pageCount}";
            }

            if (previousButton != null)
            {
                previousButton.interactable = currentPageIndex > 0;
            }

            if (nextButton != null)
            {
                SetButtonLabel(nextButton, currentPageIndex >= IntroPages.Length - 1 ? "▶" : "→");
            }
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
            image.raycastTarget = false;
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

        private static Button CreateIntroButton(string name, Transform parent, string text, Vector2 anchor, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            RectTransform rect = CreatePanel(name, parent, HorrorUITheme.PanelRed);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(76f, 58f);

            Image image = rect.GetComponent<Image>();
            image.raycastTarget = true;
            Button button = rect.gameObject.AddComponent<Button>();
            HorrorUITheme.ApplyButton(button, image);
            button.onClick.AddListener(onClick);

            TextMeshProUGUI label = CreateText("Label", rect, text, 30f, TextAlignmentOptions.Center, HorrorUITheme.TextMain);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static void SetButtonLabel(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = text;
            }
        }
    }
}
