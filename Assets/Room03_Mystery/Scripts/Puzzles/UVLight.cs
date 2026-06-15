using UnityEngine;
using UnityEngine.Events;

namespace Room03Mystery
{
    // 자외선(UV) 펜라이트 — 켜져 있는 동안 UVRevealable 오브젝트가 드러난다.
    // 인벤토리에서 UV 펜라이트를 얻은 뒤에만 켤 수 있도록 requiredItemID 로 게이팅.
    // 토글: 키 입력(기본 U) 또는 UI 버튼에서 Toggle() 호출.
    public class UVLight : MonoBehaviour
    {
        public static bool IsOn { get; private set; }

        [Header("사용 조건")]
        [Tooltip("이 아이템을 인벤토리에 가지고 있어야 켤 수 있음 (예: item_uv_light)")]
        [SerializeField] private string requiredItemID = "item_uv_light";

        [Header("입력")]
        [SerializeField] private bool allowKeyToggle = true;
        [SerializeField] private KeyCode toggleKey = KeyCode.U;

        [Header("연출(선택)")]
        // 실제 UV 광원 GameObject (Spot Light 등). 켜고 끌 때 함께 토글.
        [SerializeField] private GameObject lightVisual;

        public UnityEvent onTurnedOn;
        public UnityEvent onTurnedOff;

        void OnDisable() => SetOn(false);

        void Update()
        {
            if (allowKeyToggle && Input.GetKeyDown(toggleKey))
                Toggle();
        }

        public void Toggle() => SetOn(!IsOn);

        public void SetOn(bool on)
        {
            // 아이템 없으면 켤 수 없음
            if (on && !HasLight()) return;

            IsOn = on;
            if (lightVisual != null) lightVisual.SetActive(on);
            if (on) onTurnedOn?.Invoke(); else onTurnedOff?.Invoke();
        }

        bool HasLight()
        {
            if (string.IsNullOrEmpty(requiredItemID)) return true;
            return InventoryManager.Instance != null &&
                   InventoryManager.Instance.HasItem(requiredItemID);
        }
    }
}
