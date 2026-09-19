using System;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// 하나의 Objective Runtime 진행 상태를 외부에 저장하거나 복원할 수 있도록 표현한 Snapshot입니다.
    /// </summary>
    [Serializable]
    public sealed class ObjectiveStateSnapshot
    {
        [SerializeField]
        private string objectiveId = string.Empty;

        [SerializeField]
        private int currentProgress;

        /// <summary>
        /// Snapshot이 나타내는 Objective의 ID입니다.
        /// </summary>
        public string ObjectiveId => objectiveId;

        /// <summary>
        /// Snapshot 생성 시점의 Objective 현재 진행도입니다.
        /// </summary>
        public int CurrentProgress => currentProgress;

        /// <summary>
        /// 지정한 Objective ID와 현재 진행도로 Snapshot을 생성합니다.
        /// 값의 유효성은 실제 Quest Definition과 함께 Restore할 때 검증됩니다.
        /// </summary>
        public ObjectiveStateSnapshot(string objectiveId, int currentProgress)
        {
            this.objectiveId = objectiveId;
            this.currentProgress = currentProgress;
        }
    }
}