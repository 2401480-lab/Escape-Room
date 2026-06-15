using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Room03Mystery
{
    // 시체 안치실(Room03) 전체 진행 상태 관리 — 싱글톤
    // 세 퍼즐(서랍 / 칠판 / 키패드)이 모두 풀리면 방 클리어 처리.
    //   서랍 퍼즐  : 머리 속 황동 열쇠 + 코드 앞 2자리   → SolvePuzzle("drawer")
    //   칠판 퍼즐  : UV 라이트로 뒤 2자리 해독           → SolvePuzzle("blackboard")
    //   키패드     : 4자리 코드 입력 → 탈출              → SolvePuzzle("keypad")
    public class Room03PuzzleManager : MonoBehaviour
    {
        public static Room03PuzzleManager Instance { get; private set; }

        [Header("클리어 조건")]
        [Tooltip("이 개수만큼 SolvePuzzle 이 호출되면 방 클리어")]
        [SerializeField] private int requiredPuzzleCount = 3;

        [Header("이벤트 — Inspector 에서 연결")]
        // 방 클리어 시 발생. RoomLoader.ReturnToRoomSelect() 또는 탈출 연출 UI 등에 연결.
        public UnityEvent OnRoomCleared;

        // 단서 추적 (InteractableObject 가 사용 — Room01 시스템과 동일 역할)
        private readonly HashSet<string> _collectedClues = new HashSet<string>();
        // 퍼즐 해결 추적
        private readonly HashSet<string> _solvedPuzzles = new HashSet<string>();

        private bool _roomCleared;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ─── 단서 ─────────────────────────────────────────────────────

        public void RegisterClueFound(string clueID)
        {
            if (string.IsNullOrEmpty(clueID)) return;
            _collectedClues.Add(clueID);
        }

        public bool IsClueCollected(string clueID) => _collectedClues.Contains(clueID);

        // ─── 퍼즐 ─────────────────────────────────────────────────────

        // 각 퍼즐의 트리거에서 호출. 같은 ID 중복 호출은 무시.
        public void SolvePuzzle(string puzzleID)
        {
            if (_roomCleared) return;
            if (string.IsNullOrEmpty(puzzleID)) return;
            if (!_solvedPuzzles.Add(puzzleID)) return;

            Debug.Log($"[Room03] 퍼즐 해결: {puzzleID} ({_solvedPuzzles.Count}/{requiredPuzzleCount})");

            if (_solvedPuzzles.Count >= requiredPuzzleCount)
                ClearRoom();
        }

        public bool IsPuzzleSolved(string puzzleID) => _solvedPuzzles.Contains(puzzleID);

        // ─── 클리어 ───────────────────────────────────────────────────

        void ClearRoom()
        {
            _roomCleared = true;
            Debug.Log("[Room03] 방 클리어 — 탈출 성공");

            // 전역 클리어 기록 (DontDestroyOnLoad 싱글톤)
            EscapeGame.GameData.Instance?.SetRoomCleared(3);

            // 씬 전환/연출은 Inspector 에서 OnRoomCleared 에 연결
            // (예: RoomLoader.ReturnToRoomSelect)
            OnRoomCleared?.Invoke();
        }
    }
}
