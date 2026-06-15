### [2026-06-13 23:00:00]
- 한글 폰트 지원 추가: malgun.ttf(맑은 고딕)를 Assets/Fonts/에 복사
- KoreanFontSetup 에디터 스크립트 추가 (Tools > Room02 > Setup Korean Font)
  - Dynamic Atlas TMP 폰트 에셋 자동 생성 (MalgunGothic_TMP.asset)
  - TMP Settings 기본 폰트로 자동 등록

### [2026-06-13 22:00:00]
- ShowSceneSetup 에디터 스크립트 추가 (Tools > Room02 > Convert Show Scene to Game)
  - Show.unity 기존 데모 Capsule 제거, CharacterController 기반 Player로 교체
  - 카메라 눈높이 1.7m 적용, HUD_Canvas·TimerUI·단서팝업 자동 생성
  - 노란색 테스트 단서 큐브 1개 배치 (F키 수집 테스트용)
  - Show.unity 기존 라이팅·건물·Light Probe 그대로 유지

### [2026-06-13 21:30:00]
- SceneLightingSetup 에디터 스크립트 추가 (Tools > Room02 > Setup Scene Lighting)
  - Ambient Mode를 Flat + 거의 검정으로 설정 (Show.unity와 동일)
  - Directional Light 강도 0.15, 차가운 파란색으로 조정
  - 복도 PointLight 4개 자동 생성 (_CorridorLights 루트)
- ClueSceneSetupTool에 단일 테스트 큐브 배치 메뉴 추가 (Tools > Clues > Place Single Test Clue)
  - 기존 Clues 루트 큐브 전부 삭제 후 PlayerStart 앞 2m에 1개 배치
  - cast_notice ClueData 자동 연결, F키 수집 테스트용

### [2026-06-13 02:00:00]
- Scene_Corridor, Scene_DressingRoom, Scene_OperatingRoom 세 씬에 HUD_Canvas 배치 완료
- TimerUI, ProximityVignetteUI, CluePickupPopupUI, SuspectConfirmUI, InteractionPromptUI 생성 확인

### [2026-06-13 01:00:00]
- SceneUISetup.cs 에디터 스크립트 추가 (Tools > Room02 > Setup Scene UI)
- HUD_Canvas 생성: ScreenSpaceOverlay, ScaleWithScreenSize 1920x1080
- TimerUI, ProximityVignetteUI, CluePickupPopupUI, SuspectConfirmUI, InteractionPromptUI 자동 배치
- 기존 씬 오브젝트(StoryProgressManager 등) 참조 자동 연결
- 중복 실행 방지 (HUD_Canvas 존재 시 스킵)

### [2026-06-13 00:00:00]
- 단서 전면 확장: 19개 → 26개, 시나리오 흐름 5단계로 재구성
  - StoryPhase: cast_notice, memorial_frame, visitor_log, security_log, hasho_will, ward_calendar
  - MisdirectionPhase: yoanna_note, nurse_log, medical_certificate, conversation_memo_a, isolation_bloodstain, bong_rebuttal
  - MotivePhase: torn_letter_piece_a/b, cctv_memo, phone_memo, sumi_memo, makeup_diary, mirror_message
  - EvidencePhase: poison_ampoule, hidden_camera, jin_sneakers, gloves, locked_locker, paint_footprints, paint_toolbox
  - FinalPhase: under_table_space, yoanna_relic
- RoomGameManager 페이즈 5단계로 재설계 (StoryPhase → MisdirectionPhase → MotivePhase → EvidencePhase → FinalPhase)
- 추리 팝업 5개로 확장 (봉태현 미스디렉션 팝업 추가)
- SceneSetupTool 구역별 그룹화 (01~06 네이밍) 및 EndingUI_Trigger 추가
- ClueDataGenerator 전체 갱신 지원 (기존 에셋 덮어쓰기)

