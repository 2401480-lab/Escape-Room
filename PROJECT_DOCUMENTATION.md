# Escape-Room 프로젝트 문서

작성일: 2026-06-16  
현재 기준 브랜치: `feat/admin-y-skip`  
Unity 버전: `6000.0.36f1`  
렌더 파이프라인: Universal Render Pipeline `17.0.3`

## 1. 프로젝트 개요

`Escape-Room`은 Unity 기반 방탈출/추리 게임 프로젝트다. 현재 저장소에는 공통 로딩 시스템, Room01 추리 게임 원형, Room02 수술실 추리-탈출 콘텐츠, 테스트 스크립트, 외부 에셋 팩이 함께 들어 있다.

현재 구현의 중심은 `Room02_Operating`이다. Room02는 플레이어가 수술실과 병원 관련 공간을 탐색하면서 단서를 수집하고, 충분한 단서를 모으면 범인을 추리한 뒤, 탈출 열쇠와 엔딩 QTE로 이어지는 구조다. 게임은 단순히 단서만 모으는 방식이 아니라 다음 흐름을 하나의 플레이 루프로 연결한다.

```text
온보딩
  -> Room02 Show 씬 진입
  -> 단서 박스 조사
  -> 수사 노트 / 용의자 수첩 확인
  -> 범인찾기 단계 해금
  -> 용의자 선택
  -> 정답이면 탈출 열쇠 획득
  -> 추격 / 스페이스바 QTE
  -> 탈출 성공 또는 Game Over
```

## 2. 기술 환경

| 항목 | 내용 |
| --- | --- |
| 엔진 | Unity `6000.0.36f1` |
| 렌더링 | Universal Render Pipeline `17.0.3` |
| 입력 | Unity 구 Input API와 Input System UI 모듈 혼용 |
| UI | UGUI, TextMeshPro |
| 폰트 | Galmuri 계열 TMP 폰트 적용 |
| 테스트 | PowerShell 기반 구조/회귀 테스트 |
| 솔루션 | `Escape-Room.sln` |
| 주요 씬 | `Assets/Onboarding.unity`, `Assets/Room02_Operating/Scenes/Show.unity` |

주요 Unity 패키지는 `Packages/manifest.json`에 기록되어 있다. 핵심 패키지는 `com.unity.inputsystem`, `com.unity.ugui`, `com.unity.render-pipelines.universal`, `com.unity.test-framework`, `com.unity.ai.navigation` 등이다.

## 3. 루트 구조

```text
Escape-Room/
  Assets/
    _Shared/
    Room01_Conan/
    Room02_Operating/
    Abandoned_Asylum/
    Scenes/
    Settings/
    TextMesh Pro/
  Packages/
  ProjectSettings/
  Tests/
  CHANGELOG.md
  README.md
  Escape-Room.sln
```

### 주요 폴더 역할

| 경로 | 역할 |
| --- | --- |
| `Assets/_Shared/Scripts` | 방 공통 로더, 진행 데이터, 문 상호작용, 온보딩 UI |
| `Assets/Room01_Conan` | Room01용 원형 시스템 |
| `Assets/Room02_Operating` | Room02 수술실 추리/탈출 콘텐츠 |
| `Assets/Room02_Operating/Clues` | Room02 단서, 진행도, HUD, 범인 추리, Game Over 시스템 |
| `Assets/Room02_Operating/Ending` | 엔딩/QTE, 출구 상호작용, 열쇠 디버그 지급 |
| `Assets/Room02_Operating/Scenes` | Room02 플레이 씬 |
| `Assets/Room02_Operating/Resources` | 런타임 로드용 프리팹 |
| `Tests` | PowerShell 기반 회귀 테스트 |

## 4. Build Settings와 씬 흐름

현재 `ProjectSettings/EditorBuildSettings.asset` 기준 활성 씬은 다음 두 개다.

| 순서 | 씬 경로 | 역할 |
| --- | --- | --- |
| 0 | `Assets/Onboarding.unity` | 게임 시작/온보딩 화면 |
| 1 | `Assets/Room02_Operating/Scenes/Show.unity` | Room02 메인 플레이 씬 |

공통 로더는 `Assets/_Shared/Scripts/RoomLoader.cs`에 있다. `RoomLoader.LoadRoom(2)`는 Room02 씬 이름인 `Show`를 로드하도록 되어 있다.

## 5. 공통 시스템

### `RoomLoader`

