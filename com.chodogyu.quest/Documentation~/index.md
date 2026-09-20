# ChoDogyu Quest Framework & Editor

`ChoDogyu Quest Framework & Editor`는 Unity 프로젝트에서 Quest Definition, Objective 진행, 선행 Quest, 반복 Quest, Runtime 상태, 상태 저장·복원 및 Quest 제작 Workflow를 일관되게 관리하기 위한 범용 Quest Framework입니다.

Framework는 특정 게임의 전투, NPC, Inventory, Dialogue, UI 또는 Save 구현을 직접 소유하지 않습니다.

게임 코드가 실제 게임 이벤트를 감지하고 Quest Runtime API를 호출하는 구조를 사용합니다.

```text
Game Event
예: 적 처치 / 아이템 획득 / 지역 도착
        ↓
Game Code
        ↓
QuestManager
        ↓
Objective Progress
        ↓
Objective Complete
        ↓
Quest Complete
        ↓
Game Code
        ↓
Reward / UI / Dialogue / 기타 시스템
```

Quest 데이터 제작은 별도의 Quest Editor에서 수행할 수 있습니다.

---

# 1. Package Information

```text
Package Name : com.chodogyu.quest
Display Name : ChoDogyu Quest Framework & Editor
Version      : 1.0.0
Unity        : 6000.3+
Author       : ChoDogyu
```

개발 환경:

```text
Unity 6.3 LTS
6000.3.9f1
```

Assembly:

```text
Runtime      : CDG.Quest
Editor       : CDG.Quest.Editor
Runtime Test : CDG.Quest.Tests.Runtime
Editor Test  : CDG.Quest.Tests.Editor
```

Namespace:

```text
CDG.Quest
CDG.Quest.Editor
```

---

# 2. Requirements

필수 패키지:

```text
ChoDogyu Core 1.0.0
```

Quest Framework는 ChoDogyu Core의 다음 타입을 사용합니다.

```text
Result
Result<T>
ResultError
```

Quest Framework의 `package.json`은 Core의 Git URL을 직접 Dependency로 선언하지 않습니다.

따라서 Git UPM으로 설치할 때는 **Core를 먼저 설치**해야 합니다.

다음 CDG 패키지는 필수 의존성이 아닙니다.

```text
ChoDogyu General Editor Tools
ChoDogyu Object Pooling
ChoDogyu Data Framework
ChoDogyu Save / Load Framework
ChoDogyu UI Framework
ChoDogyu Audio Framework
ChoDogyu Scene Framework
ChoDogyu Procedural Map Generation
```

Quest Framework는 다른 게임 시스템과 직접 결합하지 않습니다.

---

# 3. Installation

## 3.1 ChoDogyu Core v1.0.0

Unity Package Manager에서 다음 Git URL을 사용합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

Unity에서:

```text
Window
→ Package Management
→ Package Manager
→ +
→ Install package from git URL...
```

Core 설치가 완료되고 Console Error가 없는 것을 확인합니다.

---

## 3.2 ChoDogyu Quest Framework & Editor v1.0.0

Core 설치 후 다음 URL을 사용합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuQuest.git?path=/com.chodogyu.quest#v1.0.0
```

설치 후 Package Manager에서 다음 패키지를 확인할 수 있습니다.

```text
ChoDogyu Quest Framework & Editor
1.0.0
```

---

# 4. Package Structure

최종 패키지 구조:

```text
com.chodogyu.quest/
├─ Documentation~/
│  └─ index.md
├─ Editor/
│  ├─ AssemblyInfo.cs
│  ├─ CDG.Quest.Editor.asmdef
│  └─ Window/
├─ Runtime/
│  ├─ Definitions/
│  ├─ State/
│  ├─ Validation/
│  ├─ AssemblyInfo.cs
│  ├─ CDG.Quest.asmdef
│  ├─ QuestErrorCodes.cs
│  └─ QuestManager.cs
├─ Tests/
│  ├─ Editor/
│  └─ Runtime/
├─ CHANGELOG.md
├─ README.md
└─ package.json
```

이 패키지는 별도의 `Samples~`를 포함하지 않습니다.

기본 사용 흐름은 README와 본 Documentation의 Runtime 예제를 통해 제공합니다.

---

# 5. Architecture

Quest Framework는 다음 책임을 분리합니다.

```text
Quest Definition
        ↓
QuestCatalog
        ↓
QuestValidator
        ↓
QuestManager
        ↓
Runtime State
        ↓
Snapshot
```

Editor 측 흐름:

```text
QuestCatalog Asset
        ↓
Quest Editor
        ↓
SerializedProperty Editing
        ↓
Runtime QuestValidator
        ↓
