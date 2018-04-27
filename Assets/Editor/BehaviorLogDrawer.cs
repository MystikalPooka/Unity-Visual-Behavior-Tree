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
        private Rect DrawHere;

        public Vector2 BoxSize;
        public RectOffset TotalOffset;

        protected bool isDrawing = false;

        /// <summary>
        /// Depth of this drawer (taken from entry) Default: 0
        /// </summary>
        private int DrawDepth = 0;

        private IntReactiveProperty NumChildren;

        private Dictionary<int, BehaviorLogDrawer> ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();

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
            ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();
            NumChildren = new IntReactiveProperty(0);
            LogStream =  ObservableBehaviorLogger.Listener
                .Where(x =>
                        x.BehaviorID == BehaviorID &&
                        x.LoggerName == ManagerName)
                .Do(x =>
                {
                    Entry = x;
                    if(Entry.State.HasChildren)
                    {
                        foreach (var child in Entry.State.Children)
                        {
                            if (!ChildrenDrawers.ContainsKey(child.ID))
                            {
                                ChildrenDrawers.Add(child.ID, new BehaviorLogDrawer(ManagerName, child.ID, BoxSize, Style)
                                {
                                    DrawDepth = child.Depth
                                });
                            }
                        }
                        NumChildren.SetValueAndForceNotify(Entry.State.Children.Count);
                    }
                });
            
            NumChildren.ObserveEveryValueChanged(x => x.Value)
                .Do(x =>
                {
                    OnNumChildrenChanged();
                }).Subscribe();

            Initialized = true;
        }

        private Rect SurroundingBoxRect;
        private void OnNumChildrenChanged()
        {
            SurroundingBoxRect = GetSurroundingRect();
        }

        protected void SetChildrenRects()
        {
            if(ChildrenDrawers.Values.Count != 0)
            {
                int numDrawn = 0;

                BehaviorLogDrawer prevChild = null;
                foreach (var child in ChildrenDrawers.Values)
                {
                    var totalX = numDrawn * (BoxSize.x);
                    Rect childRect = new Rect(totalX,
                                            Style.margin.top,
                                            BoxSize.x,
                                            BoxSize.y);

                    child.DrawHere.Set(childRect.x,childRect.y,
                                       childRect.width,childRect.height);
                    if (prevChild != null)
                        child.DrawHere.x += prevChild.TotalOffset.right;
                    ++numDrawn;
                    prevChild = child;
                }
            }
        }

        public void DrawBehaviorWithAllChildren()
        {
            LogStream.Subscribe();
            if (Entry == null)
            {
                return;
            }
            else if (Entry.State.HasChildren)
            {
                SetChildrenRects();
                DrawChildrenAndBoundingBox();   
            }

            DrawBehaviorLogEntry();

        }

        private void DrawChildrenAndBoundingBox()
        {
            this.TotalOffset.left = (int)(SurroundingBoxRect.width / 2);
            this.TotalOffset.right = (int)(SurroundingBoxRect.width / 2);
            CustomGUI.DrawQuad(SurroundingBoxRect, new Color(0.3f, 0.3f, 0.3f, 0.2f));

            DrawChildren();
        }

        private Rect GetSurroundingRect()
        {
            float bbWidth = Style.margin.right;
            foreach(var child in ChildrenDrawers.Values)
            {
                bbWidth += (BoxSize.x + child.TotalOffset.left);
            }
                                             
            float bbPosY = (DrawDepth+1) * (BoxSize.x + Style.margin.bottom) + Style.margin.top;

            return new Rect(0f, bbPosY, bbWidth, BoxSize.y + Style.margin.vertical);
        }

        private void DrawChildren()
        {
            foreach (var child in ChildrenDrawers.Values)
            {
                child.DrawBehaviorWithAllChildren();
                      
            }
        }

        protected void DrawBehaviorLogEntry()
        {
            if (!Initialized)
            {
                Initialize();
            }

            if(Entry != null)
            {
                var totalDepthY = DrawDepth * (BoxSize.y + Style.margin.top);
                var totalX = DrawHere.x + TotalOffset.left;
                var totalPosition = new Rect(totalX, totalDepthY,
                                             BoxSize.x, BoxSize.y);

                Debug.Log(Entry.State.Name + " totalPosition = " + totalPosition);
                Debug.LogWarning(Entry.State.Name + " DrawDepth: " + DrawDepth);
                CustomGUI.DrawQuad(totalPosition, Entry.State.CurrentState.GetBehaviorStateColor());

                GUI.BeginGroup(totalPosition);

                GUI.Label(new Rect(0, 0, 120, 20), new GUIContent("Depth: " + Entry.State.Depth));
                GUI.Label(new Rect(0, 10, 120, 20), new GUIContent("Draw Depth: " + DrawDepth));
                if (Entry.State.Parent != null) GUI.Label(new Rect(0,20,120,30), new GUIContent(Entry.State.Parent.Name));
                GUI.Label(new Rect(0,35,120,30), new GUIContent(Entry.State.Name));
                GUI.EndGroup();
            }
        }
    }
}
