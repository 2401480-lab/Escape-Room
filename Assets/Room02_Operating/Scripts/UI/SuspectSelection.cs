using UnityEngine;
using UnityEngine.UI;

namespace Room02Operating
{
    public class SuspectSelection : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        [Header("카드 버튼")]
        [SerializeField] private Button bongButton;
        [SerializeField] private Button moonButton;
        [SerializeField] private Button jinButton;

        [Header("카드 이미지 (선택사항)")]
        [SerializeField] private Image bongPortrait;
        [SerializeField] private Image moonPortrait;
        [SerializeField] private Image jinPortrait;

        void Awake()
        {
            bongButton.onClick.AddListener(() => Choose(SuspectType.BongTaehyeon));
            moonButton.onClick.AddListener(() => Choose(SuspectType.MoonSumi));
            jinButton.onClick.AddListener(() => Choose(SuspectType.JinSewoong));
            panelRoot.SetActive(false);
        }

        public void Show() => panelRoot.SetActive(true);

        void Choose(SuspectType suspect)
        {
            panelRoot.SetActive(false);
            RoomGameManager.Instance?.OnSuspectChosen(suspect);
        }
    }
}
