using System;
using CDG.Core.Results;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestManagerCreationTests
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
        public void Create_WhenCatalogIsNull_ReturnsFailure()
        {
            Result<QuestManager> result = QuestManager.Create(null);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.CatalogRequired, result.Error.Code);
        }

        [Test]
        public void Create_WhenCatalogHasValidationError_ReturnsFailure()
        {
            QuestDefinition quest = new QuestDefinition("quest_001", "Quest", "설명", Array.Empty<ObjectiveDefinition>(), null, null, false);
            catalog.ReplaceQuests(new[] { quest });

            Result<QuestManager> result = QuestManager.Create(catalog);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.ValidationFailed, result.Error.Code);
        }

        [Test]
        public void Create_WhenCatalogHasOnlyWarnings_Succeeds()
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("objective_001", string.Empty, string.Empty, 1);
            QuestDefinition quest = new QuestDefinition("quest_001", string.Empty, string.Empty, new[] { objective }, null, null, false);
            catalog.ReplaceQuests(new[] { quest });

            Result<QuestManager> result = QuestManager.Create(catalog);

            Assert.IsTrue(result.IsSuccess);
        }

        [Test]
        public void GetQuestDefinition_WhenQuestExists_ReturnsDefinition()
        {
            QuestDefinition quest = CreateQuest();
            QuestManager manager = CreateManager(quest);

            Result<QuestDefinition> result = manager.GetQuestDefinition("quest_001");

            Assert.IsTrue(result.IsSuccess);
            Assert.AreSame(quest, result.Value);
        }

        [Test]
        public void StateQueries_WhenCreated_ReturnInitialValues()
        {
            QuestManager manager = CreateManager(CreateQuest());

            Result<QuestStatus> statusResult = manager.GetQuestStatus("quest_001");
            Result<int> completionResult = manager.GetCompletionCount("quest_001");
            Result<int> progressResult = manager.GetObjectiveProgress("quest_001", "objective_001");

            Assert.AreEqual(QuestStatus.Inactive, statusResult.Value);
            Assert.AreEqual(0, completionResult.Value);
            Assert.AreEqual(0, progressResult.Value);
        }

        [Test]
        public void GetQuestDefinition_WhenQuestIdIsInvalid_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest());

            Result<QuestDefinition> result = manager.GetQuestDefinition(string.Empty);

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.InvalidQuestId, result.Error.Code);
        }

        [Test]
        public void GetQuestStatus_WhenQuestDoesNotExist_ReturnsFailure()
        {
            QuestManager manager = CreateManager(CreateQuest());

            Result<QuestStatus> result = manager.GetQuestStatus("missing");

            Assert.IsTrue(result.IsFailure);
            Assert.AreEqual(QuestErrorCodes.QuestNotFound, result.Error.Code);
        }

        [Test]
        public void GetActiveQuests_WhenCreated_ReturnsEmptyCollection()
        {
            QuestManager manager = CreateManager(CreateQuest());

            Assert.AreEqual(0, manager.GetActiveQuests().Count);
        }

        private QuestManager CreateManager(params QuestDefinition[] quests)
        {
            catalog.ReplaceQuests(quests);
            Result<QuestManager> result = QuestManager.Create(catalog);
            Assert.IsTrue(result.IsSuccess);
            return result.Value;
        }

        private static QuestDefinition CreateQuest()
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("objective_001", "Objective", "Objective 설명", 1);
            return new QuestDefinition("quest_001", "Quest", "Quest 설명", new[] { objective }, null, null, false);
        }
    }
}