Validation Result
```

핵심 설계 원칙은 다음과 같습니다.

```text
Definition
≠
Runtime State
```

Quest Asset 자체에는 플레이 진행 상태를 저장하지 않습니다.

---

# 6. Quest Definition

하나의 Quest는 `QuestDefinition`으로 표현됩니다.

주요 정보:

```text
Id
Title
Description
Objectives
PrerequisiteQuestIds
Rewards
IsRepeatable
```

Definition은 게임 실행 중 변경되는 진행 상태가 아니라 **정적인 Quest 설정 데이터**입니다.

---

# 7. QuestCatalog

여러 Quest Definition은 `QuestCatalog` ScriptableObject에서 관리합니다.

생성:

```text
Project Window
→ Create
→ CDG
→ Quest
→ Quest Catalog
```

`QuestCatalog`는 다음 역할만 담당합니다.

```text
Quest Definition 보관
Quest Definition 순서 유지
Runtime에 읽기 전용 Definition 제공
```

다음 정보는 저장하지 않습니다.

```text
현재 Quest 상태
Objective 진행도
완료 횟수
현재 Active Quest
```

이 정보는 `QuestManager`의 Runtime State에서 관리됩니다.

---

# 8. Objective Definition

Quest는 하나 이상의 Objective를 가집니다.

`ObjectiveDefinition`:

```text
Id
Title
Description
TargetProgress
```

예:

```text
Id             = kill_enemy
Title          = 적 처치
Description    = 적을 10마리 처치합니다.
TargetProgress = 10
```

`TargetProgress`는 1 이상이어야 합니다.

동일한 Quest 내부에서 Objective ID는 중복될 수 없습니다.

---

# 9. Reward Definition

Quest는 0개 이상의 Reward Definition을 가질 수 있습니다.

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

또는:

```text
Type   = Item
Key    = Potion
Amount = 3
```

Reward는 **게임에서 해석할 수 있는 메타데이터**입니다.

Quest Framework는 다음 작업을 직접 수행하지 않습니다.

```text
재화 추가
아이템 지급
경험치 추가
스킬 해금
스테이지 해금
Achievement 지급
```

실제 지급은 프로젝트 코드가 담당합니다.

---

# 10. Prerequisite Quest

Quest는 다른 Quest를 선행 조건으로 사용할 수 있습니다.

예:

```text
quest_tutorial
        ↓
quest_first_battle
        ↓
quest_boss
```

`quest_boss`가 `quest_first_battle`을 Prerequisite로 가진다면 해당 Quest가 최소 한 번 완료되기 전에는 시작할 수 없습니다.

여러 선행 Quest가 있다면 v1.0에서는 모두 만족해야 합니다.

```text
A 완료
AND
B 완료
AND
C 완료
```

완료 조건은 현재 상태가 아니라:

```text
CompletionCount > 0
```

입니다.

따라서 반복 Quest가 Reset되어 `Inactive` 상태로 돌아가도 과거 완료 이력은 유지됩니다.

---

# 11. QuestStatus

Quest Runtime 상태는 다음 세 가지입니다.

```csharp
QuestStatus.Inactive
QuestStatus.Active
QuestStatus.Completed
```

흐름:

```text
Inactive
   ↓ StartQuest
Active
   ↓ 모든 Objective 완료
Completed
```

반복 가능한 Quest:

```text
Completed
   ↓ ResetQuest
Inactive
```

---

# 12. QuestManager

Runtime의 주요 진입점은 `QuestManager`입니다.

`QuestManager`는 MonoBehaviour가 아닙니다.

게임 코드가 필요한 위치에서 생성하고 소유합니다.

---

# 13. Creating QuestManager

```csharp
using CDG.Core.Results;
using CDG.Quest;

Result<QuestManager> result =
    QuestManager.Create(catalog);
```

성공:

```csharp
QuestManager manager = result.Value;
```

실패:

```csharp
if (result.IsFailure)
{
    string code = result.Error.Code;
    string message = result.Error.Message;
}
```

`Create` 과정:

```text
QuestCatalog null 검사
        ↓
QuestValidator 실행
        ↓
Error Count 확인
        ↓
Error 존재
→ Failure

Error 없음
→ Runtime State 생성
→ Success
```

Warning은 Manager 생성을 막지 않습니다.

Error가 하나라도 존재하면 생성에 실패합니다.

---

# 14. Starting Quest

Quest 시작:

```csharp
Result result =
    manager.StartQuest("quest_main");
```

시작 조건:

```text
Quest 존재
+
현재 상태 = Inactive
+
모든 Prerequisite 완료
```

시작 가능 여부만 확인:

```csharp
bool canStart =
    manager.CanStartQuest("quest_main");
```

정상 시작 후:

```text
Inactive
→ Active
```

그리고:

```csharp
QuestStarted
```

이벤트가 발생합니다.

---

# 15. Objective Progress

진행도 증가:

```csharp
Result result =
    manager.AdvanceObjective(
        "quest_main",
        "kill_enemy",
        1);
```

현재 값에서 `amount`만큼 증가합니다.

`amount`는 1 이상이어야 합니다.

---

## 15.1 Absolute Progress

특정 값으로 직접 설정할 수도 있습니다.

```csharp
Result result =
    manager.SetObjectiveProgress(
        "quest_main",
        "kill_enemy",
        5);
```

음수 값은 허용되지 않습니다.

---

## 15.2 Clamp

Objective Target이:

```text
10
```

이고 다음과 같이 호출해도:

```csharp
manager.AdvanceObjective(
    "quest_main",
    "kill_enemy",
    100);
```

최종 진행도는:

```text
10
```

으로 제한됩니다.

Target을 초과한 진행도는 저장되지 않습니다.

---

# 16. Objective Completion

Objective가 처음 Target Progress에 도달하면:

```csharp
ObjectiveCompleted
```

이벤트가 발생합니다.

진행도 변경 시에는:

```csharp
ObjectiveProgressChanged
```

이벤트가 발생합니다.

전달 값:

```text
QuestDefinition
ObjectiveDefinition
Current Progress
Target Progress
```

---

# 17. Automatic Quest Completion

Quest의 모든 Objective가 완료되면 Quest는 자동으로 완료됩니다.

```text
모든 Objective Completed
        ↓
