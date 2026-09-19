using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// 하나의 Quest를 구성하는 변경되지 않는 정의 데이터입니다.
    /// 플레이 중 변화하는 상태와 진행도는 이 타입에 저장하지 않고 별도의 Runtime State에서 관리합니다.
    /// </summary>
    [Serializable]
    public sealed class QuestDefinition
    {
        [SerializeField]
        private string id = string.Empty;

        [SerializeField]
        private string title = string.Empty;

        [SerializeField, TextArea]
        private string description = string.Empty;

        [SerializeField]
        private List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>();

        [SerializeField]
        private List<string> prerequisiteQuestIds = new List<string>();

        [SerializeField]
        private List<RewardDefinition> rewards = new List<RewardDefinition>();

        [SerializeField]
        private bool isRepeatable;

        private List<ObjectiveDefinition> readOnlyObjectivesSource;
        private ReadOnlyCollection<ObjectiveDefinition> readOnlyObjectives;
        private List<string> readOnlyPrerequisiteQuestIdsSource;
        private ReadOnlyCollection<string> readOnlyPrerequisiteQuestIds;
        private List<RewardDefinition> readOnlyRewardsSource;
        private ReadOnlyCollection<RewardDefinition> readOnlyRewards;

        /// <summary>
        /// Quest를 고유하게 식별하는 ID입니다.
        /// 하나의 Catalog 안에서는 중복되지 않아야 합니다.
        /// </summary>
        public string Id => id;

        /// <summary>
        /// Quest를 표시할 때 사용할 제목입니다.
        /// </summary>
        public string Title => title;

        /// <summary>
        /// Quest에 대한 설명입니다.
        /// </summary>
        public string Description => description;

        /// <summary>
        /// Quest에 포함된 Objective를 정의된 순서대로 제공합니다.
        /// 반환된 컬렉션을 통해 항목을 추가하거나 제거할 수 없습니다.
        /// </summary>
        public IReadOnlyList<ObjectiveDefinition> Objectives
        {
            get
            {
                if (!ReferenceEquals(readOnlyObjectivesSource, objectives))
                {
                    readOnlyObjectivesSource = objectives;
                    readOnlyObjectives = objectives.AsReadOnly();
                }

                return readOnlyObjectives;
            }
        }

        /// <summary>
        /// 이 Quest를 시작하기 전에 완료되어야 하는 선행 Quest ID 목록입니다.
        /// v1.0에서는 목록의 모든 Quest가 완료되어야 시작할 수 있습니다.
        /// </summary>
        public IReadOnlyList<string> PrerequisiteQuestIds
        {
            get
            {
                if (!ReferenceEquals(readOnlyPrerequisiteQuestIdsSource, prerequisiteQuestIds))
                {
                    readOnlyPrerequisiteQuestIdsSource = prerequisiteQuestIds;
                    readOnlyPrerequisiteQuestIds = prerequisiteQuestIds.AsReadOnly();
                }

                return readOnlyPrerequisiteQuestIds;
            }
        }

        /// <summary>
        /// Quest 완료 후 게임 코드가 처리할 수 있는 보상 정의 목록입니다.
        /// Framework는 이 정보를 제공할 뿐 실제 보상 지급은 수행하지 않습니다.
        /// </summary>
        public IReadOnlyList<RewardDefinition> Rewards
        {
            get
            {
                if (!ReferenceEquals(readOnlyRewardsSource, rewards))
                {
                    readOnlyRewardsSource = rewards;
                    readOnlyRewards = rewards.AsReadOnly();
                }

                return readOnlyRewards;
            }
        }

        /// <summary>
        /// 완료된 Quest를 Reset하여 다시 시작할 수 있는지를 나타냅니다.
        /// 반복 시점이나 주기는 게임 코드가 결정합니다.
        /// </summary>
        public bool IsRepeatable => isRepeatable;

        internal QuestDefinition()
        {
        }

        internal QuestDefinition(string id, string title, string description, IEnumerable<ObjectiveDefinition> objectives, IEnumerable<string> prerequisiteQuestIds, IEnumerable<RewardDefinition> rewards, bool isRepeatable)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.objectives = objectives != null ? new List<ObjectiveDefinition>(objectives) : new List<ObjectiveDefinition>();
            this.prerequisiteQuestIds = prerequisiteQuestIds != null ? new List<string>(prerequisiteQuestIds) : new List<string>();
            this.rewards = rewards != null ? new List<RewardDefinition>(rewards) : new List<RewardDefinition>();
            this.isRepeatable = isRepeatable;
        }
    }
}