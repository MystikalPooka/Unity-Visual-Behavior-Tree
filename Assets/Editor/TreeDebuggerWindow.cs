using System;
using Assets.Editor.BehaviorTreeViewEditor;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Behavior_Logger;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class TreeDebuggerWindow : EditorWindow, System.IObserver<BehaviorLogEntry>
    {
        private static Color GetBehaviorStateColor(int state)
        {
            switch (state)
            {
                case (int)BehaviorState.Fail:
                    return Color.red;
                case (int)BehaviorState.Running:
                    return Color.blue;
                case (int)BehaviorState.Success:
                    return new Color(0.1f, 0.9f, 0.2f);
                case (int)BehaviorState.Null:
                    return Color.grey;
                default:
                    return Color.black;
            }
        }

        public string DebugMessages = "debug?";
        public string ManagerName = "";

        Rect TopToolbarRect
        {
            get { return new Rect(10f, 5f, position.width - 40f, 30f); }
        }

        [MenuItem("Behavior Tree/Debugger")]
        public static TreeDebuggerWindow ShowWindow()
        {
            var window = GetWindow<TreeDebuggerWindow>();
            window.Focus();
            window.Repaint();
            return window;
        }

        private void OnGUI()
        {
            if (!Initialized) Initialize();
            TopToolbar(TopToolbarRect);
            EditorGUILayout.TextArea(DebugMessages);
        }

        GenericMenu ManagerSelectMenu = new GenericMenu();
        private void TopToolbar(Rect rect)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                string dropDownName = "Manager To Debug";
                if (ManagerName != "") dropDownName = ManagerName;

                if (EditorGUILayout.DropdownButton(new GUIContent(dropDownName, "Change visible debugger"), FocusType.Passive, GUILayout.Height(30)))
                {
                    ManagerSelectMenu.CreateManagerMenu(OnManagerSelected);
                    ManagerSelectMenu.ShowAsContext();
                }
            }
        }

        private void OnManagerSelected(object name)
        {
            ManagerName = (string)name;
        }

        private bool Initialized = false;
        private void Initialize()
        {
            ObservableBehaviorLogger.Listener                
                    .Where(x => x.LoggerName.Contains(ManagerName))
                    .Do(x =>
                    {
                        
                    })
                    
                    .Subscribe();

            Initialized = true;
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(BehaviorLogEntry value)
        {
            throw new NotImplementedException();
        }
    }
}
