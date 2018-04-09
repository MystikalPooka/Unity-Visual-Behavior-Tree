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
        private static Vector2 BehaviorLogRectSize = new Vector2(25, 25);

        private CompositeDisposable Disposables = new CompositeDisposable();

        public StringReactiveProperty ManagerName = new StringReactiveProperty();

        /// <summary>
        /// key: Behavior ID to track current behaviors being watched.
        /// value: rect to draw this log "inspector"
        /// </summary>
        public Dictionary<int, Vector2> LogInspectorDict = new Dictionary<int, Vector2>();

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
            
            TreeLogArea(new Rect());
        }

        private bool Initialized = false;
        private void Initialize()
        {
            ManagerName
                .ObserveEveryValueChanged(x => x.Value)
                .Do(x =>
                {
                    Debug.Log("name changed! disposing..." + x);
                    LogInspectorDict.Clear();
                })
                .Subscribe(_ => { }, () => { Debug.Log("ManagerName complete"); })
                .AddTo(Disposables);

            Initialized = true;
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
        }

        private void TreeLogArea(Rect rect)
        {
            GUILayout.BeginArea(rect);
            ObservableBehaviorLogger.Listener
                .Where(x => x.LoggerName.Contains(ManagerName.Value))
                .Do(x =>
                {
                    //NEEDS TO ROUTE TO CORRECT BOX ON UPDATE
                    //Keep the rects the same as when they were first created
                    if (!LogInspectorDict.ContainsKey(x.BehaviorID))
                    {
                        Debug.Log("new key " + x.BehaviorID);
                        LogInspectorDict.Add(x.BehaviorID, new Vector2(5,5));
                    }
                    // TODO: Needs to draw itself AND children.
                    //DrawBehaviorLog(log);
                    
                })
                .Subscribe().AddTo(Disposables);

            GUILayout.EndArea();
        }

        private void DrawBehaviorLog(BehaviorLogEntry log)
        {
            var logRect = new Rect(LogInspectorDict[log.BehaviorID], BehaviorLogRectSize);
            Debug.Log("Attempting to draw rect... ");
            EditorGUI.DrawRect(logRect, log.NewState.GetBehaviorStateColor());
        }

        private void OnDestroy()
        {
            Disposables.Clear();
            Disposables.Dispose();
        }
    }
}
