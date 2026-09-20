using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using CDG.Quest.Editor;

namespace CDG.Quest.Tests.Editor
{
    public sealed class QuestCatalogEditorSessionTests
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
        public void AddQuest_WhenCatalogIsEmpty_CreatesDefaultQuest()
        {
            int index = session.AddQuest();

            Apply();

            Assert.AreEqual(0, index);
            Assert.AreEqual(1, catalog.Count);

            QuestDefinition quest = catalog.Quests[0];

            Assert.AreEqual("quest_001", quest.Id);
            Assert.AreEqual("New Quest", quest.Title);
            Assert.AreEqual(string.Empty, quest.Description);
            Assert.IsFalse(quest.IsRepeatable);
            Assert.AreEqual(0, quest.Objectives.Count);
            Assert.AreEqual(0, quest.PrerequisiteQuestIds.Count);
            Assert.AreEqual(0, quest.Rewards.Count);
        }

        [Test]
        public void AddQuest_WhenCalledRepeatedly_CreatesUniqueSequentialIds()
        {
            session.AddQuest();
            session.AddQuest();
            session.AddQuest();

            Apply();

            Assert.AreEqual(3, catalog.Count);
            Assert.AreEqual("quest_001", catalog.Quests[0].Id);
            Assert.AreEqual("quest_002", catalog.Quests[1].Id);
            Assert.AreEqual("quest_003", catalog.Quests[2].Id);
        }

        [Test]
        public void AddQuest_AfterFirstIdBecomesAvailable_ReusesFirstAvailableId()
        {
            session.AddQuest();
            session.AddQuest();

            Apply();

            session.DeleteQuest(0);
            Apply();

            session.AddQuest();
            Apply();

            Assert.AreEqual(2, catalog.Count);
            Assert.AreEqual("quest_002", catalog.Quests[0].Id);
            Assert.AreEqual("quest_001", catalog.Quests[1].Id);
        }

        [Test]
        public void DeleteQuest_WhenMiddleQuestIsRemoved_ReturnsNextValidIndex()
        {
            session.AddQuest();
            session.AddQuest();
            session.AddQuest();

            Apply();

            int selectedIndex = session.DeleteQuest(1);

            Apply();

            Assert.AreEqual(1, selectedIndex);
            Assert.AreEqual(2, catalog.Count);
            Assert.AreEqual("quest_001", catalog.Quests[0].Id);
            Assert.AreEqual("quest_003", catalog.Quests[1].Id);
        }

        [Test]
        public void DeleteQuest_WhenLastQuestIsRemoved_ReturnsMinusOne()
        {
            session.AddQuest();

            Apply();

            int selectedIndex = session.DeleteQuest(0);

            Apply();

            Assert.AreEqual(-1, selectedIndex);
            Assert.AreEqual(0, catalog.Count);
        }

        [Test]
        public void QuestProperties_WhenEditedThroughSerializedProperty_UpdateCatalog()
        {
            session.AddQuest();
            Apply();

            SerializedProperty questProperty = session.GetQuestProperty(0);

            questProperty.FindPropertyRelative("id").stringValue = "quest_main";
            questProperty.FindPropertyRelative("title").stringValue = "Main Quest";
            questProperty.FindPropertyRelative("description").stringValue = "메인 Quest 설명";
            questProperty.FindPropertyRelative("isRepeatable").boolValue = true;

            Apply();

            QuestDefinition quest = catalog.Quests[0];

            Assert.AreEqual("quest_main", quest.Id);
            Assert.AreEqual("Main Quest", quest.Title);
            Assert.AreEqual("메인 Quest 설명", quest.Description);
            Assert.IsTrue(quest.IsRepeatable);
        }

        [Test]
        public void AddObjective_WhenCalledRepeatedly_CreatesDefaultObjectivesWithUniqueIds()
        {
            session.AddQuest();
            Apply();

            session.AddObjective(0);
            session.AddObjective(0);

            Apply();

            QuestDefinition quest = catalog.Quests[0];

            Assert.AreEqual(2, quest.Objectives.Count);

            Assert.AreEqual("objective_001", quest.Objectives[0].Id);
            Assert.AreEqual("New Objective", quest.Objectives[0].Title);
            Assert.AreEqual(string.Empty, quest.Objectives[0].Description);
            Assert.AreEqual(1, quest.Objectives[0].TargetProgress);

            Assert.AreEqual("objective_002", quest.Objectives[1].Id);
            Assert.AreEqual(1, quest.Objectives[1].TargetProgress);
        }

