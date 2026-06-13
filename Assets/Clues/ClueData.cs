using UnityEngine;

namespace EscapeRoom
{
    [CreateAssetMenu(fileName = "NewClueData", menuName = "EscapeRoom/Clue Data")]
    public class ClueData : ScriptableObject
    {
        public string clueName;
        public string description;
        public string meaning;
        public string areaName;
    }
}
