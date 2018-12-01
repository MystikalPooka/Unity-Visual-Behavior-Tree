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

        public Vector2 BoxSize;
        public RectOffset TotalOffset;

        protected BehaviorLogDrawer Parent = null;

        /// <summary>
        /// Depth of this drawer (taken from entry) Default: 0
        /// </summary>
        private int DrawDepth = 0;

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
            
            TotalOffset = Style.margin;
            Initialize();
        }

        private bool Initialized = false;
        public void Initialize()
        {
            ChildrenDrawers = new Dictionary<int, BehaviorLogDrawer>();
            LogStream = ObservableBehaviorLogger.Listener
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
                                float y = (child.Depth+1) * (BoxSize.y + Style.margin.top);
                                ChildrenDrawers.Add(child.ID, new BehaviorLogDrawer(ManagerName, child.ID, BoxSize, Style)
                                {
                                    Parent = this,
                                    DrawHere = new Rect(Style.margin.left, y, BoxSize.x, BoxSize.y)
                                });
                            }
                        }
                    }
                });

            Initialized = true;
        }

        public void DrawBehaviorWithAllChildren()
        {
            if (!Initialized)
            {
                Initialize();
            }
            //Draw Breadth First, Offset parent second
            LogStream.Subscribe();

            int offset = Style.margin.left;

            if(Entry == null)
            {
                return;
            }
            else if(Entry.State.HasChildren)
            {
               offset = DrawChildrenAndGetOffset() / 2;
            }
            var parent = Entry.State.Parent;
            if (parent != null)
            {
                if (parent.HasChildren)
                {
                    if (parent.Children.Count == 1)
                    {
                        offset = (int)BoxSize.x / 2;
                    }
                }
            }
            this.TotalOffset.left = offset;
            this.TotalOffset.right = offset;
            DrawBehaviorLogEntry();
        }

        private int DrawChildrenAndGetOffset()
        {
            var newOffset = 0;
            BehaviorLogDrawer prevChildDrawer = null;
            foreach (var child in ChildrenDrawers.Values)
            {
                if (prevChildDrawer == null)
                {
                    child.DrawHere.x = this.DrawHere.x;
                }
                else
                {
                    child.DrawHere.x = prevChildDrawer.DrawHere.x +
                                       BoxSize.x +
                                       prevChildDrawer.TotalOffset.right;
                    
                    newOffset += prevChildDrawer.TotalOffset.right;
                }
                
                newOffset += (int)BoxSize.x;
                prevChildDrawer = child;
                child.DrawBehaviorWithAllChildren();
            }
            return newOffset;
        }
        //FIXED

        public void DrawBehaviorLogEntry()
        {
            if(Entry != null)
            {
                var totalX = DrawHere.x + TotalOffset.left;
                var totalPosition = new Rect(totalX, DrawHere.y,
                                             BoxSize.x, BoxSize.y);

                if(Parent != null)
                {
                    var startVector = new Vector3(totalX + BoxSize.x / 2, DrawHere.y);
                    var endVector = new Vector3(Parent.DrawHere.x + BoxSize.x / 2 + Parent.TotalOffset.left,
                                                Parent.DrawHere.y + BoxSize.y);
                    using (new Handles.DrawingScope(Color.black))
                    {
                        Handles.DrawLine(startVector, endVector);
                    }    
                }

                CustomGUI.DrawQuad(totalPosition, Entry.State.CurrentState.GetBehaviorStateColor());

                GUI.BeginGroup(totalPosition);

                if (Entry.State.Parent != null)
                {
                    GUI.Label(new Rect(0, 20, 120, 30), new GUIContent(Entry.State.Parent.Name));
                }
                GUI.Label(new Rect(0,35,120,30), new GUIContent(Entry.State.Name));
                GUI.EndGroup();
            }
        }
    }
}
