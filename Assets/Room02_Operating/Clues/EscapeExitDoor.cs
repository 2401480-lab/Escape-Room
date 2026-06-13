using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom
{
    public class EscapeExitDoor : MonoBehaviour
    {
        [SerializeField] private ChaseController chaseController;
        [SerializeField] private Transform player;
        [SerializeField] private float interactDistance = 2f;
        [SerializeField] private TextMeshProUGUI promptText;

        public UnityEvent OnEscapeDoorOpened;
        public UnityEvent OnEscapeDoorLocked;

        private const string OpenPrompt = "[E] 입구 문 열기";
        private const string LockedPrompt = "탈출 열쇠가 필요하다";

        private void Awake()
        {
            if (chaseController == null)
            {
                chaseController = FindObjectOfType<ChaseController>();
            }
        }

        private void Update()
        {
            Transform target = GetPlayer();
            bool inRange = target != null && Vector3.Distance(target.position, transform.position) <= interactDistance;
            SetPrompt(inRange);

            if (inRange && Input.GetKeyDown(KeyCode.E))
            {
                TryOpenExit();
            }
        }

        public bool TryOpenExit()
        {
            if (StoryProgressManager.Instance == null || !StoryProgressManager.Instance.HasEscapeKey)
            {
                OnEscapeDoorLocked?.Invoke();
                return false;
            }

            bool escaped = chaseController != null && chaseController.TryEscape();
            if (escaped)
            {
                OnEscapeDoorOpened?.Invoke();
            }

            return escaped;
        }

        private void SetPrompt(bool visible)
        {
            if (promptText == null)
            {
                return;
            }

            bool hasEscapeKey = StoryProgressManager.Instance != null && StoryProgressManager.Instance.HasEscapeKey;
            promptText.text = hasEscapeKey ? OpenPrompt : LockedPrompt;
            promptText.gameObject.SetActive(visible);
        }

        private Transform GetPlayer()
        {
            if (player != null)
            {
                return player;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                player = mainCamera.transform;
                return player;
            }

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }

            return player;
        }
    }
}
