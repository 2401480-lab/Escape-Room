using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room03Mystery
{
    // 단서/아이템 클릭 시 뜨는 확대 팝업 패널 (싱글톤)
    // Canvas 안에 panelRoot(Panel) → Image + Text + 닫기 Button 구조
    public class CluePanel : MonoBehaviour
    {
        public static CluePanel Instance { get; private set; }

        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image detailImage;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private Button closeButton;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (closeButton != null) closeButton.onClick.AddListener(Hide);
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private bool _open;

        public void Show(ClueData data)
        {
            if (detailImage != null)
            {
                detailImage.sprite = data.detailImage;
                detailImage.gameObject.SetActive(data.detailImage != null);
            }
            if (descriptionText != null) descriptionText.text = data.description;
            if (panelRoot != null) panelRoot.SetActive(true);

            if (!_open) { _open = true; UIFocus.Push(); }   // 커서 해제 + 1인칭 조작 정지
        }

        public void Hide()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (_open) { _open = false; UIFocus.Pop(); }
        }
    }
}
