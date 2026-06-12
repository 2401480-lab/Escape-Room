using UnityEngine;

namespace Room02Operating
{
    [CreateAssetMenu(fileName = "ClueData", menuName = "OperatingEscape/ClueData")]
    public class ClueData : ScriptableObject
    {
        public string clueID;
        public string displayName;
        public string description;
        public Sprite thumbnail;
        public Sprite detailImage;
        public bool isCollectable;
        public string hoverLabel;
    }
}
