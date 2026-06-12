using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room02Operating
{
    // 절규의 수술실 진행 상태 관리
    // 구역: 입구 로비 → 복도/대기실 → 병실 → 보관실 → 분장실 → 수술실(엔딩)
    public class RoomGameManager : MonoBehaviour
    {
        public static RoomGameManager Instance { get; private set; }

        [Header("진행 단계")]
        public GamePhase currentPhase = GamePhase.Opening;

        [Header("단서 수집 추적")]
        // 전반전 필수 단서 (입구+병실): 하시호 배경 파악
        [SerializeField] private List<string> lobbyClueIDs;
        // 중반전 필수 단서 (보관실+분장실): 범행 준비 증거
        [SerializeField] private List<string> midClueIDs;
        // 후반전 필수 단서 (수술실): 결정적 증거
        [SerializeField] private List<string> finalClueIDs;

        [Header("추리 팝업 참조")]
        [SerializeField] private DeductionPopup deductionPopup;

        [Header("UI 참조")]
        [SerializeField] private NotebookUI notebookUI;
        [SerializeField] private SuspectSelection suspectSelection;

        [Header("이벤트")]
        public UnityEvent OnRoomCleared;
        public UnityEvent OnRoomFailed;

        private HashSet<string> _collectedClueIDs = new HashSet<string>();
        private bool _deduction1Shown;
        private bool _deduction2Shown;
        private bool _finalDeductionShown;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            StartOpening();
        }

        void StartOpening()
        {
            currentPhase = GamePhase.Opening;
            notebookUI?.OpenNotebook();
        }

        public void CloseOpeningNotebook()
        {
            notebookUI?.CloseNotebook();
            currentPhase = GamePhase.LobbyPhase;
        }

        public void RegisterClueFound(string clueID)
        {
            if (_collectedClueIDs.Contains(clueID)) return;
            _collectedClueIDs.Add(clueID);
            CheckPhaseProgression();
        }

        void CheckPhaseProgression()
        {
            switch (currentPhase)
            {
                case GamePhase.LobbyPhase:
                    if (HasAllClues(lobbyClueIDs)) EnterMidPhase();
                    break;
                case GamePhase.MidPhase:
                    TryShowDeduction1();
                    TryShowDeduction2();
                    if (HasAllClues(midClueIDs)) EnterFinalPhase();
                    break;
                case GamePhase.FinalPhase:
                    TryShowFinalDeduction();
                    break;
            }
        }

        void EnterMidPhase()  => currentPhase = GamePhase.MidPhase;
        void EnterFinalPhase() => currentPhase = GamePhase.FinalPhase;

        // 추리 팝업 #1 — 수술대 아래 숨은 흔적 발견
        void TryShowDeduction1()
        {
            if (_deduction1Shown) return;
            if (_collectedClueIDs.Contains("clue_under_table_dust") &&
                _collectedClueIDs.Contains("clue_shoe_print"))
            {
                _deduction1Shown = true;
                deductionPopup?.Show(
                    "추리 팝업 #1",
                    "수술대 아래에 누군가 숨어 있었던 흔적이 있다. 발버둥 치는 연기를 했던 것인가?"
                );
            }
        }

        // 추리 팝업 #2 — 독약 경로 특정
        void TryShowDeduction2()
        {
            if (_deduction2Shown) return;
            if (_collectedClueIDs.Contains("clue_poison_bottle") &&
                _collectedClueIDs.Contains("clue_glass"))
            {
                _deduction2Shown = true;
                deductionPopup?.Show(
                    "추리 팝업 #2",
                    "독약은 수술실 내부에서 준비되었다. 피해자는 자신이 독을 마신다는 것을 몰랐을 것이다."
                );
            }
        }

        // 최종 추리 팝업 — 운동화+발가락 페인트 자국으로 범인 특정
        void TryShowFinalDeduction()
        {
            if (_finalDeductionShown) return;
            if (HasAllClues(finalClueIDs))
            {
                _finalDeductionShown = true;
                deductionPopup?.Show(
                    "최종 추리 팝업",
                    "운동화 페인트 자국과 발가락 페인트 자국... 수술대 아래에 숨어 있었던 사람은 단 한 명이다.",
                    OnFinalDeductionClosed,
                    isFinal: true
                );
            }
        }

        void OnFinalDeductionClosed()
        {
            currentPhase = GamePhase.SuspectSelection;
            suspectSelection?.Show();
        }

        public void OnSuspectChosen(SuspectType suspect)
        {
            if (suspect == SuspectType.JinSewoong)
            {
                currentPhase = GamePhase.Ending;
                FindObjectOfType<EndingManager>()?.PlayCorrectEnding();
            }
            else
            {
                OnRoomFailed?.Invoke();
                FindObjectOfType<EndingManager>()?.PlayWrongAnswer();
            }
        }

        bool HasAllClues(List<string> requiredIDs)
        {
            foreach (var id in requiredIDs)
                if (!_collectedClueIDs.Contains(id)) return false;
            return true;
        }

        public bool IsClueCollected(string clueID) => _collectedClueIDs.Contains(clueID);
    }

    public enum GamePhase
    {
        Opening,
        LobbyPhase,     // 입구 로비 + 복도/대기실 + 병실
        MidPhase,       // 보관실 + 분장실
        FinalPhase,     // 수술실
        SuspectSelection,
        Ending
    }

    public enum SuspectType
    {
        BongTaehyeon,   // 봉태현
        MoonSumi,       // 문수미
        JinSewoong      // 진세웅 (범인)
    }
}