### [2026-06-12 01:00:00]
- 단서 목록 19개 확정 및 구역별 배분 완료
  - 구역1 입구 로비: clue_newspaper, clue_visitor_log, clue_incident_report
  - 구역2 복도/대기실: clue_cctv_memo, clue_staff_schedule, clue_broken_locker
  - 구역3 병실: clue_hasho_photo, clue_hasho_diary, clue_medicine_bottle, clue_letter
  - 구역4 보관실/분장실: clue_poison_bottle, clue_storage_log, clue_paint_can, clue_gloves
  - 구역5 수술실: clue_shoe_print, clue_toe_print, clue_under_table_dust, clue_poison_glass, clue_yoanna_body
- ClueDataGenerator.cs 에디터 스크립트 추가 (Tools > Room02 > Generate All Clue Assets)
- RoomGameManager 단서 ID 목록 및 추리 팝업 텍스트 시나리오에 맞게 업데이트

### [2026-06-12 00:00:00]
- Room02_Operating 폴더 신규 생성: 절규의 수술실 시나리오 전용 스크립트 작성
- 네임스페이스 Room02Operating으로 분리 (Room01Conan과 독립)
- 등장인물 변경: 봉태현/문수미/진세웅(범인) 반영
- SuspectType enum 수정: BongTaehyeon / MoonSumi / JinSewoong
- GamePhase enum 수정: LobbyPhase / MidPhase / FinalPhase 구역 구조 반영
- 추리 팝업 내용 수정: 수술대 아래 흔적 + 독약 경로
- 최종 추리 팝업 수정: 운동화/발가락 페인트 자국 결정적 증거
- 엔딩 독백/자백 텍스트 수술실 버전으로 교체
- NotebookUI 용의자 프로필 텍스트 수술실 버전으로 교체
- RoomLoader.cs: Room02 씬 이름을 Scene_OperatingRoom으로 확정

### [2026-06-12 22:04:04]
- 로컬 Unity 프로젝트를 원격 저장소 기준 young 브랜치에 연결하고 신규 에셋 및 프로젝트 설정 변경을 커밋 대상으로 정리
- Unity 프로젝트에서 생성되는 로컬 IDE 설정 폴더가 커밋되지 않도록 .gitignore에 .vscode 제외 규칙 추가

### [2026-06-12 22:18:23]
- 절규의 수술실 스테이지용 Scene_Corridor, Scene_DressingRoom, Scene_OperatingRoom 씬 3개를 생성하고 Build Settings에 등록
- SceneLoader.cs를 추가하여 복도, 분장실, 수술실 씬 전환 메서드와 현재 씬 재시작 메서드 구현
- 씬 구성과 SceneLoader API를 확인하는 PowerShell 검증 테스트 추가
- 공유 씬 플레이스홀더의 Unity meta 파일을 함께 추가하여 에셋 GUID 누락 방지

### [2026-06-12 22:27:42]
- Scene_Corridor, Scene_DressingRoom, Scene_OperatingRoom 씬에 StageRoot, PlayerStart, Clues, Doors, Triggers 기본 루트 오브젝트 배치
- Scene_Corridor 씬에 SceneLoader 오브젝트와 SceneLoader 컴포넌트를 추가하여 첫 씬에서 씬 전환 관리자가 생성되도록 구성
- 씬 기본 구조와 SceneLoader 컴포넌트 배치를 검증하도록 SceneLoader.Tests.ps1 테스트 확장

### [2026-06-12 22:38:07]
- PlayerMove가 transform.position 직접 이동으로 콜라이더를 우회하던 문제를 CharacterController.Move 기반 이동으로 수정
- 기존 씬의 플레이어 오브젝트에 CharacterController가 없어도 런타임에 자동 추가되도록 보강
- PlayerMove 충돌 이동 방식을 검증하는 PowerShell 테스트 추가

### [2026-06-12 22:41:54]
- 플레이어가 시작하자마자 바닥 아래로 떨어지던 문제를 수정하기 위해 PlayerMove의 임시 중력 이동 제거
- 런타임에 추가되는 CharacterController의 중심을 y=1로 설정하여 컨트롤러 하단이 바닥 높이에 맞도록 보정
- PlayerMove 검증 테스트에 시작 낙하 방지 조건 추가

### [2026-06-12 22:44:25]
- PlayerMove에 CharacterController 기반 중력 이동을 복구하여 계단이나 단차에서 내려올 수 있도록 수정
- CharacterController 중심 보정은 유지하여 시작 직후 바닥 아래로 빠지지 않도록 처리
- PlayerMove 검증 테스트에 중력 적용, 접지 상태 보정, 수직 이동 조건 추가

