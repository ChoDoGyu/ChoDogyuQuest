using System.Collections.Generic;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestRuntimeIntegrationTests
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
        public void FullFlow_PrerequisiteAndMultipleObjectives_CompletesInExpectedOrder()
        {
            QuestDefinition first = CreateQuest(
                "quest_a",
                new[]
                {
                    CreateObjective("objective_a", 2),
                    CreateObjective("objective_b", 1)
                });

            QuestDefinition second = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestManager manager = CreateManager(first, second);

            Assert.IsFalse(manager.CanStartQuest("quest_b"));
            Assert.IsTrue(manager.StartQuest("quest_a").IsSuccess);

            manager.AdvanceObjective("quest_a", "objective_a", 2);

            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_a").Value);
            Assert.IsFalse(manager.CanStartQuest("quest_b"));

            manager.AdvanceObjective("quest_a", "objective_b", 1);

            Assert.AreEqual(QuestStatus.Completed, manager.GetQuestStatus("quest_a").Value);
            Assert.AreEqual(1, manager.GetCompletionCount("quest_a").Value);
            Assert.IsTrue(manager.CanStartQuest("quest_b"));
            Assert.IsTrue(manager.StartQuest("quest_b").IsSuccess);
            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_b").Value);
        }

        [Test]
        public void SetObjectiveProgress_WhenReachesTarget_CompletesQuestAutomatically()
        {
            QuestDefinition quest = CreateQuest(
                "quest_001",
                new[] { CreateObjective("objective_001", 3) });

            QuestManager manager = CreateManager(quest);
            manager.StartQuest("quest_001");

            Result result = manager.SetObjectiveProgress("quest_001", "objective_001", 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, manager.GetObjectiveProgress("quest_001", "objective_001").Value);
            Assert.AreEqual(QuestStatus.Completed, manager.GetQuestStatus("quest_001").Value);
            Assert.AreEqual(1, manager.GetCompletionCount("quest_001").Value);
        }

        [Test]
        public void GetActiveQuests_WhenMultipleQuestsAreActive_ReturnsCatalogOrder()
        {
            QuestDefinition first = CreateQuest("quest_a");
            QuestDefinition second = CreateQuest("quest_b");
            QuestDefinition third = CreateQuest("quest_c");
            QuestManager manager = CreateManager(first, second, third);

            manager.StartQuest("quest_c");
            manager.StartQuest("quest_a");

            IReadOnlyList<QuestDefinition> activeQuests = manager.GetActiveQuests();

            Assert.AreEqual(2, activeQuests.Count);
            Assert.AreEqual("quest_a", activeQuests[0].Id);
            Assert.AreEqual("quest_c", activeQuests[1].Id);
        }

        [Test]
        public void RepeatableQuest_WhenCompletedTwice_IncrementsCompletionCountAndKeepsPrerequisiteHistory()
        {
            QuestDefinition repeatable = CreateQuest("quest_a", isRepeatable: true);
            QuestDefinition dependent = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestManager manager = CreateManager(repeatable, dependent);

            manager.StartQuest("quest_a");
            manager.AdvanceObjective("quest_a", "objective_001", 1);

            Assert.AreEqual(1, manager.GetCompletionCount("quest_a").Value);
            Assert.IsTrue(manager.CanStartQuest("quest_b"));

            manager.ResetQuest("quest_a");
            manager.StartQuest("quest_a");
            manager.AdvanceObjective("quest_a", "objective_001", 1);

            Assert.AreEqual(QuestStatus.Completed, manager.GetQuestStatus("quest_a").Value);
            Assert.AreEqual(2, manager.GetCompletionCount("quest_a").Value);
            Assert.IsTrue(manager.CanStartQuest("quest_b"));
        }

        [Test]
        public void RestoreState_WhenMultipleQuestsExist_RestoresAndCanContinueProgress()
        {
            QuestDefinition first = CreateQuest("quest_a");
            QuestDefinition second = CreateQuest(
                "quest_b",
                new[] { CreateObjective("objective_001", 5) },
                new[] { "quest_a" });

            QuestManager source = CreateManager(first, second);

            source.StartQuest("quest_a");
            source.AdvanceObjective("quest_a", "objective_001", 1);
            source.StartQuest("quest_b");
            source.AdvanceObjective("quest_b", "objective_001", 2);

            QuestStateCollectionSnapshot snapshot = source.CaptureState();

            QuestManager restored = CreateManager(first, second);
            Result restoreResult = restored.RestoreState(snapshot);

            Assert.IsTrue(restoreResult.IsSuccess);
            Assert.AreEqual(QuestStatus.Completed, restored.GetQuestStatus("quest_a").Value);
            Assert.AreEqual(1, restored.GetCompletionCount("quest_a").Value);
            Assert.AreEqual(QuestStatus.Active, restored.GetQuestStatus("quest_b").Value);
            Assert.AreEqual(2, restored.GetObjectiveProgress("quest_b", "objective_001").Value);

            restored.AdvanceObjective("quest_b", "objective_001", 3);

            Assert.AreEqual(QuestStatus.Completed, restored.GetQuestStatus("quest_b").Value);
            Assert.AreEqual(1, restored.GetCompletionCount("quest_b").Value);
        }

        [Test]
        public void RestoreState_WhenLaterQuestIsInvalid_DoesNotModifyAnyQuest()
        {
            QuestDefinition first = CreateQuest(
                "quest_a",
                new[] { CreateObjective("objective_001", 5) });

            QuestDefinition second = CreateQuest(
                "quest_b",
                new[] { CreateObjective("objective_001", 5) });

            QuestManager manager = CreateManager(first, second);

            manager.StartQuest("quest_a");
            manager.StartQuest("quest_b");
            manager.AdvanceObjective("quest_a", "objective_001", 1);
            manager.AdvanceObjective("quest_b", "objective_001", 1);

            QuestStateCollectionSnapshot invalidSnapshot = new QuestStateCollectionSnapshot(new[]
            {
                new QuestStateSnapshot(
                    "quest_a",
                    QuestStatus.Active,
                    0,
                    new[]
                    {
                        new ObjectiveStateSnapshot("objective_001", 3)
                    }),
                new QuestStateSnapshot(
                    "quest_b",
                    QuestStatus.Active,
                    0,
                    new[]
                    {
                        new ObjectiveStateSnapshot("objective_001", 10)
                    })
            });

            Result result = manager.RestoreState(invalidSnapshot);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidStateSnapshot, result.Error.Code);
            Assert.AreEqual(1, manager.GetObjectiveProgress("quest_a", "objective_001").Value);
            Assert.AreEqual(1, manager.GetObjectiveProgress("quest_b", "objective_001").Value);
            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_a").Value);
            Assert.AreEqual(QuestStatus.Active, manager.GetQuestStatus("quest_b").Value);
        }

        [Test]
        public void RuntimeEvents_WhenMultiObjectiveQuestCompletes_RaiseExpectedCounts()
        {
            QuestDefinition quest = CreateQuest(
                "quest_001",
                new[]
                {
                    CreateObjective("objective_a", 1),
                    CreateObjective("objective_b", 1)
                });

            QuestManager manager = CreateManager(quest);
            int startedCount = 0;
            int progressChangedCount = 0;
            int objectiveCompletedCount = 0;
            int questCompletedCount = 0;

            manager.QuestStarted += definition => startedCount++;
            manager.ObjectiveProgressChanged += (definition, objective, current, target) => progressChangedCount++;
            manager.ObjectiveCompleted += (definition, objective) => objectiveCompletedCount++;
            manager.QuestCompleted += definition => questCompletedCount++;

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_a", 1);
            manager.AdvanceObjective("quest_001", "objective_b", 1);

            Assert.AreEqual(1, startedCount);
            Assert.AreEqual(2, progressChangedCount);
            Assert.AreEqual(2, objectiveCompletedCount);
            Assert.AreEqual(1, questCompletedCount);
        }

        [Test]
        public void PublicQuestMethods_WhenQuestDoesNotExist_ReturnQuestNotFound()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));

            Result startResult = manager.StartQuest("missing");
            Result advanceResult = manager.AdvanceObjective("missing", "objective_001", 1);
            Result resetResult = manager.ResetQuest("missing");
            Result<QuestStatus> statusResult = manager.GetQuestStatus("missing");

            Assert.AreEqual(QuestErrorCodes.QuestNotFound, startResult.Error.Code);
            Assert.AreEqual(QuestErrorCodes.QuestNotFound, advanceResult.Error.Code);
            Assert.AreEqual(QuestErrorCodes.QuestNotFound, resetResult.Error.Code);
            Assert.AreEqual(QuestErrorCodes.QuestNotFound, statusResult.Error.Code);
        }

        [Test]
        public void ObjectiveMethods_WhenObjectiveIdIsInvalid_ReturnExpectedErrors()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001"));
            manager.StartQuest("quest_001");

            Result invalidIdResult = manager.AdvanceObjective("quest_001", string.Empty, 1);
            Result missingResult = manager.AdvanceObjective("quest_001", "missing", 1);
            Result<int> queryResult = manager.GetObjectiveProgress("quest_001", "missing");

            Assert.AreEqual(QuestErrorCodes.InvalidObjectiveId, invalidIdResult.Error.Code);
            Assert.AreEqual(QuestErrorCodes.ObjectiveNotFound, missingResult.Error.Code);
            Assert.AreEqual(QuestErrorCodes.ObjectiveNotFound, queryResult.Error.Code);
        }

        [Test]
        public void CaptureState_AfterRepeatableQuestReset_PreservesHistoryAndClearsProgress()
        {
            QuestManager manager = CreateManager(CreateQuest("quest_001", isRepeatable: true));

            manager.StartQuest("quest_001");
            manager.AdvanceObjective("quest_001", "objective_001", 1);
            manager.ResetQuest("quest_001");

            QuestStateCollectionSnapshot snapshot = manager.CaptureState();
            QuestStateSnapshot questSnapshot = snapshot.Quests[0];

            Assert.AreEqual(QuestStatus.Inactive, questSnapshot.Status);
            Assert.AreEqual(1, questSnapshot.CompletionCount);
            Assert.AreEqual(0, questSnapshot.Objectives[0].CurrentProgress);
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