using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 정답 시: 코난 독백 3단계 → 할아버지 자백 → 범행 재현 컷씬
// 오답 시: 오답 메시지 표시 (팀원들과 확정 후 채울 것)
public class EndingManager : MonoBehaviour
{
    [Header("정답 엔딩 UI")]
    [SerializeField] private GameObject endingPanel;
    [SerializeField] private TextMeshProUGUI monologueText;
    [SerializeField] private Button nextButton;

    [Header("오답 UI")]
    [SerializeField] private GameObject wrongAnswerPanel;
    [SerializeField] private TextMeshProUGUI wrongAnswerText;
    [SerializeField] private Button retryButton;   // 용의자 선택 화면으로 돌아가기

    [Header("범행 재현 컷씬 (이미지 슬라이드 방식)")]
    [SerializeField] private GameObject cutscenePanel;
    [SerializeField] private Image cutsceneImage;
    [SerializeField] private TextMeshProUGUI cutsceneCaption;
    [SerializeField] private Button cutsceneNextButton;

    [Header("컷씬 데이터")]
    [SerializeField] private Sprite[] cutsceneImages;
    [SerializeField] private string[] cutsceneCaptions;

    // 코난 독백 3단계 텍스트
    private readonly string[] _monologues =
    {
        "독백 1\n열쇠의 바늘구멍, 테이프 자국...\n범인은 줄로 열쇠를 조작해 밀실을 만들었습니다.",
        "독백 2\n책장에서 빼낸 책으로 카메라를 가리고,\n잠든 피해자에게 독침을 놓았습니다.",
        "독백 3\n이 정교한 낚싯줄 트릭을 실행할 수 있는 사람은 단 한 명.\n할아버지, 당신입니다!"
    };

    private readonly string _confession =
        "할아버지 자백\n\"내 딸을 불행하게 만든 그 녀석을 용서할 수 없었네...\"";

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

    // ─── 정답 엔딩 ────────────────────────────────────────────────

    public void PlayCorrectEnding()
    {
        _monologueStep = 0;
        endingPanel.SetActive(true);
        ShowMonologue(_monologueStep);
    }

    void ShowMonologue(int step)
    {
        if (step < _monologues.Length)
        {
            monologueText.text = _monologues[step];
        }
        else if (step == _monologues.Length)
        {
            // 자백 화면
            monologueText.text = _confession;
        }
        else
        {
            // 독백 끝 → 범행 재현 컷씬
            endingPanel.SetActive(false);
            StartCutscene();
        }
    }

    void OnNextClicked()
    {
        _monologueStep++;
        ShowMonologue(_monologueStep);
    }

    // ─── 범행 재현 컷씬 ───────────────────────────────────────────

    void StartCutscene()
    {
        if (cutsceneImages == null || cutsceneImages.Length == 0)
        {
            // 이미지 없으면 바로 클리어
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

    // 방 클리어 완료 → 팀원 통합 시 여기서 씬 전환 또는 이벤트 발생
    void OnRoomComplete()
    {
        RoomGameManager.Instance?.OnRoomCleared?.Invoke();
    }

    // ─── 오답 처리 ────────────────────────────────────────────────

    // 오답 연출은 팀원들과 확정 후 이 메서드를 채울 것
    public void PlayWrongAnswer()
    {
        wrongAnswerText.text =
            "추리가 틀렸습니다.\n범인은 밀실 트릭을 비웃으며 유유히 사라졌습니다.";
        wrongAnswerPanel.SetActive(true);
    }

    void OnRetry()
    {
        wrongAnswerPanel.SetActive(false);
        FindObjectOfType<SuspectSelection>()?.Show();
    }
}
