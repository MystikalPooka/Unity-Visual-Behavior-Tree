using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UniRx;
using UnityEngine;
using Assets.Visual_Behavior_Tree.Scripts;

namespace Assets.Scripts.AI.Components
{
    [Serializable]
    [Description("Runs all children at same time. Fails early if NumFailures are >0 and total children failed reach that number. Succeeds otherwise.")]
    public class Merge : BehaviorComponent
    {
        [Range(0, 100)]
        [SerializeField]
        public float SucceedFailPercentForSucceess = 51;

        public Merge(string name, int depth, int id)
            : base(name, depth, id) { }

        public override IObservable<BehaviorState> Start()
        {
            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in parallel runner");
                return Observable.Return(BehaviorState.Fail);
            }

            var source = Children.ToObservable()
                                 .SelectMany(child =>
                                           ((BehaviorTreeElement)child).Start()
                                                                       .Where(st => st != BehaviorState.Running));


            //should take all streams and publish running until last stream finishes...
            //once last stream finishes, must emit "success" or "fail" based on set ratio...
            return source.Publish(src =>
                src.Aggregate(new { total = 0, succeeded = 0 }, (acc, childResult) =>
                {
                    return childResult == BehaviorState.Success ?
                        new { total = acc.total + 1, succeeded = acc.succeeded + 1 } :
                        new { total = acc.total + 1, acc.succeeded };
                })
                .Select(a => 100 * ((float)a.succeeded / a.total))
                .Select(ratio => ratio >= SucceedFailPercentForSucceess ?
                                    BehaviorState.Success : BehaviorState.Fail)
                .Publish(srcLast =>
                            src.Select(s => BehaviorState.Running)
                               .TakeUntil(srcLast).Merge(srcLast)));
        }
    
    }
}
