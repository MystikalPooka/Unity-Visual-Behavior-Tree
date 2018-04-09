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
        public string DebugMessages = "debug?";
        public string ManagerName = "";

        /// <summary>
        /// key: Behavior ID to track current behaviors being watched.
        /// value: rect to draw this log "inspector"
        /// </summary>
        public Dictionary<int, Rect> LogInspectorDict = new Dictionary<int, Rect>();

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
            
            TreeLogArea(new Rect());
        }

        private bool Initialized = false;
        private void Initialize()
        {
            ObservableBehaviorLogger.Listener
                    .Where(x => x.LoggerName.Contains(ManagerName))
                    .Do(x =>
                    {
                        if (!LogInspectorDict.ContainsKey(x.BehaviorID))
                        {
                            LogInspectorDict.Add(x.BehaviorID, new Rect());
                        }
                    })
                    .Subscribe();

            Initialized = true;
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

        private void TreeLogArea(Rect rect)
        {
            var style = EditorStyles.objectField;
            style.stretchWidth = false;
            style.fixedWidth = EditorGUIUtility.fieldWidth + EditorGUIUtility.labelWidth + 4;

        }

        private void DrawBehaviorLog(BehaviorLogEntry log)
        {

        }
    }
}
