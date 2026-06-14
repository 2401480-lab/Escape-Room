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

        private void Start()
        {
            if (playOnStart && bgmClip != null && audioSource != null && !audioSource.isPlaying)
            {
                audioSource.Play();
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
            audioSource.clip = bgmClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.priority = 128;
        }
    }
}
