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

            var source = from child in Children.ToObservable()
                         select child as BehaviorTreeElement;

            var sourceConcat = 
                              source.Select(child => child.Start().Where(state => state != BehaviorState.Running))
                                    .Concat();

            return Observable.CreateSafe((IObserver<BehaviorState> observer) =>
            {
                var childrenDisposable = sourceConcat.Do(st =>
                {
                    if (st == BehaviorState.Fail)
                    {
                        observer.OnNext(BehaviorState.Fail);
                        observer.OnCompleted();
                    }
                    else observer.OnNext(BehaviorState.Running);
                })
                .Do(st =>
                {
                    if (st == BehaviorState.Success)
                        observer.OnNext(BehaviorState.Success);
                    else
                        observer.OnNext(BehaviorState.Fail);
                })
                .Publish().RefCount();

                return childrenDisposable.Subscribe();
            })
            .Publish()
            .RefCount();
        }
    }
}