using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UniRx.Diagnostics;

namespace Assets.Visual_Behavior_Tree.Tests
{
    public class TestingResources
    {
        public static IObservable<BehaviorState> Fail = Observable.Return(BehaviorState.Fail);
        public static IObservable<BehaviorState> Running = Observable.Return(BehaviorState.Running);
        public static IObservable<BehaviorState> Success = Observable.Return(BehaviorState.Success);

        internal sealed class RunRunFail : BehaviorTreeElement
        {
            public RunRunFail(string name, int depth, int id)
            : base(name, depth, id) { }

            public override IObservable<BehaviorState> Start()
            {
                return Observable.Concat(Running, Running, Fail);
            }
        }

        internal sealed class RunRunSuccess : BehaviorTreeElement
        {
            public RunRunSuccess(string name, int depth, int id)
            : base(name, depth, id) { }

            public override IObservable<BehaviorState> Start()
            {
                return Observable.Concat(Running, Running, Success);
            }
        }

        internal sealed class RunCustom : BehaviorTreeElement
        {
            public RunCustom(string name, int depth, int id)
            : base(name, depth, id) { }

            public IObservable<BehaviorState> CustomStream;

            public override IObservable<BehaviorState> Start()
            {
                return CustomStream;
            }
        }

        internal static RunRunFail GetRunRunFail()
        {
            return new RunRunFail("",1,1);
        }

        internal static RunRunSuccess GetRunRunSuccess()
        {
            return new RunRunSuccess("", 1, 1);
        }

        internal static RunCustom GetRunCustom(IObservable<BehaviorState> customStream)
        {
            var custom =  new RunCustom("", 1, 1);
            custom.CustomStream = customStream;

            return custom;
        }

    }
}
