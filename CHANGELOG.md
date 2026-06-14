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
