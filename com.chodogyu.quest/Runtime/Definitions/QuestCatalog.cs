using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// 게임에서 사용할 Quest 정의들을 하나의 Unity Asset으로 관리하는 Catalog입니다.
    /// Runtime에서는 정의 데이터를 읽기 전용으로 제공하며 Quest의 플레이 상태는 저장하지 않습니다.
    /// </summary>
    [CreateAssetMenu(fileName = "QuestCatalog", menuName = "CDG/Quest/Quest Catalog")]
    public sealed class QuestCatalog : ScriptableObject
    {
        [SerializeField]
        private List<QuestDefinition> quests = new List<QuestDefinition>();

        private List<QuestDefinition> readOnlyQuestsSource;
        private ReadOnlyCollection<QuestDefinition> readOnlyQuests;

        /// <summary>
        /// Catalog에 포함된 Quest 정의 수입니다.
        /// </summary>
        public int Count => quests.Count;

        /// <summary>
        /// Catalog에 포함된 Quest 정의를 직렬화된 순서대로 제공합니다.
        /// 반환된 컬렉션을 통해 Quest를 추가하거나 제거할 수 없습니다.
        /// </summary>
        public IReadOnlyList<QuestDefinition> Quests
        {
            get
            {
                if (!ReferenceEquals(readOnlyQuestsSource, quests))
                {
                    readOnlyQuestsSource = quests;
                    readOnlyQuests = quests.AsReadOnly();
                }

                return readOnlyQuests;
            }
        }

        internal void ReplaceQuests(IEnumerable<QuestDefinition> source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            quests = new List<QuestDefinition>(source);
            readOnlyQuestsSource = null;
            readOnlyQuests = null;
        }
    }
}