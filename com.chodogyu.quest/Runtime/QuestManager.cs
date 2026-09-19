using System;
using System.Collections.Generic;
using CDG.Core.Results;

namespace CDG.Quest
{
    /// <summary>
    /// Quest Definition을 기반으로 Runtime 상태와 진행 흐름을 관리하는 주요 진입점입니다.
    /// 게임 고유 이벤트를 직접 감지하지 않으며 외부 게임 코드가 Start 및 Objective 진행 API를 호출하는 방식으로 사용합니다.
    /// </summary>
    public sealed class QuestManager
    {
        private readonly QuestDefinition[] orderedDefinitions;
        private readonly Dictionary<string, QuestDefinition> definitionsById;
        private readonly Dictionary<string, QuestRuntimeState> statesById;

        /// <summary>
        /// Quest가 정상적으로 시작된 후 발생합니다.
        /// </summary>
        public event Action<QuestDefinition> QuestStarted;

        /// <summary>
        /// Objective의 현재 진행도가 실제로 변경된 후 발생합니다.
        /// Quest, Objective, 현재 진행도, 목표 진행도 순서로 값을 전달합니다.
        /// </summary>
        public event Action<QuestDefinition, ObjectiveDefinition, int, int> ObjectiveProgressChanged;

        /// <summary>
        /// Objective가 목표 진행도에 처음 도달했을 때 발생합니다.
        /// </summary>
        public event Action<QuestDefinition, ObjectiveDefinition> ObjectiveCompleted;

        /// <summary>
        /// 모든 Objective가 완료되어 Quest가 자동으로 Completed 상태가 된 후 발생합니다.
        /// </summary>
        public event Action<QuestDefinition> QuestCompleted;

        /// <summary>
        /// 반복 가능한 Quest가 Reset되어 다시 Inactive 상태가 된 후 발생합니다.
        /// </summary>
        public event Action<QuestDefinition> QuestReset;

        private QuestManager(QuestDefinition[] orderedDefinitions, Dictionary<string, QuestDefinition> definitionsById, Dictionary<string, QuestRuntimeState> statesById)
        {
            this.orderedDefinitions = orderedDefinitions;
            this.definitionsById = definitionsById;
            this.statesById = statesById;
        }

        /// <summary>
        /// 지정한 QuestCatalog를 검증하고 사용할 수 있는 QuestManager를 생성합니다.
        /// Warning은 생성을 막지 않지만 하나 이상의 Error가 발견되면 실패합니다.
        /// </summary>
        /// <param name="catalog">Runtime에서 사용할 Quest 정의 Catalog입니다.</param>
        /// <returns>생성된 QuestManager 또는 Catalog 오류 정보를 포함하는 결과입니다.</returns>
        public static Result<QuestManager> Create(QuestCatalog catalog)
        {
            if (catalog == null)
            {
                return Result<QuestManager>.Failure(new ResultError(QuestErrorCodes.CatalogRequired, "QuestManager를 생성하려면 QuestCatalog가 필요합니다."));
            }

            IReadOnlyList<QuestValidationIssue> validationIssues = QuestValidator.Validate(catalog);
            int errorCount = 0;

            for (int i = 0; i < validationIssues.Count; i++)
            {
                if (validationIssues[i].Severity == QuestValidationSeverity.Error)
                {
                    errorCount++;
                }
            }

            if (errorCount > 0)
            {
                return Result<QuestManager>.Failure(new ResultError(QuestErrorCodes.ValidationFailed, $"QuestCatalog 검증에 실패했습니다. 발견된 Error 수: {errorCount}"));
            }

            QuestDefinition[] orderedDefinitions = new QuestDefinition[catalog.Quests.Count];
            Dictionary<string, QuestDefinition> definitionsById = new Dictionary<string, QuestDefinition>(StringComparer.Ordinal);
            Dictionary<string, QuestRuntimeState> statesById = new Dictionary<string, QuestRuntimeState>(StringComparer.Ordinal);

            for (int i = 0; i < catalog.Quests.Count; i++)
            {
                QuestDefinition definition = catalog.Quests[i];
                orderedDefinitions[i] = definition;
                definitionsById.Add(definition.Id, definition);
                statesById.Add(definition.Id, new QuestRuntimeState(definition));
            }

            return Result<QuestManager>.Success(new QuestManager(orderedDefinitions, definitionsById, statesById));
        }

