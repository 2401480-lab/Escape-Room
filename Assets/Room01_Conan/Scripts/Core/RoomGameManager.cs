using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room01Conan
{
    // 이 방(코난 외교관 사건) 전체 진행 상태를 관리
    // 팀원 통합 시: OnRoomCleared 이벤트에 EscapeGame.RoomLoader.ReturnToRoomSelect() 연결
    public class RoomGameManager : MonoBehaviour
    {
        public static RoomGameManager Instance { get; private set; }

        [Header("진행 단계")]
        public GamePhase currentPhase = GamePhase.Opening;

        [Header("단서 수집 추적")]
        // Inspector에서 각 단서 ID를 순서대로 등록
        // 단서 ①~③ 조사 완료 → 중반전, ④~⑦ 완료 → 후반전
        [SerializeField] private List<string> halfTimeClueIDs;   // 전반전 필수 단서
        [SerializeField] private List<string> fullTimeClueIDs;   // 중반전 필수 단서

        [Header("추리 팝업 참조")]
        [SerializeField] private DeductionPopup deductionPopup;

        [Header("UI 참조")]
        [SerializeField] private NotebookUI notebookUI;
        [SerializeField] private SuspectSelection suspectSelection;

        [Header("이벤트 — 팀원 통합 시 여기에 연결")]
        public UnityEvent OnRoomCleared;   // 정답 선택 후 방 클리어 시 발생
        public UnityEvent OnRoomFailed;    // 오답 시 발생 (현재 미정 — 빈 이벤트)

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

        // ─── 단계 진입 ───────────────────────────────────────────────

        void StartOpening()
        {
            currentPhase = GamePhase.Opening;
            notebookUI?.OpenNotebook();
        }

        public void CloseOpeningNotebook()
        {
            notebookUI?.CloseNotebook();
            currentPhase = GamePhase.ExamineBody;
        }

        // ─── 단서 획득 처리 ──────────────────────────────────────────

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
                case GamePhase.ExamineBody:
                    if (HasAllClues(halfTimeClueIDs)) EnterMiddlePhase();
                    break;
                case GamePhase.ExamineStudy:
                    TryShowDeduction1();
                    TryShowDeduction2();
                    if (HasAllClues(fullTimeClueIDs)) EnterLatePhase();
                    break;
                case GamePhase.LatePhase:
                    TryShowFinalDeduction();
                    break;
            }
        }

        void EnterMiddlePhase() => currentPhase = GamePhase.ExamineStudy;
        void EnterLatePhase()   => currentPhase = GamePhase.LatePhase;

        // ─── 추리 팝업 ────────────────────────────────────────────────

        void TryShowDeduction1()
        {
            if (_deduction1Shown) return;
            if (_collectedClueIDs.Contains("clue_key_tape") && _collectedClueIDs.Contains("clue_doorframe"))
            {
                _deduction1Shown = true;
                deductionPopup?.Show(
                    "🧠 추리 팝업 #1",
                    "범인은 어떤 것으로 열쇠를 조작해 문을 잠근 뒤 빠져나왔다. 하지만 어떤 것을 이용해서?"
                );
            }
        }

        void TryShowDeduction2()
        {
            if (_deduction2Shown) return;
            if (_collectedClueIDs.Contains("clue_camera") && _collectedClueIDs.Contains("clue_bookshelf_gap"))
            {
                _deduction2Shown = true;
                deductionPopup?.Show(
                    "🧠 추리 팝업 #2",
                    "범인은 책으로 카메라를 가리고 잠든 피해자의 얼굴도 가린 채 범행을 저질렀다. 피해자는 저항도 못 했을 것이다."
                );
            }
        }

        void TryShowFinalDeduction()
        {
            if (_finalDeductionShown) return;
            if (HasAllClues(fullTimeClueIDs))
            {
                _finalDeductionShown = true;
                deductionPopup?.Show(
                    "🧠 최종 추리 팝업",
                    "줄을 정교하게 다뤄 열쇠를 주머니 속으로 빼낼 수 있는 사람... 범인은 한 명뿐이다.",
                    OnFinalDeductionClosed
                );
            }
        }

        void OnFinalDeductionClosed()
        {
            currentPhase = GamePhase.SuspectSelection;
            suspectSelection?.Show();
        }

        // ─── 용의자 선택 결과 ─────────────────────────────────────────

        public void OnSuspectChosen(SuspectType suspect)
        {
            if (suspect == SuspectType.Grandfather)
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

        // ─── 유틸 ─────────────────────────────────────────────────────

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
        ExamineBody,
        ExamineStudy,
        LatePhase,
        SuspectSelection,
        Ending
    }

    public enum SuspectType
    {
        Wife,
        Secretary,
        Grandfather
    }
}
