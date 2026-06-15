using System.Collections;
using UnityEngine;

namespace Room03Mystery
{
    // 출구 문 — Open() 호출 시 경첩(Y축) 기준으로 회전하며 열린다.
    // KeypadLock.onUnlocked 에 연결해 사용.
    public class DoorSwing : MonoBehaviour
    {
        [SerializeField] private float openAngle = 100f;
        [SerializeField] private float duration = 1.5f;

        private bool _opened;

        public void Open()
        {
            if (_opened) return;
            _opened = true;
            StartCoroutine(SwingRoutine());
        }

        IEnumerator SwingRoutine()
        {
            Quaternion start = transform.localRotation;
            Quaternion end = start * Quaternion.Euler(0f, openAngle, 0f);
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / duration);
                transform.localRotation = Quaternion.Slerp(start, end, k);
                yield return null;
            }
            transform.localRotation = end;
        }
    }
}
