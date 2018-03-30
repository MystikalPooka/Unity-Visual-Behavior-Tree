using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    /// <summary>
    /// Runs until a child returns in a fail state
    /// </summary>
    [System.Serializable]
    public class Sequencer : BehaviorComponent
    {
        public Sequencer(string name, int depth, int id) 
            : base(name, depth, id)
        {
        }

        public override IEnumerator Tick(UnityEngine.WaitForSeconds delayStart = null)
        {
            yield return delayStart;
            CurrentState = BehaviorState.Running;
            foreach (BehaviorTreeElement behavior in Children)
            {
                yield return BehaviorTreeManager.StartCoroutine(behavior.Tick());

                if (behavior.CurrentState != BehaviorState.Success)
                {
                    this.CurrentState = behavior.CurrentState;

                    if (this.CurrentState == BehaviorState.Fail)
                    {
                        //This selector has completed, break out of the operation
                        yield break;
                    }
                }
            }
            //if it gets here, it went through all subbehaviors and had no fails
            CurrentState = BehaviorState.Success;
            yield break;
        }
    }
}