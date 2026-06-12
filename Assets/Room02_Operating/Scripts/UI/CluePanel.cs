using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room02Operating
{
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
            closeButton.onClick.AddListener(Hide);
            panelRoot.SetActive(false);
        }

        public void Show(ClueData data)
        {
            detailImage.sprite = data.detailImage;
            detailImage.gameObject.SetActive(data.detailImage != null);
            descriptionText.text = data.description;
            panelRoot.SetActive(true);
        }

        public void Hide() => panelRoot.SetActive(false);
    }
}