        /// <summary>
        /// 지정한 ID의 Quest Definition을 조회합니다.
        /// </summary>
        public Result<QuestDefinition> GetQuestDefinition(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return Result<QuestDefinition>.Failure(new ResultError(QuestErrorCodes.InvalidQuestId, "Quest ID는 비어 있을 수 없습니다."));
            }

            if (!definitionsById.TryGetValue(questId, out QuestDefinition definition))
            {
                return Result<QuestDefinition>.Failure(new ResultError(QuestErrorCodes.QuestNotFound, $"Quest를 찾을 수 없습니다: '{questId}'"));
            }

            return Result<QuestDefinition>.Success(definition);
        }

        /// <summary>
        /// 지정한 Quest의 현재 Runtime 상태를 조회합니다.
        /// </summary>
        public Result<QuestStatus> GetQuestStatus(string questId)
        {
            Result<QuestRuntimeState> stateResult = GetQuestStateInternal(questId);

            if (stateResult.IsFailure)
            {
                return Result<QuestStatus>.Failure(stateResult.Error);
            }

            return Result<QuestStatus>.Success(stateResult.Value.Status);
        }

        /// <summary>
        /// 지정한 Quest가 지금까지 완료된 횟수를 조회합니다.
        /// Reset된 반복 Quest의 과거 완료 기록도 유지됩니다.
        /// </summary>
        public Result<int> GetCompletionCount(string questId)
        {
            Result<QuestRuntimeState> stateResult = GetQuestStateInternal(questId);

            if (stateResult.IsFailure)
            {
                return Result<int>.Failure(stateResult.Error);
            }

            return Result<int>.Success(stateResult.Value.CompletionCount);
        }

        /// <summary>
        /// 지정한 Objective의 현재 진행도를 조회합니다.
        /// </summary>
        public Result<int> GetObjectiveProgress(string questId, string objectiveId)
        {
            Result<QuestRuntimeState> questStateResult = GetQuestStateInternal(questId);

            if (questStateResult.IsFailure)
            {
                return Result<int>.Failure(questStateResult.Error);
            }

            Result<ObjectiveRuntimeState> objectiveStateResult = GetObjectiveStateInternal(questStateResult.Value, objectiveId);

            if (objectiveStateResult.IsFailure)
            {
                return Result<int>.Failure(objectiveStateResult.Error);
            }

            return Result<int>.Success(objectiveStateResult.Value.CurrentProgress);
        }

        /// <summary>
        /// 현재 Active 상태인 Quest Definition들을 Catalog에 정의된 순서대로 반환합니다.
        /// </summary>
        public IReadOnlyList<QuestDefinition> GetActiveQuests()
        {
            List<QuestDefinition> activeQuests = new List<QuestDefinition>();

            for (int i = 0; i < orderedDefinitions.Length; i++)
            {
                QuestDefinition definition = orderedDefinitions[i];

                if (statesById[definition.Id].Status == QuestStatus.Active)
                {
                    activeQuests.Add(definition);
                }
            }

            return Array.AsReadOnly(activeQuests.ToArray());
        }

