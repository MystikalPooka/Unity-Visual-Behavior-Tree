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

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            yield return delayStart;
            CurrentState = (BehaviorState.Running);
            foreach (BehaviorTreeElement behavior in Children)
            {
                if (CurrentState != BehaviorState.Running) yield break;

                yield return behavior.Tick().ToObservable().Subscribe(_ =>
                {
                    if (behavior.CurrentState == BehaviorState.Fail)
                    {
                        this.CurrentState = BehaviorState.Fail;
                        return;
                    }
                }).AddTo(Disposables);
            }
            //if it gets here, it went through all subbehaviors and had no failures
            CurrentState = BehaviorState.Success;
            yield break;
        }
    }
}