using UnityEngine;

// Inspector에서 각 오브젝트의 단서 정보를 설정하는 데이터 컨테이너
[CreateAssetMenu(fileName = "ClueData", menuName = "ConanEscape/ClueData")]
public class ClueData : ScriptableObject
{
    public string clueID;           // 고유 ID (예: "clue_needle", "clue_key")
    public string displayName;      // 단서 이름 (인벤토리에 표시)
    public string description;      // 클릭 시 팝업에 표시할 텍스트
    public Sprite thumbnail;        // 인벤토리 슬롯 아이콘
    public Sprite detailImage;      // 팝업 확대 이미지
    public bool isCollectable;      // true면 인벤토리에 추가됨
    public string hoverLabel;       // 마우스 올렸을 때 표시할 라벨 (예: "피해자 둘러보기")
}