        [Test]
        public void DeleteObjective_RemovesSelectedObjective()
        {
            session.AddQuest();
            session.AddObjective(0);
            session.AddObjective(0);

            Apply();

            session.DeleteObjective(0, 0);

            Apply();

            Assert.AreEqual(1, catalog.Quests[0].Objectives.Count);
            Assert.AreEqual("objective_002", catalog.Quests[0].Objectives[0].Id);
        }

        [Test]
        public void AddReward_CreatesDefaultReward()
        {
            session.AddQuest();
            Apply();

            session.AddReward(0);

            Apply();

            Assert.AreEqual(1, catalog.Quests[0].Rewards.Count);

            RewardDefinition reward = catalog.Quests[0].Rewards[0];

            Assert.AreEqual(string.Empty, reward.Type);
            Assert.AreEqual(string.Empty, reward.Key);
            Assert.AreEqual(1, reward.Amount);
        }

        [Test]
        public void DeleteReward_RemovesSelectedReward()
        {
            session.AddQuest();
            session.AddReward(0);
            session.AddReward(0);

            Apply();

            SerializedProperty questProperty = session.GetQuestProperty(0);
            SerializedProperty rewardsProperty = questProperty.FindPropertyRelative("rewards");

            rewardsProperty.GetArrayElementAtIndex(0).FindPropertyRelative("key").stringValue = "Gold";
            rewardsProperty.GetArrayElementAtIndex(1).FindPropertyRelative("key").stringValue = "Potion";

            Apply();

            session.DeleteReward(0, 0);

            Apply();

            Assert.AreEqual(1, catalog.Quests[0].Rewards.Count);
            Assert.AreEqual("Potion", catalog.Quests[0].Rewards[0].Key);
        }

        [Test]
        public void AddPrerequisite_AddsFirstAvailableQuestAndAvoidsDuplicate()
        {
            session.AddQuest();
            session.AddQuest();
            session.AddQuest();

            Apply();

            Assert.IsTrue(session.AddPrerequisite(1));
            Assert.IsTrue(session.AddPrerequisite(1));

            Apply();

            QuestDefinition quest = catalog.Quests[1];

            Assert.AreEqual(2, quest.PrerequisiteQuestIds.Count);
            Assert.AreEqual("quest_001", quest.PrerequisiteQuestIds[0]);
            Assert.AreEqual("quest_003", quest.PrerequisiteQuestIds[1]);
        }

        [Test]
        public void CanAddPrerequisite_WhenNoOtherQuestExists_ReturnsFalse()
        {
            session.AddQuest();

            Apply();

            Assert.IsFalse(session.CanAddPrerequisite(0));
        }

        [Test]
        public void GetPrerequisiteOptions_ExcludesSelfAndIdsUsedByOtherEntries()
        {
            session.AddQuest();
            session.AddQuest();
            session.AddQuest();

            Apply();

            session.AddPrerequisite(2);
            session.AddPrerequisite(2);

            Apply();

            string[] firstEntryOptions = session.GetPrerequisiteOptions(2, 0);

            CollectionAssert.Contains(firstEntryOptions, "quest_001");
            CollectionAssert.DoesNotContain(firstEntryOptions, "quest_002");
            CollectionAssert.DoesNotContain(firstEntryOptions, "quest_003");
        }

        [Test]
        public void RelationshipQueries_ReturnExpectedQuestIndices()
        {
            session.AddQuest();
            session.AddQuest();
            session.AddQuest();

            Apply();

            session.AddPrerequisite(1);
            session.AddPrerequisite(2);

            Apply();

            Assert.AreEqual(0, session.FindQuestIndexById("quest_001"));
            Assert.AreEqual(1, session.FindQuestIndexById("quest_002"));
            Assert.AreEqual(-1, session.FindQuestIndexById("missing"));

            CollectionAssert.AreEqual(
                new[] { "quest_001" },
                session.GetPrerequisiteIds(1));

            CollectionAssert.AreEqual(
                new[] { 1, 2 },
                session.GetDependentQuestIndices(0));
        }

        [Test]
        public void UndoRedo_AfterAddQuest_RestoresSerializedCatalog()
        {
            session.AddQuest();

            Apply();

            Assert.AreEqual(1, catalog.Count);

            Undo.PerformUndo();
            session.Update();

            Assert.AreEqual(0, session.QuestCount);
            Assert.AreEqual(0, catalog.Count);

            Undo.PerformRedo();
            session.Update();

            Assert.AreEqual(1, session.QuestCount);
            Assert.AreEqual(1, catalog.Count);
            Assert.AreEqual("quest_001", catalog.Quests[0].Id);
        }

        private void Apply()
        {
            session.ApplyModifiedProperties();
            session.Update();
        }
    }
}