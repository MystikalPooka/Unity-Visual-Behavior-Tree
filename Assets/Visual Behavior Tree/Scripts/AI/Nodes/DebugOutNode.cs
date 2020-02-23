using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Nodes
{
    [Serializable]
    public class DebugOutNode : BehaviorNode
    {
        public string debugOutText;

        public DebugOutNode(string name, int depth, int id) 
            : base(name, depth, id)
        { }


        public override IObservable<BehaviorState> Start()
        {
            //base.Initialize();

            CurrentState = BehaviorState.Null; //Forces an update on the state between succeeds.
            //DO STUFF HERE
            Debug.Log(debugOutText);
            //DO MORE STUFF?!

            return Observable.Return(BehaviorState.Success);
        }
    }
}
