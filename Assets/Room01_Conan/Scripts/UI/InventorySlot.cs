using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room01Conan
{
    // 인벤토리 슬롯 하나 — 클릭 시 확대 검사 팝업 열기
    public class InventorySlot : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private Button button;

        private ClueData _data;

        void Awake()
        {
            button.onClick.AddListener(OnSlotClicked);
            button.interactable = false;
        }

        public void SetItem(ClueData data)
        {
            _data = data;
            iconImage.sprite = data.thumbnail;
            iconImage.gameObject.SetActive(data.thumbnail != null);
            itemNameText.text = data.displayName;
            button.interactable = true;
        }

        void OnSlotClicked()
        {
            if (_data != null)
                CluePanel.Instance?.Show(_data);
        }
    }
}
