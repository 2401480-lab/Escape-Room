using System;
using System.Collections.Generic;
using UnityEngine;

namespace EscapeRoom
{
    public class ClueJournalManager : MonoBehaviour
    {
        public static ClueJournalManager Instance { get; private set; }

        [SerializeField] private List<ClueData> allClues = new List<ClueData>();

        private readonly List<ClueData> collectedClues = new List<ClueData>();
        private readonly List<ClueData> keyClues = new List<ClueData>();

        public event Action OnCluesChanged;
        public event Action<ClueData> OnClueAdded;

        public IReadOnlyList<ClueData> AllClues => allClues;
        public IReadOnlyList<ClueData> CollectedClues => collectedClues;
        public IReadOnlyList<ClueData> KeyClues => keyClues;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public bool AddClue(ClueData clueData)
        {
            if (clueData == null || HasClue(clueData))
            {
                return false;
            }

            collectedClues.Add(clueData);
            if (clueData.category == ClueCategory.KeyClue && !keyClues.Contains(clueData))
            {
                keyClues.Add(clueData);
            }

            if (!allClues.Contains(clueData))
            {
                allClues.Add(clueData);
            }

            OnClueAdded?.Invoke(clueData);
            OnCluesChanged?.Invoke();
            return true;
        }

        public bool HasClue(ClueData clueData)
        {
            return clueData != null && collectedClues.Contains(clueData);
        }

        public void RegisterClueDefinition(ClueData clueData)
        {
            if (clueData != null && !allClues.Contains(clueData))
            {
                allClues.Add(clueData);
                OnCluesChanged?.Invoke();
            }
        }

        public void RegisterClueDefinitions(IEnumerable<ClueData> clueDataList)
        {
            if (clueDataList == null)
            {
                return;
            }

            bool changed = false;
            foreach (ClueData clueData in clueDataList)
            {
                if (clueData != null && !allClues.Contains(clueData))
                {
                    allClues.Add(clueData);
                    changed = true;
                }
            }

            if (changed)
            {
                OnCluesChanged?.Invoke();
            }
        }
    }
}
