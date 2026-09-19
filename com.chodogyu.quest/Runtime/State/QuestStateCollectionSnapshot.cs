using System;
using System.Collections.Generic;
using UnityEngine;

namespace CDG.Quest
{
    /// <summary>
    /// QuestManager가 관리하는 전체 Quest Runtime 상태를 저장하기 위한 Snapshot입니다.
    /// 파일 저장이나 직렬화 방식은 외부 시스템이 결정합니다.
    /// </summary>
    [Serializable]
    public sealed class QuestStateCollectionSnapshot
    {
        [SerializeField]
        private QuestStateSnapshot[] quests = Array.Empty<QuestStateSnapshot>();

        [NonSerialized]
        private IReadOnlyList<QuestStateSnapshot> readOnlyQuests;

        /// <summary>
        /// Snapshot에 포함된 전체 Quest 상태를 Capture 당시 순서대로 제공합니다.
        /// </summary>
        public IReadOnlyList<QuestStateSnapshot> Quests
        {
            get
            {
                if (quests == null)
                {
                    return Array.Empty<QuestStateSnapshot>();
                }

                if (readOnlyQuests == null)
                {
                    readOnlyQuests = Array.AsReadOnly(quests);
                }

                return readOnlyQuests;
            }
        }

        /// <summary>
        /// 지정한 Quest 상태들로 전체 Quest Snapshot을 생성합니다.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="quests"/>가 null인 경우 발생합니다.</exception>
        public QuestStateCollectionSnapshot(IEnumerable<QuestStateSnapshot> quests)
        {
            if (quests == null)
            {
                throw new ArgumentNullException(nameof(quests));
            }

            this.quests = new List<QuestStateSnapshot>(quests).ToArray();
        }
    }
}