using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Room02Operating
{
    // 수사 수첩 — 용의자 프로필
    public class NotebookUI : MonoBehaviour
    {
        [SerializeField] private GameObject notebookPanel;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;

        [Header("용의자 프로필 텍스트")]
        [SerializeField] private TextMeshProUGUI bongText;
        [SerializeField] private TextMeshProUGUI moonText;
        [SerializeField] private TextMeshProUGUI jinText;

        void Awake()
        {
            openButton.onClick.AddListener(OpenNotebook);
            closeButton.onClick.AddListener(CloseNotebook);
            notebookPanel.SetActive(false);

            bongText.text =
                "봉태현\n" +
                "동기: 중간 (페이크)\n" +
                "유안나와 오래된 지인. 사건 당일 현장에 있었음.\n" +
                "평소 하시호와 사이가 좋지 않았다는 소문이 있음.";

            moonText.text =
                "문수미\n" +
                "동기: 중간 (페이크)\n" +
                "수술 당일 보조 역할을 맡았던 인물.\n" +
                "유안나 피해 직전 수술실을 나간 것이 목격됨.";

            jinText.text =
                "진세웅\n" +
                "동기: 강함\n" +
                "하시호의 절친한 친구. 하시호의 죽음에 유안나가 연루되었다고 믿음.\n" +
                "수술실 구조와 약품 지식에 정통. 당일 현장 근처에서 목격됨.";
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
