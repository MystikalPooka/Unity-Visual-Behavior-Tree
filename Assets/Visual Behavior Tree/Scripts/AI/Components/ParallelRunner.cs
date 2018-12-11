using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    [Serializable]
    [Description("Runs all children at same time. Fails early if NumFailures are >0 and total children failed reach that number. Succeeds otherwise.")]
    public class ParallelRunner : BehaviorComponent
    {
        /// <summary>
        /// Number of times the children return fail before the parallel runner returns in a fail state.
        /// 0 means ignore number of failures.
        /// 0 for both succeed and fail means run this once
        /// </summary>
        [SerializeField] public int NumberOfFailuresBeforeFail = 0;
        protected IntReactiveProperty NumberOfFailures = new IntReactiveProperty(0);

        /// <summary>
        /// Number of times the children return success before the parallel runner returns in a success state.
        /// 0 means ignore number of sucesses.
        /// 0 for both succeed and fail means this will only run once
        /// </summary>
        [SerializeField] public int NumberOfSuccessBeforeSucceed = 0;
        protected IntReactiveProperty NumberOfSuccesses = new IntReactiveProperty(0);

        public ParallelRunner(string name, int depth, int id)
            : base(name, depth, id) { }

        public override IObservable<BehaviorState> Tick()
        {
            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in parallel runner");
                return Observable.Return(BehaviorState.Fail);
            }

            var allChildren = Children.ToObservable()
                                      .SelectMany(child => ((BehaviorTreeElement)child).Tick())
                                      .Do(state => {
                                          if (state == BehaviorState.Fail)
                                              NumberOfFailures.SetValueAndForceNotify(NumberOfFailures.Value + 1);
                                          if (state == BehaviorState.Success)
                                              NumberOfSuccesses.SetValueAndForceNotify(NumberOfSuccesses.Value + 1);
                                      })
                                      .TakeWhile(_ => NumberOfSuccesses.Value < NumberOfSuccessBeforeSucceed && 
                                                      NumberOfFailures.Value < NumberOfFailuresBeforeFail)
                                      .Publish()
                                      .RefCount();

            var failedChildren = allChildren.Where(state => state == BehaviorState.Fail)
                                            .Do(_ => NumberOfFailures.SetValueAndForceNotify(NumberOfFailures.Value + 1))
                                            .Subscribe();

            var succeedChildren = allChildren.Where(state => state == BehaviorState.Success)
                                             .Do(_ => NumberOfSuccesses.SetValueAndForceNotify(NumberOfFailures.Value + 1))
                                             .Subscribe();





        }
    }
}
