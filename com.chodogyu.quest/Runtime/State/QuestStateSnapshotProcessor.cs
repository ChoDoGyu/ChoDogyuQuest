using System;
using System.Collections.Generic;
using CDG.Core.Results;

namespace CDG.Quest
{
    /// <summary>
    /// QuestManager의 Runtime 상태를 Snapshot으로 Capture하거나 저장된 Snapshot을 검증하고 복원합니다.
    /// 파일 저장과 직렬화는 담당하지 않으며 현재 Quest Definition과의 상태 일관성만 관리합니다.
    /// </summary>
    internal static class QuestStateSnapshotProcessor
    {
        internal static QuestStateCollectionSnapshot Capture(QuestDefinition[] orderedDefinitions, IReadOnlyDictionary<string, QuestRuntimeState> statesById)
        {
            if (orderedDefinitions == null)
            {
                throw new ArgumentNullException(nameof(orderedDefinitions));
            }

            if (statesById == null)
            {
                throw new ArgumentNullException(nameof(statesById));
            }

            QuestStateSnapshot[] questSnapshots = new QuestStateSnapshot[orderedDefinitions.Length];

            for (int questIndex = 0; questIndex < orderedDefinitions.Length; questIndex++)
            {
                QuestDefinition definition = orderedDefinitions[questIndex];

                if (!statesById.TryGetValue(definition.Id, out QuestRuntimeState state))
                {
                    throw new InvalidOperationException($"Runtime Quest 상태를 찾을 수 없습니다: '{definition.Id}'");
                }

                ObjectiveStateSnapshot[] objectiveSnapshots = new ObjectiveStateSnapshot[definition.Objectives.Count];

                for (int objectiveIndex = 0; objectiveIndex < definition.Objectives.Count; objectiveIndex++)
                {
                    ObjectiveDefinition objectiveDefinition = definition.Objectives[objectiveIndex];

                    if (!state.TryGetObjectiveState(objectiveDefinition.Id, out ObjectiveRuntimeState objectiveState))
                    {
                        throw new InvalidOperationException($"Runtime Objective 상태를 찾을 수 없습니다: '{objectiveDefinition.Id}'");
                    }

                    objectiveSnapshots[objectiveIndex] = new ObjectiveStateSnapshot(objectiveDefinition.Id, objectiveState.CurrentProgress);
                }

                questSnapshots[questIndex] = new QuestStateSnapshot(definition.Id, state.Status, state.CompletionCount, objectiveSnapshots);
            }

            return new QuestStateCollectionSnapshot(questSnapshots);
        }

        internal static Result Restore(QuestStateCollectionSnapshot snapshot, QuestDefinition[] orderedDefinitions, IReadOnlyDictionary<string, QuestDefinition> definitionsById, IReadOnlyDictionary<string, QuestRuntimeState> statesById)
        {
            if (snapshot == null)
            {
                return Result.Failure(new ResultError(QuestErrorCodes.StateSnapshotRequired, "복원할 Quest 상태 Snapshot이 필요합니다."));
            }

            if (orderedDefinitions == null)
            {
                throw new ArgumentNullException(nameof(orderedDefinitions));
            }

            if (definitionsById == null)
            {
                throw new ArgumentNullException(nameof(definitionsById));
            }

            if (statesById == null)
            {
                throw new ArgumentNullException(nameof(statesById));
            }

            Result<RestorePlan> planResult = CreateRestorePlan(snapshot, orderedDefinitions, definitionsById);

            if (planResult.IsFailure)
            {
                return Result.Failure(planResult.Error);
            }

            RestorePlan plan = planResult.Value;

            for (int i = 0; i < orderedDefinitions.Length; i++)
            {
                QuestDefinition definition = orderedDefinitions[i];
                QuestRestoreData restoreData = plan.QuestDataById[definition.Id];

                if (!statesById.TryGetValue(definition.Id, out QuestRuntimeState state))
                {
                    throw new InvalidOperationException($"Runtime Quest 상태를 찾을 수 없습니다: '{definition.Id}'");
                }

                state.Restore(restoreData.Status, restoreData.CompletionCount, restoreData.ObjectiveProgressById);
            }

            return Result.Success();
        }

