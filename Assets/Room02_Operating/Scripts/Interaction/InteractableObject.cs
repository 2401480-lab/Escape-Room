using UnityEngine;

namespace Room02Operating
{
    [RequireComponent(typeof(Collider))]
    public class InteractableObject : MonoBehaviour
    {
        [SerializeField] private ClueData clueData;
        [SerializeField] private string requiredClueID = "";
        [SerializeField] private bool disableAfterInspect = false;

        private bool _inspected;

        public bool IsInteractable()
        {
            if (clueData == null) return false;
            if (!string.IsNullOrEmpty(requiredClueID))
                return RoomGameManager.Instance.IsClueCollected(requiredClueID);
            return true;
        }

        public string GetHoverLabel() => clueData != null ? clueData.hoverLabel : "조사하기";

        public void OnClick()
        {
            if (clueData == null) return;
            CluePanel.Instance?.Show(clueData);

            if (!_inspected)
            {
                _inspected = true;
                RoomGameManager.Instance?.RegisterClueFound(clueData.clueID);

                if (clueData.isCollectable)
                    InventoryManager.Instance?.AddItem(clueData);

                if (disableAfterInspect)
                    GetComponent<Collider>().enabled = false;
            }
        }
    }
}
