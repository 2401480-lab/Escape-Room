using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class EscapeChaseQTE : MonoBehaviour
    {
        public static EscapeChaseQTE Instance { get; private set; }

        [SerializeField] private float durationSeconds = 5f;
        [SerializeField] private int requiredSpacePresses = 30;
        [SerializeField] private float warningSeconds = 0.5f;
        [SerializeField] private float culpritRushSeconds = 0.32f;
        [SerializeField] private float culpritStartDistance = 8f;
        [SerializeField] private float culpritEndDistance = 0.08f;
        [SerializeField] private float culpritFinalScaleMultiplier = 6.2f;
        [SerializeField] private float culpritPreviewYOffset = -2.3f;
        [SerializeField] private GameObject culpritPrefab;
        [SerializeField] private string culpritResourcesPath = "Room02_CulpritChaser";

        private Canvas qteCanvas;
        private GameObject panelRoot;
        private TextMeshProUGUI messageText;
        private TextMeshProUGUI endingTitleText;
        private GameObject chaseWindowRoot;
        private RawImage chasePreviewImage;
        private GameObject progressBarRoot;
        private Image progressFill;
        private Image endingBackground;
        private Button homeButton;
        private Camera chasePreviewCamera;
        private RenderTexture chasePreviewTexture;
        private Transform chasePreviewRig;
        private GameObject spawnedCulprit;
        private float startedAt;
        private int pressCount;
        private bool isRunning;
        private Coroutine activeSequence;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureUI();
            SetVisible(false);
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                pressCount++;
            }

            RefreshProgress();

            if (pressCount >= requiredSpacePresses)
            {
                CompleteSuccess();
                return;
            }

            if (Time.unscaledTime - startedAt >= durationSeconds)
            {
                CompleteFailure();
            }
        }

        public void StartQTE()
        {
            EnsureUI();
            if (activeSequence != null)
            {
                StopCoroutine(activeSequence);
            }

            activeSequence = StartCoroutine(PlayWarningThenStartQTE());
        }

        private IEnumerator PlayWarningThenStartQTE()
        {
            isRunning = false;
            PrepareOverlay();
            SetProgressBarVisible(false);
            endingTitleText.gameObject.SetActive(false);
            homeButton.gameObject.SetActive(false);

            messageText.text = "\uc9c4\ubc94\uc744 \uc54c\uc544\ucc58\uc9c0\ub9cc,\n\ub108\ubb34 \ub2a6\uc5c8\ub2e4.";
            yield return new WaitForSecondsRealtime(warningSeconds);

            SetChaseWindowVisible(true);
            SetupChasePreviewScene();
            SpawnCulpritInPreview();
            yield return RushCulpritInPreviewWindow();
            CleanupCulprit();
            SetChaseWindowVisible(false);

            startedAt = Time.unscaledTime;
            pressCount = 0;
            isRunning = true;
            SetProgressBarVisible(true);
            RefreshProgress();
            activeSequence = null;
        }

        private void CompleteSuccess()
        {
            isRunning = false;
            ShowEnding(true);
        }

        private void CompleteFailure()
        {
            isRunning = false;
            ShowEnding(false);
        }

        private void ShowEnding(bool success)
        {
            PrepareOverlay();
            SetProgressBarVisible(false);
            SetChaseWindowVisible(false);
            CleanupCulprit();

            endingBackground.gameObject.SetActive(true);
            endingBackground.color = success ? Color.white : Color.black;

            messageText.text = success ? "\ubc29\ud0c8\ucd9c\uc744 \uc131\uacf5\ud588\uc2b5\ub2c8\ub2e4!" : "";
            messageText.color = success ? Color.black : Color.white;

            endingTitleText.text = success ? "FINISH" : "GAME OVER";
            endingTitleText.color = success ? new Color(0.08f, 0.08f, 0.08f, 1f) : Color.red;
            endingTitleText.rectTransform.localScale = Vector3.one;
            endingTitleText.gameObject.SetActive(true);
            HideYellowClueMarkers();

            ShowHomeButton();
            OrderEndingLayers();
        }

        private void RefreshProgress()
        {
            float progress = Mathf.Clamp01((float)pressCount / requiredSpacePresses);
            if (progressFill != null)
            {
                RectTransform fillRect = progressFill.rectTransform;
                fillRect.anchorMax = new Vector2(progress, 1f);
                fillRect.offsetMax = Vector2.zero;
            }

            if (messageText != null)
            {
                float remaining = Mathf.Max(0f, durationSeconds - (Time.unscaledTime - startedAt));
                messageText.color = Color.white;
                messageText.text = $"\ubc94\uc778\uc774 \ucad3\uc544\uc624\uace0 \uc788\uc2b5\ub2c8\ub2e4.\n\uc2a4\ud398\uc774\uc2a4\ub97c \uc5f0\ud0c0\ud558\uc138\uc694!\n{remaining:0.0}\ucd08";
            }
        }

        private void PrepareOverlay()
        {
            EnsureUI();
            SetVisible(true);
            qteCanvas.overrideSorting = true;
            qteCanvas.sortingOrder = 1000000;
            endingBackground.gameObject.SetActive(false);
            OrderEndingLayers();
        }

        private void EnsureUI()
        {
            if (qteCanvas == null)
            {
                GameObject canvasObject = new GameObject("EscapeChaseQTECanvas");
                qteCanvas = canvasObject.AddComponent<Canvas>();
                qteCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            if (panelRoot != null)
            {
                return;
            }

            panelRoot = CreatePanel("EscapeChaseQTEPanel", qteCanvas.transform, new Color(0f, 0f, 0f, 0.76f)).gameObject;
            RectTransform panelRect = (RectTransform)panelRoot.transform;
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            chaseWindowRoot = CreatePanel("CulpritChaseWindow", panelRect, new Color(0f, 0f, 0f, 1f)).gameObject;
            RectTransform chaseRect = (RectTransform)chaseWindowRoot.transform;
            chaseRect.anchorMin = Vector2.zero;
            chaseRect.anchorMax = Vector2.one;
            chaseRect.offsetMin = Vector2.zero;
            chaseRect.offsetMax = Vector2.zero;

            GameObject previewObject = new GameObject("Preview");
            previewObject.transform.SetParent(chaseRect, false);
            chasePreviewImage = previewObject.AddComponent<RawImage>();
            RectTransform previewRect = chasePreviewImage.rectTransform;
            previewRect.anchorMin = Vector2.zero;
            previewRect.anchorMax = Vector2.one;
            previewRect.offsetMin = Vector2.zero;
            previewRect.offsetMax = Vector2.zero;
            chaseWindowRoot.SetActive(false);

            endingBackground = CreatePanel("EndingBackground", panelRect, Color.clear).GetComponent<Image>();
            RectTransform backgroundRect = endingBackground.rectTransform;
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            endingBackground.gameObject.SetActive(false);

            messageText = CreateText("Message", panelRect, "", 34f);
            messageText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            messageText.rectTransform.anchoredPosition = new Vector2(0f, 105f);
            messageText.rectTransform.sizeDelta = new Vector2(860f, 170f);

            RectTransform bar = CreatePanel("ProgressBar", panelRect, new Color(0.08f, 0.08f, 0.08f, 0.96f));
            progressBarRoot = bar.gameObject;
            bar.anchorMin = new Vector2(0.5f, 0.5f);
            bar.anchorMax = new Vector2(0.5f, 0.5f);
            bar.pivot = new Vector2(0.5f, 0.5f);
            bar.anchoredPosition = new Vector2(0f, -45f);
            bar.sizeDelta = new Vector2(700f, 46f);

            progressFill = CreatePanel("Fill", bar, new Color(0.68f, 0.02f, 0.02f, 1f)).GetComponent<Image>();
            RectTransform fillRect = progressFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            endingTitleText = CreateText("EndingTitle", panelRect, "", 78f);
            endingTitleText.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            endingTitleText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            endingTitleText.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            endingTitleText.rectTransform.anchoredPosition = new Vector2(0f, 30f);
            endingTitleText.rectTransform.sizeDelta = new Vector2(920f, 140f);
            endingTitleText.gameObject.SetActive(false);

            homeButton = CreateHomeButton(panelRect);
            homeButton.gameObject.SetActive(false);
        }

        private void SetVisible(bool visible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(visible);
            }
        }

        private void SetProgressBarVisible(bool visible)
        {
            if (progressBarRoot != null)
            {
                progressBarRoot.SetActive(visible);
            }
        }

        private void SetChaseWindowVisible(bool visible)
        {
            if (chaseWindowRoot != null)
            {
                chaseWindowRoot.SetActive(visible);
            }
        }

        private void OrderEndingLayers()
        {
            if (chaseWindowRoot != null)
            {
                chaseWindowRoot.transform.SetAsFirstSibling();
            }
            if (endingBackground != null)
            {
                endingBackground.transform.SetAsLastSibling();
            }
            if (messageText != null)
            {
                messageText.transform.SetAsLastSibling();
            }
            if (endingTitleText != null)
            {
                endingTitleText.transform.SetAsLastSibling();
            }
            if (homeButton != null)
            {
                homeButton.transform.SetAsLastSibling();
            }
        }

        private void SetupChasePreviewScene()
        {
            if (chasePreviewRig == null)
            {
                GameObject rigObject = new GameObject("EscapeChasePreviewRig");
                rigObject.hideFlags = HideFlags.HideAndDontSave;
                chasePreviewRig = rigObject.transform;
                chasePreviewRig.position = new Vector3(10000f, 10000f, 10000f);
                chasePreviewRig.rotation = Quaternion.identity;
            }

            if (chasePreviewCamera == null)
            {
                GameObject cameraObject = new GameObject("EscapeChasePreviewCamera");
                cameraObject.hideFlags = HideFlags.HideAndDontSave;
                cameraObject.transform.SetParent(chasePreviewRig, false);
                cameraObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
                cameraObject.transform.localRotation = Quaternion.identity;

                chasePreviewCamera = cameraObject.AddComponent<Camera>();
                chasePreviewCamera.clearFlags = CameraClearFlags.SolidColor;
                chasePreviewCamera.backgroundColor = Color.black;
                chasePreviewCamera.fieldOfView = 36f;
                chasePreviewCamera.nearClipPlane = 0.03f;
                chasePreviewCamera.farClipPlane = 40f;
            }

            if (chasePreviewTexture == null)
            {
                chasePreviewTexture = new RenderTexture(1280, 720, 16);
                chasePreviewTexture.name = "EscapeChasePreviewTexture";
                chasePreviewCamera.targetTexture = chasePreviewTexture;
            }

            if (chasePreviewImage != null)
            {
                chasePreviewImage.texture = chasePreviewTexture;
            }
        }

        private void SpawnCulpritInPreview()
        {
            if (culpritPrefab == null && !string.IsNullOrWhiteSpace(culpritResourcesPath))
            {
                culpritPrefab = Resources.Load<GameObject>(culpritResourcesPath);
            }

            if (culpritPrefab == null || chasePreviewRig == null)
            {
                return;
            }

            CleanupCulprit();
            Vector3 spawnPosition = chasePreviewRig.position
                + chasePreviewRig.forward * culpritStartDistance
                + Vector3.up * culpritPreviewYOffset;

            spawnedCulprit = Instantiate(
                culpritPrefab,
                spawnPosition,
                Quaternion.LookRotation(-chasePreviewRig.forward, Vector3.up),
                chasePreviewRig);

            ApplyCulpritPreviewLook(spawnedCulprit);
            TryPlayCulpritAnimation(spawnedCulprit);
        }

        private IEnumerator RushCulpritInPreviewWindow()
        {
            if (spawnedCulprit == null || chasePreviewCamera == null)
            {
                yield return new WaitForSecondsRealtime(0.25f);
                yield break;
            }

            Transform culprit = spawnedCulprit.transform;
            Transform cameraTransform = chasePreviewCamera.transform;
            Vector3 startPosition = culprit.position;
            Vector3 startScale = culprit.localScale;
            float startedRushAt = Time.unscaledTime;

            while (Time.unscaledTime - startedRushAt < culpritRushSeconds)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - startedRushAt) / culpritRushSeconds);
                float eased = t * t * (3f - 2f * t);
                Vector3 endPosition = cameraTransform.position
                    + cameraTransform.forward * culpritEndDistance
                    + Vector3.up * culpritPreviewYOffset;

                culprit.position = Vector3.Lerp(startPosition, endPosition, eased);
                culprit.rotation = Quaternion.LookRotation(cameraTransform.position - culprit.position, Vector3.up);
                culprit.localScale = Vector3.Lerp(startScale, startScale * culpritFinalScaleMultiplier, eased);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.12f);
        }

        private void ApplyCulpritPreviewLook(GameObject culprit)
        {
            Renderer[] renderers = culprit.GetComponentsInChildren<Renderer>(true);
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit") ??
                            Shader.Find("Sprites/Default");

            foreach (Renderer renderer in renderers)
            {
                if (ShouldHideCulpritRenderer(renderer) || shader == null)
                {
                    renderer.enabled = false;
                    continue;
                }

                Material material = new Material(shader);
                SetMaterialColor(material, new Color(0.015f, 0.013f, 0.013f, 1f));

                int materialCount = Mathf.Max(1, renderer.sharedMaterials.Length);
                Material[] materials = new Material[materialCount];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = material;
                }

                renderer.sharedMaterials = materials;
            }
        }

        private static bool ShouldHideCulpritRenderer(Renderer renderer)
        {
            Transform current = renderer.transform;
            while (current != null)
            {
                string objectName = current.name.ToLowerInvariant();
                if (objectName.Contains("char_shadow") || objectName == "shadow" || objectName.EndsWith("_shadow"))
                {
                    return true;
                }

                current = current.parent;
            }

            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    return true;
                }

                string materialName = material.name.ToLowerInvariant();
                string shaderName = material.shader != null ? material.shader.name.ToLowerInvariant() : "";
                if (materialName.Contains("char_shadow") ||
                    materialName.Contains("pink") ||
                    materialName.Contains("magenta") ||
                    shaderName.Contains("reflective/diffuse"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private void TryPlayCulpritAnimation(GameObject culprit)
        {
            AnimationClip[] clips = Resources.LoadAll<AnimationClip>(culpritResourcesPath);
            if (clips == null || clips.Length == 0)
            {
                return;
            }

            Animation animation = culprit.GetComponent<Animation>() ?? culprit.AddComponent<Animation>();
            foreach (AnimationClip clip in clips)
            {
                if (clip == null || clip.name.ToLowerInvariant().Contains("preview"))
                {
                    continue;
                }

                clip.wrapMode = WrapMode.Loop;
                animation.AddClip(clip, clip.name);
                animation.clip = clip;
                animation.Play(clip.name);
                return;
            }
        }

        private void CleanupCulprit()
        {
            if (spawnedCulprit != null)
            {
                Destroy(spawnedCulprit);
                spawnedCulprit = null;
            }
        }

        private void HideYellowClueMarkers()
        {
            TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
            foreach (TMP_Text text in texts)
            {
                if (text == null || (panelRoot != null && text.transform.IsChildOf(panelRoot.transform)))
                {
                    continue;
                }

                Color color = text.color;
                bool yellowColor = color.r > 0.7f && color.g > 0.45f && color.b < 0.35f;
                bool markerText = text.text.Contains("\u25bc") || text.text.Contains("\u25be");
                if (yellowColor || markerText)
                {
                    DisableMarkerObject(text.transform);
                }
            }

            Graphic[] graphics = FindObjectsOfType<Graphic>(true);
            foreach (Graphic graphic in graphics)
            {
                if (graphic == null || (panelRoot != null && graphic.transform.IsChildOf(panelRoot.transform)))
                {
                    continue;
                }

                Color color = graphic.color;
                bool yellowColor = color.r > 0.7f && color.g > 0.45f && color.b < 0.35f;
                if (yellowColor)
                {
                    DisableMarkerObject(graphic.transform);
                }
            }
        }

        private void DisableMarkerObject(Transform marker)
        {
            Transform target = marker;
            while (target.parent != null && panelRoot != null && !target.parent.IsChildOf(panelRoot.transform))
            {
                string parentName = target.parent.name.ToLowerInvariant();
                if (!parentName.Contains("clue") &&
                    !parentName.Contains("marker") &&
                    !parentName.Contains("label") &&
                    !parentName.Contains("canvas"))
                {
                    break;
                }

                target = target.parent;
            }

            target.gameObject.SetActive(false);
        }

        private Button CreateHomeButton(Transform parent)
        {
            RectTransform buttonRect = CreatePanel("HomeButton", parent, new Color(0.02f, 0.02f, 0.02f, 0.92f));
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = new Vector2(0f, -145f);
            buttonRect.sizeDelta = new Vector2(330f, 60f);

            Button button = buttonRect.gameObject.AddComponent<Button>();
            button.onClick.AddListener(ReturnToOnboarding);

            TextMeshProUGUI label = CreateText("Label", buttonRect, "\ud648\ud654\uba74\uc73c\ub85c \ub3cc\uc544\uac00\uae30", 24f);
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;
            label.color = Color.white;

            return button;
        }

        private void ShowHomeButton()
        {
            EnsureEventSystem();
            homeButton.gameObject.SetActive(true);
            homeButton.transform.SetAsLastSibling();
        }

        private void ReturnToOnboarding()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
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
}
