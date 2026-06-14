using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class TimerUI : MonoBehaviour
    {
        [SerializeField] private Canvas timerCanvas;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Color deductionColor = HorrorUITheme.TextMain;
        [SerializeField] private Color chaseColor = HorrorUITheme.BloodRed;
        [SerializeField] private Color urgentColor = HorrorUITheme.SickYellow;
        [SerializeField] private float urgentThresholdSeconds = 180f;

        private void Awake()
        {
            EnsureUI();
        }

        private void Update()
        {
            StoryProgressManager storyProgressManager = StoryProgressManager.Instance;
            if (storyProgressManager == null)
            {
                return;
            }

            EnsureUI();
            timerText.text = FormatTime(storyProgressManager.CurrentTimerRemaining);
            timerText.color = GetTimerColor(storyProgressManager);
        }

        private void EnsureUI()
        {
            if (timerCanvas == null)
            {
                GameObject canvasObject = GameObject.Find("HUD_Canvas");
                if (canvasObject == null)
                {
                    canvasObject = new GameObject("HUD_Canvas");
                    timerCanvas = canvasObject.AddComponent<Canvas>();
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    timerCanvas = canvasObject.GetComponent<Canvas>();
                    if (timerCanvas == null)
                    {
                        timerCanvas = canvasObject.AddComponent<Canvas>();
                    }
                }
            }

            timerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (timerText != null)
            {
                return;
            }

            GameObject textObject = GameObject.Find("TimerText");
            if (textObject != null)
            {
                timerText = textObject.GetComponent<TextMeshProUGUI>();
            }

            if (timerText == null)
            {
                textObject = new GameObject("TimerText");
                textObject.transform.SetParent(timerCanvas.transform, false);
                timerText = textObject.AddComponent<TextMeshProUGUI>();
            }

            timerText.text = "20:00";
            HorrorUITheme.ApplyText(timerText, 34f, deductionColor);
            timerText.alignment = TextAlignmentOptions.TopRight;
            timerText.color = deductionColor;

            RectTransform rect = timerText.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-92f, -24f);
            rect.sizeDelta = new Vector2(180f, 48f);
        }

        private Color GetTimerColor(StoryProgressManager storyProgressManager)
        {
            if (storyProgressManager.IsChaseTimerActive)
            {
                return chaseColor;
            }

            return storyProgressManager.CurrentTimerRemaining <= urgentThresholdSeconds
                ? urgentColor
                : deductionColor;
        }

        private static string FormatTime(float seconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(seconds));
            int minutes = totalSeconds / 60;
            int remainingSeconds = totalSeconds % 60;
            return $"{minutes.ToString("D2")}:{remainingSeconds.ToString("D2")}";
        }
    }
}
