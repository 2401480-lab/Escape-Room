using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class EscapeKeyNoticeUI : MonoBehaviour
    {
        public const string KeyAcquiredMessage = "열쇠를 얻었습니다";
        public const string EscapeKeyAcquiredMessage = "탈출 열쇠를 얻었습니다";

        [SerializeField] private Canvas noticeCanvas;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TextMeshProUGUI noticeText;
        [SerializeField] private float visibleSeconds = 2.2f;

        private static EscapeKeyNoticeUI instance;
        private Coroutine hideRoutine;

        public static void ShowKeyAcquired()
        {
            Instance.Show(KeyAcquiredMessage);
        }

        public static void ShowEscapeKeyAcquired()
        {
            Instance.Show(EscapeKeyAcquiredMessage);
        }

        private static EscapeKeyNoticeUI Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                instance = FindObjectOfType<EscapeKeyNoticeUI>();
                if (instance != null)
                {
                    return instance;
                }

                GameObject noticeObject = new GameObject("EscapeKeyNoticeUI");
                instance = noticeObject.AddComponent<EscapeKeyNoticeUI>();
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            EnsureUI();
            HideImmediate();
        }

        private void Show(string message)
        {
            EnsureUI();
            noticeText.text = message;
            panelRoot.gameObject.SetActive(true);

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }

            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSecondsRealtime(visibleSeconds);
            HideImmediate();
        }

        private void HideImmediate()
        {
            if (panelRoot != null)
            {
                panelRoot.gameObject.SetActive(false);
            }

            hideRoutine = null;
        }

        private void EnsureUI()
        {
            if (noticeCanvas == null)
            {
                GameObject canvasObject = new GameObject("EscapeKeyNoticeCanvas");
                canvasObject.transform.SetParent(transform, false);
                noticeCanvas = canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            noticeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            noticeCanvas.sortingOrder = 1550;

            if (panelRoot != null && noticeText != null)
            {
                return;
            }

            GameObject panelObject = new GameObject("EscapeKeyNoticePanel");
            panelObject.transform.SetParent(noticeCanvas.transform, false);
            panelRoot = panelObject.AddComponent<RectTransform>();
            panelRoot.anchorMin = new Vector2(0.5f, 1f);
            panelRoot.anchorMax = new Vector2(0.5f, 1f);
            panelRoot.pivot = new Vector2(0.5f, 1f);
            panelRoot.anchoredPosition = new Vector2(0f, -96f);
            panelRoot.sizeDelta = new Vector2(420f, 58f);

            Image panelImage = panelObject.AddComponent<Image>();
            HorrorUITheme.ApplyPanel(panelImage, new Color(0.03f, 0.02f, 0.02f, 0.94f));

            GameObject textObject = new GameObject("EscapeKeyNoticeText");
            textObject.transform.SetParent(panelRoot, false);
            noticeText = textObject.AddComponent<TextMeshProUGUI>();
            HorrorUITheme.ApplyText(noticeText, 24f, HorrorUITheme.SickYellow);
            noticeText.alignment = TextAlignmentOptions.Center;
            noticeText.fontStyle = FontStyles.Bold;
            noticeText.rectTransform.anchorMin = Vector2.zero;
            noticeText.rectTransform.anchorMax = Vector2.one;
            noticeText.rectTransform.offsetMin = new Vector2(18f, 6f);
            noticeText.rectTransform.offsetMax = new Vector2(-18f, -6f);
        }
    }
}