        private static Result<RestorePlan> CreateRestorePlan(QuestStateCollectionSnapshot snapshot, QuestDefinition[] orderedDefinitions, IReadOnlyDictionary<string, QuestDefinition> definitionsById)
        {
            if (snapshot.Quests.Count != orderedDefinitions.Length)
            {
                return InvalidPlan($"Snapshot의 Quest 수가 현재 Catalog와 일치하지 않습니다. 예상: {orderedDefinitions.Length}, 실제: {snapshot.Quests.Count}");
            }

            Dictionary<string, QuestRestoreData> restoreDataById = new Dictionary<string, QuestRestoreData>(StringComparer.Ordinal);

            for (int questIndex = 0; questIndex < snapshot.Quests.Count; questIndex++)
            {
                QuestStateSnapshot questSnapshot = snapshot.Quests[questIndex];

                Result<QuestRestoreData> questResult = CreateQuestRestoreData(questSnapshot, questIndex, definitionsById, restoreDataById);

                if (questResult.IsFailure)
                {
                    return Result<RestorePlan>.Failure(questResult.Error);
                }

                restoreDataById.Add(questSnapshot.QuestId, questResult.Value);
            }

            for (int i = 0; i < orderedDefinitions.Length; i++)
            {
                QuestDefinition definition = orderedDefinitions[i];

                if (!restoreDataById.ContainsKey(definition.Id))
                {
                    return InvalidPlan($"Snapshot에 필요한 Quest 상태가 없습니다: '{definition.Id}'");
                }
            }

            Result prerequisiteResult = ValidatePrerequisiteHistory(orderedDefinitions, restoreDataById);

            if (prerequisiteResult.IsFailure)
            {
                return Result<RestorePlan>.Failure(prerequisiteResult.Error);
            }

            return Result<RestorePlan>.Success(new RestorePlan(restoreDataById));
        }

        private static Result<QuestRestoreData> CreateQuestRestoreData(QuestStateSnapshot questSnapshot, int questIndex, IReadOnlyDictionary<string, QuestDefinition> definitionsById, IReadOnlyDictionary<string, QuestRestoreData> registeredRestoreData)
        {
            if (questSnapshot == null)
            {
                return InvalidQuestData($"Snapshot에 null Quest 상태가 포함되어 있습니다. 인덱스: {questIndex}");
            }

            if (string.IsNullOrWhiteSpace(questSnapshot.QuestId))
            {
                return InvalidQuestData($"Snapshot의 Quest ID가 비어 있습니다. 인덱스: {questIndex}");
            }

            if (!definitionsById.TryGetValue(questSnapshot.QuestId, out QuestDefinition definition))
            {
                return InvalidQuestData($"현재 Catalog에 존재하지 않는 Quest 상태가 포함되어 있습니다: '{questSnapshot.QuestId}'");
            }

            if (registeredRestoreData.ContainsKey(questSnapshot.QuestId))
            {
                return InvalidQuestData($"Snapshot에 동일한 Quest가 중복되어 있습니다: '{questSnapshot.QuestId}'");
            }

            if (!Enum.IsDefined(typeof(QuestStatus), questSnapshot.Status))
            {
                return InvalidQuestData($"유효하지 않은 Quest 상태입니다: '{questSnapshot.QuestId}'");
            }

            if (questSnapshot.CompletionCount < 0)
            {
                return InvalidQuestData($"Quest CompletionCount는 0 이상이어야 합니다: '{questSnapshot.QuestId}'");
            }

            if (!definition.IsRepeatable && questSnapshot.CompletionCount > 1)
            {
                return InvalidQuestData($"반복할 수 없는 Quest의 CompletionCount는 1을 초과할 수 없습니다: '{questSnapshot.QuestId}'");
            }

            Result<Dictionary<string, int>> objectiveResult = CreateObjectiveProgressMap(questSnapshot, definition);

            if (objectiveResult.IsFailure)
            {
                return Result<QuestRestoreData>.Failure(objectiveResult.Error);
            }

            Dictionary<string, int> objectiveProgressById = objectiveResult.Value;

            Result statusResult = ValidateQuestStateConsistency(questSnapshot, definition, objectiveProgressById);

            if (statusResult.IsFailure)
            {
                return Result<QuestRestoreData>.Failure(statusResult.Error);
            }

            return Result<QuestRestoreData>.Success(new QuestRestoreData(questSnapshot.Status, questSnapshot.CompletionCount, objectiveProgressById));
        }

