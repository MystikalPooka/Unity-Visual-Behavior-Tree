using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using Assets.Scripts.AI.Nodes;
using NUnit.Framework;
using UniRx;

namespace Tests.Visual_Behavior_Tree.AI.Components
{

    class SelectorTests
    {
        static IObservable<BehaviorState> Fail = Observable.Return(BehaviorState.Fail);
        static IObservable<BehaviorState> Success = Observable.Return(BehaviorState.Success);
        static IObservable<BehaviorState> Running = Observable.Return(BehaviorState.Running);

        internal class RunRunFail : BehaviorNode
        {
            public RunRunFail(string name, int depth, int id)
                : base(name, depth, id)
            { }
            public override IObservable<BehaviorState> Start()
            {
                return Observable.Concat(Running, Running, Fail);
            }
        }

        internal class RunRunSuccess : BehaviorNode
        {
            public RunRunSuccess(string name, int depth, int id)
                : base(name, depth, id)
            { }
            public override IObservable<BehaviorState> Start()
            {
                return Observable.Concat(Running, Running, Success);
            }
        }


        [Test]
        public void TestSelectorIsSequential()
        {
            var selector = new Selector("",0,1);
            selector.AddChild(new RunRunFail("", 0, 0));
            selector.AddChild(new RunRunSuccess("", 0, 0));

            Assert.AreEqual(selector.Start(), Observable.Concat(Running, Success));
        }
    }
}