### [2026-06-12 22:51:00]
- 플레이어가 바라보는 문 오브젝트를 E키로 열 수 있도록 DoorInteractor 추가
- DoorInteractor가 문 오브젝트를 회전시키고 문 하위 콜라이더를 비활성화하여 막힌 입구를 통과할 수 있도록 구현
- 기존 플레이어 오브젝트에 DoorInteractor가 런타임 자동 부착되도록 PlayerMove 보강 및 검증 테스트 추가

### [2026-06-12 23:00:00]
- DoorInteractor의 문 열림 처리를 문짝 중심 회전이 아닌 실제 문 경첩 모서리 기준 회전으로 수정
- 문 Renderer/Collider Bounds를 계산해 문 폭 방향과 경첩 위치를 찾고 RotateAround로 자연스럽게 열리도록 변경
- 이미 열린 문이 다시 90도씩 추가 회전하지 않도록 열린 문 추적 처리를 추가하고, 문 경첩 회전 검증 테스트를 보강

### [2026-06-13 19:04:50]
- 절규의 수술실 단서 구조를 17개 방 구역 기준 28개 단서로 확장
- 사건 파악, 봉태현 미스디렉션, 진세웅 동기, 범행 물증, 최종 범인 선택 흐름으로 RoomGameManager 진행 단계를 재구성
- ClueData 자동 생성기와 씬 자동 세팅 도구를 28개 단서 배치 기준으로 갱신하고 검증 테스트 추가

### [2026-06-13 19:18:34]
- EscapeRoom 네임스페이스 기반 공통 단서 데이터, 저널 매니저, 단서 상호작용, 단서 저널 UI 시스템 추가
- J키와 Tab키로 열고 닫는 증거 저널 UI를 구현하고 수집 증거 탭, 용의자 수첩 탭, 구역별 미획득 표시와 진행도 표시를 구성
- 단서 상호작용은 2m 이내 E키 수집 방식으로 구현하고 CursorController 및 Time.timeScale 변경 없이 독립 동작하도록 검증 테스트 추가

### [2026-06-13 19:59:27]
- 5단계 스토리 진행을 관리하는 StoryProgressManager를 추가하고 진범 인식, 열쇠 단서 수집, 범인 선택, 추격 탈출 단계를 구성
- LockSystem, EndingUI, SilhouetteController, ChaseController, EscapeExitDoor, GameOverUI를 추가해 열쇠 획득, 오답 실패, 정답 추격, 탈출 성공과 실패 분기를 구현
- ClueData와 ClueJournalManager에 열쇠 단서 구분을 추가하고 ClueJournalUI에서 열쇠 단서를 별도 섹션으로 표시하도록 확장

### [2026-06-13 20:05:39]
- Assets/Clues/Normal 및 Assets/Clues/KeyClue 경로에 28개 일반 단서와 3개 열쇠 단서를 일괄 생성하는 ClueAssetGenerator 에디터 메뉴 추가
- ClueData에 필수 단서 여부를 저장하는 isRequired 필드를 추가하고 단서 생성기에서 이름, 설명, 의미, 구역, 분류, 필수 여부를 모두 설정하도록 구현
- 생성되는 단서 ID를 StoryProgressManager의 진범 인식 및 열쇠 단서 진행 조건과 연결되도록 맞추고 검증 테스트 추가

### [2026-06-13 20:13:35]
- 단서 수집 키를 E에서 F로 변경하고 화면 하단 안내 문구를 "[F] 증거 수집"으로 수정해 문 열기 입력과 분리
- 추격 중 진세웅 접근 판정을 1m 미만으로 조정하고 3m 비네트, 2m 심장박동 요청, 암전 GameOver 이벤트를 추가
- 플레이어 기본 이동 속도를 3.0, Shift 달리기 속도를 5.0으로 조정하고 진세웅 NavMesh 추격 속도를 3.8로 설정