QuestStatus.Completed
        ↓
CompletionCount +1
        ↓
QuestCompleted Event
```

별도의:

```text
CompleteQuest()
```

API는 존재하지 않습니다.

Quest 완료 여부는 Objective 상태를 기준으로 자동 결정됩니다.

---

# 18. Runtime Events

`QuestManager`는 다음 이벤트를 제공합니다.

```csharp
public event Action<QuestDefinition> QuestStarted;

public event Action<
    QuestDefinition,
    ObjectiveDefinition,
    int,
    int> ObjectiveProgressChanged;

public event Action<
    QuestDefinition,
    ObjectiveDefinition> ObjectiveCompleted;

public event Action<QuestDefinition> QuestCompleted;

public event Action<QuestDefinition> QuestReset;
```

기본 연결 예:

```csharp
manager.QuestCompleted += OnQuestCompleted;

private void OnQuestCompleted(
    QuestDefinition quest)
{
    // 보상 지급
    // UI 갱신
    // Dialogue 시작
    // 다음 Quest 확인
}
```

---

# 19. Reward Integration

Quest 완료 이벤트에서 Reward를 확인할 수 있습니다.

```csharp
manager.QuestCompleted += quest =>
{
    for (int i = 0; i < quest.Rewards.Count; i++)
    {
        RewardDefinition reward =
            quest.Rewards[i];

        // 프로젝트 시스템으로 전달
    }
};
```

예:

```text
RewardDefinition
        ↓
Game Reward Handler
        ↓
Currency System
Inventory System
Unlock System
Experience System
```

Framework는 구체적인 Reward 처리기를 강제하지 않습니다.

---

# 20. Query API

Quest Definition 조회:

```csharp
Result<QuestDefinition> result =
    manager.GetQuestDefinition("quest_main");
```

Quest 상태:

```csharp
Result<QuestStatus> result =
    manager.GetQuestStatus("quest_main");
```

Objective 진행도:

```csharp
Result<int> result =
    manager.GetObjectiveProgress(
        "quest_main",
        "kill_enemy");
```

완료 횟수:

```csharp
Result<int> result =
    manager.GetCompletionCount("quest_main");
```

현재 Active Quest 목록:

```csharp
IReadOnlyList<QuestDefinition> active =
    manager.GetActiveQuests();
```

---

# 21. Repeatable Quest

Quest Definition에서:

```text
IsRepeatable = true
```

인 Quest는 완료 후 Reset할 수 있습니다.

```csharp
Result result =
    manager.ResetQuest("quest_daily");
```

조건:

```text
QuestStatus.Completed
+
IsRepeatable = true
```

Reset 결과:

```text
Quest Status
Completed
→ Inactive
```

Objective:

```text
CurrentProgress
→ 0
```

완료 횟수:

```text
CompletionCount
→ 유지
```

---

# 22. Repeat Policy Responsibility

Framework는 다음 정책을 직접 관리하지 않습니다.

```text
Daily Quest
Weekly Quest
매일 00:00 Reset
서버 시간 기준 Reset
로그인 시 Reset
Season Reset
```

프로젝트가 Reset 조건을 판단한 뒤:

```csharp
manager.ResetQuest(id);
```

를 호출합니다.

이렇게 함으로써 Quest Runtime이 게임의 시간 정책에 종속되지 않습니다.

---

# 23. Capture State

현재 Quest Runtime 전체 상태:

```csharp
QuestStateCollectionSnapshot snapshot =
    manager.CaptureState();
```

Snapshot 구조:

```text
QuestStateCollectionSnapshot
└─ QuestStateSnapshot[]
   ├─ QuestId
   ├─ Status
   ├─ CompletionCount
   └─ ObjectiveStateSnapshot[]
      ├─ ObjectiveId
      └─ CurrentProgress
```

Capture된 데이터는 현재 Runtime과 독립적인 상태 데이터입니다.

---

# 24. Restore State

복원:

```csharp
Result result =
    manager.RestoreState(snapshot);
```

Restore는 Snapshot을 바로 적용하지 않습니다.

먼저 전체 상태를 검증합니다.

```text
Snapshot
   ↓
전체 Validation
   ↓
Quest ID 확인
Objective ID 확인
Status 확인
Progress 확인
Completion Count 확인
Definition 일치 확인
   ↓
모두 유효
→ Runtime 적용
```

잘못된 Snapshot이라면:

```text
Failure
+
기존 Runtime State 유지
```

입니다.

부분적으로만 적용되는 상태를 만들지 않습니다.

---

# 25. Restore Event Policy

Restore 과정에서는 Runtime Event를 발생시키지 않습니다.

즉 다음 이벤트가 Restore 때문에 다시 실행되지 않습니다.

```text
QuestStarted
ObjectiveProgressChanged
ObjectiveCompleted
QuestCompleted
QuestReset
```

Save 데이터 복원과 실제 Gameplay Event를 구분하기 위한 정책입니다.

---

# 26. Save Responsibility

Quest Framework는 Snapshot만 제공합니다.

다음 작업은 수행하지 않습니다.

```text
JSON 변환
Binary 변환
파일 생성
파일 암호화
PlayerPrefs 저장
Cloud Save
Save Slot 관리
자동 Save
자동 Load
```

프로젝트 또는 Save Framework가 처리합니다.

예:

```text
QuestManager
→ CaptureState()

