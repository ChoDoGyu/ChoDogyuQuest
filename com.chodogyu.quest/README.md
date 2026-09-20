# ChoDogyu Quest Framework & Editor

게임 고유 시스템에 의존하지 않고 Quest 정의, Objective 진행 상태, 완료 판정, 선행 관계, 반복 Quest 및 상태 저장·복원 흐름을 관리할 수 있도록 구성한 범용 Unity Quest Framework & Editor입니다.

Quest Definition과 Runtime State를 분리하고, 게임 코드가 실제 게임 이벤트를 감지하여 Quest 진행 API를 호출하는 구조로 구성했습니다.

## 주요 기능

* `QuestCatalog` 기반 Quest Definition 관리
* Quest / Objective / Reward 데이터 정의
* Quest 시작 및 Runtime 상태 관리
* Objective 진행도 증가 및 절대값 설정
* Objective 완료 자동 판정
* 모든 Objective 완료 시 Quest 자동 완료
* Quest 시작 / Objective 진행 / 완료 이벤트
* 선행 Quest 기반 시작 조건
* 반복 가능한 Quest Reset
* Quest 완료 횟수 유지
* Quest Runtime 상태 Capture / Restore
* Runtime 상태와 Definition 분리
* Quest Definition Validation
* 선행 관계 순환 참조 검증
* 안정적인 Runtime 오류 코드 제공
* 안정적인 Validation Issue Code 제공
* Quest Editor Window
* Quest / Objective / Reward 편집
* Prerequisite 편집
* Quest 검색
* 선행 / 참조 관계 탐색
* Runtime Validator 기반 Editor Validation
* Undo / Redo 및 Asset 저장 지원
* Runtime / Editor 자동화 테스트 제공

## 패키지 정보

* Package Name: `com.chodogyu.quest`
* Runtime Assembly: `CDG.Quest`
* Editor Assembly: `CDG.Quest.Editor`
* Namespace: `CDG.Quest`
* Unity: `6000.3` 이상
* Dependency: `com.chodogyu.core`

## 설치

먼저 ChoDogyu Core 패키지가 설치되어 있어야 합니다.

### ChoDogyu Core v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### ChoDogyu Quest Framework & Editor v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuQuest.git?path=/com.chodogyu.quest#v1.0.0
```

Unity Package Manager의 **Install package from git URL...**을 사용하여 Core를 먼저 설치한 뒤 Quest Framework를 설치합니다.

## QuestCatalog 생성

Quest Definition은 `QuestCatalog` ScriptableObject에서 관리합니다.

Project Window에서 다음 메뉴를 사용합니다.

```text
Create
└── CDG
    └── Quest
        └── Quest Catalog
```

하나의 Catalog 안에서 여러 Quest Definition을 관리할 수 있습니다.

Runtime 진행 상태는 `QuestCatalog`에 저장되지 않습니다.

## Quest Definition

하나의 Quest는 다음 정보를 가집니다.

```text
ID
Title
Description
Objectives
Prerequisites
Rewards
Repeatable
```

### Objective

Objective는 Quest 내부의 개별 진행 목표를 나타냅니다.

각 Objective는 다음 정보를 가집니다.

```text
ID
Title
Description
Target Progress
```

Objective ID는 동일한 Quest 안에서 중복되지 않아야 하며 `Target Progress`는 1 이상이어야 합니다.

### Prerequisite

Quest는 다른 Quest ID를 선행 조건으로 지정할 수 있습니다.

v1.0에서는 등록된 모든 선행 Quest가 최소 한 번 이상 완료되어야 해당 Quest를 시작할 수 있습니다.

### Reward

Reward는 다음 메타데이터를 제공합니다.

```text
Type
Key
Amount
```

예:

```text
Type   = Currency
Key    = Gold
Amount = 100
```

Quest Framework는 Reward 정보를 정의하고 제공하지만 실제 재화 지급, 아이템 지급 또는 기능 해금은 수행하지 않습니다.

실제 Reward 처리는 게임 코드의 책임입니다.

## QuestManager 생성

Runtime의 주요 진입점은 `QuestManager`입니다.

```csharp
using CDG.Core.Results;
using CDG.Quest;
using UnityEngine;

