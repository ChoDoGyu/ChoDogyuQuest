using System;
using NUnit.Framework;

namespace CDG.Quest.Tests.Runtime
{
    public sealed class ObjectiveRuntimeStateTests
    {
        [Test]
        public void Constructor_WhenCreated_StartsWithZeroProgress()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));

            Assert.AreEqual("objective_001", state.Id);
            Assert.AreEqual(5, state.TargetProgress);
            Assert.AreEqual(0, state.CurrentProgress);
            Assert.IsFalse(state.IsCompleted);
        }

        [Test]
        public void Advance_WhenCalled_IncreasesProgress()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));

            state.Advance(2);

            Assert.AreEqual(2, state.CurrentProgress);
            Assert.IsFalse(state.IsCompleted);
        }

        [Test]
        public void Advance_WhenAmountExceedsTarget_ClampsToTarget()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));

            state.Advance(100);

            Assert.AreEqual(5, state.CurrentProgress);
            Assert.IsTrue(state.IsCompleted);
        }

        [Test]
        public void SetProgress_WhenValueExceedsTarget_ClampsToTarget()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));

            state.SetProgress(10);

            Assert.AreEqual(5, state.CurrentProgress);
            Assert.IsTrue(state.IsCompleted);
        }

        [Test]
        public void Reset_WhenCalled_ClearsProgress()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));
            state.Advance(5);

            state.Reset();

            Assert.AreEqual(0, state.CurrentProgress);
            Assert.IsFalse(state.IsCompleted);
        }

        [Test]
        public void Advance_WhenAmountIsNotPositive_ThrowsArgumentOutOfRangeException()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));

            Assert.Throws<ArgumentOutOfRangeException>(() => state.Advance(0));
        }

        [Test]
        public void SetProgress_WhenProgressIsNegative_ThrowsArgumentOutOfRangeException()
        {
            ObjectiveRuntimeState state = new ObjectiveRuntimeState(CreateDefinition(5));

            Assert.Throws<ArgumentOutOfRangeException>(() => state.SetProgress(-1));
        }

        private static ObjectiveDefinition CreateDefinition(int targetProgress)
        {
            return new ObjectiveDefinition("objective_001", "Objective", "Objective 설명", targetProgress);
        }
    }
}