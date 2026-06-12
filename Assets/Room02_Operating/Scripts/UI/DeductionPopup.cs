using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room02Operating
{
    public class DeductionPopup : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Image panelBackground;

        [Header("색상 설정")]
        [SerializeField] private Color defaultColor = new Color(1f, 0.976f, 0.765f);
        [SerializeField] private Color finalColor   = new Color(0.91f, 0.96f, 0.91f);

        private Action _onConfirm;

        void Awake()
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
            panelRoot.SetActive(false);
        }

        public void Show(string title, string body, Action onConfirm = null, bool isFinal = false)
        {
            titleText.text = title;
            bodyText.text  = body;
            _onConfirm = onConfirm;
            panelBackground.color = isFinal ? finalColor : defaultColor;
            panelRoot.SetActive(true);
        }

        void OnConfirmClicked()
        {
            panelRoot.SetActive(false);
            _onConfirm?.Invoke();
            _onConfirm = null;
        }
    }
}
