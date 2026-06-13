using UnityEngine;

namespace EscapeRoom
{
    [CreateAssetMenu(fileName = "NewClueData", menuName = "EscapeRoom/Clue Data")]
    public class ClueData : ScriptableObject
    {
        public string clueID;
        public string clueName;
        public string description;
        public string meaning;
        public string areaName;
        public ClueCategory category = ClueCategory.General;
        public bool isRequired;
    }

    public enum ClueCategory
    {
        General,
        KeyClue,
        EscapeKey
    }
}
