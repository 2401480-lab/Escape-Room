using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class ClueAdminGuideOverlay : MonoBehaviour
    {
        [SerializeField] private bool adminGuideEnabled = true;
        [SerializeField] private bool editorOnly = true;
        [SerializeField] private Vector3 clueWorldOffset = new Vector3(0f, 1.65f, 0f);
        [SerializeField] private Color arrowColor = new Color(1f, 0.82f, 0.18f, 1f);
        [SerializeField] private Color labelColor = new Color(1f, 0.95f, 0.72f, 1f);
        [SerializeField] private float refreshInterval = 1f;

        private readonly Dictionary<ClueBoxInteractable, TextMeshProUGUI> arrowLabels = new Dictionary<ClueBoxInteractable, TextMeshProUGUI>();
        private Canvas guideCanvas;
        private RectTransform canvasRect;
        private float nextRefreshTime;

        private void Awake()
        {
            TrySetEditorOnlyTag(gameObject);
            EnsureCanvas();
            RefreshGuideTargets();
        }

        private void OnEnable()
        {
            EnsureCanvas();
            RefreshGuideTargets();
        }

        private void LateUpdate()
        {
            bool visible = adminGuideEnabled && (!editorOnly || Application.isEditor);
            if (guideCanvas != null && guideCanvas.gameObject.activeSelf != visible)
            {
                guideCanvas.gameObject.SetActive(visible);
            }

            if (!visible)
            {
                return;
            }

            if (Time.unscaledTime >= nextRefreshTime)
            {
                RefreshGuideTargets();
                nextRefreshTime = Time.unscaledTime + refreshInterval;
            }

            UpdateGuidePositions();
        }

        private void RefreshGuideTargets()
        {
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

        private TextMeshProUGUI CreateArrowLabel(ClueBoxInteractable clueBox)
        {
            GameObject labelObject = new GameObject($"AdminGuideArrow_{GetClueId(clueBox)}");
            TrySetEditorOnlyTag(labelObject);
            labelObject.transform.SetParent(guideCanvas.transform, false);

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            FontHelper.Apply(label);
            label.fontSize = 24f;
            label.color = labelColor;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            label.text = $"▼\n{GetClueName(clueBox)}";

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

                Vector3 screenPosition = mainCamera.WorldToScreenPoint(clueBox.transform.position + clueWorldOffset);
                bool isVisible = screenPosition.z > 0f;
                label.gameObject.SetActive(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    screenPosition,
                    null,
                    out Vector2 localPoint);

                label.rectTransform.anchoredPosition = localPoint;
                label.color = arrowColor;
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

            TrySetEditorOnlyTag(canvasObject);
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

        private static void TrySetEditorOnlyTag(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            try
            {
                target.tag = "EditorOnly";
            }
            catch (UnityException)
            {
                // EditorOnly is a built-in tag in normal Unity projects; ignore custom test contexts.
            }
        }
    }
}
