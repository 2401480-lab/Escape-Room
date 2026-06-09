using UnityEngine;
using UnityEngine.UI;

namespace Room01Conan
{
    // 최종 추리 — 용의자 3명 얼굴 카드 UI
    // 카드 선택 시 RoomGameManager.OnSuspectChosen() 호출
    public class SuspectSelection : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;

        [Header("카드 버튼")]
        [SerializeField] private Button wifeButton;
        [SerializeField] private Button secretaryButton;
        [SerializeField] private Button grandfatherButton;

        [Header("카드 이미지 (선택사항)")]
        [SerializeField] private UnityEngine.UI.Image wifePortrait;
        [SerializeField] private UnityEngine.UI.Image secretaryPortrait;
        [SerializeField] private UnityEngine.UI.Image grandfatherPortrait;

        void Awake()
        {
            wifeButton.onClick.AddListener(() => Choose(SuspectType.Wife));
            secretaryButton.onClick.AddListener(() => Choose(SuspectType.Secretary));
            grandfatherButton.onClick.AddListener(() => Choose(SuspectType.Grandfather));
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
