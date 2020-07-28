using Assets.Visual_Behavior_Tree.Editor.NodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor
{
    class BehaviorDebuggerWindow : BehaviorNodeEditorWindow
    {
        [MenuItem("Window/uVBT/Node Debugger")]
        private static void OpenWindow()
        {
            BehaviorDebuggerWindow window = GetWindow<BehaviorDebuggerWindow>();
            window.titleContent = new GUIContent("Behavior Debugger");
        }

        protected override void OnGUI()
        {
            DrawToolbar();

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();

            DrawConnectionLine(Event.current);

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }
    }
}
