using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom
{
    public class GameOverUI : MonoBehaviour
    {
        public static GameOverUI Instance { get; private set; }

        [SerializeField] private Canvas gameOverCanvas;
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Image blackoutImage;
        [SerializeField] private TextMeshProUGUI gameOverTitleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Animator jumpscareAnimator;
        [SerializeField] private GameObject jumpscareModelPrefab;
        [SerializeField] private string jumpscareModelResourcesPath = "Room02_Models/Ch45_nonPBR";
        [SerializeField] private string editorJumpscareModelAssetPath = "Assets/Room02_Operating/Models/Ch45_nonPBR.fbx";
        [SerializeField] private float jumpscareDistance = 1.15f;
        [SerializeField] private float jumpscareVerticalOffset = -0.18f;
        [SerializeField] private float jumpscareScale = 1.35f;
        [SerializeField] private float blackoutDelay = 0.38f;
        [SerializeField] private string mainMenuSceneName = "RoomSelect";

        public UnityEvent OnJumpscareStarted = new UnityEvent();
        public UnityEvent OnSurvivalEndingShown = new UnityEvent();

        private AudioSource jumpscareAudioSource;
        private AudioClip dudungTakClip;
        private GameObject spawnedJumpscareModel;
        private Coroutine gameOverSequence;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureUI();
            panelRoot.SetActive(false);
        }

        public void PlayGameOver(GameOverReason reason)
        {
            EnsureUI();

            if (gameOverSequence != null)
            {
                StopCoroutine(gameOverSequence);
            }

            if (ShouldPlayJumpscare(reason))
            {
                gameOverSequence = StartCoroutine(PlayJumpscareSequence(reason));
                return;
            }

            ShowFinalGameOver(reason);
        }

        public void ShowSurvivalEnding()
        {
            EnsureUI();
            panelRoot.SetActive(true);
            blackoutImage.color = new Color(0f, 0f, 0f, 0.92f);
            gameOverTitleText.text = "SURVIVED";
            gameOverTitleText.color = new Color(0.78f, 0.9f, 0.72f, 1f);
            messageText.text = "You survived.\n\nThe clues pointed to Jin Sewoong: the poison, the red paint, and the space under the operating table.";
            OnSurvivalEndingShown?.Invoke();
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void MainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private bool ShouldPlayJumpscare(GameOverReason reason)
        {
            return reason == GameOverReason.WrongAnswer ||
                   reason == GameOverReason.DeductionTimerExpired;
        }

        private IEnumerator PlayJumpscareSequence(GameOverReason reason)
        {
            panelRoot.SetActive(false);
            OnJumpscareStarted?.Invoke();

            spawnedJumpscareModel = SpawnJumpscareModel();
            if (jumpscareAnimator != null)
            {
                jumpscareAnimator.SetTrigger("Jumpscare");
            }

            PlayDudungTakImpact();
            yield return new WaitForSecondsRealtime(blackoutDelay);

            ShowFinalGameOver(reason);
        }

        private GameObject SpawnJumpscareModel()
        {
            GameObject prefab = ResolveJumpscareModel();
            if (prefab == null)
            {
                return null;
            }

            Camera targetCamera = Camera.main;
            Vector3 position = transform.position + transform.forward * jumpscareDistance;
            Quaternion rotation = transform.rotation;

            if (targetCamera != null)
            {
                position = targetCamera.transform.position +
                           targetCamera.transform.forward * jumpscareDistance +
                           targetCamera.transform.up * jumpscareVerticalOffset;
                Vector3 lookDirection = position - targetCamera.transform.position;
                if (lookDirection.sqrMagnitude > 0.001f)
                {
                    rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
                }
            }

            if (spawnedJumpscareModel != null)
            {
                Destroy(spawnedJumpscareModel);
            }

            GameObject model = Instantiate(prefab, position, rotation);
            model.name = "GameOverJumpscareModel";
            model.transform.localScale = Vector3.one * jumpscareScale;
            model.SetActive(true);
            return model;
        }

        private GameObject ResolveJumpscareModel()
        {
            if (jumpscareModelPrefab != null)
            {
                return jumpscareModelPrefab;
            }

            if (!string.IsNullOrWhiteSpace(jumpscareModelResourcesPath))
            {
                jumpscareModelPrefab = Resources.Load<GameObject>(jumpscareModelResourcesPath);
                if (jumpscareModelPrefab != null)
                {
                    return jumpscareModelPrefab;
                }
            }

#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(editorJumpscareModelAssetPath))
            {
                jumpscareModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(editorJumpscareModelAssetPath);
            }
#endif

            return jumpscareModelPrefab;
        }

        private void PlayDudungTakImpact()
        {
            if (jumpscareAudioSource == null)
            {
                jumpscareAudioSource = GetComponent<AudioSource>();
                if (jumpscareAudioSource == null)
                {
                    jumpscareAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            jumpscareAudioSource.playOnAwake = false;
            jumpscareAudioSource.loop = false;
            jumpscareAudioSource.spatialBlend = 0f;
            jumpscareAudioSource.volume = 1f;
            jumpscareAudioSource.mute = false;
            jumpscareAudioSource.enabled = true;
            jumpscareAudioSource.PlayOneShot(GetOrCreateDudungTakClip());
        }

        private AudioClip GetOrCreateDudungTakClip()
        {
            if (dudungTakClip != null)
            {
                return dudungTakClip;
            }

            const int sampleRate = 44100;
            const float duration = 0.72f;
            int sampleCount = Mathf.CeilToInt(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float boomOne = ImpactEnvelope(t, 0f, 70f, 10f) * 0.9f;
                float boomTwo = ImpactEnvelope(t, 0.16f, 52f, 9f) * 0.8f;
                float tak = ImpactEnvelope(t, 0.34f, 170f, 26f) * 0.55f;
                float crack = Mathf.Sin(2f * Mathf.PI * 880f * t) * ImpactEnvelope(t, 0.34f, 1f, 42f) * 0.16f;
                samples[i] = Mathf.Clamp(boomOne + boomTwo + tak + crack, -0.98f, 0.98f);
            }

            dudungTakClip = AudioClip.Create("DudungTak_Jumpscare", sampleCount, 1, sampleRate, false);
            dudungTakClip.SetData(samples, 0);
            return dudungTakClip;
        }

        private static float ImpactEnvelope(float time, float start, float frequency, float decay)
        {
            float localTime = time - start;
            if (localTime < 0f)
            {
                return 0f;
            }

            return Mathf.Sin(2f * Mathf.PI * frequency * localTime) * Mathf.Exp(-decay * localTime);
        }

        private void ShowFinalGameOver(GameOverReason reason)
        {
            EnsureUI();
            panelRoot.SetActive(true);
            blackoutImage.color = Color.black;
            gameOverTitleText.text = "GAME OVER";
            gameOverTitleText.color = Color.red;
            messageText.text = GetMessage(reason);
        }

        private static string GetMessage(GameOverReason reason)
        {
            switch (reason)
            {
                case GameOverReason.WrongAnswer:
                    return "You chose the wrong culprit.\nSomething was waiting in the dark.";
                case GameOverReason.CaughtDuringChase:
                    return "Jin Sewoong caught you before you reached the exit.";
                case GameOverReason.DeductionTimerExpired:
                    return "The deduction time is over.\nThe figure behind you finally moved.";
                case GameOverReason.ChaseTimerExpired:
                    return "You ran out of time before escaping.";
                default:
                    return "Game Over";
            }
        }

        private void EnsureUI()
        {
            if (gameOverCanvas == null)
            {
                GameObject canvasObject = new GameObject("GameOverCanvas");
                gameOverCanvas = canvasObject.AddComponent<Canvas>();
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            gameOverCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameOverCanvas.sortingOrder = 2000;

            if (panelRoot != null && blackoutImage != null && gameOverTitleText != null && messageText != null)
            {
                return;
            }

            panelRoot = CreatePanel("GameOverPanel", gameOverCanvas.transform, Color.black).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            blackoutImage = panelRoot.GetComponent<Image>();
            blackoutImage.color = Color.black;

            gameOverTitleText = CreateText("GameOverTitle", panelRect, "GAME OVER", 76f);
            gameOverTitleText.color = Color.red;
            gameOverTitleText.fontStyle = FontStyles.Bold;
            gameOverTitleText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            gameOverTitleText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            gameOverTitleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            gameOverTitleText.rectTransform.anchoredPosition = new Vector2(0f, 82f);
            gameOverTitleText.rectTransform.sizeDelta = new Vector2(760f, 100f);

            messageText = CreateText("Message", panelRect, "", 25f);
            messageText.color = new Color(0.86f, 0.82f, 0.78f, 1f);
            messageText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.anchoredPosition = new Vector2(0f, -20f);
            messageText.rectTransform.sizeDelta = new Vector2(760f, 120f);

            restartButton = CreateButton("RestartButton", panelRect, "Restart", new Vector2(-95f, -145f));
            restartButton.onClick.AddListener(Restart);

            mainMenuButton = CreateButton("MainMenuButton", panelRect, "Main Menu", new Vector2(105f, -145f));
            mainMenuButton.onClick.AddListener(MainMenu);
        }

        private static Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPosition)
        {
            RectTransform rect = CreatePanel(name, parent, new Color(0.16f, 0.015f, 0.025f, 0.96f));
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(170f, 46f);

            Button button = rect.gameObject.AddComponent<Button>();
            TextMeshProUGUI text = CreateText("Label", rect, label, 20f);
            text.color = new Color(0.94f, 0.9f, 0.86f, 1f);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;
            return button;
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, string text, float fontSize)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableWordWrapping = true;
            return tmp;
        }
    }

    public enum GameOverReason
    {
        WrongAnswer,
        CaughtDuringChase,
        DeductionTimerExpired,
        ChaseTimerExpired
    }
}
