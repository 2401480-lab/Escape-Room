using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room02Operating
{
    // 절규의 수술실 진행 상태 관리
    // 흐름: 사건 파악 -> 봉태현 미스디렉션 -> 진세웅 동기 -> 물증 연결 -> 범인 선택
    public class RoomGameManager : MonoBehaviour
    {
        public static RoomGameManager Instance { get; private set; }

        [Header("진행 단계")]
        public GamePhase currentPhase = GamePhase.Opening;

        [Header("사건 파악 단서")]
        [SerializeField] private List<string> storyClueIDs = new List<string>
        {
            "clue_cast_notice",
            "clue_memorial_frame",
            "clue_visitor_log",
            "clue_security_log",
            "clue_hasho_will",
            "clue_ward_calendar"
        };

        [Header("봉태현 미스디렉션 단서")]
        [SerializeField] private List<string> misdirectionClueIDs = new List<string>
        {
            "clue_yoanna_note",
            "clue_nurse_log",
            "clue_medical_certificate",
            "clue_conversation_memo_a",
            "clue_isolation_bloodstain",
            "clue_bong_rebuttal"
        };

        [Header("진세웅 동기 단서")]
        [SerializeField] private List<string> motiveClueIDs = new List<string>
        {
            "clue_torn_letter_piece_a",
            "clue_torn_letter_piece_b",
            "clue_cctv_memo",
            "clue_phone_memo",
            "clue_sumi_memo",
            "clue_makeup_diary",
            "clue_mirror_message"
        };

        [Header("범행 물증 단서")]
        [SerializeField] private List<string> evidenceClueIDs = new List<string>
        {
            "clue_poison_ampoule",
            "clue_hidden_camera",
            "clue_jin_sneakers",
            "clue_gloves",
            "clue_locked_locker",
            "clue_paint_footprints",
            "clue_paint_toolbox"
        };

        [Header("수술실 최종 단서")]
        [SerializeField] private List<string> finalClueIDs = new List<string>
        {
            "clue_under_table_space",
            "clue_yoanna_relic"
        };

        [Header("추리 팝업 참조")]
        [SerializeField] private DeductionPopup deductionPopup;

        [Header("UI 참조")]
        [SerializeField] private NotebookUI notebookUI;
        [SerializeField] private SuspectSelection suspectSelection;

        [Header("이벤트")]
        public UnityEvent OnRoomCleared;
        public UnityEvent OnRoomFailed;

        private readonly HashSet<string> _collectedClueIDs = new HashSet<string>();
        private bool _storyPopupShown;
        private bool _misdirectionPopupShown;
        private bool _motivePopupShown;
        private bool _evidencePopupShown;
        private bool _finalDeductionShown;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            StartOpening();
        }

        private void StartOpening()
        {
            currentPhase = GamePhase.Opening;
            notebookUI?.OpenNotebook();
        }

        public void CloseOpeningNotebook()
        {
            notebookUI?.CloseNotebook();
            currentPhase = GamePhase.StoryPhase;
        }

        public void RegisterClueFound(string clueID)
        {
            if (string.IsNullOrEmpty(clueID) || _collectedClueIDs.Contains(clueID))
            {
                return;
            }

            _collectedClueIDs.Add(clueID);
            CheckPhaseProgression();
        }

        private void CheckPhaseProgression()
        {
            TryShowStoryPopup();
            TryShowMisdirectionPopup();
            TryShowMotivePopup();
            TryShowEvidencePopup();

            switch (currentPhase)
            {
                case GamePhase.StoryPhase:
                    if (HasAllClues(storyClueIDs))
                    {
                        currentPhase = GamePhase.MisdirectionPhase;
                    }
                    break;
                case GamePhase.MisdirectionPhase:
                    if (HasAllClues(misdirectionClueIDs))
                    {
                        currentPhase = GamePhase.MotivePhase;
                    }
                    break;
                case GamePhase.MotivePhase:
                    if (HasAllClues(motiveClueIDs))
                    {
                        currentPhase = GamePhase.EvidencePhase;
                    }
                    break;
                case GamePhase.EvidencePhase:
                    if (HasAllClues(evidenceClueIDs))
                    {
                        currentPhase = GamePhase.FinalPhase;
                    }
                    break;
                case GamePhase.FinalPhase:
                    TryShowFinalDeduction();
                    break;
            }

            if (currentPhase == GamePhase.FinalPhase)
            {
                TryShowFinalDeduction();
            }
        }

        private void TryShowStoryPopup()
        {
            if (_storyPopupShown || !HasAllClues(storyClueIDs))
            {
                return;
            }

            _storyPopupShown = true;
            deductionPopup?.Show(
                "사건 파악",
                "하시호는 단순한 사고로 죽은 것이 아니다. 2년 전 수술과 오늘 공연은 같은 장소에서 다시 이어지고 있다."
            );
        }

        private void TryShowMisdirectionPopup()
        {
            if (_misdirectionPopupShown)
            {
                return;
            }

            if (_collectedClueIDs.Contains("clue_medical_certificate") &&
                _collectedClueIDs.Contains("clue_conversation_memo_a") &&
                _collectedClueIDs.Contains("clue_bong_rebuttal"))
            {
                _misdirectionPopupShown = true;
                deductionPopup?.Show(
                    "봉태현 의심",
                    "진단서와 유안나의 메모는 봉태현을 가리킨다. 하지만 반박문은 그가 무언가를 숨겼다기보다 누명을 두려워했다는 쪽에 가깝다."
                );
            }
        }

        private void TryShowMotivePopup()
        {
            if (_motivePopupShown)
            {
                return;
            }

            if (_collectedClueIDs.Contains("clue_torn_letter_piece_a") &&
                _collectedClueIDs.Contains("clue_torn_letter_piece_b") &&
                _collectedClueIDs.Contains("clue_makeup_diary"))
            {
                _motivePopupShown = true;
                deductionPopup?.Show(
                    "진세웅의 동기",
                    "찢긴 편지와 분장 일기장은 같은 방향을 가리킨다. 진세웅은 유안나가 하시호를 죽음으로 몰았다고 믿고 있었다."
                );
            }
        }

        private void TryShowEvidencePopup()
        {
            if (_evidencePopupShown)
            {
                return;
            }

            if (_collectedClueIDs.Contains("clue_poison_ampoule") &&
                _collectedClueIDs.Contains("clue_jin_sneakers") &&
                _collectedClueIDs.Contains("clue_paint_footprints") &&
                _collectedClueIDs.Contains("clue_under_table_space"))
            {
                _evidencePopupShown = true;
                deductionPopup?.Show(
                    "물증 연결",
                    "독약, 운동화, 페인트 발자국, 수술대 아래 공간이 하나로 이어진다. 범인은 먼저 숨어 있다가 유안나를 기다렸다."
                );
            }
        }

        private void TryShowFinalDeduction()
        {
            if (_finalDeductionShown || !HasAllClues(finalClueIDs))
            {
                return;
            }

            _finalDeductionShown = true;
            deductionPopup?.Show(
                "최종 추리",
                "수술대 아래 숨을 수 있는 공간과 유안나의 유품이 범행 과정을 완성한다. 이제 범인을 선택해야 한다.",
                OnFinalDeductionClosed,
                isFinal: true
            );
        }

        private void OnFinalDeductionClosed()
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

        private bool HasAllClues(List<string> requiredIDs)
        {
            foreach (string id in requiredIDs)
            {
                if (!_collectedClueIDs.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        public bool IsClueCollected(string clueID)
        {
            return _collectedClueIDs.Contains(clueID);
        }
    }

    public enum GamePhase
    {
        Opening,
        StoryPhase,
        MisdirectionPhase,
        MotivePhase,
        EvidencePhase,
        FinalPhase,
        SuspectSelection,
        Ending
    }

    public enum SuspectType
    {
        BongTaehyeon,
        MoonSumi,
        JinSewoong
    }
}