QuestStateCollectionSnapshot
→ Save System
→ JSON / Binary / 기타 형식
→ File
```

Load:

```text
File
→ Save System
→ QuestStateCollectionSnapshot
→ QuestManager.RestoreState()
```

---

# 27. Save Framework Integration

ChoDogyu Save / Load Framework를 함께 사용할 수 있지만 두 패키지는 직접 의존하지 않습니다.

```text
Quest
-X→ Save
```

프로젝트 Composition Layer에서 연결합니다.

예:

```text
Quest Runtime
→ Capture Snapshot

Game Save Data
→ Snapshot 보관

Save Framework
→ Game Save Data 저장
```

이 구조를 통해 Quest Framework가 특정 저장 형식을 강제하지 않습니다.

---

# 28. Quest Validation

Quest Definition은 `QuestValidator`를 통해 검증할 수 있습니다.

```csharp
IReadOnlyList<QuestValidationIssue> issues =
    QuestValidator.Validate(catalog);
```

각 Issue:

```text
Severity
Code
Message
QuestIndex
QuestId
ObjectiveIndex
ObjectiveId
PrerequisiteIndex
RewardIndex
```

---

# 29. Validation Severity

두 단계로 구분됩니다.

```text
Warning
Error
```

### Warning

Quest 실행 자체를 반드시 막을 필요는 없지만 제작자가 확인해야 하는 문제입니다.

### Error

정상적인 Runtime 실행을 보장할 수 없어 수정해야 하는 문제입니다.

`QuestManager.Create`는 Error가 존재하면 실패합니다.

---

# 30. Quest Validation Rules

## Quest

검사 항목:

```text
Null Quest
Empty Quest ID
Duplicate Quest ID
Empty Title
Empty Description
No Objective
```

Title과 Description 누락은 Warning입니다.

ID와 Objective 구조 문제는 Error입니다.

---

# 31. Objective Validation

검사 항목:

```text
Null Objective
Empty Objective ID
Duplicate Objective ID
Empty Objective Title
TargetProgress <= 0
```

Objective Title 누락은 Warning입니다.

ID 및 Target Progress 문제는 Error입니다.

---

# 32. Prerequisite Validation

검사 항목:

```text
Empty ID
Duplicate ID
Self Reference
Missing Quest
Cycle
```

예:

```text
Quest A
→ Quest B 필요

Quest B
→ Quest A 필요
```

위 구조는 Cycle Error입니다.

더 긴 순환도 검사합니다.

```text
A
→ B
→ C
→ A
```

---

# 33. Reward Validation

검사 항목:

```text
Null Reward
Empty Type
Empty Key
Amount <= 0
```

Reward가 하나도 없는 Quest는 허용됩니다.

Reward가 존재한다면 각 Reward는 유효한 값을 가져야 합니다.

---

# 34. Validation Codes

Validation 문제 종류는 `QuestValidationCodes`를 사용해 안정적으로 비교할 수 있습니다.

예:

```csharp
if (issue.Code ==
    QuestValidationCodes.PrerequisiteCycle)
{
    // 선행 Quest 순환 문제 처리
}
```

대표 코드:

```text
QUEST_NULL
QUEST_ID_REQUIRED
QUEST_ID_DUPLICATE
QUEST_TITLE_MISSING
QUEST_DESCRIPTION_MISSING

OBJECTIVE_MISSING
OBJECTIVE_NULL
OBJECTIVE_ID_REQUIRED
OBJECTIVE_ID_DUPLICATE
OBJECTIVE_TITLE_MISSING
OBJECTIVE_TARGET_PROGRESS_INVALID

PREREQUISITE_ID_REQUIRED
PREREQUISITE_DUPLICATE
PREREQUISITE_SELF_REFERENCE
PREREQUISITE_NOT_FOUND
PREREQUISITE_CYCLE

REWARD_NULL
REWARD_TYPE_REQUIRED
REWARD_KEY_REQUIRED
REWARD_AMOUNT_INVALID
```

Message 문자열 대신 Code를 기준으로 처리하는 것을 권장합니다.

---

# 35. Runtime Error Codes

Runtime 실패는 `QuestErrorCodes`를 사용합니다.

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

예:

```csharp
Result result =
    manager.StartQuest("quest_boss");

