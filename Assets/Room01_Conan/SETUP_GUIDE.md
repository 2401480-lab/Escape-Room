# 코난 방 — Unity 세팅 가이드

## 스크립트 구조
```
Scripts/
  Core/
    ClueData.cs          ← ScriptableObject, 각 단서 데이터
    RoomGameManager.cs   ← 게임 진행 상태 관리 (싱글톤)
  Interaction/
    HoverDetector.cs     ← Main Camera에 부착
    InteractableObject.cs ← 조사 가능한 오브젝트마다 부착
  UI/
    CluePanel.cs         ← 단서 팝업 패널
    InventoryManager.cs  ← 인벤토리 (싱글톤)
    InventorySlot.cs     ← 슬롯 하나
    NotebookUI.cs        ← 수사 수첩
    DeductionPopup.cs    ← 추리 팝업 (노랑/초록)
    SuspectSelection.cs  ← 최종 용의자 선택 카드 UI
  Ending/
    EndingManager.cs     ← 독백 → 자백 → 컷씬
```

## 단서 ID 목록 (ClueData에서 clueID 필드에 정확히 입력)
| ID                  | 단서 내용                            | 수집 여부 |
|---------------------|--------------------------------------|-----------|
| clue_ear            | 귀 밑 바늘구멍과 핏자국 → 독살       | X         |
| clue_needle         | 카펫의 독침 바늘                      | O         |
| clue_key            | 안쪽 구멍 뚫린 열쇠                  | O         |
| clue_key_tape       | 열쇠 손잡이 테이프 자국 (인벤토리 확대) | X        |
| clue_doorframe      | 책장·문틀의 마찰 흔적                | X         |
| clue_camera         | 홈캠 앞 책 더미                      | X         |
| clue_bookshelf_gap  | 책장 빈 공간 (4~5권 분량)            | X         |

## RoomGameManager Inspector 설정
- halfTimeClueIDs: clue_ear, clue_needle, clue_key
- fullTimeClueIDs: clue_ear, clue_needle, clue_key, clue_key_tape, clue_doorframe, clue_camera, clue_bookshelf_gap

## 씬 오브젝트 체크리스트
- [ ] Empty GameObject → RoomGameManager 부착
- [ ] Main Camera → HoverDetector 부착, HoverText(TMP) 연결
- [ ] Canvas → CluePanel, InventoryManager, NotebookUI, DeductionPopup, SuspectSelection, EndingManager
- [ ] 각 조사 오브젝트 → Collider + InteractableObject 부착 + ClueData 에셋 연결
- [ ] Layer: 조사 가능 오브젝트에 "Interactable" 레이어 설정 → HoverDetector의 interactableLayer에 지정

## 팀원 통합 시 연결 포인트
- RoomGameManager.OnRoomCleared → 다음 씬 로드 or 방 선택 화면 복귀
- RoomGameManager.OnRoomFailed  → 오답 처리 (현재 미정)
- EndingManager.PlayWrongAnswer() 내부 채울 것
```
