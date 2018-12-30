using System.Collections.Generic;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using NUnit.Framework;
using UniRx;

namespace Assets.Visual_Behavior_Tree.Tests
{
    public class SequencerTests
    {
        [Test, Description("If any child fails, a sequencer should return running")]
        public void TestSequencerFailsOnAnyFail()
        {
            var sequencer = new Sequencer("", 1, 1);

            sequencer.AddChild(TestingResources.GetRunRunFail());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Fail);

            sequencer.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }

        [Test, Description("If a child fails, a sequencer should return fail immediately, and not continue running children")]
        public void TestSequencerCompletesOnFirstFail()
        {
            var sequencer = new Sequencer("", 1, 1);

            sequencer.AddChild(TestingResources.GetRunRunSuccess());
            sequencer.AddChild(TestingResources.GetRunRunFail());

            //all of these should be ignored
            //TODO: Should probably check if these are run anyways
            //      through the Concat operation
            sequencer.AddChild(TestingResources.GetRunRunFail());
            sequencer.AddChild(TestingResources.GetRunRunFail());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Fail);

            sequencer.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }

        [Test, Description("If none of the children fail, a sequencer should succeed")]
        public void TestSequencerSucceedsIfNoneFail()
        {
            var sequencer = new Sequencer("", 1, 1);

            sequencer.AddChild(TestingResources.GetRunRunSuccess());
            sequencer.AddChild(TestingResources.GetRunRunSuccess());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Success);

            sequencer.Start().Subscribe((x) => actual.Add(x));
            Assert.AreEqual(expected, actual);
        }
    }
}
