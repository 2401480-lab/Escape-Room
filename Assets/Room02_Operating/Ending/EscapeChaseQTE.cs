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
        [SerializeField] private float culpritRushSeconds = 0.36f;
        [SerializeField] private float culpritStartDistance = 8f;
        [SerializeField] private float culpritEndDistance = 0.04f;
        [SerializeField] private float culpritFinalScaleMultiplier = 12.5f;
        [SerializeField] private float culpritPreviewYOffset = 0.25f;
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
        private RectTransform fireworksRoot;
        private Button homeButton;
        private Camera chasePreviewCamera;
        private RenderTexture chasePreviewTexture;
        private Transform chasePreviewRig;
        private GameObject spawnedCulprit;
        private float startedAt;
        private int pressCount;
        private bool isRunning;
        private bool endingStarted;
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
            SetChaseWindowVisible(false);
            CleanupCulprit();
            endingTitleText.gameObject.SetActive(false);
            endingBackground.gameObject.SetActive(false);
            if (fireworksRoot != null)
            {
                fireworksRoot.gameObject.SetActive(false);
            }
            homeButton.gameObject.SetActive(false);
            endingStarted = false;

            messageText.gameObject.SetActive(true);

            startedAt = Time.unscaledTime;
            pressCount = 0;
            isRunning = true;
            SetProgressBarVisible(true);
            RefreshProgress();
            yield return null;
            activeSequence = null;
        }

        private void CompleteSuccess()
        {
            if (endingStarted)
            {
                return;
            }

            isRunning = false;
            endingStarted = true;
            Debug.Log("[EscapeChaseQTE] Success ending started.");
            StartCoroutine(PlaySuccessEnding());
        }

        private void CompleteFailure()
        {
            if (endingStarted)
            {
                return;
            }

            isRunning = false;
            endingStarted = true;
            Debug.Log("[EscapeChaseQTE] Failure ending started.");
            StartCoroutine(PlayFailureEnding());
        }

        private IEnumerator PlaySuccessEnding()
        {
            PrepareOverlay();
            SetProgressBarVisible(false);
            SetChaseWindowVisible(false);
            CleanupCulprit();

            endingTitleText.gameObject.SetActive(false);
            homeButton.gameObject.SetActive(false);
            messageText.gameObject.SetActive(true);
            messageText.text = "\ubc29\ud0c8\ucd9c\uc5d0 \uc131\uacf5\ud558\uc168\uc2b5\ub2c8\ub2e4!";
            messageText.color = Color.white;
            OrderEndingLayers();

            yield return new WaitForSecondsRealtime(1f);
            yield return FadeEnding(Color.white, 0.9f);
            messageText.text = "";
            ShowEndingTitle("FINISH!", new Color(0.08f, 0.08f, 0.08f, 1f));
            StartCoroutine(PlayFireworks());
            yield return PopEndingTitle();
            ShowHomeButton();
        }

        private IEnumerator PlayFailureEnding()
        {
            PrepareOverlay();
            SetProgressBarVisible(false);
            SetChaseWindowVisible(false);
            CleanupCulprit();

            endingTitleText.gameObject.SetActive(false);
            homeButton.gameObject.SetActive(false);
            messageText.gameObject.SetActive(true);
            messageText.text = "\uc9c4\ubc94\uc744 \uc54c\uc544\ucc58\uc9c0\ub9cc,\n\ub108\ubb34 \ub2a6\uc5c8\ub2e4.";
            messageText.color = Color.white;
            OrderEndingLayers();

            yield return new WaitForSecondsRealtime(1f);
            yield return FadeEnding(Color.black, 0.9f);
            messageText.text = "";
            ShowEndingTitle("GAME OVER", Color.red);
            yield return PopEndingTitle();
            ShowHomeButton();
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
                EnsureLateUIElements((RectTransform)panelRoot.transform);
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

            fireworksRoot = new GameObject("Fireworks").AddComponent<RectTransform>();
            fireworksRoot.SetParent(panelRect, false);
            fireworksRoot.anchorMin = Vector2.zero;
            fireworksRoot.anchorMax = Vector2.one;
            fireworksRoot.offsetMin = Vector2.zero;
            fireworksRoot.offsetMax = Vector2.zero;
            fireworksRoot.gameObject.SetActive(false);

            homeButton = CreateHomeButton(panelRect);
            homeButton.gameObject.SetActive(false);
        }

        private void EnsureLateUIElements(RectTransform panelRect)
        {
            if (fireworksRoot == null)
            {
                Transform existingFireworks = panelRect.Find("Fireworks");
                fireworksRoot = existingFireworks != null
                    ? (RectTransform)existingFireworks
                    : new GameObject("Fireworks").AddComponent<RectTransform>();

                fireworksRoot.SetParent(panelRect, false);
                fireworksRoot.anchorMin = Vector2.zero;
                fireworksRoot.anchorMax = Vector2.one;
                fireworksRoot.offsetMin = Vector2.zero;
                fireworksRoot.offsetMax = Vector2.zero;
                fireworksRoot.gameObject.SetActive(false);
            }
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
            if (fireworksRoot != null && fireworksRoot.gameObject.activeSelf)
            {
                fireworksRoot.SetAsLastSibling();
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

        private IEnumerator FadeEnding(Color targetColor, float seconds)
        {
            endingBackground.gameObject.SetActive(true);
            OrderEndingLayers();

            Color start = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
            Color end = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);
            float startedFadeAt = Time.unscaledTime;

            while (Time.unscaledTime - startedFadeAt < seconds)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - startedFadeAt) / seconds);
                endingBackground.color = Color.Lerp(start, end, t);
                yield return null;
            }

            endingBackground.color = end;
        }

        private void ShowEndingTitle(string text, Color color)
        {
            endingTitleText.text = text;
            endingTitleText.color = color;
            endingTitleText.rectTransform.localScale = Vector3.zero;
            endingTitleText.gameObject.SetActive(true);
            OrderEndingLayers();
        }

        private IEnumerator PopEndingTitle()
        {
            float startedPopAt = Time.unscaledTime;
            const float popSeconds = 0.28f;

            while (Time.unscaledTime - startedPopAt < popSeconds)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - startedPopAt) / popSeconds);
                float scale = Mathf.Lerp(0.1f, 1.2f, Mathf.Sin(t * Mathf.PI * 0.5f));
                endingTitleText.rectTransform.localScale = Vector3.one * scale;
                yield return null;
            }

            endingTitleText.rectTransform.localScale = Vector3.one;
        }

        private IEnumerator PlayFireworks()
        {
            fireworksRoot.gameObject.SetActive(true);
            fireworksRoot.SetAsLastSibling();
            ClearFireworks();

            Color[] colors =
            {
                new Color(1f, 0.18f, 0.18f, 1f),
                new Color(1f, 0.82f, 0.12f, 1f),
                new Color(0.18f, 0.5f, 1f, 1f),
                new Color(0.22f, 0.9f, 0.45f, 1f)
            };

            for (int burst = 0; burst < 5; burst++)
            {
                Vector2 center = new Vector2(Random.Range(-520f, 520f), Random.Range(-260f, 260f));
                for (int i = 0; i < 30; i++)
                {
                    RectTransform spark = CreatePanel("Spark", fireworksRoot, colors[(burst + i) % colors.Length]);
                    spark.anchorMin = new Vector2(0.5f, 0.5f);
                    spark.anchorMax = new Vector2(0.5f, 0.5f);
                    spark.pivot = new Vector2(0.5f, 0.5f);
                    spark.anchoredPosition = center;
                    spark.sizeDelta = new Vector2(18f, 18f);
                    StartCoroutine(AnimateSpark(spark, center, i, 30));
                }

                yield return new WaitForSecondsRealtime(0.18f);
            }
        }

        private IEnumerator AnimateSpark(RectTransform spark, Vector2 center, int index, int count)
        {
            Image image = spark.GetComponent<Image>();
            float angle = index * Mathf.PI * 2f / count;
            Vector2 target = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(130f, 300f);
            float startedSparkAt = Time.unscaledTime;
            const float sparkSeconds = 0.85f;

            while (Time.unscaledTime - startedSparkAt < sparkSeconds)
            {
                float t = Mathf.Clamp01((Time.unscaledTime - startedSparkAt) / sparkSeconds);
                spark.anchoredPosition = Vector2.Lerp(center, target, t);
                spark.localScale = Vector3.one * Mathf.Lerp(1f, 0.2f, t);
                Color color = image.color;
                color.a = 1f - t;
                image.color = color;
                yield return null;
            }

            Destroy(spark.gameObject);
        }

        private void ClearFireworks()
        {
            if (fireworksRoot == null)
            {
                return;
            }

            for (int i = fireworksRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(fireworksRoot.GetChild(i).gameObject);
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
                chasePreviewCamera.backgroundColor = new Color(0.18f, 0.13f, 0.13f, 1f);
                chasePreviewCamera.fieldOfView = 34f;
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
            FrameCulpritForPreview(spawnedCulprit);
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
                float runShake = Mathf.Sin(Time.unscaledTime * 65f) * Mathf.Lerp(0.08f, 0.24f, eased);
                Vector3 endPosition = cameraTransform.position
                    + cameraTransform.forward * culpritEndDistance
                    + Vector3.up * culpritPreviewYOffset;

                culprit.position = Vector3.Lerp(startPosition, endPosition, eased) + Vector3.up * runShake;
                Quaternion lookRotation = Quaternion.LookRotation(cameraTransform.position - culprit.position, Vector3.up);
                culprit.rotation = lookRotation * Quaternion.Euler(runShake * 30f, 0f, runShake * 18f);
                culprit.localScale = Vector3.Lerp(startScale, startScale * culpritFinalScaleMultiplier, eased);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.12f);
        }

        private void FrameCulpritForPreview(GameObject culprit)
        {
            Bounds bounds = GetRendererBounds(culprit);
            if (bounds.size == Vector3.zero)
            {
                return;
            }

            float height = Mathf.Max(bounds.size.y, 0.1f);
            float targetHeight = 3.2f;
            float scaleMultiplier = targetHeight / height;
            culprit.transform.localScale *= scaleMultiplier;

            bounds = GetRendererBounds(culprit);
            Vector3 cameraPosition = chasePreviewCamera.transform.position;
            Vector3 targetCenter = cameraPosition
                + chasePreviewCamera.transform.forward * culpritStartDistance
                + Vector3.up * 0.35f;

            culprit.transform.position += targetCenter - bounds.center;
        }

        private static Bounds GetRendererBounds(GameObject target)
        {
            Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
            Bounds bounds = new Bounds(target.transform.position, Vector3.zero);
            bool hasBounds = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return hasBounds ? bounds : new Bounds(target.transform.position, Vector3.zero);
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

            HideLowDuplicateRenderers(culprit);
        }

        private static void HideLowDuplicateRenderers(GameObject culprit)
        {
            Renderer[] renderers = culprit.GetComponentsInChildren<Renderer>(true);
            Bounds visibleBounds = GetRendererBounds(culprit);
            if (visibleBounds.size == Vector3.zero)
            {
                return;
            }

            float lowerCutoff = visibleBounds.center.y - visibleBounds.extents.y * 0.62f;
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                string rendererName = renderer.name.ToLowerInvariant();
                bool suspiciousName = rendererName.Contains("shadow") || rendererName.Contains("char_shadow");
                bool entirelyBelowBody = renderer.bounds.max.y < lowerCutoff;
                if (suspiciousName || entirelyBelowBody)
                {
                    renderer.enabled = false;
                }
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
                Color materialColor = material.HasProperty("_Color") ? material.color : Color.black;
                bool brokenPink = materialColor.r > 0.8f && materialColor.b > 0.8f && materialColor.g < 0.25f;
                if (materialName.Contains("char_shadow") ||
                    materialName.Contains("pink") ||
                    materialName.Contains("magenta") ||
                    shaderName.Contains("reflective/diffuse") ||
                    shaderName.Contains("internalerror") ||
                    shaderName.Contains("error") ||
                    brokenPink)
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
            Animator animator = culprit.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animator.speed = 1.7f;
            }

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
                clip.legacy = true;
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
            FontHelper.Apply(tmp);
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }
    }
}
