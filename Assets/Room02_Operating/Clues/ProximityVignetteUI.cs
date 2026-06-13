using UnityEngine;
using UnityEngine.UI;

namespace EscapeRoom
{
    public class ProximityVignetteUI : MonoBehaviour
    {
        [SerializeField] private Canvas vignetteCanvas;
        [SerializeField] private Image vignetteImage;
        [SerializeField] private ChaseController chaseController;
        [SerializeField] private Color vignetteColor = new Color(1f, 0f, 0f, 0f);
        [SerializeField] private float nearAlpha = 0.3f;
        [SerializeField] private float heartbeatAlpha = 0.6f;

        private void Awake()
        {
            EnsureUI();
            UpdateAlpha(0f);
        }

        private void OnEnable()
        {
            ChaseController targetChase = GetChaseController();
            if (targetChase != null)
            {
                targetChase.OnVignetteChanged.AddListener(UpdateAlpha);
            }
        }

        private void OnDisable()
        {
            if (chaseController != null)
            {
                chaseController.OnVignetteChanged.RemoveListener(UpdateAlpha);
            }
        }

        public void UpdateAlpha(float alpha)
        {
            EnsureUI();
            Color color = vignetteColor;
            color.a = SnapAlpha(alpha);
            vignetteImage.color = color;
            vignetteImage.enabled = color.a > 0f;
        }

        private float SnapAlpha(float alpha)
        {
            if (alpha >= heartbeatAlpha)
            {
                return heartbeatAlpha;
            }

            return alpha > 0f ? nearAlpha : 0f;
        }

        private ChaseController GetChaseController()
        {
            if (chaseController != null)
            {
                return chaseController;
            }

            chaseController = FindObjectOfType<ChaseController>();
            return chaseController;
        }

        private void EnsureUI()
        {
            if (vignetteCanvas == null)
            {
                GameObject canvasObject = GameObject.Find("ProximityVignetteCanvas");
                if (canvasObject == null)
                {
                    canvasObject = new GameObject("ProximityVignetteCanvas");
                    vignetteCanvas = canvasObject.AddComponent<Canvas>();
                    canvasObject.AddComponent<CanvasScaler>();
                    canvasObject.AddComponent<GraphicRaycaster>();
                }
                else
                {
                    vignetteCanvas = canvasObject.GetComponent<Canvas>();
                    if (vignetteCanvas == null)
                    {
                        vignetteCanvas = canvasObject.AddComponent<Canvas>();
                    }
                }
            }

            vignetteCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            if (vignetteImage != null)
            {
                return;
            }

            GameObject imageObject = new GameObject("ProximityVignetteImage");
            imageObject.transform.SetParent(vignetteCanvas.transform, false);
            vignetteImage = imageObject.AddComponent<Image>();
            vignetteImage.raycastTarget = false;
            vignetteImage.type = Image.Type.Filled;
            vignetteImage.fillMethod = Image.FillMethod.Radial360;
            vignetteImage.fillAmount = 1f;
            vignetteImage.preserveAspect = true;

            RectTransform rect = vignetteImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
