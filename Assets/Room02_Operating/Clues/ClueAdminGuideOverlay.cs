using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom
{
    public class ClueAdminGuideOverlay : MonoBehaviour
    {
        [SerializeField] private bool adminGuideEnabled = true;
        [SerializeField] private bool allowRuntimeAdminGuide = false;
        [SerializeField] private bool drawEditorSceneGuides = true;
        [SerializeField] private Vector3 clueWorldOffset = new Vector3(0f, 1.65f, 0f);
        [SerializeField] private Color arrowColor = new Color(1f, 0.82f, 0.18f, 1f);
        [SerializeField] private Color labelColor = new Color(1f, 0.95f, 0.72f, 1f);
        [SerializeField] private float edgeViewportPadding = 0.08f;
        [SerializeField] private float refreshInterval = 1f;
        [SerializeField] private float sceneGuideHeight = 2.35f;
        [SerializeField] private float sceneArrowHeadSize = 0.24f;
        [SerializeField] private float sceneClueMarkerRadius = 0.45f;

        private readonly Dictionary<ClueBoxInteractable, TextMeshProUGUI> arrowLabels = new Dictionary<ClueBoxInteractable, TextMeshProUGUI>();
        private Canvas guideCanvas;
        private RectTransform canvasRect;
        private float nextRefreshTime;

        private bool IsGuideVisible => adminGuideEnabled && allowRuntimeAdminGuide;

        private void Awake()
        {
            if (IsGuideVisible)
            {
                EnsureCanvas();
                RefreshGuideTargets();
            }
        }

        private void OnEnable()
        {
            if (IsGuideVisible)
            {
                EnsureCanvas();
                RefreshGuideTargets();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                CollectAllCluesAndGrantKey();
            }
        }

        private void LateUpdate()
        {
            bool visible = IsGuideVisible;
            SetCanvasVisible(visible);

            if (!visible)
            {
                ClearGuideTargets();
                return;
            }

            EnsureCanvas();

            if (Time.unscaledTime >= nextRefreshTime)
            {
                RefreshGuideTargets();
                nextRefreshTime = Time.unscaledTime + refreshInterval;
            }

            UpdateGuidePositions();
        }

        private void CollectAllCluesAndGrantKey()
        {
            ClueBoxInteractable[] clueBoxes = FindObjectsOfType<ClueBoxInteractable>(true);
            foreach (ClueBoxInteractable clueBox in clueBoxes)
            {
                if (clueBox != null)
                {
                    clueBox.AdminCollectClue();
                }
            }

            StoryProgressManager.Instance?.GrantEscapeKeyFromAdminSkip();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!IsGuideVisible || !drawEditorSceneGuides)
            {
                return;
            }

            ClueBoxInteractable[] clueBoxes = FindObjectsOfType<ClueBoxInteractable>(true);
            foreach (ClueBoxInteractable clueBox in clueBoxes)
            {
                if (clueBox == null)
                {
                    continue;
                }

                DrawSceneGuide(clueBox);
            }
        }

        private void DrawSceneGuide(ClueBoxInteractable clueBox)
        {
            Vector3 cluePosition = clueBox.transform.position;
            Vector3 arrowTip = cluePosition + Vector3.up * 0.35f;
            Vector3 arrowStart = cluePosition + Vector3.up * sceneGuideHeight;
            Vector3 cameraRight = GetSceneCameraRight();

            Gizmos.color = arrowColor;
            Gizmos.DrawLine(arrowStart, arrowTip);
            Gizmos.DrawLine(arrowTip, arrowTip + Vector3.up * sceneArrowHeadSize - cameraRight * sceneArrowHeadSize);
            Gizmos.DrawLine(arrowTip, arrowTip + Vector3.up * sceneArrowHeadSize + cameraRight * sceneArrowHeadSize);
            Gizmos.DrawSphere(arrowTip, sceneArrowHeadSize * 0.35f);
            Gizmos.DrawWireSphere(cluePosition, sceneClueMarkerRadius);
        }

        private static Vector3 GetSceneCameraRight()
        {
            SceneView sceneView = SceneView.currentDrawingSceneView;
            return sceneView != null && sceneView.camera != null
                ? sceneView.camera.transform.right
                : Vector3.right;
        }
#endif

        private void RefreshGuideTargets()
        {
            if (!IsGuideVisible)
            {
                ClearGuideTargets();
                SetCanvasVisible(false);
                return;
            }

            EnsureCanvas();
            ClueBoxInteractable[] clueBoxes = FindObjectsOfType<ClueBoxInteractable>(true);
            HashSet<ClueBoxInteractable> activeClues = new HashSet<ClueBoxInteractable>(clueBoxes);

            foreach (ClueBoxInteractable clueBox in clueBoxes)
            {
                if (clueBox == null || arrowLabels.ContainsKey(clueBox))
                {
                    continue;
                }

                arrowLabels[clueBox] = CreateArrowLabel(clueBox);
            }

            List<ClueBoxInteractable> staleClues = new List<ClueBoxInteractable>();
            foreach (ClueBoxInteractable clueBox in arrowLabels.Keys)
            {
                if (!activeClues.Contains(clueBox))
                {
                    staleClues.Add(clueBox);
                }
            }

            foreach (ClueBoxInteractable staleClue in staleClues)
            {
                TextMeshProUGUI label = arrowLabels[staleClue];
                if (label != null)
                {
                    Destroy(label.gameObject);
                }

                arrowLabels.Remove(staleClue);
            }
        }

        private void ClearGuideTargets()
        {
            foreach (TextMeshProUGUI label in arrowLabels.Values)
            {
                if (label != null)
                {
                    Destroy(label.gameObject);
                }
            }

            arrowLabels.Clear();
        }

        private TextMeshProUGUI CreateArrowLabel(ClueBoxInteractable clueBox)
        {
            GameObject labelObject = new GameObject($"AdminGuideArrow_{GetClueId(clueBox)}");
            labelObject.transform.SetParent(guideCanvas.transform, false);

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(label);
            label.fontSize = 24f;
            label.color = labelColor;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.text = "\u25BC\n" + GetClueName(clueBox);

            RectTransform rect = label.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(260f, 70f);
            return label;
        }

        private void UpdateGuidePositions()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null || canvasRect == null)
            {
                return;
            }

            foreach (KeyValuePair<ClueBoxInteractable, TextMeshProUGUI> pair in arrowLabels)
            {
                ClueBoxInteractable clueBox = pair.Key;
                TextMeshProUGUI label = pair.Value;
                if (clueBox == null || label == null)
                {
                    continue;
                }

                Vector3 viewportPosition = mainCamera.WorldToViewportPoint(clueBox.transform.position + clueWorldOffset);
                if (viewportPosition.z < 0f)
                {
                    viewportPosition.x = 1f - viewportPosition.x;
                    viewportPosition.y = 1f - viewportPosition.y;
                }

                float clampedX = Mathf.Clamp(viewportPosition.x, edgeViewportPadding, 1f - edgeViewportPadding);
                float clampedY = Mathf.Clamp(viewportPosition.y, edgeViewportPadding, 1f - edgeViewportPadding);
                Vector2 screenPosition = new Vector2(clampedX * Screen.width, clampedY * Screen.height);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    null,
                    out Vector2 localPoint);

                bool onScreen = viewportPosition.z > 0f
                    && viewportPosition.x >= 0f
                    && viewportPosition.x <= 1f
                    && viewportPosition.y >= 0f
                    && viewportPosition.y <= 1f;

                label.rectTransform.anchoredPosition = localPoint;
                label.color = onScreen ? labelColor : arrowColor;
                label.gameObject.SetActive(true);
            }
        }

        private void EnsureCanvas()
        {
            if (guideCanvas != null)
            {
                return;
            }

            GameObject canvasObject = GameObject.Find("Admin_ClueGuideCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("Admin_ClueGuideCanvas");
            }

            guideCanvas = canvasObject.GetComponent<Canvas>();
            if (guideCanvas == null)
            {
                guideCanvas = canvasObject.AddComponent<Canvas>();
            }

            guideCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            guideCanvas.sortingOrder = 120;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = canvasObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            {
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            canvasRect = guideCanvas.GetComponent<RectTransform>();
        }

        private void SetCanvasVisible(bool visible)
        {
            if (guideCanvas != null && guideCanvas.gameObject.activeSelf != visible)
            {
                guideCanvas.gameObject.SetActive(visible);
            }
        }

        private static string GetClueId(ClueBoxInteractable clueBox)
        {
            return clueBox != null && clueBox.clueData != null && !string.IsNullOrWhiteSpace(clueBox.clueData.clueID)
                ? clueBox.clueData.clueID
                : clueBox != null ? clueBox.name : "Unknown";
        }

        private static string GetClueName(ClueBoxInteractable clueBox)
        {
            return clueBox != null && clueBox.clueData != null && !string.IsNullOrWhiteSpace(clueBox.clueData.clueName)
                ? clueBox.clueData.clueName
                : GetClueId(clueBox);
        }
    }
}
