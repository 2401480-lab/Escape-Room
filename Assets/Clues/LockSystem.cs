using UnityEngine;
using UnityEngine.Events;

namespace EscapeRoom
{
    public class LockSystem : MonoBehaviour
    {
        [Header("보관실 사물함")]
        [SerializeField] private string storageLockerCode = "201";
        [SerializeField] private GameObject storageLockerCodePanel;
        [SerializeField] private GameObject storageLockerContents;

        [Header("냉장 약품함")]
        [SerializeField] private GameObject fridgeMedicineBox;
        [SerializeField] private GameObject escapeKeyObject;

        [Header("이벤트")]
        public UnityEvent OnStorageLockerUnlocked;
        public UnityEvent OnWrongStorageLockerCode;
        public UnityEvent OnFridgeMedicineBoxUnlocked;
        public UnityEvent OnEscapeKeyCollected;

        private bool storageLockerUnlocked;
        private bool fridgeMedicineBoxUnlocked;

        public bool StorageLockerUnlocked => storageLockerUnlocked;
        public bool FridgeMedicineBoxUnlocked => fridgeMedicineBoxUnlocked;

        private void Awake()
        {
            if (storageLockerContents != null)
            {
                storageLockerContents.SetActive(false);
            }

            if (escapeKeyObject != null)
            {
                escapeKeyObject.SetActive(false);
            }
        }

        public bool TryEnterCode(string enteredCode)
        {
            if (storageLockerUnlocked)
            {
                return true;
            }

            if (enteredCode == storageLockerCode)
            {
                storageLockerUnlocked = true;
                if (storageLockerCodePanel != null)
                {
                    storageLockerCodePanel.SetActive(false);
                }

                if (storageLockerContents != null)
                {
                    storageLockerContents.SetActive(true);
                }

                OnStorageLockerUnlocked?.Invoke();
                return true;
            }

            OnWrongStorageLockerCode?.Invoke();
            return false;
        }

        public bool TryUnlockFridgeMedicineBox()
        {
            if (fridgeMedicineBoxUnlocked)
            {
                return true;
            }

            StoryProgressManager progress = StoryProgressManager.Instance;
            if (progress == null || !progress.HasAllKeyClues())
            {
                return false;
            }

            fridgeMedicineBoxUnlocked = true;
            if (fridgeMedicineBox != null)
            {
                fridgeMedicineBox.SetActive(false);
            }

            if (escapeKeyObject != null)
            {
                escapeKeyObject.SetActive(true);
            }

            OnFridgeMedicineBoxUnlocked?.Invoke();
            return true;
        }

        public bool TryCollectEscapeKey()
        {
            if (!TryUnlockFridgeMedicineBox())
            {
                return false;
            }

            StoryProgressManager.Instance?.CollectEscapeKey();
            if (escapeKeyObject != null)
            {
                escapeKeyObject.SetActive(false);
            }

            OnEscapeKeyCollected?.Invoke();
            return true;
        }
    }
}