경로: `Assets/_Shared/Scripts/RoomLoader.cs`

역할:

- 온보딩 씬 로드
- 방 선택 씬 로드
- 방 번호 기반 씬 로드
- 방 클리어 후 방 선택 씬 복귀

현재 Room02는 `Show` 씬으로 연결되어 있다.

### `GameData`

경로: `Assets/_Shared/Scripts/GameData.cs`

역할:

- Room01, Room02, Room03 클리어 여부 저장
- `SetRoomCleared(int roomNumber)`로 방 클리어 기록
- `AllRoomsCleared()`로 전체 클리어 여부 확인

### `PlayerMove`

경로: `Assets/PlayerMove.cs`

역할:

- `CharacterController` 기반 1인칭 이동
- WASD 이동
- 마우스 시점 이동
- `Left Shift` 또는 `Right Shift` 달리기
- 플레이어에 `DoorInteractor`가 없으면 자동 추가
- 카메라를 플레이어 자식으로 맞추고 커서를 잠금 상태로 전환

### `DoorInteractor`

경로: `Assets/_Shared/Scripts/DoorInteractor.cs`

역할:

- 기본 상호작용 키 `E`
- 카메라 전방 레이캐스트와 보조 SphereCast 사용
- 가까운 문은 `OverlapSphereNonAlloc`로도 탐지
- 이름 또는 문 크기 형태를 기준으로 door-like 오브젝트 판정
- 일반 문 상호작용 시 `ControlHintUI.SetDoorPromptVisible()`와 연결

## 6. Room02 전체 플레이 흐름

Room02의 핵심 상태는 `StoryProgressManager`가 관리한다.

경로: `Assets/Room02_Operating/Clues/StoryProgressManager.cs`

### 진행 단계

```csharp
public enum StoryPhase
{
    ClueCollection,
    TrueCulpritRevealed,
    KeyClueCollection,
    SuspectSelection,
    ChaseEscape,
    GameOver,
    Escaped
}
```

| 단계 | 의미 |
| --- | --- |
| `ClueCollection` | 기본 단서 수집 |
| `TrueCulpritRevealed` | 특정 핵심 단서로 진범 윤곽이 드러남 |
| `KeyClueCollection` | 탈출 열쇠 관련 단서 수집 단계 |
| `SuspectSelection` | 범인 선택 가능 |
| `ChaseEscape` | 추격/QTE 단계 |
| `GameOver` | 실패 |
| `Escaped` | 탈출 성공 |

### 진행 조건

| 조건 | 동작 |
| --- | --- |
| 단서 10개 이상 | 범인 선택 가능 |
| 전체 스토리 단서 15개 이상 | 탈출 열쇠 자동 지급 가능 |
| 핵심 열쇠 단서 3개 수집 | 열쇠 준비 이벤트 발생 |
| 정답 범인 선택 | 열쇠 지급 후 추격/QTE 시작 |
| 오답 범인 선택 | Game Over |
| 추리 타이머 종료 | Game Over |

## 7. 단서 시스템

### `ClueData`

경로: `Assets/Room02_Operating/Clues/ClueData.cs`

`ClueData`는 ScriptableObject 기반 단서 데이터다.

필드:

| 필드 | 의미 |
| --- | --- |
| `clueID` | 진행도 판정에 쓰는 고유 ID |
| `clueName` | 단서 이름 |
| `description` | 상세 설명 |
| `meaning` | 추리상 의미 |
| `areaName` | 단서가 속한 구역 |
| `category` | 일반 단서, 핵심 단서, 탈출 열쇠 구분 |
| `isRequired` | 필수 단서 여부 |

### 단서 에셋 위치

| 위치 | 내용 |
| --- | --- |
| `Assets/Room02_Operating/Clues/Normal` | 일반 단서 에셋 |
| `Assets/Room02_Operating/Clues/KeyClue` | 열쇠 관련 핵심 단서 에셋 |

현재 핵심 열쇠 단서는 다음 세 개다.

- `KeyClue_진세웅의쪽지`
- `KeyClue_온도경고스티커`
- `KeyClue_긁힌자국`

### `ClueJournalManager`

경로: `Assets/Room02_Operating/Clues/ClueJournalManager.cs`

역할:

- 수집된 단서 목록 관리
- 핵심 단서 목록 별도 관리
- 중복 수집 방지
- UI 갱신 이벤트 제공
- `StoryProgressManager`가 단서 추가 이벤트를 구독해 진행도를 갱신

