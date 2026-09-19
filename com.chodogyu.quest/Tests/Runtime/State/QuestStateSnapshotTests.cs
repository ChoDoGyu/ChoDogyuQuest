using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestStateSnapshotTests
    {
        [Test]
        public void Snapshot_WhenCreated_CopiesSourceCollections()
        {
            List<ObjectiveStateSnapshot> objectives = new List<ObjectiveStateSnapshot>
            {
                new ObjectiveStateSnapshot("objective_001", 2)
            };

            QuestStateSnapshot quest = new QuestStateSnapshot("quest_001", QuestStatus.Active, 0, objectives);
            objectives.Clear();

            List<QuestStateSnapshot> quests = new List<QuestStateSnapshot>
            {
                quest
            };

            QuestStateCollectionSnapshot collection = new QuestStateCollectionSnapshot(quests);
            quests.Clear();

            Assert.AreEqual(1, quest.Objectives.Count);
            Assert.AreEqual(1, collection.Quests.Count);
        }

        [Test]
        public void Snapshot_JsonUtilityRoundTrip_RestoresSerializedValues()
        {
            QuestStateCollectionSnapshot original = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_001",
                    QuestStatus.Active,
                    2,
                    new[]
                    {
                        new ObjectiveStateSnapshot("objective_001", 3)
                    })
            });

            string json = JsonUtility.ToJson(original);
            QuestStateCollectionSnapshot restored = JsonUtility.FromJson<QuestStateCollectionSnapshot>(json);

            Assert.AreEqual(1, restored.Quests.Count);
            Assert.AreEqual("quest_001", restored.Quests[0].QuestId);
            Assert.AreEqual(QuestStatus.Active, restored.Quests[0].Status);
            Assert.AreEqual(2, restored.Quests[0].CompletionCount);
            Assert.AreEqual("objective_001", restored.Quests[0].Objectives[0].ObjectiveId);
            Assert.AreEqual(3, restored.Quests[0].Objectives[0].CurrentProgress);
        }
    }
}