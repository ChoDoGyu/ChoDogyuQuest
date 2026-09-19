using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestManagerStateTests
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
        public void CaptureState_WhenCalled_CapturesCurrentRuntimeValues()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", targetProgress: 5));
            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 3);

            QuestStateCollectionSnapshot snapshot = manager.CaptureState();

            Assert.AreEqual(1, snapshot.Quests.Count);
            Assert.AreEqual("quest_001", snapshot.Quests[0].QuestId);
            Assert.AreEqual(QuestStatus.Active, snapshot.Quests[0].Status);
            Assert.AreEqual(0, snapshot.Quests[0].CompletionCount);
            Assert.AreEqual(3, snapshot.Quests[0].Objectives[0].CurrentProgress);
        }

        [Test]
        public void RestoreState_WhenSnapshotIsValid_RestoresRuntimeValues()
        {
            QuestDefinition quest = CreateQuest("quest_001", targetProgress: 5, isRepeatable: true);

            QuestManager source = CreateManager(quest);
            source.StartQuest("quest_001");
            source.AdvanceObjective("quest_001", "objective_001", 5);
            source.ResetQuest("quest_001");
            source.StartQuest("quest_001");
            source.AdvanceObjective("quest_001", "objective_001", 2);

            QuestStateCollectionSnapshot snapshot = source.CaptureState();
            QuestManager target = CreateManager(quest);

            Result result = target.RestoreState(snapshot);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(QuestStatus.Active, target.GetQuestStatus("quest_001").Value);
            Assert.AreEqual(1, target.GetCompletionCount("quest_001").Value);
            Assert.AreEqual(2, target.GetObjectiveProgress("quest_001", "objective_001").Value);
        }

        [Test]
        public void RestoreState_WhenCalled_DoesNotRaiseRuntimeEvents()
        {
            QuestManager source = CreateManager(CreateQuest("quest_001", targetProgress: 5));
            source.StartQuest("quest_001");
            source.AdvanceObjective("quest_001", "objective_001", 2);
            QuestStateCollectionSnapshot snapshot = source.CaptureState();

            QuestManager target = CreateManager(CreateQuest("quest_001", targetProgress: 5));
            int eventCount = 0;

            target.QuestStarted += quest => eventCount++;
            target.ObjectiveProgressChanged += (quest, objective, current, total) => eventCount++;
            target.ObjectiveCompleted += (quest, objective) => eventCount++;
            target.QuestCompleted += quest => eventCount++;
            target.QuestReset += quest => eventCount++;

            Result result = target.RestoreState(snapshot);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, eventCount);
        }

        [Test]
        public void RestoreState_WhenSnapshotIsNull_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));

            Result result = manager.RestoreState(null);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.StateSnapshotRequired, result.Error.Code);
        }

        [Test]
        public void RestoreState_WhenQuestCountDoesNotMatch_ReturnsFailureAndKeepsCurrentState()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", targetProgress: 5));
            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 2);

            QuestStateCollectionSnapshot invalidSnapshot = new QuestStateCollectionSnapshot(new QuestStateSnapshot[0]);

            Result result = manager.RestoreState(invalidSnapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidStateSnapshot, result.Error.Code);
            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_001").Value);
            Assert.AreEqual(2, manager.GetObjectiveProgress("quest_001", "objective_001").Value);
        }

        [Test]
        public void RestoreState_WhenQuestDoesNotExist_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));

            QuestStateCollectionSnapshot snapshot = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_missing",
                    QuestStatus.Inactive,
                    0,
                    new[] { new ObjectiveStateSnapshot("objective_001", 0) })
            });

            Result result = manager.RestoreState(snapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidStateSnapshot, result.Error.Code);
        }

        [Test]
        public void RestoreState_WhenObjectiveProgressIsInvalid_ReturnsFailureAndKeepsCurrentState()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", targetProgress: 5));
            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 2);

            QuestStateCollectionSnapshot snapshot = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_001",
                    QuestStatus.Active,
                    0,
                    new[] { new ObjectiveStateSnapshot("objective_001", 10) })
            });

            Result result = manager.RestoreState(snapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(2, manager.GetObjectiveProgress("quest_001", "objective_001").Value);
        }

        [Test]
        public void RestoreState_WhenCompletedQuestHasIncompleteObjective_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", targetProgress: 5));

            QuestStateCollectionSnapshot snapshot = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_001",
                    QuestStatus.Completed,
                    1,
                    new[] { new ObjectiveStateSnapshot("objective_001", 4) })
            });

            Result result = manager.RestoreState(snapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidStateSnapshot, result.Error.Code);
        }

        [Test]
        public void RestoreState_WhenActiveQuestHasAllObjectivesCompleted_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", targetProgress: 5));

            QuestStateCollectionSnapshot snapshot = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_001",
                    QuestStatus.Active,
                    0,
                    new[] { new ObjectiveStateSnapshot("objective_001", 5) })
            });

            Result result = manager.RestoreState(snapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidStateSnapshot, result.Error.Code);
        }

        [Test]
        public void RestoreState_WhenPrerequisiteHistoryIsInvalid_ReturnsFailure()
        {
            QuestDefinition first = CreateQuest("quest_a");
            QuestDefinition second = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestManager manager = CreateManager(first, second);

            QuestStateCollectionSnapshot snapshot = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_a",
                    QuestStatus.Inactive,
                    0,
                    new[] { new ObjectiveStateSnapshot("objective_001", 0) }),
                new QuestStateSnapshot(
                    "quest_b",
                    QuestStatus.Active,
                    0,
                    new[] { new ObjectiveStateSnapshot("objective_001", 0) })
            });

            Result result = manager.RestoreState(snapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidStateSnapshot, result.Error.Code);
        }

        private QuestManager CreateManager(params QuestDefinition[] quests)
        {
            catalog.ReplaceQuests(quests);
            Result<QuestManager> result = QuestManager.Create(catalog);
            Assert.IsTrue(result.IsSuccess);
            return result.Value;
        }

        private static QuestDefinition CreateQuest(string id, int targetProgress = 1, string[] prerequisites = null, bool isRepeatable = false)
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("objective_001", "Objective", "Objective 설명", targetProgress);
            return new QuestDefinition(id, "Quest", "Quest 설명", new[] { objective }, prerequisites, null, isRepeatable);
        }
    }
}