# 방탈출 게임 — 팀 프로젝트

## 게임 흐름
```
온보딩 화면 → 테마(방) 선택 → 선택한 방 진행 → 클리어 후 방 선택으로 복귀
```

## 폴더 구조
```
Assets/
├── _Shared/               ← 팀 공통 (온보딩·방선택 씬, RoomLoader, GameData)
├── Room01_Conan/          ← 담당: [내 이름]  — 명탐정 코난 외교관 사건
├── Room02_Empty/          ← 담당: [팀원2 이름] — 에피소드 미정
└── Room03_Empty/          ← 담당: [팀원3 이름] — 에피소드 미정
```

## 팀원별 작업 규칙
1. **자기 방 폴더(`Room0X_이름/`)만 작업** — 다른 방 폴더 절대 수정 금지
2. `_Shared/`는 팀 전체 합의 후 수정
3. **네임스페이스 필수** — 아래 규칙 준수

## 네임스페이스 규칙 (충돌 방지 필수)
| 폴더 | namespace |
|------|-----------|
| `_Shared/Scripts/` | `EscapeGame` |
| `Room01_Conan/Scripts/` | `Room01Conan` |
| `Room02_.../Scripts/` | `Room02[팀원이름]` (예: `Room02Kim`) |
| `Room03_.../Scripts/` | `Room03[팀원이름]` |

```csharp
// 예시: Room02 담당자의 모든 스크립트 상단
namespace Room02Kim
{
    public class MyManager : MonoBehaviour { ... }
}
```

## Build Settings 씬 순서
| 번호 | 씬 이름 | 파일 위치 | 담당 |
|------|---------|-----------|------|
| 0 | `Onboarding` | `_Shared/Scenes/Onboarding.unity` | 팀 공통 |
| 1 | `RoomSelect` | `_Shared/Scenes/RoomSelect.unity` | 팀 공통 |
| 2 | `Room01_Conan` | `Room01_Conan/Scenes/Room01_Conan.unity` | 나 |
| 3 | `Room02` | `Room02_.../Scenes/Room02.unity` | 팀원 2 |
| 4 | `Room03` | `Room03_.../Scenes/Room03.unity` | 팀원 3 |

## 방 클리어 연결 방법 (각 방 담당자 필수)
Inspector에서 자기 방 `RoomGameManager.OnRoomCleared` 이벤트에
`EscapeGame.RoomLoader` 오브젝트의 `ReturnToRoomSelect()` 연결

## _Shared 스크립트 사용법
```csharp
// 다른 방 씬으로 이동
using EscapeGame;
EscapeGame.RoomLoader.Instance.LoadRoom(1);

// 방 클리어 기록
EscapeGame.GameData.Instance.SetRoomCleared(1);
```

## Unity 환경
- **버전**: Unity 6000.0.36f1 (Unity 6 LTS)
- **렌더 파이프라인**: URP
- **Serialization Mode**: Force Text
  (Edit → Project Settings → Editor → Asset Serialization → Force Text)

## Asset Store 에셋 (각자 직접 다운로드 필요)
`.gitignore`로 제외된 대용량 에셋 — Unity Asset Store에서 무료 다운로드
- Multistory Dungeons 2
- Fantasy Pixel Locks and Keys
- AlchemyLabProps
