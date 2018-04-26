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
        protected Rect DrawHere;

        public Vector2 BoxSize;
        public RectOffset TotalOffset;

        public bool isDrawing = false;

        /// <summary>
        /// Depth of this drawer (taken from entry) Default: 0
        /// </summary>
        public int DrawDepth = 0;

        public Dictionary<int, BehaviorLogDrawer> ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();

        /// <summary>
        /// Custom Styling options for this behavior log drawer
        /// </summary>
        public GUIStyle Style = new GUIStyle();

        public BehaviorLogEntry Entry { get; set; }
        private IObservable<BehaviorLogEntry> LogStream;

        public BehaviorLogDrawer(string loggerName, int ID, Vector2 boxSize, GUIStyle subStyle = null)
        {
            BehaviorID = ID;
            ManagerName = loggerName;
            BoxSize = boxSize;
            if (subStyle != null)
                Style = subStyle;
            else
            {
                Style = new GUIStyle();
                Style.margin = new RectOffset(15, 15, 30, 30);
            }
            DrawHere = new Rect(Style.margin.left, Style.margin.top, BoxSize.x, BoxSize.y);
            TotalOffset = Style.margin;

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
                    DrawDepth = x.State.Depth+1;
                    if (Entry.State.HasChildren)
                    {
                        ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();
                        foreach (var child in Entry.State.Children)
                        {
                            if (!ChildrenDrawers.ContainsKey(child.ID))
                            {
                                ChildrenDrawers.Add(child.ID, new BehaviorLogDrawer(ManagerName, child.ID, BoxSize, Style)
                                {
                                    Entry = x,
                                    DrawDepth = x.State.Depth + 2
                                });
                            }
                        }
                        if(!Initialized) SetChildrenRects();
                    }
                });


            Initialized = true;
        }

        protected void SetChildrenRects()
        {
            if(ChildrenDrawers.Values.Count != 0)
            {
                int numDrawn = 0;

                BehaviorLogDrawer prevChild = null;
                foreach (var child in ChildrenDrawers.Values)
                {
                    var totalDepthY = child.DrawDepth * (BoxSize.y + Style.margin.top);
                    Rect childRect = new Rect(numDrawn * (BoxSize.x + Style.margin.left) + Style.margin.left,
                                            totalDepthY,
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

        private bool enabled = true;

        public void DrawBehaviorWithAllChildren()
        {
            if(Entry.State.HasChildren)
            {
                DrawChildrenAndBoundingBox();
            }

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
                                             
            float bbPosY = (DrawDepth+1) * (BoxSize.x + Style.margin.vertical) + Style.margin.top;
            float bbPosX = 0f;

            return new Rect(bbPosX, bbPosY, bbWidth, DrawHere.height + Style.margin.vertical);
        }

        protected void DrawChildren()
        {
            SetChildrenRects();
            foreach (var child in ChildrenDrawers.Values)
            {
                if(!child.isDrawing)
                    child.DrawBehaviorLogEntry();
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
                GUI.BeginGroup(totalPosition);

                if (Entry.State.Parent != null) GUI.Label(new Rect(0,0,120,30), new GUIContent(Entry.State.Parent.Name));
                GUI.Label(new Rect(0,35,120,30), new GUIContent(Entry.State.Name));
                GUI.EndGroup();

                isDrawing = true;

            }
            else
            {
                isDrawing = false;
            }
        }
    }
}
