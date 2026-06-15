using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class CluePickupPopupUI : MonoBehaviour
    {
        [SerializeField] private Canvas popupCanvas;
        [SerializeField] private CanvasGroup popupGroup;
        [SerializeField] private Image popupPanelImage;
        [SerializeField] private TextMeshProUGUI popupTitleText;
        [SerializeField] private TextMeshProUGUI popupBodyText;

        private bool subscribed;
        private bool popupVisible;

        private void Awake()
        {
            EnsureUI();
            HidePopupImmediate();
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (!subscribed)
            {
                TrySubscribe();
            }

            if (popupVisible && Input.GetMouseButtonDown(0))
            {
                DismissPopup();
            }
        }

        private void TrySubscribe()
        {
            if (subscribed || ClueJournalManager.Instance == null)
            {
                return;
            }

            ClueJournalManager.Instance.OnClueAdded += ShowCluePopup;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || ClueJournalManager.Instance == null)
            {
                return;
            }

            ClueJournalManager.Instance.OnClueAdded -= ShowCluePopup;
            subscribed = false;
        }

        private void ShowCluePopup(ClueData clueData)
        {
            if (clueData == null)
            {
                return;
            }

            EnsureUI();
            popupTitleText.text = clueData.clueName;
            popupBodyText.text = $"\uB2E8\uC11C\uAC00 \uC190\uC5D0 \uB2FF\uC558\uB2E4.\n\n{clueData.description}\n\n\uB0A8\uC740 \uD754\uC801: {clueData.meaning}";

            SetPopupActive(true);
            popupGroup.alpha = 1f;
            popupGroup.blocksRaycasts = false;
            popupGroup.interactable = false;
            popupVisible = true;
        }

        private void DismissPopup()
        {
            HidePopupImmediate();
        }

        private void HidePopupImmediate()
        {
            popupVisible = false;

            if (popupGroup != null)
            {
                popupGroup.alpha = 0f;
                popupGroup.blocksRaycasts = false;
                popupGroup.interactable = false;
            }

            SetPopupActive(false);
        }

        private void SetPopupActive(bool active)
        {
            if (popupPanelImage != null && popupPanelImage.gameObject != gameObject)
            {
                popupPanelImage.gameObject.SetActive(active);
            }
        }

        private void EnsureUI()
        {
            if (popupCanvas == null)
            {
                GameObject canvasObject = GameObject.Find("CluePickupPopupCanvas");
                if (canvasObject == null)
                {
                    canvasObject = new GameObject("CluePickupPopupCanvas");
                    popupCanvas = canvasObject.AddComponent<Canvas>();
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    popupCanvas = canvasObject.GetComponent<Canvas>();
                    if (popupCanvas == null)
                    {
                        popupCanvas = canvasObject.AddComponent<Canvas>();
                    }
                }
            }

            popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            popupCanvas.sortingOrder = 80;

            CanvasScaler scaler = popupCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = popupCanvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (popupPanelImage == null)
            {
                Transform existingPanel = popupCanvas.transform.Find("CluePickupPopupPanel");
                GameObject panelObject = existingPanel != null
                    ? existingPanel.gameObject
                    : new GameObject("CluePickupPopupPanel");

                panelObject.transform.SetParent(popupCanvas.transform, false);
                popupPanelImage = panelObject.GetComponent<Image>();
                if (popupPanelImage == null)
                {
                    popupPanelImage = panelObject.AddComponent<Image>();
                }

                popupPanelImage.raycastTarget = false;
                popupPanelImage.color = new Color(0.045f, 0.04f, 0.035f, 0.94f);
            }

            RectTransform panelRect = popupPanelImage.rectTransform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(780f, 360f);

            if (popupGroup == null)
            {
                popupGroup = popupPanelImage.GetComponent<CanvasGroup>();
                if (popupGroup == null)
                {
                    popupGroup = popupPanelImage.gameObject.AddComponent<CanvasGroup>();
                }
            }

            popupGroup.blocksRaycasts = false;

            if (popupTitleText == null)
            {
                popupTitleText = CreateText("CluePickupPopupTitle", popupPanelImage.transform, 32f, HorrorUITheme.SickYellow);
                popupTitleText.fontStyle = FontStyles.Bold;
                RectTransform titleRect = popupTitleText.rectTransform;
                titleRect.anchorMin = new Vector2(0.5f, 1f);
                titleRect.anchorMax = new Vector2(0.5f, 1f);
                titleRect.pivot = new Vector2(0.5f, 1f);
                titleRect.anchoredPosition = new Vector2(0f, -34f);
                titleRect.sizeDelta = new Vector2(700f, 64f);
            }

            if (popupBodyText == null)
            {
                popupBodyText = CreateText("CluePickupPopupBody", popupPanelImage.transform, 22f, HorrorUITheme.TextMain);
                RectTransform bodyRect = popupBodyText.rectTransform;
                bodyRect.anchorMin = new Vector2(0.5f, 0.5f);
                bodyRect.anchorMax = new Vector2(0.5f, 0.5f);
                bodyRect.pivot = new Vector2(0.5f, 0.5f);
                bodyRect.anchoredPosition = new Vector2(0f, -40f);
                bodyRect.sizeDelta = new Vector2(680f, 230f);
            }
        }

        private TextMeshProUGUI CreateText(string objectName, Transform parent, float fontSize, Color color)
        {
            GameObject textObject = new GameObject(objectName);
            textObject.transform.SetParent(parent, false);

            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            HorrorUITheme.ApplyText(text, fontSize, color);
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            return text;
        }
    }
}
