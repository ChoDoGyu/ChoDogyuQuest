using System;
using System.Collections.Generic;

namespace CDG.Quest
{
    /// <summary>
    /// 하나의 Quest에 대한 Runtime 상태와 Objective 진행 상태를 관리합니다.
    /// Definition 데이터는 수정하지 않으며 상태 전이에 필요한 최소 정보만 보관합니다.
    /// </summary>
    internal sealed class QuestRuntimeState
    {
        private readonly Dictionary<string, ObjectiveRuntimeState> objectiveStates;

        internal QuestDefinition Definition { get; }
        internal string QuestId => Definition.Id;
        internal QuestStatus Status { get; private set; }
        internal int CompletionCount { get; private set; }
        internal IEnumerable<ObjectiveRuntimeState> ObjectiveStates => objectiveStates.Values;

        internal bool AreAllObjectivesCompleted
        {
            get
            {
                if (objectiveStates.Count == 0)
                {
                    return false;
                }

                foreach (ObjectiveRuntimeState objectiveState in objectiveStates.Values)
                {
                    if (!objectiveState.IsCompleted)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal QuestRuntimeState(QuestDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            objectiveStates = new Dictionary<string, ObjectiveRuntimeState>(StringComparer.Ordinal);

            for (int i = 0; i < definition.Objectives.Count; i++)
            {
                ObjectiveDefinition objective = definition.Objectives[i];

                if (objective == null)
                {
                    throw new ArgumentException("Quest에는 null Objective를 포함할 수 없습니다.", nameof(definition));
                }

                objectiveStates.Add(objective.Id, new ObjectiveRuntimeState(objective));
            }

            Status = QuestStatus.Inactive;
        }

        internal bool TryGetObjectiveState(string objectiveId, out ObjectiveRuntimeState objectiveState)
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
            {
                objectiveState = null;
                return false;
            }

            return objectiveStates.TryGetValue(objectiveId, out objectiveState);
        }

        internal void Start()
        {
            if (Status != QuestStatus.Inactive)
            {
                throw new InvalidOperationException("Inactive 상태의 Quest만 시작할 수 있습니다.");
            }

            Status = QuestStatus.Active;
        }

        internal void Complete()
        {
            if (Status != QuestStatus.Active)
            {
                throw new InvalidOperationException("Active 상태의 Quest만 완료할 수 있습니다.");
            }

            if (!AreAllObjectivesCompleted)
            {
                throw new InvalidOperationException("모든 Objective가 완료되기 전에는 Quest를 완료할 수 없습니다.");
            }

            Status = QuestStatus.Completed;
            CompletionCount++;
        }

        internal void Reset()
        {
            if (Status != QuestStatus.Completed)
            {
                throw new InvalidOperationException("Completed 상태의 Quest만 Reset할 수 있습니다.");
            }

            if (!Definition.IsRepeatable)
            {
                throw new InvalidOperationException("반복할 수 없는 Quest는 Reset할 수 없습니다.");
            }

            foreach (ObjectiveRuntimeState objectiveState in objectiveStates.Values)
            {
                objectiveState.Reset();
            }

            Status = QuestStatus.Inactive;
        }

        internal void Restore(QuestStatus status, int completionCount, IReadOnlyDictionary<string, int> objectiveProgressById)
        {
            if (!Enum.IsDefined(typeof(QuestStatus), status))
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            if (completionCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(completionCount));
            }

            if (objectiveProgressById == null)
            {
                throw new ArgumentNullException(nameof(objectiveProgressById));
            }

            if (objectiveProgressById.Count != objectiveStates.Count)
            {
                throw new ArgumentException("복원할 Objective 상태 수가 Runtime 상태와 일치하지 않습니다.", nameof(objectiveProgressById));
            }

            foreach (KeyValuePair<string, ObjectiveRuntimeState> pair in objectiveStates)
            {
                if (!objectiveProgressById.TryGetValue(pair.Key, out int progress))
                {
                    throw new ArgumentException($"복원할 Objective 상태를 찾을 수 없습니다: '{pair.Key}'", nameof(objectiveProgressById));
                }

                if (progress < 0 || progress > pair.Value.TargetProgress)
                {
                    throw new ArgumentOutOfRangeException(nameof(objectiveProgressById), $"Objective 진행도가 허용 범위를 벗어났습니다: '{pair.Key}'");
                }
            }

            foreach (KeyValuePair<string, ObjectiveRuntimeState> pair in objectiveStates)
            {
                pair.Value.SetProgress(objectiveProgressById[pair.Key]);
            }

            Status = status;
            CompletionCount = completionCount;
        }
    }
}