if (result.IsFailure &&
    result.Error.Code ==
        QuestErrorCodes.PrerequisiteNotCompleted)
{
    // 아직 선행 Quest 미완료
}
```

---

# 36. Quest Editor

Quest 제작을 위한 Editor Window를 제공합니다.

메뉴:

```text
Tools
→ ChoDogyu
→ Quest Editor
```

Window Title:

```text
CDG Quest
```

---

# 37. Selecting QuestCatalog

Quest Editor 상단에서 편집할 Catalog를 지정합니다.

다음 두 방식으로 선택할 수 있습니다.

```text
Quest Catalog Object Field
```

또는 현재 Project Window에서 선택한 QuestCatalog를:

```text
Use Selection
```

으로 적용할 수 있습니다.

---

# 38. Quest List

왼쪽 영역에는 현재 Catalog의 Quest 목록이 표시됩니다.

각 Quest는 ID와 Title을 기준으로 확인할 수 있습니다.

Quest 선택 시 오른쪽에서 Definition을 편집합니다.

---

# 39. Add Quest

```text
Add Quest
```

버튼을 사용하면 새로운 Quest가 추가됩니다.

기본 ID:

```text
quest_001
quest_002
quest_003
...
```

현재 사용되지 않는 첫 번째 번호를 자동으로 사용합니다.

예:

```text
quest_001
quest_003
```

상태라면 새 Quest는:

```text
quest_002
```

를 사용할 수 있습니다.

---

# 40. Delete Quest

선택한 Quest를 삭제할 수 있습니다.

삭제 후 가능한 다음 Quest로 Selection이 이동합니다.

Quest가 하나도 남지 않으면 선택 상태가 해제됩니다.

---

# 41. Quest Definition Editing

다음 값을 수정할 수 있습니다.

```text
ID
Title
Description
Repeatable
```

ID 변경으로 인해 기존 Prerequisite가 깨질 수 있습니다.

이 경우 Editor Validation에서:

```text
PREREQUISITE_NOT_FOUND
```

문제를 확인할 수 있습니다.

Framework가 다른 Quest의 Prerequisite ID를 자동 변경하지는 않습니다.

---

# 42. Objective Editing

Quest에 Objective를 추가하거나 삭제할 수 있습니다.

기본 Objective ID:

```text
objective_001
objective_002
objective_003
...
```

편집 항목:

```text
ID
Title
Description
Target Progress
```

---

# 43. Reward Editing

Reward 추가 및 삭제를 지원합니다.

편집 항목:

```text
Type
Key
Amount
```

Reward Type과 Key의 의미는 프로젝트가 정의합니다.

Editor는 특정 Reward Type 목록을 강제하지 않습니다.

---

# 44. Prerequisite Editing

다른 Quest를 선행 조건으로 추가할 수 있습니다.

후보에서 자동 제외되는 항목:

```text
현재 Quest 자신
이미 다른 Prerequisite Entry에서 사용 중인 Quest
빈 Quest ID
```

이미 저장되어 있는 잘못된 기존 값은 즉시 삭제하지 않고 UI에서 유지합니다.

Validation을 통해 문제를 확인한 뒤 제작자가 수정할 수 있도록 하기 위한 정책입니다.

---

# 45. Search

Quest Editor에서 Quest ID 또는 Title을 기준으로 검색할 수 있습니다.

```text
Search
Clear
```

검색은 Catalog 자체를 변경하지 않습니다.

표시되는 Quest 목록만 필터링합니다.

---

# 46. Relationship Navigation

선택한 Quest의 관계를 확인할 수 있습니다.

### Prerequisites

현재 Quest가 필요로 하는 Quest 목록입니다.

```text
Current Quest
→ Prerequisite Quest
```

### Dependents

현재 Quest를 Prerequisite로 사용하는 다른 Quest 목록입니다.

```text
Other Quest
→ Current Quest를 필요로 함
```

관계 목록의:

```text
Go
```

버튼을 사용하면 해당 Quest로 바로 이동할 수 있습니다.

---

# 47. Editor Validation

Quest Editor 하단에서 현재 Catalog Validation 결과를 확인할 수 있습니다.

요약:

```text
Total
Errors
Warnings
```

문제별 표시:

```text
Severity
Code
Location
Message
```

Quest 위치가 있는 Issue는:

```text
Go
```

버튼으로 해당 Quest로 이동할 수 있습니다.

---

# 48. Runtime And Editor Validation

Editor는 별도의 Validation 규칙을 구현하지 않습니다.

```text
Quest Editor
        ↓
QuestValidator
```

Runtime에서도 동일합니다.

```text
QuestManager.Create
        ↓
QuestValidator
```

따라서:

```text
Editor 제작 규칙
=
Runtime 실행 규칙
```

을 유지합니다.

---

# 49. Undo / Redo

Quest Editor 편집은 Unity Undo System과 연동됩니다.

예:

```text
Quest 추가
Quest 삭제
Objective 추가
Objective 삭제
Reward 추가
Reward 삭제
Prerequisite 변경
Definition 값 편집
```

Unity 기본:

```text
Ctrl + Z
Ctrl + Y
```

를 사용할 수 있습니다.

---

# 50. Saving QuestCatalog

Editor의:

```text
Save Catalog
```

기능을 통해 현재 Catalog Asset 변경 사항을 디스크에 저장할 수 있습니다.

변경된 Asset은 Unity Dirty 상태로 유지됩니다.

---

# 51. Runtime Responsibility

Quest Framework Runtime의 책임:

```text
Definition 조회
Quest 상태 관리
Objective 진행
Quest 완료
Prerequisite 확인
Repeatable Reset
Completion Count
Runtime Event
Capture
Restore
Runtime Validation
```

---

# 52. Game Responsibility

게임 프로젝트의 책임:

```text
적 처치 감지
아이템 획득 감지
NPC 대화 감지
지역 도착 감지
상호작용 감지

Quest 자동 시작 여부
Quest 수락 UI
Quest 추적 UI
Quest Marker
Quest Journal

실제 Reward 지급
Inventory 연동
Currency 연동
Experience 연동
Unlock 연동

Daily / Weekly Reset 시간
Server Time 처리