### [2026-06-13 20:16:59]
- 우측 상단에 MM:SS 형식으로 표시되는 TimerUI를 추가하고 Screen Space Overlay Canvas 자동 구성을 구현
- StoryProgressManager에서 현재 타이머 값과 추격 타이머 활성 상태를 제공하도록 확장해 추리 타이머는 흰색, 추격 타이머는 붉은색으로 표시
- ChaseController가 StoryProgressManager에 추격 타이머를 등록하도록 연결하고 추리 및 추격 타이머 만료 시 GameOverUI 활성화 흐름을 검증

### [2026-06-13 20:21:39]
- ChaseController 거리 이벤트와 연결되는 ProximityVignetteUI를 추가해 3m 이내 알파 0.3, 2m 이내 알파 0.6 붉은 화면 가장자리 효과를 표시
- ClueJournalManager.OnClueAdded 이벤트를 구독하는 CluePickupPopupUI를 추가해 단서명과 증거 수집됨 문구를 2초 표시 후 페이드아웃하도록 구현
- EndingUI 용의자 선택 흐름에 SuspectConfirmUI 확인 팝업을 추가하고 예 선택 시 ConfirmSuspect로 정답 및 오답 1회 제한 처리를 수행하도록 변경

### [2026-06-13 20:38:45]
- 공통 단서 에셋과 ClueInteractable 씬 배치가 연결되지 않아 F키 수집 반응이 없는 원인을 확인하고 ClueSceneSetupTool을 추가
- Tools/Clues 메뉴에서 단서 에셋 생성 후 현재 씬 또는 3개 스테이지 씬 전체에 ClueInteractable, Collider, 표시용 MeshRenderer, 단서 UI 매니저를 자동 배치하도록 구현
- ClueInteractable이 배치된 ClueData를 저널 정의에 등록하고 ClueJournalManager가 씬 전환 중 유지되도록 보강

### [2026-06-13 20:45:51]
- Tools/Clues/Setup All Stage Clues 메뉴가 현재 작업 씬을 전환하며 저장할 수 있는 위험을 제거하고 현재 씬 단서 세팅 메뉴만 유지
- 잘못 생성된 3개 스테이지 씬 변경분을 git 상태에서 복구하고 단서 ScriptableObject 에셋 31개만 프로젝트 에셋으로 보존
- ClueSceneWiring 테스트에 자동 씬 전환 및 저장 금지와 생성된 일반 단서 28개, 열쇠 단서 3개 존재 검사를 추가

### [2026-06-13 20:50:42]
- ClueSceneSetupTool에 남아 있던 EditorSceneManager.MarkSceneDirty 호출을 제거해 Unity 컴파일 오류를 수정
- ClueSceneWiring 테스트가 EditorSceneManager 참조 전체를 금지하도록 강화해 같은 컴파일 오류가 재발하지 않도록 보강
### [2026-06-14 15:25:52]
- Room02 Part3 일반 단서 7개와 열쇠 단서 3개를 clueId.asset 파일명 규칙으로 생성하는 Tools/Room02/Generate Clues Part3 메뉴 추가
- DressingRoom, OperatingRoom 일반 단서는 Normal 경로에, 열쇠 단서는 KeyClue 경로에 생성되도록 분기 처리
- Part3 생성기 검증 테스트 추가

### [2026-06-14 15:22:25]
- Room02 Part2 단서 12개를 clueId.asset 파일명 규칙으로 생성하는 Tools/Room02/Generate Clues Part2 메뉴 추가
- Ward와 Storage 구역 단서 설명, 의미, 필수 여부를 시나리오에 맞게 생성하도록 ClueAssetGenerator 확장
- Part2 생성기 검증 테스트 추가

### [2026-06-14 15:16:25]
- Room02 Part1 단서 9개를 clueId.asset 파일명 규칙으로 생성하는 Tools/Room02/Generate Clues Part1 메뉴 추가
- Part1 단서 설명과 의미를 시나리오에 맞게 갱신하고 기존 ClueData ScriptableObject를 덮어쓰기 방식으로 갱신하도록 구현
- 단서 획득 팝업에 단서 이름뿐 아니라 설명과 의미까지 표시되도록 개선

