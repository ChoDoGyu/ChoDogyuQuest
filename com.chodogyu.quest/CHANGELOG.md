# Changelog

이 문서는 ChoDogyu Quest Framework & Editor의 주요 변경 사항을 기록합니다.

## [1.0.0] - 2026-09-20

### Added

* `QuestCatalog` 기반 Quest Definition 관리
* `QuestDefinition`

  * Quest ID
  * Title
  * Description
  * Objective 목록
  * Prerequisite Quest 목록
  * Reward 목록
  * Repeatable 설정
* `ObjectiveDefinition`

  * Objective ID
  * Title
  * Description
  * Target Progress
* `RewardDefinition`

  * Type
  * Key
  * Amount
* Quest Definition과 Runtime State 분리
* `QuestStatus`

  * `Inactive`
  * `Active`
  * `Completed`
* Objective Runtime 진행 상태 관리
* Quest Runtime 상태 관리
* Quest 완료 횟수 관리
* `QuestManager` 기반 Runtime Quest 관리
* `QuestManager.Create`

  * QuestCatalog null 검사
  * Runtime 생성 전 Catalog Validation
  * Validation Error 존재 시 생성 차단
  * Warning 존재 시 Runtime 생성 허용
* Quest Definition 조회
* Quest 현재 상태 조회
* Objective 현재 진행도 조회
* Quest 완료 횟수 조회
* 현재 Active Quest 목록 조회
* Quest 시작 가능 여부 확인
* Quest 시작
* 선행 Quest 완료 여부 검사
* Objective 진행도 증가
* Objective 진행도 절대값 설정
* Objective Target Progress 초과 값 Clamp
* Objective 자동 완료 판정
* 모든 Objective 완료 시 Quest 자동 완료
* 반복 가능한 Quest Reset
* Reset 이후 Objective 진행도 초기화
* 반복 Quest Reset 이후 Completion Count 유지
* Quest 시작 이벤트
* Objective 진행도 변경 이벤트
* Objective 완료 이벤트
* Quest 완료 이벤트
* Quest Reset 이벤트
* `QuestStateCollectionSnapshot`
* `QuestStateSnapshot`
* `ObjectiveStateSnapshot`
* 전체 Quest Runtime 상태 Capture
* Runtime 상태 Restore
* Restore 전 Snapshot 전체 Validation
* 잘못된 Snapshot 복원 시 기존 Runtime 상태 유지
* Restore 과정의 Runtime 이벤트 비발생 정책
* 외부 Save System에서 사용할 수 있는 Snapshot 구조
* `QuestValidator` 기반 QuestCatalog Validation
* Quest null Validation
* Quest ID 필수 검사
* Quest ID 중복 검사
* Quest Title Warning
* Quest Description Warning
* Objective 최소 1개 검사
* Objective null 검사
* Objective ID 필수 검사
* Objective ID 중복 검사
* Objective Title Warning
* Objective Target Progress 검사
* Prerequisite ID 필수 검사
* Prerequisite 중복 검사
* 자기 자신에 대한 Prerequisite 검사
* 존재하지 않는 Prerequisite 검사
* Prerequisite 순환 참조 검사
* Reward null 검사
* Reward Type 검사
* Reward Key 검사
* Reward Amount 검사
* `QuestValidationIssue`

  * Severity
  * Code
  * Message
  * Quest 위치 정보
  * Objective 위치 정보
  * Prerequisite 위치 정보
  * Reward 위치 정보
* `QuestValidationSeverity`

  * Warning
  * Error
* 외부 코드에서 사용할 수 있는 `QuestValidationCodes`
* 안정적인 Quest Runtime 오류 코드를 위한 `QuestErrorCodes`
* `QUEST_CATALOG_REQUIRED`
* `QUEST_VALIDATION_FAILED`
* `QUEST_INVALID_ID`
* `QUEST_NOT_FOUND`
* `QUEST_INVALID_OBJECTIVE_ID`
* `QUEST_OBJECTIVE_NOT_FOUND`
* `QUEST_NOT_INACTIVE`
* `QUEST_PREREQUISITE_NOT_COMPLETED`
* `QUEST_NOT_ACTIVE`
* `QUEST_INVALID_PROGRESS`
* `QUEST_OBJECTIVE_ALREADY_COMPLETED`
* `QUEST_NOT_COMPLETED`
* `QUEST_NOT_REPEATABLE`
* `QUEST_STATE_SNAPSHOT_REQUIRED`
* `QUEST_INVALID_STATE_SNAPSHOT`
* Quest 제작을 위한 전용 Quest Editor Window
* `Tools/ChoDogyu/Quest Editor` 메뉴
* QuestCatalog 선택
* 현재 Project Selection의 QuestCatalog 사용
* Quest 목록 표시
* Quest 추가
* Quest 삭제
* 자동 기본 Quest ID 생성
* 사용 가능한 Quest ID 재사용
* Quest ID 편집
* Quest Title 편집
* Quest Description 편집
* Repeatable 편집
* Objective 추가 및 삭제
* 자동 기본 Objective ID 생성
* Objective ID 편집
* Objective Title 편집
* Objective Description 편집
* Objective Target Progress 편집
* Reward 추가 및 삭제
* Reward Type 편집
* Reward Key 편집
* Reward Amount 편집
* Prerequisite 추가 및 삭제
* Prerequisite 후보 Quest 선택
* 자기 자신을 Prerequisite 후보에서 제외
* 이미 사용 중인 Prerequisite 중복 후보 제외
* 유효하지 않은 기존 Prerequisite 값을 편집 UI에서 유지
* Quest ID 및 Title 검색
* 현재 Quest의 선행 Quest 관계 표시
* 현재 Quest를 참조하는 Dependent Quest 표시
* 관계 항목을 통한 Quest 이동
* Runtime `QuestValidator` 기반 Editor Validation
* Validation Error / Warning 개수 표시
* Validation 문제별 위치 정보 표시
* Validation 문제에서 해당 Quest로 이동
* Quest Catalog 변경 사항 Dirty 상태 표시
* Quest Catalog 수동 저장
* Quest 편집 Undo / Redo
* Runtime과 Editor Assembly 분리
* Runtime Test Assembly 분리
* Editor Test Assembly 분리
* Runtime 내부 구현의 테스트용 접근 범위 제한
* Runtime Definition 테스트
* QuestCatalog 테스트
* QuestManager 생성 테스트
* Quest Runtime 진행 흐름 테스트
* Runtime Event 테스트
* Prerequisite 테스트
* Repeatable Quest 테스트
* Runtime State 테스트
* Capture / Restore 테스트
* 잘못된 Snapshot 테스트
* Quest Validation 테스트
* Prerequisite 순환 검사 테스트
* Quest Editor Session 테스트
* Quest Editor Validation 통합 테스트
* 최종 자동화 테스트

  * Runtime: 81 Passed
  * Editor: 19 Passed
  * Total: 100 Passed
* Unity Package Manager용 UPM 패키지 구조
* Unity 6.3 LTS 지원
* ChoDogyu Core `Result` / `Result<T>` 기반 오류 처리
* 다른 ChoDogyu Data, Save, UI, Audio, Scene 패키지에 대한 필수 의존성 제거
* 패키지 README
* 상세 Documentation
