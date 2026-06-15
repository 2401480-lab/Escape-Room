using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace EscapeRoom
{
    [DisallowMultipleComponent]
    public class Room02FlashlightController : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private Light flashlight;
        [SerializeField] private Canvas overlayCanvas;
        [SerializeField] private RawImage flashlightMask;
        [SerializeField] private float beamRange = 18f;
        [SerializeField] private float spotAngle = 36f;
        [SerializeField] private float innerSpotAngle = 18f;
        [SerializeField] private float lightIntensity = 7f;
        [SerializeField] private float directionalLightIntensity = 0.03f;
        [SerializeField] private int maskTextureSize = 512;
        [SerializeField] private float clearRadius = 0.28f;
        [SerializeField] private float falloffRadius = 0.58f;
        [SerializeField] private float edgeAlpha = 0.88f;
        [SerializeField] private int overlaySortingOrder = -100;
        [SerializeField] private Color ambientLight = new Color(0.006f, 0.006f, 0.012f, 1f);
        [SerializeField] private Color fogColor = new Color(0.005f, 0.005f, 0.01f, 1f);
        [SerializeField] private Color flashlightColor = new Color(0.78f, 0.75f, 0.95f, 1f);

        private Texture2D maskTexture;

        private void Awake()
        {
            ApplyAtmosphere();
            EnsureFlashlight();
            EnsureOverlay();
        }

        private void OnEnable()
        {
            ApplyAtmosphere();
            EnsureFlashlight();
            EnsureOverlay();
        }

        private void LateUpdate()
        {
            EnsureFlashlight();
            FollowCamera();
        }

        private void OnValidate()
        {
            beamRange = Mathf.Max(1f, beamRange);
            spotAngle = Mathf.Clamp(spotAngle, 1f, 179f);
            innerSpotAngle = Mathf.Clamp(innerSpotAngle, 1f, spotAngle);
            lightIntensity = Mathf.Max(0f, lightIntensity);
            directionalLightIntensity = Mathf.Max(0f, directionalLightIntensity);
            maskTextureSize = Mathf.Clamp(maskTextureSize, 64, 1024);
            clearRadius = Mathf.Clamp01(clearRadius);
            falloffRadius = Mathf.Clamp(falloffRadius, clearRadius + 0.01f, 1f);
            edgeAlpha = Mathf.Clamp01(edgeAlpha);
        }

        private void EnsureFlashlight()
        {
            Camera gameCamera = GetTargetCamera();
            if (gameCamera == null)
            {
                return;
            }

            if (flashlight == null)
            {
                Transform existing = gameCamera.transform.Find("Room02_CameraFlashlight");
                GameObject lightObject = existing != null ? existing.gameObject : new GameObject("Room02_CameraFlashlight");
                lightObject.transform.SetParent(gameCamera.transform, false);
                flashlight = lightObject.GetComponent<Light>();
                if (flashlight == null)
                {
                    flashlight = lightObject.AddComponent<Light>();
                }
            }

            Transform lightTransform = flashlight.transform;
            if (lightTransform.parent != gameCamera.transform)
            {
                lightTransform.SetParent(gameCamera.transform, false);
            }

            lightTransform.localPosition = Vector3.zero;
            lightTransform.localRotation = Quaternion.identity;
            flashlight.type = LightType.Spot;
            flashlight.color = flashlightColor;
            flashlight.range = beamRange;
            flashlight.spotAngle = spotAngle;
            flashlight.innerSpotAngle = innerSpotAngle;
            flashlight.intensity = lightIntensity;
            flashlight.shadows = LightShadows.Soft;
            flashlight.renderMode = LightRenderMode.ForcePixel;
        }

        private void FollowCamera()
        {
            if (flashlight == null)
            {
                return;
            }

            Camera gameCamera = GetTargetCamera();
            if (gameCamera == null)
            {
                return;
            }

            flashlight.transform.SetPositionAndRotation(gameCamera.transform.position, gameCamera.transform.rotation);
        }

        private Camera GetTargetCamera()
        {
            if (targetCamera != null && targetCamera.isActiveAndEnabled)
            {
                return targetCamera;
            }

            targetCamera = Camera.main;
            return targetCamera;
        }

        private void EnsureOverlay()
        {
            if (overlayCanvas == null)
            {
                GameObject canvasObject = GameObject.Find("Room02_FlashlightMaskCanvas");
                if (canvasObject == null)
                {
                    canvasObject = new GameObject("Room02_FlashlightMaskCanvas");
                }

                overlayCanvas = canvasObject.GetComponent<Canvas>();
                if (overlayCanvas == null)
                {
                    overlayCanvas = canvasObject.AddComponent<Canvas>();
                }
            }

            overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            overlayCanvas.sortingOrder = overlaySortingOrder;

            CanvasScaler scaler = overlayCanvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                scaler = overlayCanvas.gameObject.AddComponent<CanvasScaler>();
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (flashlightMask == null)
            {
                Transform existingMask = overlayCanvas.transform.Find("Room02_FlashlightMask");
                GameObject maskObject = existingMask != null ? existingMask.gameObject : new GameObject("Room02_FlashlightMask");
                maskObject.transform.SetParent(overlayCanvas.transform, false);
                flashlightMask = maskObject.GetComponent<RawImage>();
                if (flashlightMask == null)
                {
                    flashlightMask = maskObject.AddComponent<RawImage>();
                }
            }

            flashlightMask.raycastTarget = false;
            flashlightMask.color = Color.white;
            flashlightMask.texture = maskTexture != null ? maskTexture : CreateMaskTexture();

            RectTransform rect = flashlightMask.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private Texture2D CreateMaskTexture()
        {
            int size = Mathf.Clamp(maskTextureSize, 64, 1024);
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Room02_FlashlightMaskTexture",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            Color32[] pixels = new Color32[size * size];
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float halfSize = size * 0.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center) / halfSize;
                    float t = Mathf.InverseLerp(clearRadius, falloffRadius, distance);
                    float alpha = Mathf.SmoothStep(0f, edgeAlpha, t);
                    pixels[(y * size) + x] = new Color32(0, 0, 0, (byte)Mathf.RoundToInt(alpha * 255f));
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            maskTexture = texture;
            return maskTexture;
        }

        private void ApplyAtmosphere()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = ambientLight;
            RenderSettings.ambientIntensity = 0.15f;
            RenderSettings.fog = true;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = 0.045f;

            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light sceneLight in lights)
            {
                if (sceneLight != null && sceneLight.type == LightType.Directional)
                {
                    sceneLight.intensity = directionalLightIntensity;
                    sceneLight.color = new Color(0.28f, 0.3f, 0.42f, 1f);
                }
            }
        }
    }
}
