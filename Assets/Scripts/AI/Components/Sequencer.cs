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
            base.Tick().ToObservable()
                //.Do(_ => BehaviorLogger.Log("Subscribed to Sequencer at start (base.tick()"))
                .Subscribe();

            yield return delayStart;
            CurrentState = (BehaviorState.Running);
            foreach (BehaviorTreeElement behavior in Children)
            {
                if (CurrentState != BehaviorState.Running) yield break;

                yield return behavior.Tick().ToObservable()
                    .Do(_ => 
                    {
                        if (behavior.CurrentState == BehaviorState.Fail)
                        {
                            CurrentState = BehaviorState.Fail;
                            return;
                        }
                    })
                    .Subscribe()
                    .AddTo(Disposables);
            }
            //if it gets here, it went through all subbehaviors and had no failures
            if (CurrentState == BehaviorState.Running) CurrentState = BehaviorState.Success;
            yield break;
        }
    }
}