public sealed class QuestExample : MonoBehaviour
{
    [SerializeField] private QuestCatalog catalog;

    private QuestManager questManager;

    private void Start()
    {
        Result<QuestManager> result = QuestManager.Create(catalog);

        if (result.IsFailure)
        {
            Debug.LogError(
                $"{result.Error.Code} / {result.Error.Message}");

            return;
        }

        questManager = result.Value;
    }
}
```

`QuestManager.Create`는 Catalog 전체를 먼저 Validation합니다.

Validation Warning은 생성을 막지 않지만 하나 이상의 Error가 존재하면 Manager 생성이 실패합니다.

## Quest 시작

Quest는 기본적으로 `Inactive` 상태에서 시작합니다.

```csharp
Result result = questManager.StartQuest("quest_main");
```

Quest를 시작하려면 다음 조건을 만족해야 합니다.

* 유효한 Quest ID
* 현재 상태가 `Inactive`
* 모든 선행 Quest가 최소 한 번 이상 완료됨

시작에 성공하면 상태가 `Active`가 되고 `QuestStarted` 이벤트가 발생합니다.

현재 시작 가능한지는 다음 API로 확인할 수 있습니다.

```csharp
bool canStart = questManager.CanStartQuest("quest_main");
```

## Objective 진행

현재 진행도에서 값을 증가시킬 수 있습니다.

```csharp
Result result = questManager.AdvanceObjective(
    "quest_main",
    "kill_enemy",
    1);
```

또는 진행도를 절대값으로 설정할 수 있습니다.

```csharp
Result result = questManager.SetObjectiveProgress(
    "quest_main",
    "kill_enemy",
    5);
```

Target Progress보다 큰 값은 자동으로 Target Progress까지 제한됩니다.

현재 진행도는 다음과 같이 조회합니다.

```csharp
Result<int> progress =
    questManager.GetObjectiveProgress(
        "quest_main",
        "kill_enemy");
```

Objective 진행도는 `Active` 상태의 Quest에서만 변경할 수 있습니다.

## 자동 완료 판정

Objective가 처음 Target Progress에 도달하면:

```text
ObjectiveCompleted
```

이벤트가 발생합니다.

Quest의 모든 Objective가 완료되면 Quest는 자동으로:

```text
Completed
```

상태가 되고:

```text
QuestCompleted
```

이벤트가 발생합니다.

별도의 `CompleteQuest()` 호출은 필요하지 않습니다.

## Runtime 이벤트

`QuestManager`는 다음 이벤트를 제공합니다.

```csharp
questManager.QuestStarted += quest =>
{
    // Quest 시작
};

questManager.ObjectiveProgressChanged +=
    (quest, objective, current, target) =>
{
    // Objective 진행도 변경
};

questManager.ObjectiveCompleted +=
    (quest, objective) =>
{
    // Objective 완료
};

questManager.QuestCompleted += quest =>
{
    // Quest 완료
};

questManager.QuestReset += quest =>
{
    // 반복 Quest Reset
};
```

게임의 UI 갱신, 보상 처리, 연출 등의 상위 동작은 이 이벤트를 기반으로 연결할 수 있습니다.

## Reward 처리

Framework는 Reward를 직접 지급하지 않습니다.

예를 들어 Quest 완료 시 게임 코드에서 Reward 정보를 해석할 수 있습니다.

```csharp
questManager.QuestCompleted += quest =>
{
    for (int i = 0; i < quest.Rewards.Count; i++)
    {
        RewardDefinition reward = quest.Rewards[i];

        // 프로젝트의 Inventory, Currency,
        // Unlock System 등으로 전달
    }
};
```

이 구조를 통해 Quest Framework가 특정 Inventory, Economy 또는 Item System에 직접 의존하지 않습니다.

## Quest 상태 조회

현재 Quest 상태:

```csharp
Result<QuestStatus> status =
    questManager.GetQuestStatus("quest_main");
```

상태는 다음 세 가지입니다.

```text
Inactive
Active
Completed
```

현재 Active Quest 목록:

```csharp
IReadOnlyList<QuestDefinition> activeQuests =
    questManager.GetActiveQuests();
