using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Manipulators
{
    public class EditorNodeResizer : MouseManipulator
    {
        private EditorNode Node;

        public EditorNodeResizer(EditorNode node)
        {
            Node = node;
        }
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

            Vector2 diff =  evt.mousePosition - Node.worldBound.position;

            var rootContainer = Node.Q<VisualElement>("RootContainer");

            Node.style.width = diff.x;
            Node.style.height = diff.y;

            rootContainer.style.width = diff.x;
            rootContainer.style.height = diff.y;

            var inConnections = Node.inPoint.connections;

            foreach (var connection in inConnections)
            {
                connection.MarkDirtyRepaint();
            }

            if (Node.TreeElement.CanHaveChildren)
            {
                var outConnections = Node.outPoint.connections;

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
