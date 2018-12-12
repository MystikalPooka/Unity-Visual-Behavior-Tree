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
    [Description("Continually runs children in sequence until a child fails. Succeeds if no children fail. Fails if any child fails.")]
    public class Sequencer : BehaviorComponent
    {
        public Sequencer(string name, int depth, int id) 
            : base(name, depth, id)
        {
        }

        public override IObservable<BehaviorState> Tick()
        {
            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in " + this.Name);
                return Observable.Return(BehaviorState.Fail);
            }

            var source = from child in Children.ToObservable()
                         select child as BehaviorTreeElement;

            return source.Select(child => child.Tick())
                        .Concat() //Sequentially subscribe
                        .TakeWhile(childState => childState != BehaviorState.Fail) //take until "fail". then stop and publish "OnComplete"

                        .Publish()
                        .RefCount(); //automatically begin generating upon receiving first subscription, 
                                     //and automatically dispose upon last dispose
        }
    }
}
}