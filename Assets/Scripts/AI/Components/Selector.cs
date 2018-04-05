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

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            base.Tick().ToObservable().Subscribe();
            
            CurrentState = (BehaviorState.Running);
            foreach (BehaviorTreeElement behavior in Children)
            {
                if (CurrentState != BehaviorState.Running) yield break;

                yield return 
                    behavior.Tick().ToObservable()
                    .Do(_ =>
                    {
                        if (behavior.CurrentState == BehaviorState.Success)
                        {
                            CurrentState = BehaviorState.Success;
                            return;
                        }
                    })
                    .Subscribe()
                    .AddTo(Disposables);
            }
            //if it gets here, it went through all subbehaviors and had no successes
            if (CurrentState == BehaviorState.Running) CurrentState = BehaviorState.Fail;
            yield break;
        }
    } 
}