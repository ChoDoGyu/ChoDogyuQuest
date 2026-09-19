using System;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// Quest를 구성하는 하나의 목표 정의입니다.
    /// 특정 게임 행동의 종류를 직접 알지 않으며, ID와 목표 진행량을 통해 범용적인 목표를 표현합니다.
    /// </summary>
    [Serializable]
    public sealed class ObjectiveDefinition
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string title = string.Empty;

        [SerializeField, TextArea]
        private string description = string.Empty;

        [SerializeField]
        private int targetProgress = 1;

        /// <summary>
        /// Quest 내부에서 Objective를 식별하는 ID입니다.
        /// 동일한 Quest 안에서는 중복되지 않아야 합니다.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Objective를 표시할 때 사용할 제목입니다.
        /// </summary>
        public string Title => title;

        /// <summary>
        /// Objective에 대한 설명입니다.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Objective가 완료되기 위해 필요한 목표 진행량입니다.
        /// 유효한 정의에서는 1 이상의 값을 사용합니다.
        /// </summary>
        public int TargetProgress => targetProgress;

        internal ObjectiveDefinition()
        {
        }

        internal ObjectiveDefinition(string id, string title, string description, int targetProgress)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.targetProgress = targetProgress;
        }
    }
}