Save 파일 관리
Network 동기화
```

---

# 53. No Forced Singleton

`QuestManager`는 Singleton이 아닙니다.

Framework는:

```text
QuestManager.Instance
```

같은 전역 접근 API를 제공하지 않습니다.

이유:

```text
프로젝트별 생명주기 정책 유지
테스트 용이성
명시적 의존성 전달
여러 Quest Context 구성 가능
전역 상태 강제 방지
```

필요하다면 사용하는 프로젝트에서 Singleton이나 Service Layer를 구성할 수 있습니다.

---

# 54. No Automatic Game Event Detection

Framework는 게임의 적 처치 이벤트를 직접 감지하지 않습니다.

예:

```text
EnemySystem
→ EnemyKilled

Game Quest Bridge
→ QuestManager.AdvanceObjective(...)
```

아이템도 동일합니다.

```text
Inventory
→ ItemAdded

Game Quest Bridge
→ QuestManager.AdvanceObjective(...)
```

Quest Framework가 Enemy, Inventory 같은 게임 시스템에 직접 의존하지 않게 하기 위한 구조입니다.

---

# 55. No Automatic Quest Start

Prerequisite가 완료되더라도 다음 Quest를 자동으로 시작하지 않습니다.

게임 코드가:

```csharp
if (manager.CanStartQuest(nextQuestId))
{
    manager.StartQuest(nextQuestId);
}
```

처럼 정책을 결정합니다.

이를 통해:

```text
자동 시작
NPC 수락
UI 버튼 수락
특정 Scene 진입 후 시작
Dialogue 종료 후 시작
```

같은 다양한 게임 규칙을 적용할 수 있습니다.

---

# 56. No Automatic Reward Grant

Quest 완료 후 Reward는 자동 지급되지 않습니다.

Framework는:

```text
Reward Definition 제공
```

까지만 담당합니다.

프로젝트가:

```text
Reward Type
Reward Key
Reward Amount
```

를 해석합니다.

---

# 57. No Automatic Save

Quest 상태는 자동 저장되지 않습니다.

다음 시점에 저장할지는 프로젝트가 결정합니다.

```text
Objective 진행 시
Quest 완료 시
Scene 이동 시
Checkpoint 도달 시
게임 종료 시
주기적 Auto Save
```

Framework는 Capture API만 제공합니다.

---

# 58. Public QuestManager API

생성:

```csharp
public static Result<QuestManager> Create(
    QuestCatalog catalog);
```

조회:

```csharp
public Result<QuestDefinition> GetQuestDefinition(
    string questId);

public Result<QuestStatus> GetQuestStatus(
    string questId);

public Result<int> GetCompletionCount(
    string questId);

public Result<int> GetObjectiveProgress(
    string questId,
    string objectiveId);

public IReadOnlyList<QuestDefinition>
    GetActiveQuests();

public bool CanStartQuest(
    string questId);
```

진행:

```csharp
public Result StartQuest(
    string questId);

public Result AdvanceObjective(
    string questId,
    string objectiveId,
    int amount);

public Result SetObjectiveProgress(
    string questId,
    string objectiveId,
    int progress);

public Result ResetQuest(
    string questId);
```

State:

```csharp
public QuestStateCollectionSnapshot
    CaptureState();

public Result RestoreState(
    QuestStateCollectionSnapshot snapshot);
```

---

# 59. Tests

Quest Framework는 Runtime과 Editor Test Assembly를 분리합니다.

```text
Tests/
├─ Runtime/
│  └─ CDG.Quest.Tests.Runtime
└─ Editor/
   └─ CDG.Quest.Tests.Editor
```

최종 자동화 테스트 결과:

```text
Runtime: 81 Passed
Editor:  19 Passed
Total:  100 Passed
```

Runtime 검증 범위:

```text
QuestCatalog
QuestDefinition
ObjectiveDefinition
RewardDefinition

Quest Validation
Duplicate ID
Invalid Progress
Prerequisite
Cycle Detection
Reward Validation

QuestManager Creation
StartQuest
CanStartQuest
Objective Progress
Set Progress
Automatic Completion
Runtime Events
Repeatable Quest
Completion Count

Runtime State
Capture
Restore
Invalid Snapshot
Atomic Restore

Runtime Integration
```

Editor 검증 범위:

```text
Quest Add / Delete
Automatic Quest ID
Objective Add / Delete
Automatic Objective ID
Reward Add / Delete
Prerequisite Editing
Relationship Query
Undo / Redo
Runtime Validation Integration
```

---

# 60. UPM Package Design

Quest Framework는 개발 프로젝트와 실제 패키지를 분리합니다.

```text
ChoDogyuQuest/
├─ QuestDevelopment/
└─ com.chodogyu.quest/
```

`QuestDevelopment`는 개발과 검증을 위한 Unity 프로젝트입니다.

실제 배포 대상은:

```text
com.chodogyu.quest/
```

입니다.

Git UPM 설치 시:

```text
?path=/com.chodogyu.quest
```

를 사용합니다.

---

# 61. Package Removal

Quest Framework는 Core 외 다른 CDG 패키지를 자동 설치하거나 제거하지 않습니다.

Quest Package 제거:

```text
Quest Framework Remove
→ ChoDogyu Core 유지
```

Core가 다른 패키지에서도 사용될 수 있기 때문입니다.

---

# 62. v1.0 Scope

`v1.0.0`의 범위:

```text
Quest Definition
Objective Definition
Reward Metadata
QuestCatalog