```

Quest 완료 횟수:

```csharp
Result<int> completionCount =
    questManager.GetCompletionCount("quest_main");
```

## 반복 Quest

`Repeatable`이 활성화된 Quest는 완료 후 Reset할 수 있습니다.

```csharp
Result result =
    questManager.ResetQuest("quest_daily");
```

Reset 조건:

```text
현재 상태 = Completed
+
IsRepeatable = true
```

Reset이 완료되면:

```text
Quest Status
Completed → Inactive

Objective Progress
→ 0으로 초기화
```

됩니다.

기존 `CompletionCount`는 유지됩니다.

Framework는 일일, 주간, 시간 기반 반복 주기를 직접 관리하지 않습니다.

언제 Reset할지는 사용하는 프로젝트에서 결정합니다.

## 선행 Quest

Quest는 여러 선행 Quest를 가질 수 있습니다.

v1.0에서는:

```text
Prerequisite A 완료
AND
Prerequisite B 완료
AND
Prerequisite C 완료
```

처럼 모든 선행 Quest의 완료 이력이 있어야 시작할 수 있습니다.

완료 여부는 현재 상태가 아니라:

```text
CompletionCount > 0
```

을 기준으로 판단합니다.

따라서 반복 Quest가 Reset되어 다시 `Inactive` 상태가 되더라도 과거 완료 이력은 유지됩니다.

## 상태 Capture / Restore

현재 Quest Runtime 상태 전체를 Snapshot으로 Capture할 수 있습니다.

```csharp
QuestStateCollectionSnapshot snapshot =
    questManager.CaptureState();
```

Snapshot에는 다음 상태가 포함됩니다.

```text
Quest ID
Quest Status
Completion Count
Objective ID
Objective Progress
```

이 Snapshot은 외부 Save System에서 원하는 방식으로 저장할 수 있습니다.

Quest Framework 자체는 파일 입출력이나 저장 위치를 관리하지 않습니다.

복원:

```csharp
Result result =
    questManager.RestoreState(snapshot);
```

Restore 과정에서는 Snapshot 전체를 먼저 검증합니다.

유효하지 않은 Snapshot이면 기존 Runtime 상태를 변경하지 않고 실패 결과를 반환합니다.

Restore 과정에서는 Quest 시작, Objective 진행 또는 Quest 완료 이벤트를 다시 발생시키지 않습니다.

## Validation

`QuestValidator`를 사용하여 Catalog 전체를 검증할 수 있습니다.

```csharp
IReadOnlyList<QuestValidationIssue> issues =
    QuestValidator.Validate(catalog);
```

검증 결과는 다음 두 심각도로 구분됩니다.

```text
Warning
Error
```

대표적인 Validation 대상:

* Quest ID 누락
* Quest ID 중복
* Quest Title 누락
* Quest Description 누락
* Objective 미등록
* Objective ID 누락
* Objective ID 중복
* Objective Title 누락
* 잘못된 Target Progress
* Prerequisite ID 누락
* Prerequisite 중복
* 자기 자신에 대한 Prerequisite
* 존재하지 않는 Prerequisite
* Prerequisite 순환 참조
* 잘못된 Reward Type
* 잘못된 Reward Key
* 잘못된 Reward Amount

Validation 문제 종류는 `QuestValidationCodes`를 통해 안정적인 코드로 확인할 수 있습니다.

```csharp
if (issue.Code ==
    QuestValidationCodes.PrerequisiteCycle)
{
    // 순환 선행 관계 처리
}
```

## Quest Editor

Quest 제작과 Validation을 위한 전용 Editor Window를 제공합니다.

메뉴:

```text
Tools
└── ChoDogyu
    └── Quest Editor
