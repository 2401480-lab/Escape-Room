using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EscapeRoom
{
    public class Room02BgmPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip bgmClip;
        [SerializeField] private string resourcesClipPath = "Room02_Audio/dk-atmosphere";
        [SerializeField] private string editorClipAssetPath = "Assets/Room02_Operating/Audio/music/darkness/dk-atmosphere.aif";
        [SerializeField, Range(0f, 1f)] private float volume = 0.65f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnStart = true;

        private AudioSource audioSource;
        private bool missingClipWarningLogged;

        private void Awake()
        {
            audioSource = GetOrCreateAudioSource();
            ResolveBgmClip();
            ApplySourceSettings();
        }

        private void OnEnable()
        {
            audioSource = GetOrCreateAudioSource();
            ResolveBgmClip();
            ApplySourceSettings();
            TryPlay();
        }

        private void Start()
        {
            TryPlay();
        }

        private void Update()
        {
            if (playOnStart && audioSource != null && bgmClip != null && !audioSource.isPlaying)
            {
                TryPlay();
            }
        }

        private void OnValidate()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                ApplySourceSettings();
            }
        }

        private AudioSource GetOrCreateAudioSource()
        {
            AudioSource source = GetComponent<AudioSource>();
            if (source == null)
            {
                source = gameObject.AddComponent<AudioSource>();
            }

            return source;
        }

        private void ApplySourceSettings()
        {
            if (audioSource == null)
            {
                return;
            }

            ResolveBgmClip();

            audioSource.clip = bgmClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.playOnAwake = playOnStart;
            audioSource.spatialBlend = 0f;
            audioSource.priority = 128;
            audioSource.mute = false;
            audioSource.enabled = true;
        }

        private void TryPlay()
        {
            ResolveBgmClip();
            ApplySourceSettings();

            if (!playOnStart || bgmClip == null || audioSource == null || audioSource.isPlaying)
            {
                if (playOnStart && bgmClip == null && !missingClipWarningLogged)
                {
                    Debug.LogWarning("[Room02BgmPlayer] BGM 클립을 찾지 못해 재생할 수 없습니다.");
                    missingClipWarningLogged = true;
                }

                return;
            }

            audioSource.Play();
        }

        private void ResolveBgmClip()
        {
            if (bgmClip != null)
            {
                return;
            }

            if (audioSource != null && audioSource.clip != null)
            {
                bgmClip = audioSource.clip;
                return;
            }

            if (!string.IsNullOrWhiteSpace(resourcesClipPath))
            {
                bgmClip = Resources.Load<AudioClip>(resourcesClipPath);
                if (bgmClip != null)
                {
                    return;
                }
            }

#if UNITY_EDITOR
            if (!string.IsNullOrWhiteSpace(editorClipAssetPath))
            {
                bgmClip = AssetDatabase.LoadAssetAtPath<AudioClip>(editorClipAssetPath);
            }
#endif
        }
    }
}
