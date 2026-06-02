# 방탈출 게임 — 팀 프로젝트

## 게임 개요
방 3개 중 하나를 선택해 각 방의 에피소드에서 범인을 찾는 방탈출 게임

## 폴더 구조
```
Assets/
├── _Shared/          ← 팀 공통 (방 선택 씬, RoomLoader, GameData)
├── Room01_Conan/     ← 담당: [내 이름] — 명탐정 코난 외교관 사건
├── Room02_Empty/     ← 담당: [팀원2 이름] — 에피소드 미정
└── Room03_Empty/     ← 담당: [팀원3 이름] — 에피소드 미정
```

## 팀원별 작업 규칙
- **자기 방 폴더(`Room0X_이름/`)만 작업**
- 다른 방 폴더는 절대 수정 금지
- `_Shared/`는 팀 전체 합의 후 수정

## Build Settings 씬 순서
| 번호 | 씬 | 담당 |
|------|----|------|
| 0 | `_Shared/Scenes/RoomSelect.unity` | 팀 공통 |
| 1 | `Room01_Conan/Scenes/Room01_Conan.unity` | 나 |
| 2 | `Room02_.../Scenes/Room02.unity` | 팀원 2 |
| 3 | `Room03_.../Scenes/Room03.unity` | 팀원 3 |

## 방 클리어 연결 방법 (각 방 담당자 필수)
자기 방 `RoomGameManager.OnRoomCleared` 이벤트에
`_Shared/Scripts/RoomLoader.cs`의 `ReturnToRoomSelect()` 연결

## Unity 설정
- **버전**: (팀이 사용하는 버전 기입)
- **렌더 파이프라인**: URP
- **Serialization Mode**: Force Text (Edit → Project Settings → Editor)

## Asset Store 에셋 (각자 직접 다운로드 필요)
`.gitignore`로 제외된 대용량 에셋 — Unity Asset Store에서 무료 다운로드
- Multistory Dungeons 2
- Fantasy Pixel Locks and Keys
- AlchemyLabProps
