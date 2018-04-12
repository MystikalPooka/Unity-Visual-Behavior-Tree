using System;
using System.Collections.Generic;
using Assets.Editor.BehaviorTreeViewEditor;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Behavior_Logger;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    public class TreeDebuggerWindow : EditorWindow
    {
        private static Vector2 BehaviorLogRectSize = new Vector2(120, 120);

        private CompositeDisposable Disposables = new CompositeDisposable();

        public StringReactiveProperty ManagerName = new StringReactiveProperty();

        /// <summary>
        /// Behavior IDs to track current behaviors being watched.
        /// </summary>
        public Dictionary<int, BehaviorLogDrawer> LogDrawers = new Dictionary<int, BehaviorLogDrawer>();

        Rect TopToolbarRect
        {
            get { return new Rect(10f, 5f, position.width - 40f, 30f); }
        }

        Rect TreeDrawArea

        {
            get { return new Rect(10f, 40f, position.width - 40f, position.height - 40f); }
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

            TreeLogArea(EditorGUILayout.GetControlRect());
        }

        private bool Initialized = false;
        private void Initialize()
        {
            Initialized = true;
        }

        private void ReloadManagerTree()
        {
            //needs to set up the tree structure for all children
        }

        GenericMenu ManagerSelectMenu = new GenericMenu();
        private void TopToolbar(Rect rect)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(5);
                string dropDownName = "Manager To Debug";
                if (ManagerName.Value != "") dropDownName = ManagerName.Value;

                if (EditorGUILayout.DropdownButton(new GUIContent(dropDownName, "Change visible debugger"), FocusType.Passive, GUILayout.Height(30)))
                {
                    ManagerSelectMenu.CreateManagerMenu(OnManagerSelected);
                    ManagerSelectMenu.ShowAsContext();
                }
            }
        }

        private void OnManagerSelected(object name)
        {
            ManagerName.SetValueAndForceNotify((string)name);
            LogDrawers.Clear();
            LogDrawers = new Dictionary<int, BehaviorLogDrawer>();
        }

        private Dictionary<int, int> rowTotalDrawn = new Dictionary<int, int>();
        private void TreeLogArea(Rect rect)
        {
            ObservableBehaviorLogger.Listener
                .Where(x => x.LoggerName == ManagerName.Value)
                .Do(x =>
                {
                    var depth = x.State.Depth;
                    if(!rowTotalDrawn.ContainsKey(depth))
                    {
                        rowTotalDrawn.Add(depth,0);
                    }
                    else
                    {
                        ++rowTotalDrawn[depth];
                    }
                    //Keep the rects the same as when they were first created
                    if (!LogDrawers.ContainsKey(x.BehaviorID))
                    {
                        float rectX = ((BehaviorLogRectSize.x/2) * rowTotalDrawn[depth]);
                        float rectY = BehaviorLogRectSize.y * (depth < 0 ? 0 : depth*1.1f);
                        var position = new Vector2(rectX, rectY);
                        var drawRect = new Rect(position, BehaviorLogRectSize);

                        LogDrawers.Add(x.BehaviorID, new BehaviorLogDrawer(x.LoggerName, x.BehaviorID, drawRect));
                    }
                })
                .Subscribe();

            foreach(var log in LogDrawers)
            {
                log.Value.DrawBehaviorLogEntry();
            }
        }

        private void OnDestroy()
        {
            Disposables.Clear();
        }
    }
}