### [2026-06-14 14:56:09]
- Room02 단서 박스 조사 프롬프트가 너무 넓게 뜨지 않도록 근접 표시 거리를 2.2m로 조정
- 박스 선택은 360도 근접 후보 중 거리와 시야 방향을 함께 반영하되 가까이 있을 때만 활성화되도록 개선
- 게임 시작 시 현재 상황을 설명하는 공포 분위기 인트로 시나리오 UI 추가

### [2026-06-14 14:50:09]
- Room02 단서 박스 인식을 정중앙 Raycast 방식에서 360도 근접 감지 방식으로 개선
- 박스 상호작용 거리를 5m로 늘리고 거리와 시야 방향을 함께 반영해 가장 적절한 박스를 자동 선택
- F키 입력 버퍼를 추가해 박스 조사 입력이 씹히는 현상을 완화

### [2026-06-14 14:37:33]
- Scene_OperatingRoom에 기존 큐브 단서가 남아 있어 박스가 보이지 않던 문제 원인 확인
- Play 시작 시 기존 단서 마커를 Room02 전용 박스 프리팹으로 자동 변환하는 ClueBoxRuntimeAdapter 추가
- Unity 배치 적용용 Scene_OperatingRoom 저장 메서드와 관련 검증 테스트 보강

### [2026-06-14 14:18:28]
- Room02 단서 상호작용을 박스 조사 방식으로 확장하고 ClueBoxInteractable을 추가
- Room02 단서 배치 도구가 설치된 Box_V1 프리팹 기반 단서 박스를 배치하도록 수정
- HUD, 타이머, 설정, 단서 팝업 UI에 공포 게임 분위기의 어두운 패널과 붉은 강조 테마 적용

### [2026-06-14 03:07:33]
- Room02 전용 기능으로 관리되도록 기존 Assets/Clues 폴더 전체를 Assets/Room02_Operating/Clues 경로로 이동하고 Unity GUID를 유지
- 단서 생성기, 단서 배치 도구, HUD/타이머/추격/엔딩 관련 테스트 경로를 Room02 기준으로 수정
- Room02 단서 생성기가 더 이상 공용 Assets/Clues 폴더를 재생성하지 않도록 경로 생성 로직과 검증 테스트 보강

### [2026-06-14 03:02:19]
- 특정 씬에 UI 오브젝트가 배치되어 있지 않아도 게임 실행 시 HUD_Canvas, 타이머, 수집 증거, 용의자 수첩, 설정 UI가 자동 생성되도록 HudRuntimeBootstrapper 추가
- 현재 작업 중인 씬에서 바로 Play를 눌러도 HUD 버튼이 동작하도록 EventSystem과 InputSystemUIInputModule 자동 생성 처리 추가
- HUD 런타임 부트스트랩 검증 테스트를 추가하여 단일 통합 씬 외의 씬에서도 UI 생성 조건을 확인

### [2026-06-14 02:53:21]
- HUD_Canvas 기준으로 우측 상단 타이머, 좌측 상단 수집 증거 버튼, 용의자 수첩 버튼, 우측 상단 설정 버튼을 생성하도록 UI 구조 개선
- 수집 증거는 J/Tab, 용의자 수첩은 K, 설정 패널은 ESC로 열고 닫도록 연결하고 설정 패널에 볼륨/감도 및 조작법 탭 추가
- 추리 타이머 기본값을 20분으로 변경하고 3분 이하에서 빨간색으로 표시되도록 수정하며 하단 상호작용 안내를 "[F] 조사하기"로 변경

### [2026-06-14 01:56:25]
- Scene_Corridor와 Scene_DressingRoom을 제거하고 Scene_OperatingRoom 하나로 통합하여 Build Settings에 단일 씬만 남기도록 정리
- ZoneManager와 ZoneDoorActivator를 추가하여 씬 전환 대신 Zone_Lobby, Zone_Corridor, Zone_Ward, Zone_Storage, Zone_DressingRoom, Zone_OperatingRoom 활성화 방식으로 구역 전환 처리
- SceneLoader와 DontDestroyOnLoad 사용을 제거하고 DoorInteractor가 문 열림 시 다음 구역을 활성화하도록 연결

