using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom
{
    public class StoryProgressManager : MonoBehaviour
    {
        public static StoryProgressManager Instance { get; private set; }

        [Header("진행 단계")]
        [SerializeField] private StoryPhase currentPhase = StoryPhase.ClueCollection;

        [Header("진범 인식 단서")]
        [SerializeField] private string hashoWillClueID = "clue_hasho_will";
        [SerializeField] private string jinDiaryClueID = "clue_makeup_diary";

        [Header("열쇠 단서")]
        [SerializeField] private string keyClueAID = "key_clue_coldest_place";
        [SerializeField] private string keyClueBID = "key_clue_temperature_warning";
        [SerializeField] private string keyClueCID = "key_clue_fridge_scratches";

        [Header("타이머")]
        [SerializeField] private bool useDeductionTimer = true;
        [SerializeField] private float deductionTimer = 900f;

        [Header("이벤트")]
        public UnityEvent<StoryPhase> OnPhaseChanged;
        public UnityEvent OnTrueCulpritRevealed;
        public UnityEvent OnEscapeKeyReady;
        public UnityEvent OnEscapeKeyCollected;
        public UnityEvent OnDeductionTimerExpired;

        private readonly HashSet<string> collectedClueIDs = new HashSet<string>();
        private bool hasEscapeKey;
        private float deductionTimeRemaining;
        private ChaseController chaseController;

        public StoryPhase CurrentPhase => currentPhase;
        public bool HasEscapeKey => hasEscapeKey;
        public float DeductionTimeRemaining => deductionTimeRemaining;
        public bool IsChaseTimerActive => currentPhase == StoryPhase.ChaseEscape;
        public float CurrentTimerRemaining => IsChaseTimerActive && chaseController != null
            ? chaseController.ChaseTimeRemaining
            : deductionTimeRemaining;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            deductionTimeRemaining = deductionTimer;
        }

        private void OnEnable()
        {
            if (ClueJournalManager.Instance != null)
            {
                ClueJournalManager.Instance.OnClueAdded += HandleClueAdded;
            }
        }

        private void OnDisable()
        {
            if (ClueJournalManager.Instance != null)
            {
                ClueJournalManager.Instance.OnClueAdded -= HandleClueAdded;
            }
        }

        private void Update()
        {
            if (!useDeductionTimer || currentPhase == StoryPhase.ChaseEscape || currentPhase == StoryPhase.GameOver || hasEscapeKey)
            {
                return;
            }

            deductionTimeRemaining -= Time.deltaTime;
            if (deductionTimeRemaining <= 0f)
            {
                deductionTimeRemaining = 0f;
                DeductionTimerExpired();
            }
        }

        public void HandleClueAdded(ClueData clueData)
        {
            if (clueData == null)
            {
                return;
            }

            string id = GetClueID(clueData);
            if (string.IsNullOrEmpty(id))
            {
                return;
            }

            collectedClueIDs.Add(id);
            EvaluateProgress();
        }

        public bool HasAllKeyClues()
        {
            return collectedClueIDs.Contains(keyClueAID) &&
                   collectedClueIDs.Contains(keyClueBID) &&
                   collectedClueIDs.Contains(keyClueCID);
        }

        public void CollectEscapeKey()
        {
            if (!HasAllKeyClues() || hasEscapeKey)
            {
                return;
            }

            hasEscapeKey = true;
            SetPhase(StoryPhase.SuspectSelection);
            OnEscapeKeyCollected?.Invoke();
        }

        public void BeginSuspectSelection()
        {
            if (hasEscapeKey)
            {
                SetPhase(StoryPhase.SuspectSelection);
            }
        }

        public void BeginChase()
        {
            SetPhase(StoryPhase.ChaseEscape);
        }

        public void RegisterChaseController(ChaseController controller)
        {
            if (controller != null)
            {
                chaseController = controller;
            }
        }

        public void MarkGameOver()
        {
            SetPhase(StoryPhase.GameOver);
        }

        public void MarkEscaped()
        {
            SetPhase(StoryPhase.Escaped);
        }

        public bool IsKeyClueID(string clueID)
        {
            return clueID == keyClueAID || clueID == keyClueBID || clueID == keyClueCID;
        }

        private void EvaluateProgress()
        {
            if (currentPhase == StoryPhase.ClueCollection &&
                collectedClueIDs.Contains(hashoWillClueID) &&
                collectedClueIDs.Contains(jinDiaryClueID))
            {
                SetPhase(StoryPhase.TrueCulpritRevealed);
                OnTrueCulpritRevealed?.Invoke();
            }

            if ((currentPhase == StoryPhase.ClueCollection || currentPhase == StoryPhase.TrueCulpritRevealed) &&
                HasAnyKeyClue())
            {
                SetPhase(StoryPhase.KeyClueCollection);
            }

            if (HasAllKeyClues())
            {
                OnEscapeKeyReady?.Invoke();
            }
        }

        private bool HasAnyKeyClue()
        {
            return collectedClueIDs.Contains(keyClueAID) ||
                   collectedClueIDs.Contains(keyClueBID) ||
                   collectedClueIDs.Contains(keyClueCID);
        }

        private void DeductionTimerExpired()
        {
            OnDeductionTimerExpired?.Invoke();
            MarkGameOver();
            GameOverUI.Instance?.PlayGameOver(GameOverReason.DeductionTimerExpired);
        }

        private void SetPhase(StoryPhase nextPhase)
        {
            if (currentPhase == nextPhase)
            {
                return;
            }

            currentPhase = nextPhase;
            OnPhaseChanged?.Invoke(currentPhase);
        }

        private static string GetClueID(ClueData clueData)
        {
            if (!string.IsNullOrWhiteSpace(clueData.clueID))
            {
                return clueData.clueID;
            }

            return clueData.name;
        }
    }

    public enum StoryPhase
    {
        ClueCollection,
        TrueCulpritRevealed,
        KeyClueCollection,
        SuspectSelection,
        ChaseEscape,
        GameOver,
        Escaped
    }
}
