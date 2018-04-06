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


        Rect TopToolbarRect
        {
            get { return new Rect(20f, 30f, position.width - 40f, 30f); }
        }

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


            ObservableBehaviorLogger.Listener
            .Where(x => x.LoggerName != "breakfast")
            //.Select(x => x.NewState)
            .Subscribe(x =>
            {
            });

            TopToolbar(TopToolbarRect);
            EditorGUILayout.TextArea(DebugMessages);
        }

        GenericMenu ManagerSelectMenu;
        private void TopToolbar(Rect rect)
        {
            GUILayout.BeginArea(rect);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (EditorGUILayout.DropdownButton(new GUIContent("Manager To Debug"), FocusType.Passive))
                {
                    ManagerSelectMenu.CreateManagerMenu(OnManagerSelected);
                    ManagerSelectMenu.ShowAsContext();
                }
            }
            GUILayout.EndArea();
        }

        private void OnManagerSelected()
        {

        }

        private bool Initialized = false;
        private void Initialize()
        {
            ManagerSelectMenu = new GenericMenu();
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