        private static Result<Dictionary<string, int>> CreateObjectiveProgressMap(QuestStateSnapshot questSnapshot, QuestDefinition definition)
        {
            if (questSnapshot.Objectives.Count != definition.Objectives.Count)
            {
                return InvalidObjectiveData($"Quest의 Objective 상태 수가 Definition과 일치하지 않습니다: '{questSnapshot.QuestId}'");
            }

            Dictionary<string, int> objectiveProgressById = new Dictionary<string, int>(StringComparer.Ordinal);

            for (int objectiveIndex = 0; objectiveIndex < questSnapshot.Objectives.Count; objectiveIndex++)
            {
                ObjectiveStateSnapshot objectiveSnapshot = questSnapshot.Objectives[objectiveIndex];

                if (objectiveSnapshot == null)
                {
                    return InvalidObjectiveData($"Snapshot에 null Objective 상태가 포함되어 있습니다: '{questSnapshot.QuestId}'");
                }

                if (string.IsNullOrWhiteSpace(objectiveSnapshot.ObjectiveId))
                {
                    return InvalidObjectiveData($"Snapshot의 Objective ID가 비어 있습니다: '{questSnapshot.QuestId}'");
                }

                ObjectiveDefinition objectiveDefinition = FindObjectiveDefinition(definition, objectiveSnapshot.ObjectiveId);

                if (objectiveDefinition == null)
                {
                    return InvalidObjectiveData($"현재 Quest Definition에 존재하지 않는 Objective 상태입니다: '{questSnapshot.QuestId}/{objectiveSnapshot.ObjectiveId}'");
                }

                if (!objectiveProgressById.TryAdd(objectiveSnapshot.ObjectiveId, objectiveSnapshot.CurrentProgress))
                {
                    return InvalidObjectiveData($"Snapshot에 동일한 Objective가 중복되어 있습니다: '{questSnapshot.QuestId}/{objectiveSnapshot.ObjectiveId}'");
                }

                if (objectiveSnapshot.CurrentProgress < 0 || objectiveSnapshot.CurrentProgress > objectiveDefinition.TargetProgress)
                {
                    return InvalidObjectiveData($"Objective 진행도가 허용 범위를 벗어났습니다: '{questSnapshot.QuestId}/{objectiveSnapshot.ObjectiveId}'");
                }
            }

            for (int objectiveIndex = 0; objectiveIndex < definition.Objectives.Count; objectiveIndex++)
            {
                ObjectiveDefinition objectiveDefinition = definition.Objectives[objectiveIndex];

                if (!objectiveProgressById.ContainsKey(objectiveDefinition.Id))
                {
                    return InvalidObjectiveData($"Snapshot에 필요한 Objective 상태가 없습니다: '{questSnapshot.QuestId}/{objectiveDefinition.Id}'");
                }
            }

            return Result<Dictionary<string, int>>.Success(objectiveProgressById);
        }

