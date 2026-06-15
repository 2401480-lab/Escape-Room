using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Room03Mystery
{
    // 출구 문 4자리 전자 조합 자물쇠.
    // 숫자 버튼 OnClick → AppendDigit("7") 식으로 연결, 확인 버튼 → Submit().
    // 정답이면 onUnlocked 발생 (예: 문 열기 + Room03PuzzleManager.SolvePuzzle("keypad")).
    public class KeypadLock : MonoBehaviour
    {
        [Header("정답 코드")]
        [SerializeField] private string correctCode = "7391";

        [Header("표시")]
        [SerializeField] private TextMeshProUGUI display;

        [Header("이벤트")]
        public UnityEvent onUnlocked;   // 정답 입력 시 1회
        public UnityEvent onWrong;      // 오답 시 (흔들림/사운드 등)

        private string _entry = "";
        private bool _unlocked;

        public void AppendDigit(string digit)
        {
            if (_unlocked) return;
            if (_entry.Length >= correctCode.Length) return;
            _entry += digit;
            Refresh();
        }

        public void Clear()
        {
            if (_unlocked) return;
            _entry = "";
            Refresh();
        }

        public void Submit()
        {
            if (_unlocked) return;

            if (_entry == correctCode)
            {
                _unlocked = true;
                if (display != null) display.text = "● ● ● ●";
                onUnlocked?.Invoke();
            }
            else
            {
                onWrong?.Invoke();
                _entry = "";
                Refresh();
            }
        }

        void Refresh()
        {
            if (display != null) display.text = _entry.PadRight(correctCode.Length, '_');
        }
    }
}