### `ClueBoxInteractable`

경로: `Assets/Room02_Operating/Clues/ClueBoxInteractable.cs`

역할:

- Room02 단서 박스 상호작용 담당
- 기본 조사 키는 `F`
- `E`는 문 전용이라 단서 박스에서 사용하지 않음
- 카메라 위치 기준 360도 근접 탐지
- 거리와 시선 방향 점수로 가장 적합한 박스 선택
- 수집 후 박스를 사라지게 하지 않고 조사 완료 시각 상태로 변경
- 단서 팝업이 열려 있을 때 조사 프롬프트 숨김
- 플레이어가 거리 밖으로 나가면 현재 타깃을 비우고 `[F] 박스 조사하기` 프롬프트 숨김

### `ClueBoxRuntimeAdapter`

경로: `Assets/Room02_Operating/Clues/ClueBoxRuntimeAdapter.cs`

역할:

- 기존 씬의 단서 마커를 Room02 런타임 박스 프리팹으로 변환
- `Resources.Load<GameObject>("Room02_ClueBox")` 사용
- `ClueBoxInteractable`을 붙이고 단서 데이터를 복사
- 원래 마커 오브젝트는 숨김 처리

## 8. 수사 노트와 용의자 수첩

### `ClueJournalUI`

경로: `Assets/Room02_Operating/Clues/ClueJournalUI.cs`

역할:

- `HUD_Canvas` 위에 런타임 UI 생성
- 좌측 상단에 `수사 노트 (J)`와 `용의자 (K)` 버튼 생성
- `J` 또는 `Tab`: 수집 증거 창 열기/닫기
- `K`: 용의자 수첩 열기/닫기
- 창 내부에서 `1`, `2`로 탭 전환
- `ESC`로 수첩 닫기
- 수집된 단서와 미수집 단서를 구분 표시
- 새로 얻은 단서 카드 강조
- 용의자별 수집된 힌트 카드 표시
- 스크롤 뷰 사용으로 카드 겹침 방지
- 수첩이 열렸을 때 커서를 풀고, 닫으면 기존 커서 상태 복원

## 9. 범인 추리 시스템

### `EndingUI`

경로: `Assets/Room02_Operating/Clues/EndingUI.cs`

역할:

- 범인 선택 UI 생성
- `범인찾기 (G)` HUD 버튼 생성
- `G` 키로 범인 선택 UI 열기
- 마우스 없이 숫자 `1`, `2`, `3`, `4`로 용의자 선택
- 선택 전 `SuspectConfirmUI` 확인 팝업 호출
- 정답/오답 처리

현재 용의자 선택지는 다음 네 명이다.

| 번호 | 용의자 | 판정 |
| --- | --- | --- |
| 1 | 진세웅 | 정답 |
| 2 | 봉태현 | 오답 |
| 3 | 문수미 | 오답 |
| 4 | 오세진 | 오답 |

정답 처리 흐름:

```text
진세웅 선택
  -> 정답 이벤트 발생
  -> 화면 암전/실루엣 연출
  -> EscapeKeyState.GrantKey()
  -> 탈출 열쇠 획득 안내
  -> StoryPhase.ChaseEscape
  -> ChaseController.StartChase()
  -> EscapeChaseQTE.StartOrCreate()
```

오답 처리 흐름:

```text
오답 용의자 선택
  -> 오답 이벤트 발생
  -> 실루엣/점프스케어 연출
  -> GameOverUI.PlayGameOver(GameOverReason.WrongAnswer)
  -> StoryPhase.GameOver
```

### `범인찾기 (G)` 버튼 배치

현재 `범인찾기 (G)` 버튼은 오른쪽 상단 설정 영역이 아니라, 왼쪽 상단 `용의자 (K)` 버튼 옆에 표시된다. 배경은 노란색 50% 투명도이며 글자는 검정색으로 대비를 높였다.

## 10. 열쇠 상태와 엔딩 연동

### `EscapeKeyState`

경로: `Assets/Room02_Operating/Clues/EscapeKeyState.cs`

팀원 엔딩/QTE 코드와 병합하기 위해 분리된 공용 열쇠 상태다.

```csharp
EscapeRoom.EscapeKeyState.GrantKey();
EscapeRoom.EscapeKeyState.HasKey;
```

| API | 역할 |
| --- | --- |
| `GrantKey()` | 탈출 열쇠 보유 상태로 변경 |
| `HasKey` | 현재 열쇠 보유 여부 |
| `Reset()` | 열쇠 상태 초기화 |