        private static Result ValidateQuestStateConsistency(QuestStateSnapshot questSnapshot, QuestDefinition definition, IReadOnlyDictionary<string, int> objectiveProgressById)
        {
            bool allObjectivesCompleted = definition.Objectives.Count > 0;
            bool allObjectivesEmpty = true;

            for (int i = 0; i < definition.Objectives.Count; i++)
            {
                ObjectiveDefinition objective = definition.Objectives[i];
                int currentProgress = objectiveProgressById[objective.Id];

                if (currentProgress < objective.TargetProgress)
                {
                    allObjectivesCompleted = false;
                }

                if (currentProgress != 0)
                {
                    allObjectivesEmpty = false;
                }
            }

            if (questSnapshot.Status == QuestStatus.Inactive)
            {
                if (!allObjectivesEmpty)
                {
                    return InvalidSnapshot($"Inactive Quest의 Objective 진행도는 모두 0이어야 합니다: '{questSnapshot.QuestId}'");
                }

                if (!definition.IsRepeatable && questSnapshot.CompletionCount > 0)
                {
                    return InvalidSnapshot($"반복할 수 없는 Inactive Quest는 완료 이력을 가질 수 없습니다: '{questSnapshot.QuestId}'");
                }
            }
            else if (questSnapshot.Status == QuestStatus.Active)
            {
                if (allObjectivesCompleted)
                {
                    return InvalidSnapshot($"모든 Objective가 완료된 Quest는 Active 상태일 수 없습니다: '{questSnapshot.QuestId}'");
                }

                if (!definition.IsRepeatable && questSnapshot.CompletionCount > 0)
                {
                    return InvalidSnapshot($"반복할 수 없는 Active Quest는 이전 완료 이력을 가질 수 없습니다: '{questSnapshot.QuestId}'");
                }
            }
            else if (questSnapshot.Status == QuestStatus.Completed)
            {
                if (!allObjectivesCompleted)
                {
                    return InvalidSnapshot($"Completed Quest는 모든 Objective가 완료되어 있어야 합니다: '{questSnapshot.QuestId}'");
                }

                if (questSnapshot.CompletionCount <= 0)
                {
                    return InvalidSnapshot($"Completed Quest의 CompletionCount는 1 이상이어야 합니다: '{questSnapshot.QuestId}'");
                }
            }

            return Result.Success();
        }

        private static Result ValidatePrerequisiteHistory(QuestDefinition[] orderedDefinitions, IReadOnlyDictionary<string, QuestRestoreData> restoreDataById)
        {
            for (int i = 0; i < orderedDefinitions.Length; i++)
            {
                QuestDefinition definition = orderedDefinitions[i];
                QuestRestoreData questData = restoreDataById[definition.Id];

                if (questData.Status == QuestStatus.Inactive)
                {
                    continue;
                }

                for (int prerequisiteIndex = 0; prerequisiteIndex < definition.PrerequisiteQuestIds.Count; prerequisiteIndex++)
                {
                    string prerequisiteId = definition.PrerequisiteQuestIds[prerequisiteIndex];

                    if (!restoreDataById.TryGetValue(prerequisiteId, out QuestRestoreData prerequisiteData) || prerequisiteData.CompletionCount <= 0)
                    {
                        return InvalidSnapshot($"Active 또는 Completed Quest의 선행 Quest 완료 이력이 없습니다: '{definition.Id}' → '{prerequisiteId}'");
                    }
                }
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

            return null;
        }

        private static Result InvalidSnapshot(string message)
        {
            return Result.Failure(new ResultError(QuestErrorCodes.InvalidStateSnapshot, message));
        }

        private static Result<RestorePlan> InvalidPlan(string message)
        {
            return Result<RestorePlan>.Failure(new ResultError(QuestErrorCodes.InvalidStateSnapshot, message));
        }

        private static Result<QuestRestoreData> InvalidQuestData(string message)
        {
            return Result<QuestRestoreData>.Failure(new ResultError(QuestErrorCodes.InvalidStateSnapshot, message));
        }

        private static Result<Dictionary<string, int>> InvalidObjectiveData(string message)
        {
            return Result<Dictionary<string, int>>.Failure(new ResultError(QuestErrorCodes.InvalidStateSnapshot, message));
        }

        private sealed class RestorePlan
        {
            internal IReadOnlyDictionary<string, QuestRestoreData> QuestDataById { get; }

            internal RestorePlan(Dictionary<string, QuestRestoreData> questDataById)
            {
                QuestDataById = questDataById;
            }
        }

        private sealed class QuestRestoreData
        {
            internal QuestStatus Status { get; }
            internal int CompletionCount { get; }
            internal IReadOnlyDictionary<string, int> ObjectiveProgressById { get; }

            internal QuestRestoreData(QuestStatus status, int completionCount, Dictionary<string, int> objectiveProgressById)
            {
                Status = status;
                CompletionCount = completionCount;
                ObjectiveProgressById = objectiveProgressById;
            }
        }
    }
}