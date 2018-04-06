using Assets.Scripts.AI.Behavior_Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Editor
{
    public class BehaviorDebuggerWindowSink : IObserver<BehaviorLogEntry>
    {
        public BehaviorDebuggerWindowSink()
        {

        }

        public void OnCompleted()
        {
            //do nothing
        }

        public void OnError(Exception error)
        {
            //TODO: Change colors? Output exception somewhere
        }

        public void OnNext(BehaviorLogEntry value)
        {
            UnityEngine.Debug.Log("Tree Debugger onNext. New state: "+value.NewState);
        }
    }
}
