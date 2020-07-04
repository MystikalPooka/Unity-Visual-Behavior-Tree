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
    public class EditorNodeSelector : MouseManipulator
    {
        protected bool isSelected;

        private EditorNode Node;

        public EditorNodeSelector(EditorNode node)
        {
            Node = node;
        }

        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (isSelected)
            {
                Node.RemoveFromClassList("Selected");
                isSelected = false;
                evt.StopImmediatePropagation();
                return;
            }

            Node.AddToClassList("Selected");

            isSelected = true;
            evt.StopPropagation();
        }
    }
}
