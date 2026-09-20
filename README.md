# ChoDogyu Quest Framework & Editor

Unity 프로젝트에서 재사용할 수 있도록 제작한 범용 Quest Framework & Editor입니다.

Quest Definition, Objective 진행, 선행 Quest, 반복 Quest, Runtime 상태 관리, Capture / Restore 및 Quest 제작용 Editor Tool을 하나의 독립 UPM 패키지로 제공합니다.

특정 게임의 전투, NPC, Inventory, Dialogue, UI 또는 Save 시스템에 직접 의존하지 않으며, 게임 코드가 실제 게임 이벤트를 감지하여 Quest Runtime API를 호출하는 구조로 구성했습니다.

---

## 주요 기능

### Quest Definition

* `QuestCatalog` 기반 Quest 관리
* Quest ID / Title / Description
* Objective
* Prerequisite
* Reward Metadata
* Repeatable 설정
* Definition과 Runtime State 분리

### Runtime

* `QuestManager` 기반 Quest 진행 관리
* Quest 시작 가능 여부 확인
* Quest 시작
* Objective 진행도 증가
* Objective 진행도 직접 설정
* Target Progress Clamp
* Objective 자동 완료
* 모든 Objective 완료 시 Quest 자동 완료
* Active Quest 조회
* Quest 상태 조회
* Objective 진행도 조회
* 완료 횟수 조회

### Prerequisite

* 여러 선행 Quest 지원
* 모든 선행 Quest 완료 조건
* 완료 횟수 기반 선행 조건 판단
* 자기 참조 Validation
* 존재하지 않는 Quest 참조 Validation
* 순환 선행 관계 Validation

### Repeatable Quest

* 완료된 반복 Quest Reset
* Objective 진행도 초기화
* Completion Count 유지
* 반복 시점과 주기는 게임 코드에서 결정

### Runtime Events

* Quest Started
* Objective Progress Changed
* Objective Completed
* Quest Completed
* Quest Reset

### State Capture / Restore

* 전체 Quest Runtime 상태 Capture
* Quest 상태 저장
* Objective 진행 상태 저장
* Completion Count 저장
* Snapshot 전체 Validation
* Restore 실패 시 기존 Runtime 상태 유지
* Restore 과정에서 Gameplay Event 재발생 방지

### Validation

* Quest ID 누락 및 중복 검사
* Quest Title / Description Warning
* Objective 누락 검사
* Objective ID 누락 및 중복 검사
* Target Progress 검사
* Prerequisite 검사
* Prerequisite Cycle 검사
* Reward Type / Key / Amount 검사
* `QuestValidationCodes` 기반 안정적인 Issue Code

### Quest Editor

* QuestCatalog 선택
* Quest 추가 / 삭제
* Quest Definition 편집
* Objective 추가 / 삭제 및 편집
* Prerequisite 편집
* Reward 편집
* Quest 검색
* 선행 Quest 관계 탐색
* Dependent Quest 탐색
* 관계 항목으로 Quest 이동
* Runtime Validator 기반 Validation
* Validation 위치 이동
* Undo / Redo
* Catalog 저장

---

## 핵심 구조

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

게임 이벤트와 Quest Runtime의 관계:

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
Quest Complete
        ↓
Game Code
        ↓
