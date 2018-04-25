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

        public RectOffset TotalOffset;

        public Dictionary<int, BehaviorLogDrawer> ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();

        /// <summary>
        /// Custom Styling options for this behavior log drawer
        /// </summary>
        GUIStyle Style = new GUIStyle();

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

            SetChildrenRects();
            Initialized = true;
        }

        private bool enabled = true;

        public void DrawBehaviorWithAllChildren()
        {
            DrawChildrenAndBoundingBox();
            DrawBehaviorLogEntry();
        }

        protected void DrawChildrenAndBoundingBox()
        {
            Rect surroundingBox = GetSurroundingRect();
            this.TotalOffset.left = (int)surroundingBox.width / 2;
            this.TotalOffset.right = (int)surroundingBox.width / 2;
            CustomGUI.DrawQuad(surroundingBox, new Color(0.4f,0.4f,0.4f,0.4f));

            GUI.BeginGroup(surroundingBox);
            DrawChildren();
            GUI.EndGroup();
        }

        protected Rect GetSurroundingRect()
        {
            float bbWidth = Style.margin.right;
            foreach(var child in ChildrenDrawers.Values)
            {
                bbWidth += (DrawHere.width + child.TotalOffset.left);
            }
                                             
            float bbPosY = DrawHere.height + Style.margin.top;
            float bbPosX = 0f;

            return new Rect(bbPosX, bbPosY, bbWidth, DrawHere.height + Style.margin.vertical);
        }

        protected void DrawChildren()
        {
            SetChildrenRects();
            foreach (var child in ChildrenDrawers.Values)
            {
                child.DrawBehaviorLogEntry();
            }
        }

        protected void SetChildrenRects()
        {
            if (!Initialized)
            {
                int numDrawn = 0;

                BehaviorLogDrawer prevChild = null;
                foreach (var child in ChildrenDrawers.Values)
                {
                    Rect childRect = new Rect(numDrawn * (DrawHere.width + Style.margin.left) + Style.margin.left,
                                            Style.margin.top,
                                            DrawHere.width,
                                            DrawHere.height);
                    if (prevChild != null)
                        child.DrawHere.x += prevChild.TotalOffset.right;
                    child.DrawHere = childRect;
                    ++numDrawn;
                    prevChild = child;
                }
            }
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
                var totalPosition = new Rect(DrawHere.x + TotalOffset.left, DrawHere.y + TotalOffset.top,
                                             DrawHere.width, DrawHere.height);
                CustomGUI.DrawQuad(totalPosition, Entry.State.CurrentState.GetBehaviorStateColor());
            }
        }
    }
}
