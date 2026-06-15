using UnityEngine;

namespace EscapeRoom
{
    public class Room02BgmPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip bgmClip;
        [SerializeField, Range(0f, 1f)] private float volume = 0.35f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnStart = true;

        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetOrCreateAudioSource();
            ApplySourceSettings();
        }

        private void OnEnable()
        {
            audioSource = GetOrCreateAudioSource();
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

            if (bgmClip == null && audioSource.clip != null)
            {
                bgmClip = audioSource.clip;
            }

            audioSource.clip = bgmClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.playOnAwake = playOnStart;
            audioSource.spatialBlend = 0f;
            audioSource.priority = 128;
        }

        private void TryPlay()
        {
            if (!playOnStart || bgmClip == null || audioSource == null || audioSource.isPlaying)
            {
                return;
            }

            audioSource.Play();
        }
    }
}
