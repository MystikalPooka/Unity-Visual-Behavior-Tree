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

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            base.Tick(delayStart).ToObservable().Subscribe(xb => Debug.Log("Subscribed to ParallelRunner at start (base.tick()"));

            CurrentState = BehaviorState.Null;
            if (Children == null) yield return null;
            if (Children.Count == 0) yield return null;
            var behavior = Children[0] as BehaviorTreeElement;

            yield return behavior.Tick().ToObservable().Subscribe(_ =>
            {
                Debug.Log("Inverting " + behavior);
                switch (behavior.CurrentState)
                {
                    case BehaviorState.Fail:
                        this.CurrentState = BehaviorState.Success;
                        break;
                    case BehaviorState.Success:
                        CurrentState = BehaviorState.Fail;
                        break;
                    case BehaviorState.Running:
                        this.CurrentState = BehaviorState.Running;
                        break;
                    default:
                        Debug.LogError("Something went wrong in an inverter.");
                        break;
                }
            }

                
                );

        }
    }
}