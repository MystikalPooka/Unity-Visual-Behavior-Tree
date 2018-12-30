using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Decorators
{
    [System.Serializable]
    public class Inverter : BehaviorDecorator
    {
        public Inverter(string name, int depth, int id) 
            : base(name, depth, id)
        { }

        public override IObservable<BehaviorState> Start()
        {
            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in " + this.Name);
                return Observable.Return(BehaviorState.Fail);
            }

            var sourceConcat = 
                              Children.ToObservable().Select(child => ((BehaviorTreeElement)child).Start().Where(state => state != BehaviorState.Running))
                                    .Concat();

            return sourceConcat.Select(state => state == BehaviorState.Fail ? BehaviorState.Success : BehaviorState.Fail);
        }
    }
}