 using Assets.Visual_Behavior_Tree.Scripts;
using System;
using System.Collections;
using System.ComponentModel;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    /// <summary>
    /// Runs until a child returns in a fail state
    /// </summary>
    [System.Serializable]
    [Description("runs children in sequence until a child fails. Succeeds if no children fail. Fails if any child fails.")]
    public class Sequencer : BehaviorComponent
    {
        public Sequencer(string name, int depth, int id) 
            : base(name, depth, id)
        {}

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

            return sourceConcat.Publish(src =>
                src.Any(e => e == BehaviorState.Fail) //true if any fail, false if none fail
                   .Select(e => e ? BehaviorState.Fail : BehaviorState.Success) //fail if any fail, succeed if all succeed
                   .Publish(srcLast =>
                            src.Where(e => e == BehaviorState.Success) //success should keep running
                               .Select(e => BehaviorState.Running) //so return running to show this
                               .TakeUntil(srcLast).Merge(srcLast)));
        }
    }
}