using Assets.Scripts.AI.Behavior_Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Editor
{
    public class TreeDebuggerSink : IObserver<BehaviorLogEntry>
    {
        private TreeDebuggerWindow Window;
        public TreeDebuggerSink(TreeDebuggerWindow window)
        {
            Window = window;
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
            Window.DebugMessages += "New state: " + value.NewState + "\n";
        }
    }
}
