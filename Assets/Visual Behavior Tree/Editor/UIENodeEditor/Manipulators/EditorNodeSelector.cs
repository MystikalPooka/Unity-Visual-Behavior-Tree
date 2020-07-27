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
            var root = Node.Q<Box>("RootContainer");

            if (isSelected)
            {
                root.RemoveFromClassList("Selected");
                isSelected = false;
                evt.StopImmediatePropagation();
                return;
            }

            root.AddToClassList("Selected");

            isSelected = true;
            evt.StopPropagation();
        }
    }
}
