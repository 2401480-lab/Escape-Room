using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room01Conan
{
    // 수사 수첩 — 언제든지 열 수 있는 용의자 프로필 창
    // 오프닝 시 자동 오픈, 이후 버튼으로 토글
    public class NotebookUI : MonoBehaviour
    {
        [SerializeField] private GameObject notebookPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        [Header("용의자 프로필 텍스트")]
        [SerializeField] private TextMeshProUGUI wifeText;
        [SerializeField] private TextMeshProUGUI secretaryText;
        [SerializeField] private TextMeshProUGUI grandfatherText;

        void Awake()
        {
            openButton.onClick.AddListener(OpenNotebook);
            closeButton.onClick.AddListener(CloseNotebook);
            notebookPanel.SetActive(false);

            wifeText.text =
                "👩 아내\n" +
                "동기: 중간 (페이크)\n" +
                "남편 사망 시 거액의 생명보험 수령자.\n" +
                "남편과 사이가 평소에 안좋음.";

            secretaryText.text =
                "👔 비서\n" +
                "동기: 중간 (페이크)\n" +
                "최근 피해자에게 부당해고 통보를 받음.\n" +
                "평소에 과격한 언행과 폭력을 당함.";

            grandfatherText.text =
                "👴 할아버지\n" +
                "동기: 강함\n" +
                "딸(아내)이 매일 눈물로 전화함. 사위를 극도로 혐오.\n" +
                "바다낚시가 유일한 취미로 매일 낚싯줄과 장비를 직접 정비함.";
        }

        public void OpenNotebook() => notebookPanel.SetActive(true);

        public void CloseNotebook()
        {
            notebookPanel.SetActive(false);
            if (RoomGameManager.Instance.currentPhase == GamePhase.Opening)
                RoomGameManager.Instance.CloseOpeningNotebook();
        }

        public bool IsOpen => notebookPanel.activeSelf;
    }
}
