using Assets.Visual_Behavior_Tree.Scripts;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    /// <summary>
    /// runs all sub behaviors until a behavior returns success.
    /// Returns success if a child was successful, otherwise returns fail
    /// </summary>
    [Serializable]
    [Description("Runs children in order. Succeeds on first child that succeeds. Fails if no children succeed.")]
    public class Selector : BehaviorComponent
    {
        public Selector(string name, int depth, int id) 
            : base(name, depth, id)
        { }

        public override IObservable<BehaviorState> Start()
        {
            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in " + this.Name);
                return Observable.Return(BehaviorState.Fail);
            }

            var sourceConcat = Children.ToObservable()
                                       .Select(child => 
                                            ((BehaviorTreeElement)child).Start()
                                                                        .Where(st => st != BehaviorState.Running))
                                       .Concat(); //Sequentially run children and select the final value (should be fail/succeed/error)

            //
            return sourceConcat.Publish(src =>
                src.Any(e => e == BehaviorState.Success) //true if any succeed, false if none succeed
                   .Select(e => e ? BehaviorState.Success : BehaviorState.Fail) //Success if any succeed, fail if all fail
                   .Publish(srcLast =>
                            src.Where(e => e == BehaviorState.Fail)
                               .Select(e => BehaviorState.Running)
                               .TakeUntil(srcLast).Merge(srcLast)));
        }
    } 
}