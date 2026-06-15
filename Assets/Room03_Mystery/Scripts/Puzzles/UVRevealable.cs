using UnityEngine;
using UnityEngine.Events;

namespace Room03Mystery
{
    // 자외선을 비춰야만 보이는 숨겨진 표식 (칠판의 뒤 2자리 등).
    // 평소엔 Renderer 가 꺼져 있다가 UVLight.IsOn 이면 드러난다.
    // 처음 드러나는 순간 onFirstRevealed 발생 → 칠판 퍼즐 SolvePuzzle("blackboard") 연결.
    public class UVRevealable : MonoBehaviour
    {
        [Header("드러낼 대상")]
        [Tooltip("UV 켜질 때 보일 Renderer (비우면 자신의 Renderer 사용)")]
        [SerializeField] private Renderer targetRenderer;

        [Header("이벤트")]
        // 플레이어가 UV 로 처음 이 표식을 본 순간 1회 발생
        public UnityEvent onFirstRevealed;

        private bool _everRevealed;

        void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null) targetRenderer.enabled = false;
        }

        void Update()
        {
            bool show = UVLight.IsOn;
            if (targetRenderer != null) targetRenderer.enabled = show;

            if (show && !_everRevealed)
            {
                _everRevealed = true;
                onFirstRevealed?.Invoke();
            }
        }
    }
}
