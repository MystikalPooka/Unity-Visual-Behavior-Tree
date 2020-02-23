using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using UniRx;
using System.Linq;

namespace Assets.Visual_Behavior_Tree.Tests
{
    class MergeTests
    {
        static IObservable<BehaviorState> Wait100msSucceed = Observable.Timer(TimeSpan.FromMilliseconds(100))
                                                                       .Publish().Select(e => BehaviorState.Success);

        static IObservable<BehaviorState> Wait200msFail = Observable.Timer(TimeSpan.FromMilliseconds(200))
                                                               .Select(e => BehaviorState.Success);

        [Test, Description("Merge should return succeed when the percent of succeeds is over a set amount (51%)")]
        public void TestMergeSucceedRatioSucceeds()
        {
            var runner = new Merge("", 1, 1);
            runner.SucceedPercent = 51; //over 50% succeed should succeed

            runner.AddChild(TestingResources.GetRunRunSuccess()); //publish running, success = 1/1
            runner.AddChild(TestingResources.GetRunRunSuccess()); //publish running, success = 2/2
            runner.AddChild(TestingResources.GetRunRunFail());    //publish running, success = 2/3
                                                                  //publish success, success = 2/3

            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Success);

            runner.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }

        [Test, Description("Merge should return fail when the percent of succeeds is under a set amount (40%)")]
        public void TestMergeFailRatioFails()
        {
            var runner = new Merge("", 1, 1);
            runner.SucceedPercent = 40; //under 40% succeed should fail

            runner.AddChild(TestingResources.GetRunRunSuccess()); //success = 1/1
            runner.AddChild(TestingResources.GetRunRunFail()); //success = 1/2
            runner.AddChild(TestingResources.GetRunRunFail()); //publish fail, success = 1/3
            var actual = new List<BehaviorState>();

            var expected = new List<BehaviorState>();
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Running);
            expected.Add(BehaviorState.Fail);

            runner.Start().Subscribe((x) => actual.Add(x));

            Assert.AreEqual(expected, actual);
        }
    }
}