Quest Runtime State
Objective Runtime State
Quest Status

Quest Start
Objective Progress
Automatic Objective Completion
Automatic Quest Completion

Prerequisite
Multiple Prerequisites
Prerequisite Cycle Validation

Repeatable Quest
Quest Reset
Completion Count

Runtime Event

Capture
Restore
Snapshot Validation

Quest Validation
Validation Severity
Validation Code

Runtime Error Code

Quest Editor
Quest Editing
Objective Editing
Reward Editing
Prerequisite Editing
Search
Relationship Navigation
Validation Navigation
Undo / Redo

Runtime Tests
Editor Tests
UPM Package
```

---

# 63. Features Outside v1.0 Scope

다음 기능은 v1.0에서 의도적으로 제외했습니다.

```text
Quest 자동 수락
Quest 자동 시작
Quest 포기
Quest 실패 상태
Quest 시간 제한
Quest 만료 시간
Daily 자동 Reset
Weekly 자동 Reset

Optional Objective
Hidden Objective
Branch Objective
AND / OR Objective Tree
Weighted Objective
Objective Condition Graph

Reward 자동 지급
Inventory 직접 연동
Currency 직접 연동
Experience 직접 연동

NPC Quest Provider
Dialogue System
Quest Marker
World Map Marker
Quest Journal UI
Quest Tracking HUD

Addressables 자동 연동
Localization 자동 연동
Save 자동 연동
Cloud Save
Network Replication
Server Authoritative Quest

Quest Graph Editor
Node Editor
Visual Scripting Integration
```

기능 부족 때문에 제외한 것이 아니라 v1.0의 책임 범위를 명확하게 유지하기 위한 결정입니다.

---

# 64. Extension Direction

현재 구조를 유지하면서 다음 기능을 별도 계층으로 확장할 수 있습니다.

예:

```text
Quest Acceptance Layer
Quest Failure Layer
Timed Quest Layer
Daily Quest Scheduler
Quest Condition System
Optional Objective
Quest Branching

Reward Handler
Inventory Reward Adapter
Currency Reward Adapter

Quest UI Presenter
Quest Journal
Quest Marker

Save Adapter
Server Quest Adapter
Localization Adapter

Graph Editor
```

확장 기능은 기존 `QuestManager`의 기본 책임을 불필요하게 비대하게 만들지 않는 방향을 권장합니다.

---

# 65. Design Principles

## Definition / Runtime State 분리

```text
QuestCatalog
→ 정적 Definition

QuestManager
→ 실행 중 State
```

Asset 자체에 플레이 상태를 기록하지 않습니다.

---

## 최소 의존성

```text
Quest
→ Core

Quest
-X→ Data
-X→ Save
-X→ UI
-X→ Audio
-X→ Scene
-X→ Map Generation
```

---

## 명시적 진행

```text
Game Event
→ Game Code
→ Quest API
```

Framework가 게임 이벤트를 추측하거나 자동 탐색하지 않습니다.

---

## 명시적 실패

```text
Result
Result<T>
QuestErrorCodes
```

예외 중심 흐름 대신 예상 가능한 Runtime 실패를 Result로 표현합니다.

---

## 동일 Validation Core

```text
Editor
→ QuestValidator

Runtime
→ QuestValidator
```

제작 환경과 Runtime의 규칙 차이를 줄입니다.

---

## Atomic Restore

```text
Validate All
→ Apply All
```

Restore 실패 시 기존 상태를 보존합니다.

---

## 프로젝트 정책 비강제

```text
No Singleton
No Automatic Start
No Automatic Reward
No Automatic Save
No Automatic Daily Reset
No Inventory Dependency
No UI Dependency
```

---

# 66. Troubleshooting

## QuestManager.Create가 QUEST_VALIDATION_FAILED를 반환합니다

QuestCatalog에 Error 수준 Validation 문제가 존재합니다.

Quest Editor를 열고:

```text
Validation
→ Errors 확인
```

후 각 Issue의 `Go` 버튼으로 문제가 있는 Quest를 확인합니다.

---

## Quest를 시작할 수 없습니다

현재 상태를 확인합니다.

```csharp
manager.GetQuestStatus(id);
```

Quest 시작 조건:

```text
Inactive
+
모든 Prerequisite 완료
```

`CanStartQuest`도 사용할 수 있습니다.

```csharp
bool canStart =
    manager.CanStartQuest(id);
```

---

## QUEST_PREREQUISITE_NOT_COMPLETED가 반환됩니다

해당 Quest의 Prerequisite 중 아직 한 번도 완료되지 않은 Quest가 있습니다.

Prerequisite의:

```text
CompletionCount
```

가 1 이상인지 확인합니다.

---

## Objective 진행이 되지 않습니다

Quest가 Active 상태인지 확인합니다.

```text
Inactive
→ 진행 불가

Active
→ 진행 가능

