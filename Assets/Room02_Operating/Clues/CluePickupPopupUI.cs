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
        [SerializeField] private TextMeshProUGUI popupText;
        [SerializeField] private float displayDuration = 2f;
        [SerializeField] private float fadeDuration = 0.4f;

        private Coroutine fadeRoutine;
        private bool subscribed;

        private void Awake()
        {
            EnsureUI();
            popupGroup.alpha = 0f;
        }

        private void OnEnable()  { TrySubscribe(); }
        private void OnDisable() { Unsubscribe(); }

        private void Update()
        {
            // ClueJournalManager가 나중에 생성됐을 때 구독 재시도
            if (!subscribed) TrySubscribe();
        }

        private void TrySubscribe()
        {
            if (subscribed || ClueJournalManager.Instance == null) return;
            ClueJournalManager.Instance.OnClueAdded += ShowCluePopup;
            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed || ClueJournalManager.Instance == null) return;
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
            popupText.text = $"{clueData.clueName}\n증거를 확보했다";
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(ShowThenFade());
        }

        private IEnumerator ShowThenFade()
        {
            popupGroup.alpha = 1f;
            yield return new WaitForSeconds(displayDuration);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                popupGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            popupGroup.alpha = 0f;
            fadeRoutine = null;
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

            if (popupGroup == null)
            {
                popupGroup = popupCanvas.GetComponent<CanvasGroup>();
                if (popupGroup == null)
                {
                    popupGroup = popupCanvas.gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (popupText != null)
            {
                return;
            }

            GameObject textObject = new GameObject("CluePickupPopupText");
            textObject.transform.SetParent(popupCanvas.transform, false);
            popupText = textObject.AddComponent<TextMeshProUGUI>();
            HorrorUITheme.ApplyText(popupText, 26f, HorrorUITheme.SickYellow);
            popupText.alignment = TextAlignmentOptions.Center;

            RectTransform rect = popupText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 150f);
            rect.sizeDelta = new Vector2(620f, 64f);
        }
    }
}
