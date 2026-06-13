using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class ClueJournalUI : MonoBehaviour
    {
        [SerializeField] private ClueJournalManager journalManager;
        [SerializeField] private Canvas journalCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RectTransform chipContainer;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private ScrollRect evidenceScrollRect;
        [SerializeField] private RectTransform evidenceContent;
        [SerializeField] private GameObject evidenceTabRoot;
        [SerializeField] private GameObject suspectTabRoot;
        [SerializeField] private RectTransform suspectContent;

        private readonly Dictionary<ClueData, RectTransform> clueCards = new Dictionary<ClueData, RectTransform>();
        private readonly List<PersonInfo> people = new List<PersonInfo>
        {
            new PersonInfo("유안나", "피해자", "이번 사건의 피해자. 하시호의 죽음에 대해 무언가를 알고 있었다."),
            new PersonInfo("진세웅", "용의자", "하시호의 절친한 친구. 당일 혼자 1시간 일찍 도착했다."),
            new PersonInfo("봉태현", "용의자", "하시호 수술 당시 담당의. 의료 과실 의혹을 받고 있다."),
            new PersonInfo("문수미", "용의자", "진세웅과 가까운 사이. 그의 계획을 눈치챘으나 말리지 못했다."),
            new PersonInfo("하시호", "고인", "2년 전 이 병원 수술실에서 사망. 유서에 \"살해당한 것\"이라 적혀 있었다.")
        };

        private void Awake()
        {
            EnsureManager();
            EnsureUI();
            ShowEvidenceTab();
            SetOpen(false);
        }

        private void OnEnable()
        {
            EnsureManager();
            if (journalManager != null)
            {
                journalManager.OnCluesChanged += RefreshUI;
            }

            RefreshUI();
        }

        private void OnDisable()
        {
            if (journalManager != null)
            {
                journalManager.OnCluesChanged -= RefreshUI;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Tab))
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

        public void ScrollToClue(ClueData clueData)
        {
            if (clueData == null || evidenceScrollRect == null || evidenceContent == null)
            {
                return;
            }

            if (!clueCards.TryGetValue(clueData, out RectTransform target))
            {
                return;
            }

            ShowEvidenceTab();
            Canvas.ForceUpdateCanvases();

            float viewportHeight = evidenceScrollRect.viewport != null
                ? evidenceScrollRect.viewport.rect.height
                : ((RectTransform)evidenceScrollRect.transform).rect.height;
            float contentHeight = evidenceContent.rect.height;
            if (contentHeight <= viewportHeight)
            {
                evidenceScrollRect.verticalNormalizedPosition = 1f;
                return;
            }

            float targetY = Mathf.Abs(target.anchoredPosition.y);
            float normalized = 1f - Mathf.Clamp01(targetY / (contentHeight - viewportHeight));
            evidenceScrollRect.verticalNormalizedPosition = normalized;
        }

        private void EnsureManager()
        {
            if (journalManager != null)
            {
                return;
            }

            journalManager = ClueJournalManager.Instance;
            if (journalManager == null)
            {
                journalManager = FindObjectOfType<ClueJournalManager>();
            }
        }

        private void EnsureUI()
        {
            if (journalCanvas == null)
            {
                GameObject canvasObject = new GameObject("ClueJournalCanvas");
                journalCanvas = canvasObject.AddComponent<Canvas>();
                journalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                journalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            if (panelRoot != null)
            {
                return;
            }

            panelRoot = CreatePanel("ClueJournalPanel", journalCanvas.transform, new Color(0.05f, 0.05f, 0.06f, 0.92f)).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            CreateHeader(panelRect);
            CreateChipBar(panelRect);
            CreateTabButtons(panelRect);
            CreateEvidenceTab(panelRect);
            CreateSuspectTab(panelRect);
        }

        private void CreateHeader(RectTransform parent)
        {
            TextMeshProUGUI title = CreateText("JournalTitle", parent, "증거 기록", 32f, TextAlignmentOptions.Left);
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(32f, -74f);
            titleRect.offsetMax = new Vector2(-32f, -24f);

            progressText = CreateText("ProgressText", parent, "진행도: 0 / 0", 20f, TextAlignmentOptions.Right);
            RectTransform progressRect = progressText.rectTransform;
            progressRect.anchorMin = new Vector2(0f, 1f);
            progressRect.anchorMax = new Vector2(1f, 1f);
            progressRect.pivot = new Vector2(0.5f, 1f);
            progressRect.offsetMin = new Vector2(32f, -74f);
            progressRect.offsetMax = new Vector2(-32f, -24f);
        }

        private void CreateChipBar(RectTransform parent)
        {
            RectTransform bar = CreatePanel("EvidenceChipBar", parent, new Color(0.1f, 0.1f, 0.12f, 0.95f));
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.offsetMin = new Vector2(32f, -126f);
            bar.offsetMax = new Vector2(-32f, -82f);

            HorizontalLayoutGroup layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 8f;

            chipContainer = bar;
        }

        private void CreateTabButtons(RectTransform parent)
        {
            RectTransform tabBar = CreatePanel("JournalTabBar", parent, new Color(0.08f, 0.08f, 0.1f, 0.95f));
            tabBar.anchorMin = new Vector2(0f, 1f);
            tabBar.anchorMax = new Vector2(1f, 1f);
            tabBar.pivot = new Vector2(0.5f, 1f);
            tabBar.offsetMin = new Vector2(32f, -180f);
            tabBar.offsetMax = new Vector2(-32f, -134f);

            HorizontalLayoutGroup layout = tabBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.spacing = 8f;

            Button evidenceButton = CreateButton("EvidenceTabButton", tabBar, "수집 증거");
            evidenceButton.onClick.AddListener(ShowEvidenceTab);
            Button suspectButton = CreateButton("SuspectTabButton", tabBar, "용의자 수첩");
            suspectButton.onClick.AddListener(ShowSuspectTab);
        }

        private void CreateEvidenceTab(RectTransform parent)
        {
            evidenceTabRoot = CreatePanel("EvidenceTabRoot", parent, new Color(0f, 0f, 0f, 0f)).gameObject;
            RectTransform rootRect = (RectTransform)evidenceTabRoot.transform;
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.offsetMin = new Vector2(32f, 32f);
            rootRect.offsetMax = new Vector2(-32f, -190f);

            evidenceScrollRect = evidenceTabRoot.AddComponent<ScrollRect>();
            evidenceScrollRect.horizontal = false;

            RectTransform viewport = CreatePanel("EvidenceViewport", rootRect, new Color(0f, 0f, 0f, 0f));
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            evidenceContent = CreatePanel("EvidenceContent", viewport, new Color(0f, 0f, 0f, 0f));
            evidenceContent.anchorMin = new Vector2(0f, 1f);
            evidenceContent.anchorMax = new Vector2(1f, 1f);
            evidenceContent.pivot = new Vector2(0.5f, 1f);
            evidenceContent.offsetMin = Vector2.zero;
            evidenceContent.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = evidenceContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 12f;
            ContentSizeFitter fitter = evidenceContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            evidenceScrollRect.viewport = viewport;
            evidenceScrollRect.content = evidenceContent;
        }

        private void CreateSuspectTab(RectTransform parent)
        {
            suspectTabRoot = CreatePanel("SuspectTabRoot", parent, new Color(0f, 0f, 0f, 0f)).gameObject;
            RectTransform rootRect = (RectTransform)suspectTabRoot.transform;
            rootRect.anchorMin = new Vector2(0f, 0f);
            rootRect.anchorMax = new Vector2(1f, 1f);
            rootRect.offsetMin = new Vector2(32f, 32f);
            rootRect.offsetMax = new Vector2(-32f, -190f);

            suspectContent = CreatePanel("SuspectContent", rootRect, new Color(0f, 0f, 0f, 0f));
            suspectContent.anchorMin = Vector2.zero;
            suspectContent.anchorMax = Vector2.one;
            suspectContent.offsetMin = Vector2.zero;
            suspectContent.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = suspectContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 12f;

            BuildSuspectCards();
        }

        private void RefreshUI()
        {
            if (journalManager == null)
            {
                return;
            }

            BuildEvidenceChips();
            BuildEvidenceCards();
        }

        private void BuildEvidenceChips()
        {
            ClearChildren(chipContainer);

            foreach (ClueData clueData in journalManager.CollectedClues)
            {
                ClueData captured = clueData;
                Button chip = CreateButton($"Chip_{clueData.clueName}", chipContainer, clueData.clueName);
                chip.onClick.AddListener(() => ScrollToClue(captured));
            }
        }

        private void BuildEvidenceCards()
        {
            ClearChildren(evidenceContent);
            clueCards.Clear();

            IReadOnlyList<ClueData> allClues = journalManager.AllClues;
            int totalCount = allClues.Count;
            int collectedCount = journalManager.CollectedClues.Count;
            progressText.text = $"진행도: {collectedCount} / {totalCount}";

            Dictionary<string, List<ClueData>> grouped = new Dictionary<string, List<ClueData>>();
            List<ClueData> keyClueSection = new List<ClueData>();
            foreach (ClueData clueData in allClues)
            {
                if (clueData.category == ClueCategory.KeyClue)
                {
                    keyClueSection.Add(clueData);
                    continue;
                }

                string area = string.IsNullOrWhiteSpace(clueData.areaName) ? "미지정 구역" : clueData.areaName;
                if (!grouped.ContainsKey(area))
                {
                    grouped.Add(area, new List<ClueData>());
                }

                grouped[area].Add(clueData);
            }

            if (keyClueSection.Count > 0)
            {
                CreateAreaHeader("열쇠 단서");
                foreach (ClueData clueData in keyClueSection)
                {
                    CreateClueCard(clueData, journalManager.HasClue(clueData));
                }
            }

            foreach (KeyValuePair<string, List<ClueData>> group in grouped)
            {
                CreateAreaHeader(group.Key);
                foreach (ClueData clueData in group.Value)
                {
                    CreateClueCard(clueData, journalManager.HasClue(clueData));
                }
            }
        }

        private void CreateAreaHeader(string areaName)
        {
            TextMeshProUGUI header = CreateText($"Area_{areaName}", evidenceContent, areaName, 24f, TextAlignmentOptions.Left);
            header.color = new Color(0.82f, 0.88f, 1f);
            LayoutElement element = header.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = 36f;
        }

        private void CreateClueCard(ClueData clueData, bool discovered)
        {
            RectTransform card = CreatePanel($"Card_{clueData.clueName}", evidenceContent, new Color(0.12f, 0.12f, 0.15f, 0.95f));
            LayoutElement element = card.gameObject.AddComponent<LayoutElement>();
            element.preferredHeight = discovered ? 150f : 96f;
            clueCards[clueData] = card;

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 12, 12);
            layout.spacing = 6f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            if (!discovered)
            {
                CreateText("UnknownTitle", card, "???", 22f, TextAlignmentOptions.Left);
                CreateText("UnknownBody", card, "이 구역을 탐색하면 증거를 수집할 수 있습니다", 18f, TextAlignmentOptions.Left);
                return;
            }

            CreateText("ClueName", card, clueData.clueName, 22f, TextAlignmentOptions.Left);
            CreateText("ClueDescription", card, clueData.description, 18f, TextAlignmentOptions.Left);
            CreateText("ClueMeaning", card, $"의미: {clueData.meaning}", 18f, TextAlignmentOptions.Left);
        }

        private void BuildSuspectCards()
        {
            ClearChildren(suspectContent);

            foreach (PersonInfo person in people)
            {
                RectTransform card = CreatePanel($"Person_{person.name}", suspectContent, GetRoleColor(person.role));
                LayoutElement element = card.gameObject.AddComponent<LayoutElement>();
                element.preferredHeight = 110f;

                VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(16, 16, 12, 12);
                layout.spacing = 4f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;

                CreateText("PersonName", card, $"{person.name} / {person.role}", 22f, TextAlignmentOptions.Left);
                CreateText("PersonDescription", card, person.description, 18f, TextAlignmentOptions.Left);
            }
        }

        private void ShowEvidenceTab()
        {
            if (evidenceTabRoot != null)
            {
                evidenceTabRoot.SetActive(true);
            }

            if (suspectTabRoot != null)
            {
                suspectTabRoot.SetActive(false);
            }
        }

        private void ShowSuspectTab()
        {
            if (evidenceTabRoot != null)
            {
                evidenceTabRoot.SetActive(false);
            }

            if (suspectTabRoot != null)
            {
                suspectTabRoot.SetActive(true);
            }
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string text)
        {
            RectTransform rect = CreatePanel(name, parent, new Color(0.18f, 0.2f, 0.25f, 0.95f));
            LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 160f;
            element.preferredHeight = 34f;

            Button button = rect.gameObject.AddComponent<Button>();
            TextMeshProUGUI label = CreateText("Label", rect, text, 18f, TextAlignmentOptions.Center);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static Color GetRoleColor(string role)
        {
            switch (role)
            {
                case "피해자":
                    return new Color(0.28f, 0.12f, 0.12f, 0.95f);
                case "용의자":
                    return new Color(0.25f, 0.2f, 0.08f, 0.95f);
                case "고인":
                    return new Color(0.13f, 0.13f, 0.18f, 0.95f);
                default:
                    return new Color(0.15f, 0.15f, 0.16f, 0.95f);
            }
        }

        private static void ClearChildren(Transform parent)
        {
            if (parent == null)
            {
                return;
            }

            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        private struct PersonInfo
        {
            public string name;
            public string role;
            public string description;

            public PersonInfo(string name, string role, string description)
            {
                this.name = name;
                this.role = role;
                this.description = description;
            }
        }
    }
}