Completed
→ 진행 불가
```

---

## QUEST_OBJECTIVE_ALREADY_COMPLETED가 반환됩니다

해당 Objective가 이미 Target Progress에 도달했습니다.

완료된 Objective에 추가 진행도를 적용하지 않습니다.

---

## Quest가 예상보다 빨리 완료됩니다

모든 Objective의 Current Progress와 Target Progress를 확인합니다.

Quest는 모든 Objective가 완료되는 순간 자동으로 Completed 상태가 됩니다.

---

## Repeatable Quest를 Reset할 수 없습니다

다음 조건을 모두 확인합니다.

```text
Quest Status = Completed
IsRepeatable = true
```

둘 중 하나라도 만족하지 않으면 Reset할 수 없습니다.

---

## Quest가 Reset됐는데 선행 Quest 완료 조건은 여전히 만족합니다

정상 동작입니다.

Prerequisite는 현재 상태가 아니라:

```text
CompletionCount > 0
```

을 기준으로 합니다.

Reset해도 Completion Count는 유지됩니다.

---

## Quest ID를 변경한 뒤 다른 Quest에 Validation Error가 발생합니다

Prerequisite는 Quest ID 문자열을 참조합니다.

Quest ID를 변경해도 다른 Quest의 기존 Prerequisite가 자동 변경되지는 않습니다.

Validation에서:

```text
PREREQUISITE_NOT_FOUND
```

Issue를 확인하고 관계를 수정합니다.

---

## Prerequisite를 추가할 수 없습니다

현재 Catalog에 선택 가능한 다른 Quest가 있는지 확인합니다.

다음 Quest는 후보에서 제외됩니다.

```text
현재 Quest 자신
이미 같은 Quest에서 Prerequisite로 사용 중인 Quest
빈 ID Quest
```

---

## PREREQUISITE_CYCLE이 발생합니다

선행 Quest 관계가 다시 자기 자신으로 돌아오는 구조인지 확인합니다.

예:

```text
A → B
B → A
```

또는:

```text
A → B
B → C
C → A
```

순환 관계를 제거해야 합니다.

---

## Reward가 자동으로 지급되지 않습니다

정상 동작입니다.

Quest Framework는 Reward Definition만 제공합니다.

실제 Reward 처리는:

```csharp
QuestCompleted
```

이벤트를 통해 프로젝트 코드에서 수행해야 합니다.

---

## CaptureState를 했는데 파일이 생성되지 않습니다

정상 동작입니다.

`CaptureState()`는 Runtime Snapshot을 반환하는 API입니다.

파일 저장은 별도 Save System의 책임입니다.

---

## RestoreState 후 QuestCompleted 이벤트가 발생하지 않습니다

정상 동작입니다.

Restore는 Save 상태를 복구하는 작업이며 Gameplay Event를 재생하지 않습니다.

Restore 중에는 Runtime Quest Event를 발생시키지 않습니다.

---

## Quest Editor가 보이지 않습니다

메뉴:

```text
Tools
→ ChoDogyu
→ Quest Editor
```

를 확인합니다.

또한:

```text
CDG.Quest
CDG.Quest.Editor
```

Assembly Compile Error가 없는지 Console을 확인합니다.

---

## QuestCatalog 생성 메뉴가 없습니다

다음 메뉴를 확인합니다.

```text
Create
→ CDG
→ Quest
→ Quest Catalog
```

Runtime Assembly가 정상적으로 Compile되었는지 확인합니다.

---

# 67. Recommended Project Composition

프로젝트에서는 QuestManager를 모든 코드가 임의로 생성하는 방식보다 명확한 Composition Root에서 관리하는 것을 권장합니다.

예:

```text
Game Root
├─ Quest Runtime
├─ Save System
├─ Inventory System
├─ UI System
└─ Scene System
```

연결:

```text
Enemy System
→ Quest Bridge
→ QuestManager

Inventory System
→ Quest Bridge
→ QuestManager

QuestManager
→ QuestCompleted
→ Reward Handler

QuestManager
→ Runtime Events
→ Quest UI Presenter
```

Framework는 이 Composition 구조 자체를 강제하지 않습니다.

---

# 68. Summary

ChoDogyu Quest Framework & Editor v1.0.0은 다음 문제를 해결하는 것을 목표로 합니다.

```text
Quest Definition과 진행 상태가 한 곳에 섞임
→ Definition / Runtime State 분리

Quest마다 진행 로직이 반복됨
→ QuestManager로 통합

Objective 완료 판정이 분산됨
→ Framework에서 자동 판정

선행 Quest 로직이 프로젝트마다 반복됨
→ Prerequisite 관리 제공

반복 Quest 완료 이력 관리가 복잡함
→ Completion Count 제공

Quest 저장 구조가 Runtime 구현에 종속됨
→ Snapshot 제공

잘못된 Quest 데이터가 Runtime까지 전달됨
→ QuestValidator 제공

선행 Quest 순환 관계 발견이 어려움
→ Cycle Validation 제공

Quest 제작을 Inspector에서 직접 수정하기 불편함
→ 전용 Quest Editor 제공

Quest 간 관계 확인이 어려움
→ Relationship Navigation 제공

Editor와 Runtime Validation 규칙이 달라질 수 있음
→ 동일 QuestValidator 사용

Quest가 Inventory / UI / Save를 직접 알아야 함
→ 게임 시스템 책임 분리
```

Framework는 특정 게임의 완성된 Quest 시스템 전체를 대신 만드는 패키지가 아니라, 여러 Unity 프로젝트에서 반복되는 Quest Definition, Runtime 진행, Validation 및 제작 Workflow를 재사용 가능한 독립 UPM 패키지로 제공하는 것을 목표로 합니다.
