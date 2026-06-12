using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room02Operating
{
    // 정답: 진세웅 독백 → 자백 → 범행 재현 컷씬
    // 오답: 오답 메시지 + 재시도
    public class EndingManager : MonoBehaviour
    {
        [Header("정답 엔딩 UI")]
        [SerializeField] private GameObject endingPanel;
        [SerializeField] private TextMeshProUGUI monologueText;
        [SerializeField] private Button nextButton;

        [Header("오답 UI")]
        [SerializeField] private GameObject wrongAnswerPanel;
        [SerializeField] private TextMeshProUGUI wrongAnswerText;
        [SerializeField] private Button retryButton;

        [Header("범행 재현 컷씬")]
        [SerializeField] private GameObject cutscenePanel;
        [SerializeField] private Image cutsceneImage;
        [SerializeField] private TextMeshProUGUI cutsceneCaption;
        [SerializeField] private Button cutsceneNextButton;

        [Header("컷씬 데이터")]
        [SerializeField] private Sprite[] cutsceneImages;
        [SerializeField] private string[] cutsceneCaptions;

        private readonly string[] _monologues =
        {
            "독백 1\n수술대 아래 페인트 자국...\n누군가 그 안에 숨어 발버둥 치는 연기를 했습니다.",
            "독백 2\n운동화의 페인트, 그리고 발가락 자국.\n수술대 아래에서 나온 사람은 딱 한 명입니다.",
            "독백 3\n하시호의 복수를 위해 모든 것을 계획한 사람...\n진세웅, 당신입니다!"
        };

        private readonly string _confession =
            "진세웅 자백\n\"하시호가 그녀 때문에 죽었어. 나는 그냥 눈을 감을 수 없었다...\"";

        private int _monologueStep;
        private int _cutsceneStep;

        void Awake()
        {
            nextButton.onClick.AddListener(OnNextClicked);
            cutsceneNextButton.onClick.AddListener(OnCutsceneNext);
            retryButton.onClick.AddListener(OnRetry);

            endingPanel.SetActive(false);
            wrongAnswerPanel.SetActive(false);
            cutscenePanel.SetActive(false);
        }

        public void PlayCorrectEnding()
        {
            _monologueStep = 0;
            endingPanel.SetActive(true);
            ShowMonologue(_monologueStep);
        }

        void ShowMonologue(int step)
        {
            if (step < _monologues.Length)
                monologueText.text = _monologues[step];
            else if (step == _monologues.Length)
                monologueText.text = _confession;
            else
            {
                endingPanel.SetActive(false);
                StartCutscene();
            }
        }

        void OnNextClicked()
        {
            _monologueStep++;
            ShowMonologue(_monologueStep);
        }

        void StartCutscene()
        {
            if (cutsceneImages == null || cutsceneImages.Length == 0)
            {
                OnRoomComplete();
                return;
            }
            _cutsceneStep = 0;
            cutscenePanel.SetActive(true);
            ShowCutsceneFrame(_cutsceneStep);
        }

        void ShowCutsceneFrame(int step)
        {
            if (step >= cutsceneImages.Length)
            {
                cutscenePanel.SetActive(false);
                OnRoomComplete();
                return;
            }
            cutsceneImage.sprite = cutsceneImages[step];
            cutsceneCaption.text = step < cutsceneCaptions.Length ? cutsceneCaptions[step] : "";
        }

        void OnCutsceneNext()
        {
            _cutsceneStep++;
            ShowCutsceneFrame(_cutsceneStep);
        }

        void OnRoomComplete()
        {
            RoomGameManager.Instance?.OnRoomCleared?.Invoke();
        }

        public void PlayWrongAnswer()
        {
            wrongAnswerText.text =
                "추리가 틀렸습니다.\n진실은 아직 밝혀지지 않은 채, 수술실은 다시 침묵에 잠겼습니다.";
            wrongAnswerPanel.SetActive(true);
        }

        void OnRetry()
        {
            wrongAnswerPanel.SetActive(false);
            FindObjectOfType<SuspectSelection>()?.Show();
        }
    }
}
