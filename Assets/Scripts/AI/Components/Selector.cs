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
    [Description("Runs children in order. Succeeds on first child that succeeds.")]
    public class Selector : BehaviorComponent
    {
        public Selector(string name, int depth, int id) 
            : base(name, depth, id)
        { }

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            base.Tick().ToObservable().Subscribe(xb => Debug.Log("Subscribed to Selector at start (base.tick()"));
            
            CurrentState = (BehaviorState.Running);
            foreach (BehaviorTreeElement behavior in Children)
            {
                if (CurrentState != BehaviorState.Running) yield break;

                yield return behavior.Tick().ToObservable().Subscribe(_ =>
                {
                    if (behavior.CurrentState == BehaviorState.Success)
                    {
                        this.CurrentState = behavior.CurrentState;
                        return;
                    }
                });

            }
            //if it gets here, it went through all subbehaviors and had no successes
            CurrentState = BehaviorState.Fail;
            yield break;
        }
    } 
}