### [2026-06-14 15:59:35]
- Room02 통합 씬 Scene_OperatingRoom에 일반 단서 28개와 열쇠 단서 3개를 모두 ClueBoxInteractable 박스로 배치
- 빈 clueData가 남아 있던 기존 단서 오브젝트를 제거하고 단서 배치 도구가 낡은 단서 오브젝트를 정리하도록 개선
- 단서 획득 시 하단 텍스트 대신 중앙 화면창 형태의 팝업 패널로 단서명, 설명, 의미가 표시되도록 CluePickupPopupUI 개선

### [2026-06-14 16:07:57]
- Room02 통합 씬에 관리자용 Admin_ClueGuideOverlay를 추가하여 단서 위치 위에 화면 화살표와 단서명이 표시되도록 구현
- ClueAdminGuideOverlay가 Scene_OperatingRoom의 ClueBoxInteractable 단서를 자동 탐색하고 관리자 확인용 오버레이를 생성하도록 추가
- 관리자용 단서 가이드 검증 테스트를 추가하여 오버레이 스크립트와 씬 연결을 확인

### [2026-06-14 16:15:35]
- 관리자용 단서 화살표가 화면 밖으로 밀려 보이지 않던 문제를 수정하고 화면 가장자리 안쪽에 고정 표시되도록 개선
- 관리자 실행 중에도 단서 가이드가 보이도록 Application.isEditor 조건과 EditorOnly 태그 의존을 제거
- 단서가 시야 밖에 있을 때도 화면 경계 안에서 화살표와 단서명이 계속 표시되도록 검증 테스트 보강

### [2026-06-14 17:20:25]
- 범인 에셋 임포트 중 함께 추가된 3인칭 컨트롤러 스크립트, 데모 씬, 프로젝트 설정 변경을 제거
- Room02 범위 안에서 사용할 수 있도록 범인 외형 리소스인 char_shadow 모델, 머티리얼, 텍스처만 Room02_Operating 폴더에 보존
- 불필요한 패키지 확장 파일이 남지 않았는지 검증하고 기존 Show 씬 변경은 유지

### [2026-06-14 17:30:30]
- Room02 범인 외형 에셋의 메타 파일 경로를 Room02_Operating 기준으로 보정하여 이전 3rdPerson+Fly 패키지 경로 참조를 제거
- 범인 외형 에셋이 Room02 범위 안에만 남아 있고 Scene_OperatingRoom에 임의 배치되지 않았는지 확인하는 검증 테스트 추가
- 단서 관리자 화살표 오브젝트가 Scene_OperatingRoom에 활성 상태로 연결되어 있는지 기존 검증과 함께 재확인

### [2026-06-14 17:48:39]
- 단서 획득 팝업이 표시 후 2초 동안 유지되고 0.5초 동안 CanvasGroup 알파로 페이드아웃되도록 수정
- 페이드아웃 완료 후 팝업 패널을 비활성화하여 화면에 남지 않도록 처리
- 연속 단서 획득 시 기존 팝업 코루틴을 취소하고 새 코루틴으로 교체되도록 검증 테스트 보강

### [2026-06-14 17:56:44]
- 관리자용 단서 화살표가 Play 모드 Game 화면뿐 아니라 Unity Scene 편집 화면에서도 보이도록 Editor 전용 Gizmo와 단서명 라벨을 추가
- Scene 뷰 표시 코드를 UNITY_EDITOR 조건으로 분리하여 빌드와 런타임 UI 동작에 영향이 없도록 처리
- 단서 가이드 검증 테스트에 Scene 뷰 화살표와 라벨 생성 조건을 추가

### [2026-06-14 18:00:09]
- Scene_OperatingRoom에 Room02_BGM 오브젝트를 추가하고 Free Horror Ambience의 dk-theroom.aif를 배경음으로 연결
- Room02BgmPlayer를 추가하여 시작 시 AudioSource를 구성하고 2D 루프 배경음이 낮은 볼륨으로 재생되도록 처리
- Room02 배경음 오브젝트, 선택한 오디오 클립 참조, 루프 및 볼륨 설정을 검증하는 테스트 추가