### `EscapeKeyNoticeUI`

경로: `Assets/Room02_Operating/Clues/EscapeKeyNoticeUI.cs`

역할:

- 열쇠 획득 안내 표시
- 정답 범인을 맞혔을 때 “탈출 열쇠” 획득 안내
- 관리자 `Y` 스킵 중에는 열쇠 자막을 억제하도록 `StoryProgressManager`와 연동

## 11. 탈출문과 QTE

### `EscapeExitDoor`

경로: `Assets/Room02_Operating/Clues/EscapeExitDoor.cs`

역할:

- 출구 문 근처 접근 감지
- 열쇠가 없으면 잠김 안내
- 열쇠가 있으면 `E` 또는 `F`로 탈출 QTE 시작
- `EscapeKeyState.HasKey` 또는 `StoryProgressManager.HasEscapeKey` 확인

### `EscapeExitController`

경로: `Assets/Room02_Operating/Ending/EscapeExitController.cs`

역할:

- 엔딩 폴더 쪽 출구 상호작용 컨트롤러
- 열쇠가 있는 상태에서 `E` 또는 `F` 입력 시 QTE 시작
- `EscapeChaseQTE.StartOrCreate()` 호출

### `EscapeChaseQTE`

경로: `Assets/Room02_Operating/Ending/EscapeChaseQTE.cs`

역할:

- 탈출 직전 스페이스바 연타 QTE
- 제한 시간 `5초`
- 필요 입력 수 `30회`
- 메시지:

```text
범인이 쫓아오고 있습니다.
스페이스를 연타하세요!
```

성공:

- 탈출 성공 메시지
- 흰색 페이드
- `FINISH!` 엔딩
- 홈 버튼 표시

실패:

- 실패 메시지
- 검은색 페이드
- 빨간색 `GAME OVER`
- 홈 버튼 표시

## 12. Game Over 시스템

### `GameOverUI`

경로: `Assets/Room02_Operating/Clues/GameOverUI.cs`

역할:

- 최종 Game Over 화면 생성
- 빨간색 `GAME OVER` 표시
- 오답, 추리 타이머 종료, 추격 실패 등 실패 이유별 메시지 처리
- 범인 점프스케어/달려드는 연출
- `Evil Witch Laughter` 효과음 재생

효과음 리소스 경로:

```text
Resources/Audio/GameOver/EvilWitchLaughter
```

Game Over 사유:

```csharp
public enum GameOverReason
{
    WrongAnswer,
    CaughtDuringChase,
    DeductionTimerExpired,
    ChaseTimerExpired
}
```

## 13. 타이머 시스템

### `TimerUI`

경로: `Assets/Room02_Operating/Clues/TimerUI.cs`

역할:

- `StoryProgressManager.CurrentTimerRemaining` 표시
- 기본 추리 제한 시간은 20분
- 3분 이하부터 긴급 색상 표시
- 추격 단계에서는 추격 타이머 색상 사용
- 관리자 `U` 실패 테스트 시 5초 카운트다운 표시

현재 위치:

- 오른쪽 상단 기준 `(-160, -24)`
- 설정 버튼과 겹치지 않도록 왼쪽으로 배치

## 14. HUD와 설정 UI

### `HudRuntimeBootstrapper`

경로: `Assets/Room02_Operating/Clues/HudRuntimeBootstrapper.cs`

역할:

- `Show` 씬 로드 시 Room02 런타임 시스템 자동 생성
- `HUD_Canvas` 보장
- `EventSystem` 보장
- 다음 객체들을 자동 생성:

```text
ClueJournalManager
StoryProgressManager
EndingUI
GameOverUI
ClueJournalUI
TimerUI
ControlHintUI
SettingsUI
CluePickupPopupUI
ClueBoxRuntimeAdapter
IntroScenarioUI
Room02FlashlightController
Room02_BGM
```

### `ControlHintUI`

경로: `Assets/Room02_Operating/Clues/ControlHintUI.cs`

역할:

- 좌하단 키보드 조작 안내 표시
- `SHIFT - 빨리 달리기` 항상 표시
- `E - 문열기` 항상 표시
- 문 근처 상태 업데이트 API는 남겨두되, 표시 자체는 항상 켜둠

### `SettingsUI`

경로: `Assets/Room02_Operating/Clues/SettingsUI.cs`

역할:

- 오른쪽 상단 `설정 (ESC)` 버튼 생성
- `ESC`로 설정 창 열기/닫기
- 수사 노트가 ESC를 처리하는 프레임에는 설정창이 동시에 열리지 않도록 방지
- 볼륨/감도 슬라이더
- 조작법 탭

설정창 조작법에는 WASD, Left Shift, Mouse, E, F, J/Tab, K, G, ESC, 클릭/Space 등이 정리된다.

## 15. 관리자/시연용 단축키

발표와 테스트를 위해 관리자 단축키가 추가되어 있다.

| 키 | 기능 |
| --- | --- |
| `Y` | 모든 단서를 수집 처리하고 범인 추리 가능 상태로 이동 |
| `U` | 타이머를 5초로 만들어 실패/Game Over 흐름 테스트 |
| `G` | 범인 선택 UI 열기 |
| `1`, `2`, `3`, `4` | 용의자 선택 |
| `F9` | 엔딩 폴더 쪽 열쇠 디버그 지급 |

`Y` 스킵은 바로 QTE로 넘어가지 않고, 범인 추리 단계까지만 해금한다. 이후 `범인찾기 (G)` 또는 `G` 키로 범인을 선택해야 한다.

## 16. 플레이어 조작

| 입력 | 동작 |
| --- | --- |
| `WASD` | 이동 |
| `Mouse` | 시점 이동 |
| `Left Shift` / `Right Shift` | 빨리 달리기 |
| `F` | 단서 박스 조사 / 팝업 닫기 |
| `E` | 문 열기 |
| `J` / `Tab` | 수사 노트 |
| `K` | 용의자 수첩 |
| `G` | 범인찾기 |
| `1~4` | 범인 선택 UI에서 용의자 선택 |
| `ESC` | 설정 또는 수첩 닫기 |
| `Space` | 인트로 넘김 / QTE 연타 |

## 17. 시각/청각 연출

### `SilhouetteController`

역할:

- 스토리 단계에 따라 실루엣 연출 전환
- 진범 실체화
- 오답/실패 시 점프스케어

### `ProximityVignetteUI`

역할:

- 플레이어와 특정 위험 요소의 거리감에 따라 화면 분위기 강화

### `Room02FlashlightController`

역할:

- Room02 손전등/시야 보조 연출

### `Room02BgmPlayer`

경로: `Assets/Room02_Operating/Audio/Room02BgmPlayer.cs`

역할:

- Room02 배경음악 런타임 재생

## 18. 폰트와 UI 톤

### `FontHelper`

경로: `Assets/Room02_Operating/Clues/FontHelper.cs`

역할:

- Galmuri 계열 TMP 폰트 적용
- 런타임 생성 UI의 한글 표시 통일

### `HorrorUITheme`

경로: `Assets/Room02_Operating/Clues/HorrorUITheme.cs`

역할:

- 공포 분위기의 색상 정의
- 텍스트, 패널, 버튼 스타일 헬퍼 제공
- HUD와 팝업의 시각 톤 통일

## 19. Room01 상태

`Assets/Room01_Conan`에는 별도의 원형 시스템이 남아 있다.

주요 구성:

- `RoomGameManager`
- `ClueData`
- `HoverDetector`
- `InteractableObject`
- `InventoryManager`
- `NotebookUI`
- `SuspectSelection`
- `EndingManager`

Room01은 현재 Room02처럼 최근 작업의 중심은 아니지만, 공통 로더와 같은 저장소 안에서 방별 콘텐츠 구조의 기준 역할을 한다.

## 20. 테스트 구성

테스트는 `Tests/*.ps1` PowerShell 스크립트로 구성된다. 대부분 소스 파일의 구조, 문자열, API 연결, 씬/에셋 배치 조건을 검증하는 회귀 테스트다.

주요 테스트 범주:

