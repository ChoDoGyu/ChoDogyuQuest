using System.Collections.Generic;
using NUnit.Framework;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestDefinitionTests
    {
        [Test]
        public void ObjectiveDefinition_WhenCreated_StoresValues()
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("kill_enemy", "적 처치", "적을 5마리 처치합니다.", 5);

            Assert.AreEqual("kill_enemy", objective.Id);
            Assert.AreEqual("적 처치", objective.Title);
            Assert.AreEqual("적을 5마리 처치합니다.", objective.Description);
            Assert.AreEqual(5, objective.TargetProgress);
        }

        [Test]
        public void RewardDefinition_WhenCreated_StoresValues()
        {
            RewardDefinition reward = new RewardDefinition("Currency", "Gold", 100);

            Assert.AreEqual("Currency", reward.Type);
            Assert.AreEqual("Gold", reward.Key);
            Assert.AreEqual(100, reward.Amount);
        }

        [Test]
        public void QuestDefinition_WhenCreated_StoresAllDefinitionData()
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("kill_enemy", "적 처치", "적을 처치합니다.", 5);
            RewardDefinition reward = new RewardDefinition("Currency", "Gold", 100);

            QuestDefinition quest = new QuestDefinition(
                "quest_001",
                "첫 번째 Quest",
                "기본 Quest 설명입니다.",
                new[] { objective },
                new[] { "quest_000" },
                new[] { reward },
                true);

            Assert.AreEqual("quest_001", quest.Id);
            Assert.AreEqual("첫 번째 Quest", quest.Title);
            Assert.AreEqual("기본 Quest 설명입니다.", quest.Description);
            Assert.AreEqual(1, quest.Objectives.Count);
            Assert.AreSame(objective, quest.Objectives[0]);
            Assert.AreEqual(1, quest.PrerequisiteQuestIds.Count);
            Assert.AreEqual("quest_000", quest.PrerequisiteQuestIds[0]);
            Assert.AreEqual(1, quest.Rewards.Count);
            Assert.AreSame(reward, quest.Rewards[0]);
            Assert.IsTrue(quest.IsRepeatable);
        }

        [Test]
        public void QuestDefinition_WhenCreated_CopiesSourceCollections()
        {
            List<ObjectiveDefinition> objectives = new List<ObjectiveDefinition>
            {
                new ObjectiveDefinition("objective_001", "목표", "설명", 1)
            };

            List<string> prerequisites = new List<string>
            {
                "quest_previous"
            };

            List<RewardDefinition> rewards = new List<RewardDefinition>
            {
                new RewardDefinition("Currency", "Gold", 10)
            };

            QuestDefinition quest = new QuestDefinition("quest_001", "Quest", "설명", objectives, prerequisites, rewards, false);

            objectives.Clear();
            prerequisites.Clear();
            rewards.Clear();

            Assert.AreEqual(1, quest.Objectives.Count);
            Assert.AreEqual(1, quest.PrerequisiteQuestIds.Count);
            Assert.AreEqual(1, quest.Rewards.Count);
        }

        [Test]
        public void QuestDefinition_WhenCollectionsAreNull_UsesEmptyCollections()
        {
            QuestDefinition quest = new QuestDefinition("quest_001", "Quest", "설명", null, null, null, false);

            Assert.AreEqual(0, quest.Objectives.Count);
            Assert.AreEqual(0, quest.PrerequisiteQuestIds.Count);
            Assert.AreEqual(0, quest.Rewards.Count);
        }
    }
}