### [2026-06-14 18:53:15]
- Unity 6 Scene 뷰에서 관리자용 단서 라벨 표시가 Handles.Label assertion을 발생시키던 문제를 수정
- Scene 뷰 단서 가이드를 에디터 안전한 Gizmo 선, 화살촉 점, 단서 위치 원 표시 방식으로 변경
- Scene_OperatingRoom에 관리자용 Scene 가이드 옵션을 명시 저장하고 재발 방지 검증 테스트를 보강

### [2026-06-14 19:14:34]
- Room02 실제 플레이 씬을 Assets/Room02_Operating/Scenes/Scene_OperatingRoom.unity로 이동하고 Build Settings와 단서 배치 도구 경로를 Room02 기준으로 수정
- Room02 배경음 dk-theroom.aif와 음악 폴더 메타를 Assets/Room02_Operating/Audio/music 아래로 이동하여 사용 에셋이 Room02 범위 안에 있도록 정리
- Room02 씬에 Show/Abandoned Asylum 맵 프리팹을 병합하여 실제 병원 맵, 단서 31개, 관리자 화살표, BGM이 같은 Room02 씬 안에 존재하도록 수정
- Room02 범위와 Show 맵 포함 여부를 검증하는 테스트를 추가하고 기존 씬 경로 참조 테스트를 Room02 기준으로 갱신

### [2026-06-14 19:24:04]
- Assets/Abandoned_Asylum 아래에 남아 있던 Show.unity, Show 라이트맵/반사 프로브 데이터, Show_Assets 관련 파일을 Assets/Room02_Operating/Scenes 아래로 이동
- Room02 범위 검증 테스트에 Show 관련 씬과 조명 데이터가 Abandoned_Asylum에 남지 않는지 확인하는 항목을 추가

### [2026-06-15 09:20:57]
- 미커밋 상태로 남아 있던 Room02 운영 씬, Show 씬, TMP 한글 폰트 에셋, 추가 배경음 파일들을 커밋 대상에 포함
- Scene_OperatingRoom에서 빠져 있던 Show 맵 프리팹 연결을 복구하여 Room02 플레이 씬에 실제 맵 참조가 유지되도록 수정

### [2026-06-15 11:51:49]
- Scene_OperatingRoom에서 단서 31개를 PlayerStart 주변 시작 위치로 모아 배치
- Room02 범인 에셋 char_shadow를 Culprit_StartPosition 오브젝트로 시작 위치 앞에 배치
- 단서 설명과 획득 팝업 문구를 공포 게임 분위기로 수정하고 생성기와 에셋 데이터를 동기화

### [2026-06-15 12:11:53]
- Scene_OperatingRoom의 단서 31개를 게임 화면에서 보이는 단일 상자 위치로 모두 겹쳐 배치
- 범인 char_shadow 오브젝트를 단서 상자와 같은 X/Z 위치로 이동하여 시작 화면에서 함께 보이도록 조정
- 겹친 단서를 연속으로 조사할 수 있도록 이미 조사한 상자보다 미조사 상자를 우선 선택하게 수정

### [2026-06-15 12:24:51]
- Scene_OperatingRoom의 단서 31개를 Main Camera 앞 8열 격자로 분리 배치하여 게임 시작 화면에서 한꺼번에 보이도록 수정
- 범인 char_shadow 오브젝트를 단서 격자 오른쪽의 카메라 가시 영역으로 이동하여 시작 화면에서 함께 보이도록 조정
- 단서 획득 팝업의 자동 페이드아웃을 제거하고 클릭할 때까지 계속 표시되도록 변경

### [2026-06-15 14:01:25]
- 단서 획득 팝업을 실제 UI 클릭 이벤트로 닫히도록 수정하여 화면에 계속 남는 문제 해결
- 단서 팝업이 열려 있는 동안 상자 조사 프롬프트가 겹쳐 표시되지 않도록 처리
- 이미 조사한 박스 안내 문구가 계속 떠 있지 않도록 조사 완료 후 프롬프트를 숨기도록 수정

### [2026-06-15 14:08:04]
- Room02 단서 배치 툴에서 테스트 단서 1개만 남기는 메뉴를 제거하고 전체 단서 복구 메뉴로 교체
- Scene_OperatingRoom에서 TestClue만 남고 실제 단서가 부족한 에디터 상태를 스크립트 리로드 시 자동 복구하도록 처리
- 저장된 Room02 씬에 실제 단서 31개가 유지되고 임시 TestClueBox가 남지 않도록 검증 테스트 강화