| 테스트 | 검증 내용 |
| --- | --- |
| `ClueJournalSystem.Tests.ps1` | 단서 데이터, 수첩, 수집 이벤트 |
| `ClueBoxInteractable.Tests.ps1` | 박스 조사, 거리 프롬프트, 런타임 박스 어댑터 |
| `Room02FifteenClueStory.Tests.ps1` | 15개 단서 기반 스토리 흐름 |
| `StoryChaseFlow.Tests.ps1` | 범인 선택, 열쇠 획득, QTE 연결 |
| `EscapeKeyStateMerge.Tests.ps1` | 팀원 엔딩과 공유하는 열쇠 상태 계약 |
| `AdminShortcutKeys.Tests.ps1` | `Y`, `U`, `G`, `1~4` 관리자/키보드 흐름 |
| `ControlHintUI.Tests.ps1` | 조작 안내 HUD |
| `HudOverlayUI.Tests.ps1` | 타이머, 설정, 수첩, HUD 배치 |
| `GameOverJumpscare.Tests.ps1` | Game Over 연출과 효과음 |
| `Room02EndingMerge.Tests.ps1` | 엔딩 폴더 병합 계약 |
| `TimerUI.Tests.ps1` | 타이머 표시와 긴급 색상 |
| `DoorInteractor.Tests.ps1` | 문 탐지와 `E` 상호작용 |

전체 테스트 실행 예시:

```powershell
Get-ChildItem -Path .\Tests -Filter *.ps1 |
  Where-Object { $_.Name -notin @('CulpritAssetScope.Tests.ps1','Room02StartPlacement.Tests.ps1') } |
  ForEach-Object { & $_.FullName }
```

현재 작업 흐름에서 `CulpritAssetScope.Tests.ps1`, `Room02StartPlacement.Tests.ps1`는 기존 별도 이슈가 있는 테스트로 취급되어 제외하고 검증해 왔다.

빌드 검증:

```powershell
dotnet build .\Escape-Room.sln
```

## 21. 개발 및 버전 관리 규칙

프로젝트 루트의 AGENTS 지침 기준:

- 새 기능/수정은 브랜치에서 작업
- 작업 완료 시 `CHANGELOG.md`에 한글 작업 이력 추가
- 커밋 메시지는 한글 접두사 사용
  - `기능:`
  - `수정:`
  - `문서:`
  - `리팩토링:`
- 사용자 또는 Unity가 만든 미관련 변경은 되돌리지 않음
- 필요한 파일만 좁게 스테이징

현재 최근 커밋들은 Room02의 HUD, 범인찾기, QTE, Game Over, 조작 안내, 프롬프트 수정에 집중되어 있다.

## 22. 최근 핵심 구현 요약

최근 구현된 주요 변경:

- Galmuri 폰트 적용
- `U` 키 5초 실패 테스트
- `Y` 키 단서 수집/범인추리 스킵
- `G` 키 범인찾기
- 숫자 `1~4` 용의자 선택
- 정답 범인 선택 후 탈출 열쇠 획득 안내
- `EscapeKeyState` 공용 열쇠 상태 연동
- 엔딩 폴더 QTE 병합
- 출구 문과 스페이스바 QTE 연결
- Game Over 마녀 웃음 효과음
- `범인찾기 (G)` 버튼을 용의자 수첩 옆으로 이동
- `E - 문열기`, `SHIFT - 빨리 달리기` 조작 안내 HUD
- 박스 조사 프롬프트가 거리 밖에서 사라지도록 수정

## 23. 병합 포인트

팀원 엔딩/QTE 작업과 맞춰야 하는 핵심 계약은 다음과 같다.

```csharp
EscapeRoom.EscapeKeyState.GrantKey();
EscapeRoom.EscapeKeyState.HasKey;
```

출구 문 이름은 가능하면 다음 이름으로 통일하는 것이 안전하다.

```text
ExitDoor
```

열쇠가 있는 상태에서 출구 문에 `E` 또는 `F`로 상호작용하면 QTE가 시작되는 구조가 현재 엔딩 코드에 반영되어 있다.

## 24. 현재 주의 사항

- 작업 트리에는 여러 미관련 씬/에셋 변경이 남아 있을 수 있다. 새 작업 시 `git status --short`로 범위를 확인하고 필요한 파일만 스테이징해야 한다.
- `README.md`와 일부 오래된 스크립트 출력은 콘솔 인코딩 때문에 깨져 보일 수 있다.
- Room02는 런타임 부트스트랩으로 HUD와 매니저를 생성하므로, 씬에 임시 HUD 오브젝트를 직접 박는 방식은 피하는 편이 안전하다.
- 테스트는 PowerShell 기반 구조 검증이 많기 때문에, 문자열/메서드명/배치 좌표를 바꿀 때 관련 테스트 갱신이 필요하다.

## 25. 발표용 한 줄 요약

이 프로젝트의 현재 핵심은 “수술실에서 단서를 수집하고, 범인을 추리한 뒤, 열쇠를 얻어 추격 QTE로 탈출하는 Unity 기반 공포 추리 방탈출 시스템”이다.

