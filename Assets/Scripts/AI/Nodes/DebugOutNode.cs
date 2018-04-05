using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Nodes
{
    [Serializable]
    public class DebugOutNode : BehaviorNode
    {
        public DebugOutNode(string name, int depth, int id) 
            : base(name, depth, id)
        { }


        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            base.Tick();

            CurrentState = BehaviorState.Running; //Forces an update on the state between succeeds.
            Debug.Log("Debug Out Node " + Name + " Doing stuff");
            CurrentState = BehaviorState.Success;
            yield break;
        }
    }
}
