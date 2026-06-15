using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Room03Mystery
{
    // 시체 보관함 서랍 — 호출되면 시체 트레이가 localPosition 기준으로 슬라이드되어 나온다.
    // InteractableObject.onInspected 에서 Open() 을 연결해 사용.
    public class DrawerSlide : MonoBehaviour
    {
        [Header("슬라이드 설정")]
        [Tooltip("닫힌 위치 기준 이동량 (보통 보관함이 당겨지는 방향, 예: 0,0,1.2)")]
        [SerializeField] private Vector3 slideOffset = new Vector3(0f, 0f, 1.2f);
        [SerializeField] private float slideDuration = 1.2f;

        [Header("열린 뒤 동작")]
        // 트레이가 다 나온 뒤 발생 — 예: 시체 머리 InteractableObject 활성화
        public UnityEvent onOpened;

        private Vector3 _closedPos;
        private bool _opened;

        void Awake() => _closedPos = transform.localPosition;

        public void Open()
        {
            if (_opened) return;
            _opened = true;
            StartCoroutine(SlideRoutine(_closedPos + slideOffset));
        }

        IEnumerator SlideRoutine(Vector3 target)
        {
            Vector3 start = transform.localPosition;
            float t = 0f;
            while (t < slideDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / slideDuration);
                transform.localPosition = Vector3.Lerp(start, target, k);
                yield return null;
            }
            transform.localPosition = target;
            onOpened?.Invoke();
        }
    }
}