        /// <summary>
        /// 지정한 Quest가 현재 시작 가능한지 확인합니다.
        /// Quest가 존재하지 않거나 ID가 유효하지 않으면 false를 반환합니다.
        /// </summary>
        public bool CanStartQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId) || !statesById.TryGetValue(questId, out QuestRuntimeState state))
            {
                return false;
            }

            if (state.Status != QuestStatus.Inactive)
            {
                return false;
            }

            return !TryGetIncompletePrerequisite(state.Definition, out _);
        }

        /// <summary>
        /// 지정한 Quest를 시작합니다.
        /// Inactive 상태이며 모든 선행 Quest를 한 번 이상 완료한 경우에만 성공합니다.
        /// </summary>
        public Result StartQuest(string questId)
        {
            Result<QuestRuntimeState> stateResult = GetQuestStateInternal(questId);

            if (stateResult.IsFailure)
            {
                return Result.Failure(stateResult.Error);
            }

            QuestRuntimeState state = stateResult.Value;

            if (state.Status != QuestStatus.Inactive)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.QuestNotInactive, $"Inactive 상태의 Quest만 시작할 수 있습니다: '{questId}'"));
            }

            if (TryGetIncompletePrerequisite(state.Definition, out string prerequisiteId))
            {
                return Result.Failure(new ResultError(QuestErrorCodes.PrerequisiteNotCompleted, $"선행 Quest가 아직 완료되지 않았습니다: '{prerequisiteId}'"));
            }

            state.Start();
            QuestStarted?.Invoke(state.Definition);
            return Result.Success();
        }

        /// <summary>
        /// Active Quest의 지정한 Objective 진행도를 현재 값에서 증가시킵니다.
        /// 목표 진행도를 초과하는 값은 TargetProgress로 제한됩니다.
        /// </summary>
        public Result AdvanceObjective(string questId, string objectiveId, int amount)
        {
            if (amount <= 0)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.InvalidProgress, "Objective 진행 증가량은 1 이상이어야 합니다."));
            }

            Result<QuestRuntimeState> questStateResult = GetQuestStateInternal(questId);

            if (questStateResult.IsFailure)
            {
                return Result.Failure(questStateResult.Error);
            }

            QuestRuntimeState questState = questStateResult.Value;

            if (questState.Status != QuestStatus.Active)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.QuestNotActive, $"Active 상태의 Quest만 진행할 수 있습니다: '{questId}'"));
            }

            Result<ObjectiveRuntimeState> objectiveStateResult = GetObjectiveStateInternal(questState, objectiveId);

            if (objectiveStateResult.IsFailure)
            {
                return Result.Failure(objectiveStateResult.Error);
            }

            ObjectiveRuntimeState objectiveState = objectiveStateResult.Value;

            if (objectiveState.IsCompleted)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.ObjectiveAlreadyCompleted, $"이미 완료된 Objective입니다: '{objectiveId}'"));
            }

            int previousProgress = objectiveState.CurrentProgress;
            objectiveState.Advance(amount);
            return HandleProgressChanged(questState, objectiveState, previousProgress);
        }

        /// <summary>
        /// Active Quest의 지정한 Objective 진행도를 절대값으로 설정합니다.
        /// TargetProgress보다 큰 값은 TargetProgress로 제한됩니다.
        /// </summary>
        public Result SetObjectiveProgress(string questId, string objectiveId, int progress)
        {
            if (progress < 0)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.InvalidProgress, "Objective 진행도는 0 이상이어야 합니다."));
            }

            Result<QuestRuntimeState> questStateResult = GetQuestStateInternal(questId);

            if (questStateResult.IsFailure)
            {
                return Result.Failure(questStateResult.Error);
            }

            QuestRuntimeState questState = questStateResult.Value;

            if (questState.Status != QuestStatus.Active)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.QuestNotActive, $"Active 상태의 Quest만 진행할 수 있습니다: '{questId}'"));
            }

            Result<ObjectiveRuntimeState> objectiveStateResult = GetObjectiveStateInternal(questState, objectiveId);

            if (objectiveStateResult.IsFailure)
            {
                return Result.Failure(objectiveStateResult.Error);
            }

            ObjectiveRuntimeState objectiveState = objectiveStateResult.Value;

            if (objectiveState.IsCompleted)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.ObjectiveAlreadyCompleted, $"이미 완료된 Objective입니다: '{objectiveId}'"));
            }

            int previousProgress = objectiveState.CurrentProgress;
            objectiveState.SetProgress(progress);
            return HandleProgressChanged(questState, objectiveState, previousProgress);
        }

        /// <summary>
        /// 완료된 반복 Quest를 Inactive 상태로 되돌리고 모든 Objective 진행도를 초기화합니다.
        /// CompletionCount는 유지됩니다.
        /// </summary>
        public Result ResetQuest(string questId)
        {
            Result<QuestRuntimeState> stateResult = GetQuestStateInternal(questId);

            if (stateResult.IsFailure)
            {
                return Result.Failure(stateResult.Error);
            }

            QuestRuntimeState state = stateResult.Value;

            if (state.Status != QuestStatus.Completed)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.QuestNotCompleted, $"Completed 상태의 Quest만 Reset할 수 있습니다: '{questId}'"));
            }

            if (!state.Definition.IsRepeatable)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.QuestNotRepeatable, $"반복할 수 없는 Quest는 Reset할 수 없습니다: '{questId}'"));
            }

            state.Reset();
            QuestReset?.Invoke(state.Definition);
            return Result.Success();
        }

        /// <summary>
        /// 현재 관리 중인 모든 Quest와 Objective의 Runtime 상태를 독립된 Snapshot으로 Capture합니다.
        /// 반환된 데이터는 외부 저장 시스템에서 원하는 방식으로 직렬화할 수 있습니다.
        /// </summary>
        public QuestStateCollectionSnapshot CaptureState()
        {
            return QuestStateSnapshotProcessor.Capture(orderedDefinitions, statesById);
        }

        /// <summary>
        /// 이전에 저장한 Quest 상태 Snapshot을 현재 QuestManager에 복원합니다.
        /// Snapshot 전체를 먼저 검증한 후 적용하며 실패한 경우 기존 Runtime 상태는 변경하지 않습니다.
        /// 복원 과정에서는 Quest 및 Objective Runtime 이벤트를 발생시키지 않습니다.
        /// </summary>
        public Result RestoreState(QuestStateCollectionSnapshot snapshot)
        {
            return QuestStateSnapshotProcessor.Restore(snapshot, orderedDefinitions, definitionsById, statesById);
        }

        private Result<QuestRuntimeState> GetQuestStateInternal(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return Result<QuestRuntimeState>.Failure(new ResultError(QuestErrorCodes.InvalidQuestId, "Quest ID는 비어 있을 수 없습니다."));
            }

            if (!statesById.TryGetValue(questId, out QuestRuntimeState state))
            {
                return Result<QuestRuntimeState>.Failure(new ResultError(QuestErrorCodes.QuestNotFound, $"Quest를 찾을 수 없습니다: '{questId}'"));
            }

            return Result<QuestRuntimeState>.Success(state);
        }

        private static Result<ObjectiveRuntimeState> GetObjectiveStateInternal(QuestRuntimeState questState, string objectiveId)
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
            {
                return Result<ObjectiveRuntimeState>.Failure(new ResultError(QuestErrorCodes.InvalidObjectiveId, "Objective ID는 비어 있을 수 없습니다."));
            }

            if (!questState.TryGetObjectiveState(objectiveId, out ObjectiveRuntimeState objectiveState))
            {
                return Result<ObjectiveRuntimeState>.Failure(new ResultError(QuestErrorCodes.ObjectiveNotFound, $"Objective를 찾을 수 없습니다: '{objectiveId}'"));
            }

            return Result<ObjectiveRuntimeState>.Success(objectiveState);
        }

        private bool TryGetIncompletePrerequisite(QuestDefinition definition, out string prerequisiteId)
        {
            for (int i = 0; i < definition.PrerequisiteQuestIds.Count; i++)
            {
                string currentPrerequisiteId = definition.PrerequisiteQuestIds[i];

                if (!statesById.TryGetValue(currentPrerequisiteId, out QuestRuntimeState prerequisiteState) || prerequisiteState.CompletionCount <= 0)
                {
                    prerequisiteId = currentPrerequisiteId;
                    return true;
                }
            }

            prerequisiteId = null;
            return false;
        }

        private Result HandleProgressChanged(QuestRuntimeState questState, ObjectiveRuntimeState objectiveState, int previousProgress)
        {
            if (objectiveState.CurrentProgress == previousProgress)
            {
                return Result.Success();
            }

            ObjectiveDefinition objectiveDefinition = FindObjectiveDefinition(questState.Definition, objectiveState.Id);
            ObjectiveProgressChanged?.Invoke(questState.Definition, objectiveDefinition, objectiveState.CurrentProgress, objectiveState.TargetProgress);

            if (objectiveState.IsCompleted)
            {
                ObjectiveCompleted?.Invoke(questState.Definition, objectiveDefinition);
            }

            if (questState.AreAllObjectivesCompleted)
            {
                questState.Complete();
                QuestCompleted?.Invoke(questState.Definition);
            }

            return Result.Success();
        }

        private static ObjectiveDefinition FindObjectiveDefinition(QuestDefinition questDefinition, string objectiveId)
        {
            for (int i = 0; i < questDefinition.Objectives.Count; i++)
            {
                ObjectiveDefinition objective = questDefinition.Objectives[i];

                if (string.Equals(objective.Id, objectiveId, StringComparison.Ordinal))
                {
                    return objective;
                }
            }

            throw new InvalidOperationException($"Runtime Objective와 일치하는 Definition을 찾을 수 없습니다: '{objectiveId}'");
        }
    }
}