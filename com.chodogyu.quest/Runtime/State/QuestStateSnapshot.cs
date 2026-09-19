using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// 하나의 Quest Runtime 상태와 그 Objective 진행 상태를 저장하기 위한 Snapshot입니다.
    /// </summary>
    [Serializable]
    public sealed class QuestStateSnapshot
    {
        [SerializeField]
        private string questId = string.Empty;

        [SerializeField]
        private QuestStatus status;

        [SerializeField]
        private int completionCount;

        [SerializeField]
        private ObjectiveStateSnapshot[] objectives = Array.Empty<ObjectiveStateSnapshot>();

        [NonSerialized]
        private IReadOnlyList<ObjectiveStateSnapshot> readOnlyObjectives;

        /// <summary>
        /// Snapshot이 나타내는 Quest ID입니다.
        /// </summary>
        public string QuestId => questId;

        /// <summary>
        /// Snapshot 생성 시점의 Quest 상태입니다.
        /// </summary>
        public QuestStatus Status => status;

        /// <summary>
        /// Snapshot 생성 시점까지 Quest가 완료된 총 횟수입니다.
        /// </summary>
        public int CompletionCount => completionCount;

        /// <summary>
        /// Quest에 포함된 Objective의 저장된 진행 상태입니다.
        /// </summary>
        public IReadOnlyList<ObjectiveStateSnapshot> Objectives
        {
            get
            {
                if (objectives == null)
                {
                    return Array.Empty<ObjectiveStateSnapshot>();
                }

                if (readOnlyObjectives == null)
                {
                    readOnlyObjectives = Array.AsReadOnly(objectives);
                }

                return readOnlyObjectives;
            }
        }

        /// <summary>
        /// 지정한 Quest 상태와 Objective 상태들로 Snapshot을 생성합니다.
        /// 실제 Definition과의 일치 여부는 Restore 과정에서 검증됩니다.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="objectives"/>가 null인 경우 발생합니다.</exception>
        public QuestStateSnapshot(string questId, QuestStatus status, int completionCount, IEnumerable<ObjectiveStateSnapshot> objectives)
        {
            if (objectives == null)
            {
                throw new ArgumentNullException(nameof(objectives));
            }

            this.questId = questId;
            this.status = status;
            this.completionCount = completionCount;
            this.objectives = new List<ObjectiveStateSnapshot>(objectives).ToArray();
        }
    }
}