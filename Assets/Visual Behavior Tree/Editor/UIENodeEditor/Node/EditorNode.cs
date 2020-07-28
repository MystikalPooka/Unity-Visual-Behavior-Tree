using Assets.Scripts.AI;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Manipulators;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Node.EditorResizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor
{
    public class EditorNode : VisualElement
    {
        public BehaviorTreeElement TreeElement;

        public SerializedObject ElementObject;

        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;

        private Action<EditorNode> OnClickAddNode;
        private Action<EditorNode> OnClickRemoveNode;

        public EditorNode(BehaviorTreeElement wrappedElement, Rect content, Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint, Action<EditorNode> onClickAddNode, Action<EditorNode> onClickRemoveNode)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Node/EditorNode.uss");
            this.styleSheets.Add(styleSheet);

            inPoint = new ConnectionPoint(this, ConnectionPointType.In, onClickInPoint);
            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, onClickOutPoint);

            this.AddToClassList("EditorNode");
            if(content.width > 0) this.style.width = content.width;
            if(content.height > 0) this.style.height = content.height;
            this.style.left = content.position.x;
            this.style.top = content.position.y;

            this.AddManipulator(new NodeDragger());
            this.AddManipulator(new EditorNodeSelector(this));
            this.AddManipulator(new ContextualMenuManipulator(ContextMenu));

            TreeElement = wrappedElement;
            ElementObject = new SerializedObject(wrappedElement);

            OnClickAddNode = onClickAddNode;
            OnClickRemoveNode = onClickRemoveNode;

            ReBindAllProperties();
        }

        private void ContextMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (Type type in
                Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BehaviorTreeElement))))
            {
                var menuStrings = type.ToString().Split('.');
                evt.menu.AppendAction("Change/" + menuStrings[menuStrings.Length - 2] +
                      "/" + menuStrings.Last(), OnMenuChangeAction);
                evt.menu.AppendAction("Remove", OnMenuRemoveAction);
            }
        }

        private void OnMenuRemoveAction(DropdownMenuAction obj)
        {
            OnClickRemoveNode(this);
        }

        void OnMenuChangeAction(DropdownMenuAction action)
        {
            string selectedName = action.name.Split('/').Last();
            var typeName = from type in typeof(BehaviorTreeElement).Assembly.GetTypes()
                           where type.Name.Contains(selectedName)
                           select type;

            OnClickRemoveNode(this);

            var treeElement = (BehaviorTreeElement)ScriptableObject.CreateInstance(typeName.First());
            treeElement.ID = 0;
            treeElement.Name = selectedName;
            treeElement.ElementType = typeName.First().ToString();
            this.TreeElement = treeElement;
            ElementObject = new SerializedObject(TreeElement);

            OnClickAddNode(this);
            ReBindAllProperties();
        }

        internal void ReBindAllProperties()
        {
            this.Clear();
            this.Bind(ElementObject);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Node/EditorNode.uxml");
            VisualElement uxmlRoot = visualTree.Instantiate();
            this.Add(uxmlRoot);

            var rootContainer = uxmlRoot.Q<VisualElement>("RootContainer");

            rootContainer.style.width = this.style.width;
            rootContainer.style.height = this.style.height;

            var parentContainer = uxmlRoot.Q<VisualElement>("ParentConnectorContainer");
            inPoint.AddToClassList("NodeButton");
            parentContainer.Insert(1, inPoint);

            var typeLabel = this.Q<Label>("TypeLabel");
            typeLabel.text = TreeElement.ElementType.Split('.').Last();

            var nodeContainer = this.Q<VisualElement>("NodeContainer");

            nodeContainer.Clear();

            foreach (var element in GetAllPropertyFields())
            {
                nodeContainer.Add(element);
                element.Bind(ElementObject);
            }

            var childContainer = this.Q<VisualElement>("ChildrenConnectorContainer");

            childContainer.Clear();

            var resizerBuffer = new EditorResizer(this);
            resizerBuffer.visible = false;
            childContainer.Add(resizerBuffer);

            if (childContainer.Contains(outPoint)) childContainer.Remove(outPoint);

            if (this.TreeElement.CanHaveChildren)
            {
                outPoint.AddToClassList("NodeButton");
                childContainer.Add(outPoint);
            }

            var resizer = new EditorResizer(this);
            childContainer.Add(resizer);
        }

        public List<VisualElement> GetAllPropertyFields()
        {
            var elements = new List<VisualElement>();

            Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(TreeElement.ElementType);
            FieldInfo[] fields = type.GetFields();

            foreach(var field in fields)
            {
                var prop = ElementObject.FindProperty(field.Name);
                var propField = new PropertyField(prop, field.Name);
                propField.AddToClassList("Property");

                elements.Add(propField);
            }

            return elements;
        }
    }
}