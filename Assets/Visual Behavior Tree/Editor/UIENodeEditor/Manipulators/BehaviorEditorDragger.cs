using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Manipulators
{
    public class BehaviorEditorDragger : MouseManipulator
    {
        private Vector2 last;
        protected bool isActive;

        private BehaviorEditorWindow Window;

        public BehaviorEditorDragger(BehaviorEditorWindow window)
        {
            Window = window;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (isActive)
            {
                isActive = false;
                evt.StopImmediatePropagation();
                return;
            }

            last = evt.localMousePosition;

            isActive = true;
            target.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isActive || !target.HasMouseCapture())
                return;

            Vector2 diff = evt.localMousePosition - last;

            foreach(var node in Window.nodes)
            {
                node.style.left = node.layout.x + diff.x;
                node.style.top = node.layout.y + diff.y;
            }

            foreach(var connection in Window.connections)
            {
                connection.MarkDirtyRepaint();
            }

            last = evt.localMousePosition;
            evt.StopPropagation();


        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (!isActive || !target.HasMouseCapture() || !CanStopManipulation(evt))
                return;

            isActive = false;
            target.ReleaseMouse();
            evt.StopPropagation();
        }
    }
}
