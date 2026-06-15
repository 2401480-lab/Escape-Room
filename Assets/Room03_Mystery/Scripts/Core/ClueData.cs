using UnityEngine;

namespace Room03Mystery
{
    // 각 오브젝트의 단서/아이템 정보를 담는 데이터 컨테이너 (ScriptableObject)
    // Project 창에서 우클릭 → Create → MorgueEscape → ClueData 로 생성
    [CreateAssetMenu(fileName = "ClueData", menuName = "MorgueEscape/ClueData")]
    public class ClueData : ScriptableObject
    {
        public string clueID;           // 고유 ID (예: "clue_report", "item_brass_key")
        public string displayName;      // 단서/아이템 이름 (인벤토리 표시)
        [TextArea] public string description;   // 클릭 시 팝업에 표시할 텍스트 (코드 조각도 여기에)
        public Sprite thumbnail;        // 인벤토리 슬롯 아이콘
        public Sprite detailImage;      // 팝업 확대 이미지
        public bool isCollectable;      // true 면 인벤토리에 추가됨 (예: 황동 열쇠, UV 펜라이트)
        public string hoverLabel;       // 마우스 오버 라벨 (예: "머리 살펴보기")
    }
}
