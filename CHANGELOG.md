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
