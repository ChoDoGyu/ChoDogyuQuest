using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using CDG.Quest.Editor;

namespace CDG.Quest.Tests.Editor
{
    public sealed class QuestEditorValidationIntegrationTests
    {
        private QuestCatalog catalog;
        private QuestCatalogEditorSession session;

        [SetUp]
        public void SetUp()
        {
            Undo.ClearAll();

            catalog = ScriptableObject.CreateInstance<QuestCatalog>();
            session = new QuestCatalogEditorSession(catalog);
        }

        [TearDown]
        public void TearDown()
        {
            Undo.ClearAll();

            if (catalog != null)
            {
                Object.DestroyImmediate(catalog);
            }
        }

        [Test]
        public void Validator_AfterEditorCreatesDefaultQuest_ReportsExpectedDefinitionIssues()
        {
            session.AddQuest();

            Apply();

            QuestValidationIssue[] issues = QuestValidator.Validate(catalog).ToArray();

            Assert.IsTrue(issues.Any(issue => issue.Code == QuestValidationCodes.ObjectiveMissing));
            Assert.IsTrue(issues.Any(issue => issue.Code == QuestValidationCodes.QuestDescriptionMissing));
        }

        [Test]
        public void Validator_AfterEditorCreatesValidQuest_ReturnsNoIssues()
        {
            session.AddQuest();

            SetQuestDescription(0, "정상 Quest 설명");
            session.AddObjective(0);

            Apply();

            QuestValidationIssue[] issues = QuestValidator.Validate(catalog).ToArray();

            Assert.AreEqual(0, issues.Length);
        }

        [Test]
        public void Validator_WhenReferencedQuestIdChanges_ReportsMissingPrerequisite()
        {
            session.AddQuest();
            session.AddQuest();

            SetQuestDescription(0, "첫 번째 Quest");
            SetQuestDescription(1, "두 번째 Quest");

            session.AddObjective(0);
            session.AddObjective(1);
            session.AddPrerequisite(1);

            Apply();

            Assert.AreEqual("quest_001", catalog.Quests[1].PrerequisiteQuestIds[0]);

            SerializedProperty firstQuest = session.GetQuestProperty(0);
            firstQuest.FindPropertyRelative("id").stringValue = "quest_start";

            Apply();

            QuestValidationIssue[] issues = QuestValidator.Validate(catalog).ToArray();

            Assert.IsTrue(
                issues.Any(issue =>
                    issue.Code == QuestValidationCodes.PrerequisiteNotFound &&
                    issue.QuestIndex == 1 &&
                    issue.PrerequisiteIndex == 0));
        }

        [Test]
        public void Validator_WhenEditorCreatesCircularPrerequisites_ReportsBothQuests()
        {
            session.AddQuest();
            session.AddQuest();

            SetQuestDescription(0, "첫 번째 Quest");
            SetQuestDescription(1, "두 번째 Quest");

            session.AddObjective(0);
            session.AddObjective(1);

            session.AddPrerequisite(0);
            session.AddPrerequisite(1);

            Apply();

            QuestValidationIssue[] cycleIssues = QuestValidator
                .Validate(catalog)
                .Where(issue => issue.Code == QuestValidationCodes.PrerequisiteCycle)
                .ToArray();

            Assert.AreEqual(2, cycleIssues.Length);
            Assert.IsTrue(cycleIssues.Any(issue => issue.QuestIndex == 0));
            Assert.IsTrue(cycleIssues.Any(issue => issue.QuestIndex == 1));
        }

        private void SetQuestDescription(int questIndex, string description)
        {
            SerializedProperty questProperty = session.GetQuestProperty(questIndex);
            questProperty.FindPropertyRelative("description").stringValue = description;
        }

        private void Apply()
        {
            session.ApplyModifiedProperties();
            session.Update();
        }
    }
}