```

Editor에서는 다음 기능을 사용할 수 있습니다.

* QuestCatalog 선택
* Quest 추가 및 삭제
* Quest ID / Title / Description 편집
* Repeatable 설정
* Objective 추가 및 삭제
* Objective 정보 편집
* Prerequisite 추가 및 삭제
* Reward 추가 및 삭제
* Quest ID / Title 검색
* 선행 Quest 관계 확인
* 해당 Quest를 참조하는 Dependent Quest 확인
* 연결된 Quest로 바로 이동
* Runtime Validator 기반 Validation
* Validation 문제 위치로 이동
* Undo / Redo
* Catalog 저장

Editor 전용 Validation 규칙을 별도로 구현하지 않고 Runtime과 동일한 `QuestValidator`를 사용합니다.

따라서 제작 단계와 Runtime 단계에서 동일한 Definition 규칙을 기준으로 검사합니다.

## 오류 처리

Runtime 작업은 ChoDogyu Core의 `Result` 및 `Result<T>`를 사용하여 성공과 실패를 명시적으로 반환합니다.

대표적인 Runtime 오류 코드:

```text
QUEST_CATALOG_REQUIRED
QUEST_VALIDATION_FAILED
QUEST_INVALID_ID
QUEST_NOT_FOUND
QUEST_INVALID_OBJECTIVE_ID
QUEST_OBJECTIVE_NOT_FOUND
QUEST_NOT_INACTIVE
QUEST_PREREQUISITE_NOT_COMPLETED
QUEST_NOT_ACTIVE
QUEST_INVALID_PROGRESS
QUEST_OBJECTIVE_ALREADY_COMPLETED
QUEST_NOT_COMPLETED
QUEST_NOT_REPEATABLE
QUEST_STATE_SNAPSHOT_REQUIRED
QUEST_INVALID_STATE_SNAPSHOT
```

외부 코드에서는 Error Message 문자열보다 `QuestErrorCodes`에 정의된 안정적인 오류 코드를 기준으로 분기하는 것을 권장합니다.

```csharp
if (result.IsFailure &&
    result.Error.Code ==
        QuestErrorCodes.PrerequisiteNotCompleted)
{
    // 선행 Quest 미완료 처리
}
```

## 테스트

최종 자동화 테스트 결과:

```text
Runtime: 81 Passed
Editor:  19 Passed
Total:  100 Passed
```

주요 검증 범위:

* Quest / Objective / Reward Definition
* QuestCatalog
* Quest Validation
* Prerequisite 순환 검사
* QuestManager 생성
* Quest 시작 조건
* Objective 진행
* Objective 완료
* Quest 자동 완료
* Runtime 이벤트
* Repeatable Quest
* Completion Count
* State Capture
* State Restore
* 잘못된 Snapshot 복원 방지
* Quest Editor 추가 / 삭제
* Objective / Reward / Prerequisite 편집
* 관계 조회
* Undo / Redo
* Editor와 Runtime Validation 통합

## 설계 방향

이 패키지는 다음 책임을 분리합니다.

* **Definition** — Quest, Objective, Prerequisite, Reward 정적 데이터
* **Runtime State** — Quest 상태, 진행도 및 완료 횟수
* **QuestManager** — Runtime Quest 진행 흐름
* **Validation** — Quest Definition 구조 검증
* **Snapshot** — Runtime 상태의 외부 저장·복원을 위한 데이터 표현
* **Editor** — Quest Definition 제작과 관계 탐색 및 Validation

게임 고유 시스템은 Framework에 직접 포함하지 않습니다.

다음 기능은 사용하는 프로젝트가 담당합니다.

```text
적 처치 / 아이템 획득 등의 게임 이벤트 감지
Quest 자동 시작 정책
실제 Reward 지급
Inventory 연동
Currency 연동
Dialogue 연동
NPC 연동
Quest UI
Quest Marker
Quest Tracking UI
Quest 자동 수락
일일 / 주간 Quest Reset 시간 정책
파일 저장 및 불러오기
네트워크 동기화
```

Framework는 이러한 게임별 정책을 강제하지 않고 재사용 가능한 Quest Definition과 Runtime 진행 기반을 제공하는 데 집중합니다.

## v1.0 범위

`v1.0.0`은 다음 기능을 하나의 독립 Unity Package로 제공하는 것을 범위로 합니다.

```text
Quest Definition
Objective Definition
Reward Metadata
Prerequisite
Runtime Quest State
Objective Progress
Automatic Completion
Repeatable Quest
Completion Count
Runtime Events
State Capture / Restore
Definition Validation
Prerequisite Cycle Validation
Quest Editor
Search
Relationship Navigation
Validation Navigation
Undo / Redo
Result-based Error Handling
```
