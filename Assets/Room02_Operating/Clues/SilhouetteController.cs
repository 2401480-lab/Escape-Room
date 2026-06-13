using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom
{
    public class SilhouetteController : MonoBehaviour
    {
        [Header("대상")]
        [SerializeField] private Transform player;
        [SerializeField] private GameObject silhouetteObject;
        [SerializeField] private Animator silhouetteAnimator;
        [SerializeField] private Collider[] silhouetteColliders;

        [Header("출현 지점")]
        [SerializeField] private Transform[] stageOneAppearPoints;
        [SerializeField] private Transform[] stageTwoAppearPoints;
        [SerializeField] private Vector3 behindPlayerOffset = new Vector3(0f, 0f, -1.5f);

        [Header("타이밍")]
        [SerializeField] private float stageOneInterval = 18f;
        [SerializeField] private float stageTwoInterval = 8f;
        [SerializeField] private float appearDuration = 2f;

        [Header("이벤트")]
        public UnityEvent OnFlickerRequested;
        public UnityEvent OnJumpscareRequested;

        private SilhouetteStage currentStage = SilhouetteStage.StageOne;
        private float nextAppearTime;
        private float hideTime;
        private bool visible;
        private bool behindPlayer;

        private void Awake()
        {
            if (silhouetteObject == null)
            {
                silhouetteObject = gameObject;
            }

            SetVisible(false);
            SetSilhouetteColliders(false);
        }

        private void OnEnable()
        {
            if (StoryProgressManager.Instance != null)
            {
                StoryProgressManager.Instance.OnPhaseChanged.AddListener(HandlePhaseChanged);
            }
        }

        private void OnDisable()
        {
            if (StoryProgressManager.Instance != null)
            {
                StoryProgressManager.Instance.OnPhaseChanged.RemoveListener(HandlePhaseChanged);
            }
        }

        private void Update()
        {
            if (currentStage == SilhouetteStage.Chase)
            {
                return;
            }

            if (behindPlayer)
            {
                FollowBehindPlayer();
                return;
            }

            if (visible && Time.time >= hideTime)
            {
                SetVisible(false);
                ScheduleNextAppearance();
            }

            if (!visible && Time.time >= nextAppearTime)
            {
                AppearAtStagePoint();
            }
        }

        public void SetStageOne()
        {
            currentStage = SilhouetteStage.StageOne;
            behindPlayer = false;
            SetSilhouetteColliders(false);
            ScheduleNextAppearance();
        }

        public void SetStageTwo()
        {
            currentStage = SilhouetteStage.StageTwo;
            behindPlayer = false;
            SetSilhouetteColliders(false);
            ScheduleNextAppearance();
        }

        public void SetStageThree()
        {
            currentStage = SilhouetteStage.StageThree;
            behindPlayer = true;
            SetSilhouetteColliders(false);
            SetVisible(true);
        }

        public void MaterializeAsJin()
        {
            currentStage = SilhouetteStage.Chase;
            behindPlayer = false;
            SetSilhouetteColliders(true);
            SetVisible(true);
            if (silhouetteAnimator != null)
            {
                silhouetteAnimator.SetTrigger("Materialize");
            }
        }

        public void PlayJumpscare()
        {
            SetSilhouetteColliders(false);
            SetVisible(true);
            OnJumpscareRequested?.Invoke();
            if (silhouetteAnimator != null)
            {
                silhouetteAnimator.SetTrigger("Jumpscare");
            }
        }

        private void HandlePhaseChanged(StoryPhase phase)
        {
            switch (phase)
            {
                case StoryPhase.ClueCollection:
                    SetStageOne();
                    break;
                case StoryPhase.TrueCulpritRevealed:
                    SetStageTwo();
                    break;
                case StoryPhase.KeyClueCollection:
                    SetStageThree();
                    break;
                case StoryPhase.ChaseEscape:
                    MaterializeAsJin();
                    break;
            }
        }

        private void AppearAtStagePoint()
        {
            Transform[] points = currentStage == SilhouetteStage.StageTwo ? stageTwoAppearPoints : stageOneAppearPoints;
            if (points != null && points.Length > 0)
            {
                Transform point = points[Random.Range(0, points.Length)];
                if (point != null)
                {
                    transform.SetPositionAndRotation(point.position, point.rotation);
                }
            }

            if (currentStage == SilhouetteStage.StageTwo)
            {
                OnFlickerRequested?.Invoke();
            }

            SetVisible(true);
            hideTime = Time.time + appearDuration;
        }

        private void FollowBehindPlayer()
        {
            Transform target = GetPlayer();
            if (target == null)
            {
                return;
            }

            transform.position = target.TransformPoint(behindPlayerOffset);
            Vector3 lookDirection = transform.position - target.position;
            lookDirection.y = 0f;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
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

        private void ScheduleNextAppearance()
        {
            float interval = currentStage == SilhouetteStage.StageTwo ? stageTwoInterval : stageOneInterval;
            nextAppearTime = Time.time + interval;
        }

        private void SetVisible(bool isVisible)
        {
            visible = isVisible;
            if (silhouetteObject != null && silhouetteObject.activeSelf != isVisible)
            {
                silhouetteObject.SetActive(isVisible);
            }
        }

        private void SetSilhouetteColliders(bool enabled)
        {
            if (silhouetteColliders == null || silhouetteColliders.Length == 0)
            {
                silhouetteColliders = GetComponentsInChildren<Collider>(true);
            }

            foreach (Collider silhouetteCollider in silhouetteColliders)
            {
                if (silhouetteCollider != null)
                {
                    silhouetteCollider.enabled = enabled;
                }
            }
        }
    }

    public enum SilhouetteStage
    {
        StageOne,
        StageTwo,
        StageThree,
        Chase
    }
}