Reward / UI / Dialogue / 기타 시스템
```

Framework는 게임 이벤트를 직접 감지하지 않습니다.

---

## 저장소 구조

```text
ChoDogyuQuest/
├─ QuestDevelopment/
│  └─ 패키지 개발 및 검증용 Unity 프로젝트
│
├─ com.chodogyu.quest/
│  ├─ Documentation~/
│  │  └─ index.md
│  ├─ Editor/
│  ├─ Runtime/
│  ├─ Tests/
│  │  ├─ Editor/
│  │  └─ Runtime/
│  ├─ CHANGELOG.md
│  ├─ README.md
│  └─ package.json
│
├─ .gitattributes
├─ .gitignore
└─ README.md
```

### QuestDevelopment

Quest Framework의 개발, 테스트 및 통합 검증에 사용하는 Unity 프로젝트입니다.

실제 UPM 배포 대상에는 포함되지 않습니다.

### com.chodogyu.quest

실제 Unity Package Manager를 통해 설치하는 UPM 패키지입니다.

다른 Unity 프로젝트에서는 이 폴더를 Git UPM 패키지로 설치합니다.

---

## Package

```text
com.chodogyu.quest
```

Runtime Assembly:

```text
CDG.Quest
```

Editor Assembly:

```text
CDG.Quest.Editor
```

Runtime Tests:

```text
CDG.Quest.Tests.Runtime
```

Editor Tests:

```text
CDG.Quest.Tests.Editor
```

Unity:

```text
6000.3 이상
```

필수 Dependency:

```text
com.chodogyu.core
```

---

## Requirements

* Unity 6.3 이상
* ChoDogyu Core 1.0.0

개발 및 검증 환경:

```text
Unity 6.3 LTS
6000.3.9f1
```

Quest Framework는 ChoDogyu Core의 다음 Result 계열 타입을 사용합니다.

```text
Result
Result<T>
ResultError
```

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

---

## Installation

### 1. ChoDogyu Core v1.0.0

먼저 Core를 설치합니다.

```text
https://github.com/ChoDoGyu/ChoDogyuCore.git?path=/com.chodogyu.core#v1.0.0
```

### 2. ChoDogyu Quest Framework & Editor v1.0.0

```text
https://github.com/ChoDoGyu/ChoDogyuQuest.git?path=/com.chodogyu.quest#v1.0.0
```

Unity Package Manager에서:

```text
Window
→ Package Management
→ Package Manager
→ +
→ Install package from git URL...
```

순서로 설치합니다.

---

## QuestCatalog

Quest Definition은 `QuestCatalog` ScriptableObject에서 관리합니다.

생성 메뉴:

```text
Create
→ CDG
→ Quest
→ Quest Catalog
```

Catalog에는 여러 Quest Definition을 등록할 수 있습니다.

Quest 플레이 상태는 Catalog Asset에 저장되지 않습니다.

---

## QuestManager

Runtime의 주요 진입점은 `QuestManager`입니다.

```csharp
using CDG.Core.Results;
using CDG.Quest;

Result<QuestManager> result =
    QuestManager.Create(catalog);

if (result.IsFailure)
{
    return;
}

QuestManager manager = result.Value;
```

Manager 생성 시 Catalog 전체 Validation을 수행합니다.

Warning은 생성을 막지 않지만 Error가 존재하면 생성에 실패합니다.

---

## Quest 시작

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

시작 가능 여부:

```csharp
bool canStart =
    manager.CanStartQuest("quest_main");
```

---

## Objective 진행

진행도 증가:

```csharp
manager.AdvanceObjective(
    "quest_main",
    "kill_enemy",
    1);
```

진행도 직접 설정:

```csharp
manager.SetObjectiveProgress(
    "quest_main",
    "kill_enemy",
    5);
```

Target Progress를 초과한 값은 자동으로 제한됩니다.

---

## Quest 자동 완료

모든 Objective가 완료되면 Quest는 자동으로:

```text
Completed
```

상태가 됩니다.

별도의 `CompleteQuest()` API는 사용하지 않습니다.

```text
Objective Progress
        ↓
Objective Completed
        ↓
모든 Objective 완료
        ↓
Quest Completed
```

---

## Runtime Events

```text
QuestStarted
ObjectiveProgressChanged
ObjectiveCompleted
QuestCompleted
QuestReset
```

Reward 처리, UI 갱신, Dialogue 시작 등의 게임 고유 동작은 이 이벤트를 통해 연결할 수 있습니다.

---

## Reward

Reward Definition:

```text
Type
Key
Amount
```

예:

```text
Currency
Gold
100
```

Quest Framework는 실제 Reward를 지급하지 않습니다.

```text
Quest Framework
→ Reward Metadata 제공

Game Code
→ Reward 해석

Inventory / Currency / Unlock System
→ 실제 지급
```

이 구조를 통해 Quest Framework가 특정 게임 시스템에 직접 의존하지 않습니다.

---

## Repeatable Quest

Repeatable Quest는 완료 후 Reset할 수 있습니다.

```csharp
manager.ResetQuest("quest_daily");
```

Reset 결과:

```text
Completed
→ Inactive

Objective Progress
→ 0

Completion Count
→ 유지
```

Daily / Weekly 등 실제 Reset 시점은 게임 코드에서 결정합니다.

---

## Capture / Restore

Runtime 상태 Capture:

```csharp
QuestStateCollectionSnapshot snapshot =
    manager.CaptureState();
```

Restore:

```csharp
Result result =
    manager.RestoreState(snapshot);
