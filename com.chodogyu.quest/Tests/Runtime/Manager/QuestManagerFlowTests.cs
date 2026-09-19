using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestManagerFlowTests
    {
        private QuestCatalog catalog;

        [SetUp]
        public void SetUp()
        {
            catalog = ScriptableObject.CreateInstance<QuestCatalog>();
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(catalog);
        }

        [Test]
        public void StartQuest_WhenStartable_ActivatesQuestAndRaisesEvent()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));
            string startedQuestId = null;
            manager.QuestStarted += quest => startedQuestId = quest.Id;

            Result result = manager.StartQuest("quest_001");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_001").Value);
            Assert.AreEqual("quest_001", startedQuestId);
            Assert.AreEqual(1, manager.GetActiveQuests().Count);
            Assert.IsFalse(manager.CanStartQuest("quest_001"));
        }

        [Test]
        public void StartQuest_WhenPrerequisiteIsIncomplete_ReturnsFailure()
        {
            QuestDefinition first = CreateQuest("quest_a");
            QuestDefinition second = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestManager manager = CreateManager(first, second);

            Result result = manager.StartQuest("quest_b");

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.PrerequisiteNotCompleted, result.Error.Code);
            Assert.IsFalse(manager.CanStartQuest("quest_b"));
        }

        [Test]
        public void StartQuest_WhenPrerequisiteWasCompletedAndReset_CanStartDependentQuest()
        {
            QuestDefinition first = CreateQuest("quest_a", isRepeatable: true);
            QuestDefinition second = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestManager manager = CreateManager(first, second);

            manager.StartQuest("quest_a");
            manager.AdvanceObjective("quest_a", "objective_001", 1);
            manager.ResetQuest("quest_a");

            Assert.AreEqual(QuestStatus.Inactive, manager.GetQuestStatus("quest_a").Value);
            Assert.AreEqual(1, manager.GetCompletionCount("quest_a").Value);
            Assert.IsTrue(manager.CanStartQuest("quest_b"));
            Assert.IsTrue(manager.StartQuest("quest_b").IsSuccess);
        }

        [Test]
        public void AdvanceObjective_WhenQuestIsInactive_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));

            Result result = manager.AdvanceObjective("quest_001", "objective_001", 1);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.QuestNotActive, result.Error.Code);
        }

        [Test]
        public void AdvanceObjective_WhenActive_ChangesProgressAndRaisesEvent()
        {
            ObjectiveDefinition objective = CreateObjective("objective_001", 3);
            QuestManager manager = CreateManager(CreateQuest("quest_001", objectives: new[] { objective }));
            int receivedCurrent = -1;
            int receivedTarget = -1;

            manager.StartQuest("quest_001");
            manager.ObjectiveProgressChanged += (quest, changedObjective, current, target) =>
            {
                receivedCurrent = current;
                receivedTarget = target;
            };

            Result result = manager.AdvanceObjective("quest_001", "objective_001", 2);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, manager.GetObjectiveProgress("quest_001", "objective_001").Value);
            Assert.AreEqual(2, receivedCurrent);
            Assert.AreEqual(3, receivedTarget);
            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_001").Value);
        }

        [Test]
        public void AdvanceObjective_WhenMultipleObjectivesExist_CompletesQuestOnlyAfterAllObjectivesComplete()
        {
            ObjectiveDefinition first = CreateObjective("objective_a", 2);
            ObjectiveDefinition second = CreateObjective("objective_b", 1);
            QuestManager manager = CreateManager(CreateQuest("quest_001", objectives: new[] { first, second }));

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_a", 2);

            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_001").Value);

            manager.AdvanceObjective("quest_001", "objective_b", 1);

            Assert.AreEqual(QuestStatus.Completed, manager.GetQuestStatus("quest_001").Value);
            Assert.AreEqual(1, manager.GetCompletionCount("quest_001").Value);
        }

        [Test]
        public void ProgressMethods_WhenProgressValueIsInvalid_ReturnFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));
            manager.StartQuest("quest_001");

            Result advanceResult = manager.AdvanceObjective("quest_001", "objective_001", 0);
            Result setResult = manager.SetObjectiveProgress("quest_001", "objective_001", -1);

            Assert.AreEqual(QuestErrorCodes.InvalidProgress, advanceResult.Error.Code);
            Assert.AreEqual(QuestErrorCodes.InvalidProgress, setResult.Error.Code);
        }

        [Test]
        public void AdvanceObjective_WhenObjectiveIsAlreadyCompleted_ReturnsFailure()
        {
            ObjectiveDefinition first = CreateObjective("objective_a", 1);
            ObjectiveDefinition second = CreateObjective("objective_b", 1);
            QuestManager manager = CreateManager(CreateQuest("quest_001", objectives: new[] { first, second }));

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_a", 1);

            Result result = manager.AdvanceObjective("quest_001", "objective_a", 1);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.ObjectiveAlreadyCompleted, result.Error.Code);
        }

        [Test]
        public void SetObjectiveProgress_WhenActive_SetsAbsoluteProgress()
        {
            ObjectiveDefinition objective = CreateObjective("objective_001", 5);
            QuestManager manager = CreateManager(CreateQuest("quest_001", objectives: new[] { objective }));
            manager.StartQuest("quest_001");

            manager.SetObjectiveProgress("quest_001", "objective_001", 3);
            Assert.AreEqual(3, manager.GetObjectiveProgress("quest_001", "objective_001").Value);

            manager.SetObjectiveProgress("quest_001", "objective_001", 1);
            Assert.AreEqual(1, manager.GetObjectiveProgress("quest_001", "objective_001").Value);
        }

        [Test]
        public void ResetQuest_WhenQuestIsNotRepeatable_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 1);

            Result result = manager.ResetQuest("quest_001");

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.QuestNotRepeatable, result.Error.Code);
        }

        [Test]
        public void ResetQuest_WhenRepeatable_ClearsProgressKeepsCompletionCountAndRaisesEvent()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", isRepeatable: true));
            string resetQuestId = null;

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 1);
            manager.QuestReset += quest => resetQuestId = quest.Id;

            Result result = manager.ResetQuest("quest_001");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(QuestStatus.Inactive, manager.GetQuestStatus("quest_001").Value);
            Assert.AreEqual(0, manager.GetObjectiveProgress("quest_001", "objective_001").Value);
            Assert.AreEqual(1, manager.GetCompletionCount("quest_001").Value);
            Assert.AreEqual("quest_001", resetQuestId);
        }

        [Test]
        public void AdvanceObjective_WhenQuestCompletes_RaisesEventsInExpectedOrder()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));
            List<string> events = new List<string>();

            manager.ObjectiveProgressChanged += (quest, objective, current, target) => events.Add("ProgressChanged");
            manager.ObjectiveCompleted += (quest, objective) => events.Add("ObjectiveCompleted");
            manager.QuestCompleted += quest => events.Add("QuestCompleted");

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 1);

            CollectionAssert.AreEqual(new[]
            {
                "ProgressChanged",
                "ObjectiveCompleted",
                "QuestCompleted"
            }, events);
        }

        private QuestManager CreateManager(params QuestDefinition[] quests)
        {
            catalog.ReplaceQuests(quests);
            Result<QuestManager> result = QuestManager.Create(catalog);
            Assert.IsTrue(result.IsSuccess);
            return result.Value;
        }

        private static ObjectiveDefinition CreateObjective(string id, int targetProgress)
        {
            return new ObjectiveDefinition(id, "Objective", "Objective 설명", targetProgress);
        }

        private static QuestDefinition CreateQuest(string id, ObjectiveDefinition[] objectives = null, string[] prerequisites = null, bool isRepeatable = false)
        {
            ObjectiveDefinition[] actualObjectives = objectives ?? new[] { CreateObjective("objective_001", 1) };
            return new QuestDefinition(id, "Quest", "Quest 설명", actualObjectives, prerequisites, null, isRepeatable);
        }
    }
}