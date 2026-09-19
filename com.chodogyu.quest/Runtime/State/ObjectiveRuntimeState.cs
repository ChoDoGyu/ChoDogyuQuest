using System;

namespace CDG.Quest
{
    /// <summary>
    /// 하나의 Objective에 대한 Runtime 진행 상태입니다.
    /// ObjectiveDefinition은 변경하지 않고 현재 진행량만 별도로 관리합니다.
    /// </summary>
    internal sealed class ObjectiveRuntimeState
    {
        internal string Id { get; }
        internal int TargetProgress { get; }
        internal int CurrentProgress { get; private set; }
        internal bool IsCompleted => CurrentProgress >= TargetProgress;

        internal ObjectiveRuntimeState(ObjectiveDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.TargetProgress <= 0)
            {
                throw new ArgumentException("Objective TargetProgress는 1 이상이어야 합니다.", nameof(definition));
            }

            Id = definition.Id;
            TargetProgress = definition.TargetProgress;
        }

        internal void Advance(int amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Objective 진행 증가량은 1 이상이어야 합니다.");
            }

            int remainingProgress = TargetProgress - CurrentProgress;
            CurrentProgress = amount >= remainingProgress ? TargetProgress : CurrentProgress + amount;
        }

        internal void SetProgress(int progress)
        {
            if (progress < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(progress), "Objective 진행도는 0 이상이어야 합니다.");
            }

            CurrentProgress = Math.Min(progress, TargetProgress);
        }

        internal void Reset()
        {
            CurrentProgress = 0;
        }
    }
}