### [2026-06-15 14:20:30]
- Room02 게임 화면을 전반적으로 어둡게 낮추고 Main Camera를 따라가는 원형 손전등 조명과 화면 마스크를 추가
- Scene_OperatingRoom에 Room02_FlashlightController를 배치하고 런타임 부트스트래퍼에서도 자동 생성되도록 보강
- Room02_BGM에 AudioSource를 직접 연결하고 재생 중 끊기면 다시 시작되도록 BGM 플레이어를 보강
- 손전등 조명과 BGM 연결 상태를 검증하는 테스트를 추가 및 강화

### [2026-06-15 14:30:44]
- Room02 시작 인트로 본문을 사용자가 요청한 병원 복도 탈출 시나리오 문구로 교체
- 인트로 UI 테스트가 새 3문장 전체를 검증하도록 보강

### [2026-06-15 14:41:09]
- Room02 실제 플레이 기준 씬을 Show.unity로 정리하고 단서 31개, 범인, BGM, 손전등, 관리자 가이드를 Show에 배치
- Build Settings와 Room02 단서 배치 툴이 Scene_OperatingRoom이 아닌 Show를 대상으로 동작하도록 수정
- 단서, 범인, BGM, 손전등, HUD 검증 테스트가 Show 기준을 확인하도록 갱신
### [2026-06-15 14:54:52]
- 기존 Show 씬의 맵과 조명 구조를 복구하고 Scene_OperatingRoom 전체 복사가 아닌 단서 오브젝트만 Show의 Clues 루트 아래로 이관
- Show에 남아 있던 임시 TestClue_cast_notice를 제거하고 실제 단서 31개, 범인, BGM 오브젝트만 독립 배치
- Show 원본 구조 기준으로 단서 배치, HUD 런타임 생성, 손전등 런타임 생성 검증 테스트 수정
### [2026-06-15 15:08:56]
- Show 씬의 단서 31개를 한 화면 격자가 아닌 로비, 복도, 병동, 보관실, 분장실, 수술실 구역 좌표로 분산 배치
- 단서 재배치 툴이 같은 방별 좌표를 사용하도록 수정해 재생성 시에도 단서가 한 곳에 몰리지 않게 개선
- Room02 단서 배치 테스트를 방별 분산 배치 기준으로 갱신

### [2026-06-15 15:50:27]
- Room02 스토리를 15개 핵심 단서 흐름으로 축소하고 Show 씬의 단서 오브젝트도 15개만 남도록 정리
- 단서 이름, 설명, 수첩 힌트를 폐요양 병원 독살 사건 시나리오에 맞춰 갱신
- 용의자 수첩이 수집된 단서의 수첩 힌트를 인물별로 갱신하도록 ClueJournalUI 개선
- 15개 단서 수집 후 범인 선택 단계로 넘어가도록 StoryProgressManager 조건 수정

### [2026-06-15 17:29:18]
- PlayerMove가 시작 시점에 플레이어 카메라를 안정적으로 찾아 자식으로 고정하도록 수정
- 좌우 시야 회전은 누적 yaw로 처리해 360도 이상 자유롭게 돌 수 있게 개선
- 상하 시야만 pitch 값으로 분리해 -90도부터 90도까지 제한하고 시작 시 꼬인 카메라 회전을 정면으로 초기화
- PlayerMove 회전 동작 검증 테스트를 추가하고 전체 Room02 테스트 통과 확인

### [2026-06-15 17:36:46]
- 단서 10개 이상 수집 시 범인 선택 단계로 넘어가도록 StoryProgressManager 해금 기준 수정
- 범인 선택 해금 시 화면 우하단에 범인 맞추기 버튼이 나타나고 클릭하면 기존 범인 선택 UI가 열리도록 EndingUI 개선
- Show 씬에서 EndingUI가 직접 배치되어 있지 않아도 런타임 부트스트랩이 자동 생성하도록 연결
- 범인 선택 해금 기준과 버튼 표시 조건 검증 테스트를 추가하고 전체 Room02 테스트 통과 확인
