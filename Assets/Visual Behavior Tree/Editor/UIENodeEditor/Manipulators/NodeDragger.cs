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
    public class NodeDragger : MouseManipulator
    {
        private Vector2 start;
        protected bool isActive;

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

            start = evt.localMousePosition;

            isActive = true;
            target.CaptureMouse();
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (!isActive || !target.HasMouseCapture())
                return;

            Vector2 diff = evt.localMousePosition - start;

            target.style.left = target.layout.x + diff.x;
            target.style.top = target.layout.y + diff.y;
            var node = target as EditorNode;
            var inConnections = node.inPoint.connections;

            foreach(var connection in inConnections)
            {
                connection.MarkDirtyRepaint();
            }

            if(node.TreeElement.CanHaveChildren)
            {
                var outConnections = node.outPoint.connections;

                foreach (var connection in outConnections)
                {
                    connection.MarkDirtyRepaint();
                }
            }

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
