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
