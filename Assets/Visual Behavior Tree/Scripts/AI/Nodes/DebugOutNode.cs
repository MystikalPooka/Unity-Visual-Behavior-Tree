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


        public override BehaviorState Tick()
        {
            base.Initialize();

            CurrentState = BehaviorState.Running; //Forces an update on the state between succeeds.
            //DO STUFF HERE
            Debug.Log("Debug Out Node " + Name + " Doing stuff");
            //DOM MORE STUFF?!
            CurrentState = BehaviorState.Success;
            return CurrentState;
        }
    }
}
