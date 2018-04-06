using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.AI.Behavior_Logger
{
    public class BehaviorUnityDebugSink : IObserver<BehaviorLogEntry>
    {
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
            //do nothing yet
        }
    }
}
