using System.Collections.Generic;
using UnityEngine;

namespace Room02Operating
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField] private List<InventorySlot> slots;

        private List<ClueData> _items = new List<ClueData>();

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddItem(ClueData data)
        {
            if (_items.Contains(data)) return;
            if (_items.Count >= slots.Count)
            {
                Debug.LogWarning("인벤토리 슬롯이 부족합니다.");
                return;
            }
            _items.Add(data);
            slots[_items.Count - 1].SetItem(data);
        }

        public bool HasItem(string clueID) => _items.Exists(d => d.clueID == clueID);
    }
}
