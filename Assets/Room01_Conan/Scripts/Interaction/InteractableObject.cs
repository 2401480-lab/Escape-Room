using UnityEngine;

// 조사 가능한 모든 오브젝트에 부착
// Inspector에서 ClueData 에셋을 드래그해서 지정
[RequireComponent(typeof(Collider))]
public class InteractableObject : MonoBehaviour
{
    [SerializeField] private ClueData clueData;

    // 이 오브젝트를 조사하려면 먼저 이 단서가 발견되어야 함 (선행 조건 없으면 비워둠)
    [SerializeField] private string requiredClueID = "";

    // 한 번 조사 후 사라져야 하는 오브젝트 (예: 독침 바늘)
    [SerializeField] private bool disableAfterInspect = false;

    private bool _inspected;

    public bool IsInteractable()
    {
        if (clueData == null) return false;

        // 선행 단서가 없거나, 이미 수집된 경우만 활성
        if (!string.IsNullOrEmpty(requiredClueID))
            return RoomGameManager.Instance.IsClueCollected(requiredClueID);

        return true;
    }

    public string GetHoverLabel()
    {
        return clueData != null ? clueData.hoverLabel : "조사하기";
    }

    public void OnClick()
    {
        if (clueData == null) return;

        // 팝업 표시
        CluePanel.Instance?.Show(clueData);

        // 단서 등록 (처음 조사할 때만)
        if (!_inspected)
        {
            _inspected = true;
            RoomGameManager.Instance?.RegisterClueFound(clueData.clueID);

            // 인벤토리에 추가
            if (clueData.isCollectable)
                InventoryManager.Instance?.AddItem(clueData);

            // 조사 후 비활성화 (독침 바늘 등)
            if (disableAfterInspect)
                GetComponent<Collider>().enabled = false;
        }
    }
}
