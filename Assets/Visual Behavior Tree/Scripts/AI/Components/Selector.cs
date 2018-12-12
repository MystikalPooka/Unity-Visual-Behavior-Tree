using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    /// <summary>
    /// Ticks all sub behaviors until a behavior returns success.
    /// Returns and is in success state if a child was successful, otherwise returns in fail state
    /// </summary>
    [Serializable]
    [Description("Runs children in order. Succeeds on first child that succeeds. Fails if no children succeed.")]
    public class Selector : BehaviorComponent
    {
        public Selector(string name, int depth, int id) 
            : base(name, depth, id)
        { }

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
                        .TakeWhile(childState => childState != BehaviorState.Success) //take until "succeed". then stop and publish "OnComplete"
                        
                        .Publish()
                        .RefCount(); //automatically begin generating upon receiving first subscription, 
                                        //and automatically dispose upon last dispose
        }
    } 
}