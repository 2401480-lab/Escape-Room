using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace EscapeRoom
{
    [RequireComponent(typeof(Collider))]
    public class ChaseController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Transform player;
        [SerializeField] private Transform entranceExitDoor;
        [SerializeField] private GameObject audioManagerObject;
        [SerializeField] private float catchDistance = 1f;
        [SerializeField] private float nearVignetteDistance = 3f;
        [SerializeField] private float heartbeatDistance = 2f;
        [SerializeField] private float chaseMoveSpeed = 3.8f;
        [SerializeField] private float heartbeatCooldown = 1f;
        [SerializeField] private float chaseTimer = 120f;
        [SerializeField] private bool startDisabled = true;

        public UnityEvent OnChaseStarted;
        public UnityEvent OnCaughtPlayer;
        public UnityEvent OnEscapeSucceeded;
        public UnityEvent OnChaseTimerExpired;
        public UnityEvent OnBlackoutRequested;
        public UnityEvent<float> OnVignetteChanged;
        public UnityEvent OnHeartbeatRequested;

        private bool chasing;
        private float chaseTimeRemaining;
        private float nextHeartbeatTime;

        public bool IsChasing => chasing;
        public float ChaseTimeRemaining => chaseTimeRemaining;

        private void Awake()
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }

            chaseTimeRemaining = chaseTimer;
            ApplyAgentSpeed();
            if (startDisabled && navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
            }
        }

        private void Update()
        {
            if (!chasing)
            {
                return;
            }

            Transform target = GetPlayer();
            if (target != null && navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.SetDestination(target.position);
                UpdateDangerFeedback(Vector3.Distance(transform.position, target.position));
            }

            chaseTimeRemaining -= Time.deltaTime;
            if (chaseTimeRemaining <= 0f)
            {
                ChaseTimerExpired();
                return;
            }

            if (target != null && Vector3.Distance(transform.position, target.position) < catchDistance)
            {
                CatchPlayer();
            }
        }

        public void StartChase()
        {
            chasing = true;
            chaseTimeRemaining = chaseTimer;
            if (navMeshAgent != null)
            {
                ApplyAgentSpeed();
                navMeshAgent.enabled = true;
            }

            OnChaseStarted?.Invoke();
        }

        public void StopChase()
        {
            chasing = false;
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.ResetPath();
            }

            OnVignetteChanged?.Invoke(0f);
        }

        public bool TryEscape()
        {
            bool hasEscapeKey = StoryProgressManager.Instance != null && StoryProgressManager.Instance.HasEscapeKey;
            if (!chasing || !hasEscapeKey)
            {
                return false;
            }

            StopChase();
            StoryProgressManager.Instance?.MarkEscaped();
            OnEscapeSucceeded?.Invoke();
            GameOverUI.Instance?.ShowSurvivalEnding();
            return true;
        }

        private void CatchPlayer()
        {
            StopChase();
            OnBlackoutRequested?.Invoke();
            OnCaughtPlayer?.Invoke();
            StoryProgressManager.Instance?.MarkGameOver();
            GameOverUI.Instance?.PlayGameOver(GameOverReason.CaughtDuringChase);
        }

        private void ChaseTimerExpired()
        {
            StopChase();
            OnChaseTimerExpired?.Invoke();
            StoryProgressManager.Instance?.MarkGameOver();
            GameOverUI.Instance?.PlayGameOver(GameOverReason.ChaseTimerExpired);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!chasing)
            {
                return;
            }

            if (other.transform == GetPlayer() || other.CompareTag("Player"))
            {
                Transform target = GetPlayer();
                if (target != null && Vector3.Distance(transform.position, target.position) < catchDistance)
                {
                    CatchPlayer();
                }
            }
        }

        private void UpdateDangerFeedback(float distanceToPlayer)
        {
            float intensity = distanceToPlayer <= nearVignetteDistance
                ? Mathf.InverseLerp(nearVignetteDistance, catchDistance, distanceToPlayer)
                : 0f;
            OnVignetteChanged?.Invoke(intensity);

            if (distanceToPlayer <= heartbeatDistance && Time.time >= nextHeartbeatTime)
            {
                PlayHeartbeat();
                nextHeartbeatTime = Time.time + heartbeatCooldown;
            }
        }

        private void PlayHeartbeat()
        {
            if (audioManagerObject != null)
            {
                audioManagerObject.SendMessage("PlayHeartbeat", SendMessageOptions.DontRequireReceiver);
            }

            OnHeartbeatRequested?.Invoke();
        }

        private void ApplyAgentSpeed()
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.speed = chaseMoveSpeed;
            }
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
