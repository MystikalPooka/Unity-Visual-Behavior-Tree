using Assets.Scripts.AI;
using Assets.Scripts.AI.Behavior_Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace Assets.Editor
{
    public class TreeDebuggerWindow : EditorWindow
    {
        private BehaviorDebuggerWindowSink DebugSink = new BehaviorDebuggerWindowSink();
        public static void ShowWindow()
        {
            var window = GetWindow<TreeDebuggerWindow>();
            window.DebugSink = new BehaviorDebuggerWindowSink();
        }


        private void OnGUI()
        {
            ObservableBehaviorLogger.Listener.Subscribe(DebugSink);
        }
    }
}
