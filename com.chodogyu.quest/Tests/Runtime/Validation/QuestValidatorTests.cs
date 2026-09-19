using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestValidatorTests
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
        public void Validate_WhenCatalogIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => QuestValidator.Validate(null));
        }

        [Test]
        public void Validate_WhenCatalogIsValid_ReturnsNoIssues()
        {
            QuestDefinition quest = CreateQuest("quest_001");
            catalog.ReplaceQuests(new[] { quest });

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);

            Assert.AreEqual(0, issues.Count);
        }

        [Test]
        public void Validate_WhenQuestIdIsEmpty_ReturnsQuestIdRequiredError()
        {
            catalog.ReplaceQuests(new[] { CreateQuest(string.Empty) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.QuestIdRequired);

            Assert.AreEqual(QuestValidationSeverity.Error, issue.Severity);
            Assert.AreEqual(0, issue.QuestIndex);
        }

        [Test]
        public void Validate_WhenQuestIdsAreDuplicated_ReturnsDuplicateErrors()
        {
            catalog.ReplaceQuests(new[]
            {
                CreateQuest("quest_001"),
                CreateQuest("quest_001")
            });

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);

            Assert.AreEqual(2, issues.Count(x => x.Code == QuestValidationCodes.QuestIdDuplicate));
        }

        [Test]
        public void Validate_WhenQuestHasNoObjectives_ReturnsObjectiveMissingError()
        {
            QuestDefinition quest = CreateQuest("quest_001", objectives: Array.Empty<ObjectiveDefinition>());
            catalog.ReplaceQuests(new[] { quest });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.ObjectiveMissing);

            Assert.AreEqual(QuestValidationSeverity.Error, issue.Severity);
            Assert.AreEqual("quest_001", issue.QuestId);
        }

        [Test]
        public void Validate_WhenObjectiveIdIsEmpty_ReturnsObjectiveIdRequiredError()
        {
            ObjectiveDefinition objective = CreateObjective(string.Empty);
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", objectives: new[] { objective }) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.ObjectiveIdRequired);

            Assert.AreEqual(0, issue.ObjectiveIndex);
        }

        [Test]
        public void Validate_WhenObjectiveIdsAreDuplicated_ReturnsDuplicateError()
        {
            ObjectiveDefinition first = CreateObjective("objective_001");
            ObjectiveDefinition second = CreateObjective("objective_001");
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", objectives: new[] { first, second }) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.ObjectiveIdDuplicate);

            Assert.AreEqual(1, issue.ObjectiveIndex);
            Assert.AreEqual("objective_001", issue.ObjectiveId);
        }

        [Test]
        public void Validate_WhenTargetProgressIsNotPositive_ReturnsTargetProgressError()
        {
            ObjectiveDefinition objective = CreateObjective("objective_001", targetProgress: 0);
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", objectives: new[] { objective }) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.ObjectiveTargetProgressInvalid);

            Assert.AreEqual(QuestValidationSeverity.Error, issue.Severity);
        }

        [Test]
        public void Validate_WhenDisplayTextIsMissing_ReturnsWarnings()
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("objective_001", string.Empty, string.Empty, 1);
            QuestDefinition quest = new QuestDefinition("quest_001", string.Empty, string.Empty, new[] { objective }, null, null, false);
            catalog.ReplaceQuests(new[] { quest });

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);

            Assert.AreEqual(3, issues.Count);
            Assert.IsTrue(issues.All(x => x.Severity == QuestValidationSeverity.Warning));
            Assert.IsTrue(issues.Any(x => x.Code == QuestValidationCodes.QuestTitleMissing));
            Assert.IsTrue(issues.Any(x => x.Code == QuestValidationCodes.QuestDescriptionMissing));
            Assert.IsTrue(issues.Any(x => x.Code == QuestValidationCodes.ObjectiveTitleMissing));
        }

        [Test]
        public void Validate_WhenPrerequisiteIdIsEmpty_ReturnsPrerequisiteIdRequiredError()
        {
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", prerequisites: new[] { string.Empty }) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.PrerequisiteIdRequired);

            Assert.AreEqual(0, issue.PrerequisiteIndex);
        }

        [Test]
        public void Validate_WhenPrerequisiteDoesNotExist_ReturnsNotFoundError()
        {
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", prerequisites: new[] { "quest_missing" }) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.PrerequisiteNotFound);

            Assert.AreEqual(QuestValidationSeverity.Error, issue.Severity);
        }

        [Test]
        public void Validate_WhenQuestReferencesItself_ReturnsSelfReferenceError()
        {
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", prerequisites: new[] { "quest_001" }) });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.PrerequisiteSelfReference);

            Assert.AreEqual(QuestValidationSeverity.Error, issue.Severity);
        }

        [Test]
        public void Validate_WhenPrerequisiteIsDuplicated_ReturnsDuplicateError()
        {
            QuestDefinition first = CreateQuest("quest_001");
            QuestDefinition second = CreateQuest("quest_002", prerequisites: new[] { "quest_001", "quest_001" });
            catalog.ReplaceQuests(new[] { first, second });

            QuestValidationIssue issue = FindIssue(QuestValidationCodes.PrerequisiteDuplicate);

            Assert.AreEqual(1, issue.QuestIndex);
            Assert.AreEqual(1, issue.PrerequisiteIndex);
        }

        [Test]
        public void Validate_WhenPrerequisitesFormCycle_ReturnsCycleErrorForEveryQuestInCycle()
        {
            QuestDefinition questA = CreateQuest("quest_a", prerequisites: new[] { "quest_c" });
            QuestDefinition questB = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestDefinition questC = CreateQuest("quest_c", prerequisites: new[] { "quest_b" });
            catalog.ReplaceQuests(new[] { questA, questB, questC });

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);
            QuestValidationIssue[] cycleIssues = issues.Where(x => x.Code == QuestValidationCodes.PrerequisiteCycle).ToArray();

            Assert.AreEqual(3, cycleIssues.Length);
            Assert.IsTrue(cycleIssues.Any(x => x.QuestId == "quest_a"));
            Assert.IsTrue(cycleIssues.Any(x => x.QuestId == "quest_b"));
            Assert.IsTrue(cycleIssues.Any(x => x.QuestId == "quest_c"));
        }

        [Test]
        public void Validate_WhenPrerequisitesFormValidChain_ReturnsNoCycleError()
        {
            QuestDefinition questA = CreateQuest("quest_a");
            QuestDefinition questB = CreateQuest("quest_b", prerequisites: new[] { "quest_a" });
            QuestDefinition questC = CreateQuest("quest_c", prerequisites: new[] { "quest_b" });
            catalog.ReplaceQuests(new[] { questA, questB, questC });

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);

            Assert.IsFalse(issues.Any(x => x.Code == QuestValidationCodes.PrerequisiteCycle));
        }

        [Test]
        public void Validate_WhenRewardValuesAreInvalid_ReturnsRewardErrors()
        {
            RewardDefinition reward = new RewardDefinition(string.Empty, string.Empty, 0);
            catalog.ReplaceQuests(new[] { CreateQuest("quest_001", rewards: new[] { reward }) });

            IReadOnlyList<QuestValidationIssue> issues = QuestValidator.Validate(catalog);

            Assert.IsTrue(issues.Any(x => x.Code == QuestValidationCodes.RewardTypeRequired));
            Assert.IsTrue(issues.Any(x => x.Code == QuestValidationCodes.RewardKeyRequired));
            Assert.IsTrue(issues.Any(x => x.Code == QuestValidationCodes.RewardAmountInvalid));
        }

        private QuestValidationIssue FindIssue(string code)
        {
            return QuestValidator.Validate(catalog).Single(x => x.Code == code);
        }

        private static ObjectiveDefinition CreateObjective(string id = "objective_001", int targetProgress = 1)
        {
            return new ObjectiveDefinition(id, "Objective", "Objective 설명", targetProgress);
        }

        private static QuestDefinition CreateQuest(string id, ObjectiveDefinition[] objectives = null, string[] prerequisites = null, RewardDefinition[] rewards = null)
        {
            ObjectiveDefinition[] actualObjectives = objectives ?? new[] { CreateObjective() };
            return new QuestDefinition(id, "Quest", "Quest 설명", actualObjectives, prerequisites, rewards, false);
        }
    }
}