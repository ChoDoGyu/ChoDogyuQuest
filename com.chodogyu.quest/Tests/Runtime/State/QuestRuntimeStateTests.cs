using System;
using NUnit.Framework;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class QuestRuntimeStateTests
    {
        [Test]
        public void Constructor_WhenCreated_StartsInactive()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());

            Assert.AreEqual("quest_001", state.QuestId);
            Assert.AreEqual(QuestStatus.Inactive, state.Status);
            Assert.AreEqual(0, state.CompletionCount);
            Assert.IsFalse(state.AreAllObjectivesCompleted);
        }

        [Test]
        public void TryGetObjectiveState_WhenObjectiveExists_ReturnsState()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());

            bool found = state.TryGetObjectiveState("objective_001", out ObjectiveRuntimeState objectiveState);

            Assert.IsTrue(found);
            Assert.IsNotNull(objectiveState);
            Assert.AreEqual("objective_001", objectiveState.Id);
        }

        [Test]
        public void TryGetObjectiveState_WhenObjectiveDoesNotExist_ReturnsFalse()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());

            bool found = state.TryGetObjectiveState("missing", out ObjectiveRuntimeState objectiveState);

            Assert.IsFalse(found);
            Assert.IsNull(objectiveState);
        }

        [Test]
        public void Start_WhenInactive_ChangesStatusToActive()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());

            state.Start();

            Assert.AreEqual(QuestStatus.Active, state.Status);
        }

        [Test]
        public void Complete_WhenAllObjectivesAreCompleted_ChangesStatusAndIncrementsCompletionCount()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());
            state.Start();
            state.TryGetObjectiveState("objective_001", out ObjectiveRuntimeState objectiveState);
            objectiveState.Advance(3);

            state.Complete();

            Assert.AreEqual(QuestStatus.Completed, state.Status);
            Assert.AreEqual(1, state.CompletionCount);
        }

        [Test]
        public void Complete_WhenObjectiveIsIncomplete_ThrowsInvalidOperationException()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());
            state.Start();

            Assert.Throws<InvalidOperationException>(() => state.Complete());
        }

        [Test]
        public void Reset_WhenRepeatableQuestIsCompleted_ClearsProgressAndKeepsCompletionCount()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest(isRepeatable: true));
            state.Start();
            state.TryGetObjectiveState("objective_001", out ObjectiveRuntimeState objectiveState);
            objectiveState.Advance(3);
            state.Complete();

            state.Reset();

            Assert.AreEqual(QuestStatus.Inactive, state.Status);
            Assert.AreEqual(1, state.CompletionCount);
            Assert.AreEqual(0, objectiveState.CurrentProgress);
            Assert.IsFalse(objectiveState.IsCompleted);
        }

        [Test]
        public void Reset_WhenQuestIsNotRepeatable_ThrowsInvalidOperationException()
        {
            QuestRuntimeState state = new QuestRuntimeState(CreateQuest());
            state.Start();
            state.TryGetObjectiveState("objective_001", out ObjectiveRuntimeState objectiveState);
            objectiveState.Advance(3);
            state.Complete();

            Assert.Throws<InvalidOperationException>(() => state.Reset());
        }

        private static QuestDefinition CreateQuest(bool isRepeatable = false)
        {
            ObjectiveDefinition objective = new ObjectiveDefinition("objective_001", "Objective", "Objective 설명", 3);
            return new QuestDefinition("quest_001", "Quest", "Quest 설명", new[] { objective }, null, null, isRepeatable);
        }
    }
}