```

Snapshot에는 다음 정보가 포함됩니다.

```text
Quest ID
Quest Status
Completion Count
Objective ID
Objective Progress
```

Quest Framework는 Snapshot을 제공하지만 파일 저장 자체는 수행하지 않습니다.

---

## Save System과의 관계

Quest Framework와 Save Framework는 직접 의존하지 않습니다.

```text
Quest
-X→ Save
```

프로젝트에서 다음과 같이 연결할 수 있습니다.

```text
QuestManager
→ CaptureState()
→ Game Save Data
→ Save System
```

복원:

```text
Save System
→ Game Save Data
→ Quest Snapshot
→ RestoreState()
```

---

## Quest Editor

메뉴:

```text
Tools
→ ChoDogyu
→ Quest Editor
```

Editor에서 다음 기능을 사용할 수 있습니다.

```text
QuestCatalog 선택
Quest 추가 / 삭제

Quest
├─ ID
├─ Title
├─ Description
└─ Repeatable

Objective
├─ 추가 / 삭제
├─ ID
├─ Title
├─ Description
└─ Target Progress

Prerequisite
├─ 추가 / 삭제
└─ Quest 선택

Reward
├─ 추가 / 삭제
├─ Type
├─ Key
└─ Amount

Search

Relationships
├─ Prerequisites
└─ Dependents

Validation
├─ Error
├─ Warning
└─ Go

Undo / Redo
Save Catalog
```

---

## Validation

Quest Editor와 Runtime은 동일한 `QuestValidator`를 사용합니다.

```text
Quest Editor
→ QuestValidator

QuestManager.Create
→ QuestValidator
```

주요 검증:

```text
Quest ID
Quest ID Duplicate

Quest Title
Quest Description

Objective 존재 여부
Objective ID
Objective ID Duplicate
Objective Target Progress

Prerequisite
Prerequisite Duplicate
Self Reference
Missing Quest
Cycle

Reward Type
Reward Key
Reward Amount
```

Editor 제작 규칙과 Runtime 실행 규칙을 분리하지 않습니다.

---

## Runtime Error Codes

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

Message 문자열보다 `Error.Code`를 기준으로 실패 원인을 구분할 수 있습니다.

---

## Verification

최종 자동화 테스트:

```text
Runtime: 81 Passed
Editor:  19 Passed
Total:  100 Passed
```

주요 검증 범위:

```text
Quest Definition
Objective Definition
Reward Definition
QuestCatalog

Quest Validation
Prerequisite
Cycle Detection

QuestManager Creation
Quest Start
Objective Progress
Automatic Completion
Runtime Events

Repeatable Quest
Completion Count

Capture
Restore
Invalid Snapshot
Atomic Restore

Quest Editor
Objective Editing
Reward Editing
Prerequisite Editing
Relationship Query
Undo / Redo
Editor Validation Integration
```

---

## 책임 범위

Framework가 담당:

```text
Quest Definition
Objective Definition
Reward Metadata
Prerequisite

Quest Runtime State
Objective Progress
Quest Completion
Completion Count
Repeatable Reset

Runtime Events

Capture
Restore

Validation
Cycle Validation

Quest Editor
Search
Relationship Navigation
Undo / Redo
```

Framework가 담당하지 않음:

```text
게임 이벤트 감지
Quest 자동 시작
Quest 자동 수락
Quest 포기
Quest 실패 상태
Quest 시간 제한

실제 Reward 지급
Inventory 직접 연동
Currency 직접 연동

NPC Quest Provider
Dialogue System

Quest UI
Quest Journal
Quest Marker

Daily / Weekly Reset Scheduler

Save File
Cloud Save
Network Quest

Runtime Singleton
Service Locator
```

---

## 설계 방향

```text
Definition과 Runtime State 분리

게임이 진행 조건을 판단
Framework가 Quest 상태를 관리

게임 이벤트는 외부에서 전달

모든 Objective 완료 시 자동 Quest 완료

Prerequisite와 완료 이력 분리

Reward Metadata와 실제 Reward 지급 분리

Snapshot과 실제 Save 처리 분리

Editor와 Runtime에서 동일 Validator 사용

Result 기반 명시적 실패 처리

최소 패키지 의존성

게임별 정책 비강제

독립 설치 가능한 UPM 구조
```

---

## 문서

패키지 기본 사용법:

```text
com.chodogyu.quest/README.md
```

상세 설계 및 사용 규칙:

```text
com.chodogyu.quest/Documentation~/index.md
```

버전 변경 사항:

```text
com.chodogyu.quest/CHANGELOG.md
```

---

## Version

Current Release:

```text
v1.0.0
```

Package:

```text
com.chodogyu.quest
```

Runtime Assembly:

```text
CDG.Quest
```

Editor Assembly:

```text
CDG.Quest.Editor
```
