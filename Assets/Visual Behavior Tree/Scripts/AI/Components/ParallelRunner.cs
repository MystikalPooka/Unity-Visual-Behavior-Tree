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
        [Range(0,100)]
        [SerializeField] public float SucceedFailRatioForSucceess = 51;

        public ParallelRunner(string name, int depth, int id)
            : base(name, depth, id) { }

        public override IObservable<BehaviorState> Start()
        {
            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in parallel runner");
                return Observable.Return(BehaviorState.Fail);
            }

            var source = Children.ToObservable()
                                      .SelectMany(child => ((BehaviorTreeElement)child).Start());

            //should take all streams and publish running until last stream finishes...
            //once last stream finishes, must emit "success" or "fail" based on set ratio...


            return source.Aggregate(new { total = 0, succeeded = 0 }, (acc, childResult) =>
             {
                 return childResult == BehaviorState.Success ?
                     new { total = acc.total + 1, succeeded = acc.succeeded + 1 } :
                     new { total = acc.total + 1, acc.succeeded };
             })
             .Select(e => 100 * (((float)e.succeeded) / e.total))
             .Select(ratio =>
             {
                 return ratio >= SucceedFailRatioForSucceess ? BehaviorState.Success : BehaviorState.Fail;
             });
        }
    }
}
