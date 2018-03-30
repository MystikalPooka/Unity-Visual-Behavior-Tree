using System;
using System.Collections;
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
            Debug.Log(Name + "node starting... waiting...");
            yield return delayStart;
            Debug.Log("BEHAVIOR NODE DOIN THE THANG!");
            CurrentState = (BehaviorState.Success);
            yield return null;
        }
    }
}
