using Assets.Scripts.AI.Behavior_Logger;
using UniRx;
using UnityEngine;
using System;
using UnityEditor;
using Assets.Editor.BehaviorTreeViewEditor;
using System.Collections.Generic;

namespace Assets.Editor
{
    public class BehaviorLogDrawer
    {
        private int BehaviorID;
        private string ManagerName = "Wolf Pancakes Taste Like Fur";
        public Rect DrawHere;

        public Dictionary<int, BehaviorLogDrawer> ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();

        public 

        /// <summary>
        /// Custom Styling options for this behavior log drawer
        /// </summary>
        GUIStyle Style = new GUIStyle();

        /// <summary>
        /// Padding added to the styling options for the entire log drawer
        /// </summary>
        public RectOffset TotalOffset = new RectOffset(5, 5, 5, 5);
        private Vector2 subElementMargins = new Vector2(2, EditorGUIUtility.singleLineHeight+ 2);

        public BehaviorLogEntry Entry { get; private set; }
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
                    Entry = x;
                });

            foreach (var child in Entry.State.Children)
            {
                if(!ChildrenDrawers.ContainsKey(child.ID))
                {
                    ChildrenDrawers.Add(child.ID, new BehaviorLogDrawer(ManagerName, child.ID, new Rect(0, 0, 0, 0)));
                }
            }
        }

        private bool enabled = true;

        public void DrawBehaviorWithAllChildren()
        {

        }

        protected void DrawChildrenAndGetParentRect()
        {
            Rect surroundingBox = SetChildrenAndGetSurroundingRect();
            CustomGUI.DrawQuad(surroundingBox, new Color(0.4f,0.4f,0.4f,0.4f));
            GUI.BeginGroup(surroundingBox);
            foreach (var child in ChildrenDrawers.Values)
            {
                child.DrawBehaviorLogEntry();
            }
            GUI.EndGroup();
        }

        protected Rect SetChildrenAndGetSurroundingRect()
        {
            int numDrawn = 0;
            Rect surroundingBox = new Rect(0, 0, 0, 0);
            foreach(var child in ChildrenDrawers.Values)
            {

                ++numDrawn;
            }
            return surroundingBox;
        }

        protected void DrawBehaviorLogEntry()
        {
            if (!Initialized)
            {
                Initialize();
                Initialized = true;
            }

            LogStream.Subscribe();
            
            if(Entry != null)
            {
                var drawWithOffset = new Rect(TotalOffset.left,
                                              TotalOffset.top,
                                              DrawHere.width - 2, DrawHere.height - 2);
                Debug.Log(Entry.State.Name + ": " + TotalOffset);
                var DrawHereWithOffset = new Rect(DrawHere.x+ TotalOffset.left, DrawHere.y + TotalOffset.top,
                                                  DrawHere.width, DrawHere.height);

                GUI.BeginGroup(DrawHereWithOffset);
                    CustomGUI.DrawQuad(new Rect(0,0,120,120), Entry.State.CurrentState.GetBehaviorStateColor());
                    if(Entry.State.Parent != null)
                    {
                        GUI.Label(new Rect(subElementMargins.x, 1,
                                           drawWithOffset.width, drawWithOffset.height),
                                           new GUIContent(Entry.State.Parent.Name.ToString()));
                    }
                    GUI.Label(new Rect(subElementMargins.x, subElementMargins.y,
                       drawWithOffset.width, drawWithOffset.height),
                       new GUIContent(Entry.State.Name.ToString()));
                GUI.EndGroup();
            }
        }
    }
}
