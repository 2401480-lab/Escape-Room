using UnityEngine;
using UnityEngine.Events;

namespace Room03Mystery
{
    // 조사 가능한 모든 오브젝트에 부착 (책상 단서, 서랍, 시체 머리, 캐비닛 등)
    // Inspector 에서 ClueData 에셋을 지정하고, 필요 시 선행 조건과 onInspected 이벤트를 연결.
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [Header("단서 데이터")]
        [SerializeField] private ClueData clueData;

        [Header("선행 조건 (없으면 비워둠)")]
        // 이 단서가 먼저 발견돼야 조사 가능 (예: 액자에서 보관함 번호 07 확인 후 서랍 열림)
        [SerializeField] private string requiredClueID = "";
        // 이 아이템을 인벤토리에 가지고 있어야 조사 가능 (예: 황동 열쇠로 약품 캐비닛 열기)
        [SerializeField] private string requiredItemID = "";

        [Header("동작 옵션")]
        // 한 번 조사 후 더 이상 상호작용 못 하게 Collider 비활성화
        [SerializeField] private bool disableAfterInspect = false;

        [Header("조사 시 추가 동작 — Inspector 에서 연결")]
        // 예: 서랍 슬라이드 열기, Room03PuzzleManager.SolvePuzzle("drawer") 호출 등
        public UnityEvent onInspected;

        private bool _inspected;

        public bool IsInteractable()
        {
            if (clueData == null) return false;

            if (!string.IsNullOrEmpty(requiredClueID))
            {
                if (Room03PuzzleManager.Instance == null ||
                    !Room03PuzzleManager.Instance.IsClueCollected(requiredClueID))
                    return false;
            }

            if (!string.IsNullOrEmpty(requiredItemID))
            {
                if (InventoryManager.Instance == null ||
                    !InventoryManager.Instance.HasItem(requiredItemID))
                    return false;
            }

            return true;
        }

        public string GetHoverLabel()
        {
            return clueData != null ? clueData.hoverLabel : "조사하기";
        }

        public void OnClick()
        {
            if (clueData == null) return;
            if (!IsInteractable()) return;

            CluePanel.Instance?.Show(clueData);

            if (!_inspected)
            {
                _inspected = true;
                Room03PuzzleManager.Instance?.RegisterClueFound(clueData.clueID);

                if (clueData.isCollectable)
                    InventoryManager.Instance?.AddItem(clueData);

                if (disableAfterInspect)
                    GetComponent<Collider>().enabled = false;

                onInspected?.Invoke();
            }
        }
    }
}
