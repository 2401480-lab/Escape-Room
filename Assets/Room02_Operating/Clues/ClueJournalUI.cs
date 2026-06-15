using System;
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
        [SerializeField] private ScrollRect chipScrollRect;
        [SerializeField] private RectTransform chipContainer;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private ScrollRect evidenceScrollRect;
        [SerializeField] private RectTransform evidenceContent;
        [SerializeField] private GameObject evidenceTabRoot;
        [SerializeField] private GameObject suspectTabRoot;
        [SerializeField] private ScrollRect suspectScrollRect;
        [SerializeField] private RectTransform suspectContent;

        private const string NotebookPrefix = "수첩:";
        private const string CommonTarget = "공통";
        private const string CommonNotebookName = "사건 공통";

        private readonly Dictionary<ClueData, RectTransform> clueCards = new Dictionary<ClueData, RectTransform>();
        private readonly List<PersonInfo> people = new List<PersonInfo>
        {
            new PersonInfo("유안나", "피해자", "수술실에서 독살된 동아리 부원. 하시호의 죽음과 연결되어 있다."),
            new PersonInfo("진세웅", "용의자", "하시호의 복수를 위해 행사를 설계한 인물. 진범."),
            new PersonInfo("봉태현", "용의자", "하시호 담당의였던 인물. 초반 의심을 받지만 수술실 입장을 양보했다."),
            new PersonInfo("문수미", "용의자", "진세웅의 이상한 낌새를 눈치챈 인물."),
            new PersonInfo("하시호", "고인", "한 달 전 사망한 동아리 부원. 사건의 발단.", true),
            new PersonInfo(CommonNotebookName, CommonTarget, "인물 하나로 묶이지 않는 사건의 공통 단서.")
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
                ToggleEvidencePanel();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                ToggleSuspectPanel();
            }
        }

        public void Toggle()
        {
            ToggleEvidencePanel();
        }

        public void ToggleEvidencePanel()
        {
            bool shouldOpen = panelRoot == null || !panelRoot.activeSelf || !evidenceTabRoot.activeSelf;
            OpenEvidencePanel();
            SetOpen(shouldOpen);
        }

        public void ToggleSuspectPanel()
        {
            bool shouldOpen = panelRoot == null || !panelRoot.activeSelf || !suspectTabRoot.activeSelf;
            OpenSuspectPanel();
            SetOpen(shouldOpen);
        }

        public void OpenEvidencePanel()
        {
            EnsureUI();
            ShowEvidenceTab();
            SetOpen(true);
        }

        public void OpenSuspectPanel()
        {
            EnsureUI();
            ShowSuspectTab();
            SetOpen(true);
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

            OpenEvidencePanel();
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
            journalCanvas = EnsureHudCanvas();

            if (panelRoot != null)
            {
                return;
            }

            CreateHudButtons(journalCanvas.transform);

            panelRoot = CreatePanel("ClueJournalPanel", journalCanvas.transform, HorrorUITheme.PanelBlack).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(860f, 620f);
            panelRect.anchoredPosition = Vector2.zero;

            CreateHeader(panelRect);
            CreateChipBar(panelRect);
            CreateTabButtons(panelRect);
            CreateEvidenceTab(panelRect);
            CreateSuspectTab(panelRect);
        }

        private void CreateHudButtons(Transform parent)
        {
            RectTransform buttonBar = CreatePanel("HudLeftButtonBar", parent, new Color(0f, 0f, 0f, 0f));
            buttonBar.anchorMin = new Vector2(0f, 1f);
            buttonBar.anchorMax = new Vector2(0f, 1f);
            buttonBar.pivot = new Vector2(0f, 1f);
            buttonBar.anchoredPosition = new Vector2(24f, -24f);
            buttonBar.sizeDelta = new Vector2(300f, 44f);

            HorizontalLayoutGroup layout = buttonBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.spacing = 8f;

            Button evidenceButton = CreateButton("EvidenceHudButton", buttonBar, "수사 노트 (J)");
            evidenceButton.onClick.AddListener(ToggleEvidencePanel);

            Button suspectButton = CreateButton("SuspectHudButton", buttonBar, "용의자 (K)");
            suspectButton.onClick.AddListener(ToggleSuspectPanel);
        }

        private void CreateHeader(RectTransform parent)
        {
            TextMeshProUGUI title = CreateText("JournalTitle", parent, "수집 증거 기록", 32f, TextAlignmentOptions.Left);
            title.color = HorrorUITheme.BloodRed;
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.offsetMin = new Vector2(32f, -74f);
            titleRect.offsetMax = new Vector2(-32f, -24f);

            progressText = CreateText("ProgressText", parent, "수집 단서 0 / 0", 20f, TextAlignmentOptions.Right);
            progressText.color = HorrorUITheme.TextDim;
            RectTransform progressRect = progressText.rectTransform;
            progressRect.anchorMin = new Vector2(0f, 1f);
            progressRect.anchorMax = new Vector2(1f, 1f);
            progressRect.pivot = new Vector2(0.5f, 1f);
            progressRect.offsetMin = new Vector2(32f, -74f);
            progressRect.offsetMax = new Vector2(-32f, -24f);
        }

        private void CreateChipBar(RectTransform parent)
        {
            RectTransform bar = CreatePanel("EvidenceChipBar", parent, HorrorUITheme.PanelDeep);
            bar.anchorMin = new Vector2(0f, 1f);
            bar.anchorMax = new Vector2(1f, 1f);
            bar.pivot = new Vector2(0.5f, 1f);
            bar.offsetMin = new Vector2(32f, -126f);
            bar.offsetMax = new Vector2(-32f, -82f);

            chipScrollRect = bar.gameObject.AddComponent<ScrollRect>();
            chipScrollRect.horizontal = true;
            chipScrollRect.vertical = false;

            RectTransform viewport = CreatePanel("EvidenceChipViewport", bar, new Color(0f, 0f, 0f, 0f));
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            chipContainer = CreatePanel("EvidenceChipContent", viewport, new Color(0f, 0f, 0f, 0f));
            chipContainer.anchorMin = new Vector2(0f, 0f);
            chipContainer.anchorMax = new Vector2(0f, 1f);
            chipContainer.pivot = new Vector2(0f, 0.5f);
            chipContainer.anchoredPosition = Vector2.zero;
            chipContainer.sizeDelta = Vector2.zero;

            HorizontalLayoutGroup layout = chipContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = false;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.spacing = 8f;

            ContentSizeFitter fitter = chipContainer.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            chipScrollRect.viewport = viewport;
            chipScrollRect.content = chipContainer;
        }

        private void CreateTabButtons(RectTransform parent)
        {
            RectTransform tabBar = CreatePanel("JournalTabBar", parent, HorrorUITheme.PanelDeep);
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
            layout.padding = new RectOffset(0, 8, 0, 0);
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

            suspectScrollRect = suspectTabRoot.AddComponent<ScrollRect>();
            suspectScrollRect.horizontal = false;

            RectTransform viewport = CreatePanel("SuspectViewport", rootRect, new Color(0f, 0f, 0f, 0f));
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            Mask mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            suspectContent = CreatePanel("SuspectContent", viewport, new Color(0f, 0f, 0f, 0f));
            suspectContent.anchorMin = new Vector2(0f, 1f);
            suspectContent.anchorMax = new Vector2(1f, 1f);
            suspectContent.pivot = new Vector2(0.5f, 1f);
            suspectContent.offsetMin = Vector2.zero;
            suspectContent.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = suspectContent.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(0, 8, 0, 0);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 12f;

            ContentSizeFitter fitter = suspectContent.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            suspectScrollRect.viewport = viewport;
            suspectScrollRect.content = suspectContent;

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
            BuildSuspectCards();
        }

        private void BuildEvidenceChips()
        {
            ClearChildren(chipContainer);

            foreach (ClueData clueData in journalManager.CollectedClues)
            {
                ClueData captured = clueData;
                Button chip = CreateButton($"Chip_{clueData.clueName}", chipContainer, clueData.clueName);
                LayoutElement element = chip.GetComponent<LayoutElement>();
                if (element != null)
                {
                    element.preferredWidth = Mathf.Clamp(64f + clueData.clueName.Length * 14f, 132f, 240f);
                }

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
            progressText.text = $"수집 단서 {collectedCount} / {totalCount}";

            Dictionary<string, List<ClueData>> grouped = new Dictionary<string, List<ClueData>>();
            List<ClueData> keyClueSection = new List<ClueData>();
            foreach (ClueData clueData in allClues)
            {
                if (clueData.category == ClueCategory.KeyClue)
                {
                    keyClueSection.Add(clueData);
                    continue;
                }

                string area = GetAreaDisplayName(clueData.areaName);
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
            header.color = HorrorUITheme.SickYellow;
            LayoutElement element = header.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 36f;
            element.preferredHeight = 36f;
        }

        private void CreateClueCard(ClueData clueData, bool discovered)
        {
            RectTransform card = CreatePanel($"Card_{clueData.clueName}", evidenceContent, HorrorUITheme.PanelDeep);
            LayoutElement element = card.gameObject.AddComponent<LayoutElement>();
            element.minHeight = discovered ? 196f : 112f;
            element.preferredHeight = discovered ? 196f : 112f;
            clueCards[clueData] = card;

            VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(18, 18, 14, 14);
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;

            if (!discovered)
            {
                TextMeshProUGUI unknownTitle = CreateTextBlock("UnknownTitle", card, $"{GetAreaDisplayName(clueData.areaName)} 노트 - ???", 21f, TextAlignmentOptions.Left, 34f);
                unknownTitle.color = HorrorUITheme.TextDim;
                TextMeshProUGUI unknownBody = CreateTextBlock("UnknownBody", card, "이 구역을 탐색하면 증거를 수집할 수 있습니다", 17f, TextAlignmentOptions.Left, 40f);
                unknownBody.color = HorrorUITheme.TextDim;
                return;
            }

            TextMeshProUGUI nameText = CreateTextBlock("ClueName", card, CreateClueCardTitle(clueData), 21f, TextAlignmentOptions.Left, 34f);
            nameText.color = HorrorUITheme.BloodRed;
            CreateTextBlock("ClueDescription", card, $"단서 내용: {clueData.description}", 17f, TextAlignmentOptions.Left, 66f);
            TextMeshProUGUI meaningText = CreateTextBlock("ClueMeaning", card, $"수첩 업데이트: {GetNotebookHintText(clueData)}", 17f, TextAlignmentOptions.Left, 54f);
            meaningText.color = HorrorUITheme.SickYellow;
        }

        private void BuildSuspectCards()
        {
            ClearChildren(suspectContent);
            if (suspectContent == null || journalManager == null)
            {
                return;
            }

            foreach (PersonInfo person in people)
            {
                if (!ShouldShowPerson(person))
                {
                    continue;
                }

                List<string> hints = GetCollectedHintsForPerson(person);
                RectTransform card = CreatePanel($"Person_{person.name}", suspectContent, GetRoleColor(person.role));
                LayoutElement element = card.gameObject.AddComponent<LayoutElement>();
                float cardHeight = Mathf.Max(166f, 128f + Mathf.Max(1, hints.Count) * 44f);
                element.minHeight = cardHeight;
                element.preferredHeight = cardHeight;

                VerticalLayoutGroup layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.padding = new RectOffset(18, 18, 14, 14);
                layout.spacing = 8f;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandHeight = false;

                CreateTextBlock("PersonName", card, $"{person.name} / {person.role}", 21f, TextAlignmentOptions.Left, 32f);
                CreateTextBlock("PersonDescription", card, person.description, 17f, TextAlignmentOptions.Left, 42f);

                TextMeshProUGUI hintHeader = CreateTextBlock("SuspectHintHeader", card, "수집된 단서 힌트", 17f, TextAlignmentOptions.Left, 28f);
                hintHeader.color = HorrorUITheme.SickYellow;

                if (hints.Count == 0)
                {
                    TextMeshProUGUI empty = CreateTextBlock("SuspectHintEmpty", card, "아직 확인된 힌트 없음", 16f, TextAlignmentOptions.Left, 28f);
                    empty.color = HorrorUITheme.TextDim;
                    continue;
                }

                foreach (string hint in hints)
                {
                    CreateTextBlock("SuspectHint", card, $"- {hint}", 16f, TextAlignmentOptions.Left, 40f);
                }
            }
        }

        private bool ShouldShowPerson(PersonInfo person)
        {
            return !person.revealAfterHashihoClue || HasHashihoClue();
        }

        private bool HasHashihoClue()
        {
            if (journalManager == null)
            {
                return false;
            }

            foreach (ClueData clueData in journalManager.CollectedClues)
            {
                if (IsHashihoClue(clueData))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsHashihoClue(ClueData clueData)
        {
            if (clueData == null)
            {
                return false;
            }

            string clueID = clueData.clueID;
            if (clueID == "normal_memorial_frame" ||
                clueID == "clue_hasho_will" ||
                clueID == "normal_medical_certificate")
            {
                return true;
            }

            string clueText = $"{clueData.clueName} {clueData.description} {clueData.meaning}";
            return clueText.Contains("하시호");
        }

        private List<string> GetCollectedHintsForPerson(PersonInfo person)
        {
            List<string> hints = new List<string>();
            foreach (ClueData clueData in journalManager.CollectedClues)
            {
                if (!TryParseNotebookHint(clueData, out List<string> targets, out string hint))
                {
                    continue;
                }

                if (TargetsPerson(targets, person) || ClueMentionsPerson(clueData, person.name))
                {
                    hints.Add($"{clueData.clueName}: {hint}");
                }
            }

            return hints;
        }

        private static bool TryParseNotebookHint(ClueData clueData, out List<string> targets, out string hint)
        {
            targets = new List<string>();
            hint = string.Empty;
            if (clueData == null || string.IsNullOrWhiteSpace(clueData.meaning))
            {
                return false;
            }

            string meaning = clueData.meaning.Trim();
            if (!meaning.StartsWith(NotebookPrefix, StringComparison.Ordinal))
            {
                hint = meaning;
                targets.Add(CommonTarget);
                return true;
            }

            string body = meaning.Substring(NotebookPrefix.Length).Trim();
            int separator = body.IndexOf('—');
            if (separator < 0)
            {
                targets.Add(CommonTarget);
                hint = body;
                return !string.IsNullOrWhiteSpace(hint);
            }

            string targetText = body.Substring(0, separator).Trim();
            hint = body.Substring(separator + 1).Trim();
            foreach (string target in targetText.Split(new[] { '/', ',', '·' }, StringSplitOptions.RemoveEmptyEntries))
            {
                targets.Add(target.Trim());
            }

            if (targets.Count == 0)
            {
                targets.Add(CommonTarget);
            }

            return !string.IsNullOrWhiteSpace(hint);
        }

        private static bool TargetsPerson(List<string> targets, PersonInfo person)
        {
            foreach (string target in targets)
            {
                if (target == CommonTarget && person.name == CommonNotebookName)
                {
                    return true;
                }

                if (target == person.name)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ClueMentionsPerson(ClueData clueData, string personName)
        {
            if (clueData == null || string.IsNullOrWhiteSpace(personName) || personName == CommonNotebookName)
            {
                return false;
            }

            string clueText = $"{clueData.clueName} {clueData.description} {clueData.meaning}";
            return clueText.Contains(personName);
        }

        private static string GetNotebookHintText(ClueData clueData)
        {
            return TryParseNotebookHint(clueData, out _, out string hint) ? hint : clueData.meaning;
        }

        private static string CreateClueCardTitle(ClueData clueData)
        {
            string areaName = clueData != null ? GetAreaDisplayName(clueData.areaName) : "미확인 구역";
            string clueName = clueData != null && !string.IsNullOrWhiteSpace(clueData.clueName) ? clueData.clueName : "???";
            return $"{areaName} 노트 - {clueName}";
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
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        private static TextMeshProUGUI CreateTextBlock(string name, Transform parent, string text, float fontSize, TextAlignmentOptions alignment, float preferredHeight)
        {
            TextMeshProUGUI tmp = CreateText(name, parent, text, fontSize, alignment);
            LayoutElement element = tmp.gameObject.AddComponent<LayoutElement>();
            element.minHeight = preferredHeight;
            element.preferredHeight = preferredHeight;
            return tmp;
        }

        private static Button CreateButton(string name, Transform parent, string text)
        {
            RectTransform rect = CreatePanel(name, parent, HorrorUITheme.PanelRed);
            LayoutElement element = rect.gameObject.AddComponent<LayoutElement>();
            element.preferredWidth = 132f;
            element.preferredHeight = 38f;

            Button button = rect.gameObject.AddComponent<Button>();
            HorrorUITheme.ApplyButton(button, rect.GetComponent<Image>());
            TextMeshProUGUI label = CreateText("Label", rect, text, 17f, TextAlignmentOptions.Center);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static string GetAreaDisplayName(string areaName)
        {
            switch (areaName)
            {
                case "Hallway":
                    return "복도";
                case "Ward":
                    return "병실";
                case "Storage":
                    return "보관실";
                case "DressingRoom":
                    return "분장실";
                case "OperatingRoom":
                    return "수술실";
                default:
                    return string.IsNullOrWhiteSpace(areaName) ? "미확인 구역" : areaName;
            }
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
                case CommonTarget:
                    return new Color(0.12f, 0.16f, 0.16f, 0.95f);
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
            public bool revealAfterHashihoClue;

            public PersonInfo(string name, string role, string description, bool revealAfterHashihoClue = false)
            {
                this.name = name;
                this.role = role;
                this.description = description;
                this.revealAfterHashihoClue = revealAfterHashihoClue;
            }
        }
    }
}
