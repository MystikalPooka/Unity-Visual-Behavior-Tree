using Assets.Scripts.AI.Behavior_Logger;
using UniRx;
using UnityEngine;
using System;
using UnityEditor;
using Assets.Editor.BehaviorTreeViewEditor;

namespace Assets.Editor
{
    public class BehaviorLogDrawer
    {
        private int BehaviorID;
        private string ManagerName = "Wolf Pancakes Taste Like Fur";
        private Rect DrawHere;
        private BehaviorLogEntry entry;
        private IObservable<BehaviorLogEntry> LogStream;

        public BehaviorLogDrawer(string loggerName, int ID, Rect drawRect)
        {
            BehaviorID = ID;
            ManagerName = loggerName;
            DrawHere = drawRect;

            Initialize();
        }

        private bool Initialized = false;
        public void Initialize()
        {
            LogStream =  ObservableBehaviorLogger.Listener
                .Where(x =>
                        x.BehaviorID == BehaviorID &&
                        x.LoggerName == ManagerName)
                .Do(x =>
                {
                    entry = x;
                });
        }

        public void DrawBehaviorLogEntry()
        {
            if (!Initialized) Initialize();

            LogStream.Subscribe();

            if(entry != null)
            {
                EditorGUI.DrawRect(DrawHere, entry.NewState.GetBehaviorStateColor());
                GUILayout.BeginArea(DrawHere);
                EditorGUILayout.LabelField(new GUIContent(entry.ToString()));
                //EditorGUILayout.LabelField(new GUIContent(entry.TickNumber.ToString()));
                GUILayout.EndArea();
            }

        }
    }
}
