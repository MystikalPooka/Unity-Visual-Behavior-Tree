using System.Collections.Generic;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using NUnit.Framework;
using UniRx;

namespace Assets.Visual_Behavior_Tree.Tests
{
    public class SelectorTests
    {
        [Test, Description("If any child succeeds, a selector should return succeed")]
        public void TestSelectorSucceedsOnSucceed()
        {
            var selector = new Selector("", 1, 1);

            selector.AddChild(TestingResources.GetRunRunSuccess());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Success);

            selector.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }

        [Test, Description("If a child succeeds, a selector should return succeed immediately, and not continue running children")]
        public void TestSelectorCompletesOnFirstSucceed()
        {
            var selector = new Selector("", 1, 1);

            selector.AddChild(TestingResources.GetRunRunFail());
            selector.AddChild(TestingResources.GetRunRunSuccess());

            //all of these should be ignored
            //TODO: Should probably check if these are run anyways
            //      through the Concat operation
            selector.AddChild(TestingResources.GetRunRunFail());
            selector.AddChild(TestingResources.GetRunRunFail());
            selector.AddChild(TestingResources.GetRunRunFail());
            selector.AddChild(TestingResources.GetRunRunFail());

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Success);

            selector.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }

        [Test, Description("If none of the children succeed, a selector should fail")]
        public void TestSelectorFailsIfNoneSucceed()
        {
            var selector = new Selector("", 1, 1);

            selector.AddChild(TestingResources.GetRunRunFail()); //should be "running"
            selector.AddChild(TestingResources.GetRunRunFail()); //should be "running"

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Fail);

            selector.Start().Subscribe((x) => actual.Add(x));
            Assert.AreEqual(expected, actual);
        }
    }
}
