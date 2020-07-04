using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Analytics;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Manipulators;
using System.Linq;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor
{
    public class EditorNode : Box
    {
        public BehaviorTreeElement TreeElement;

        public SerializedObject ElementObject;

        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;

        public EditorNode(BehaviorTreeElement wrappedElement, Vector2 position, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<EditorNode> OnClickRemoveNode)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Node/EditorNode.uss");
            this.styleSheets.Add(styleSheet);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Node/EditorNode.uxml");
            VisualElement uxmlRoot = visualTree.Instantiate();
            this.Add(uxmlRoot);

            var parentContainer = uxmlRoot.Q<VisualElement>("ParentConnectorContainer");
            inPoint = new ConnectionPoint(this, ConnectionPointType.In, OnClickInPoint);
            inPoint.AddToClassList("NodeButton");
            parentContainer.Insert(1,inPoint);

            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, OnClickOutPoint);

            this.AddToClassList("EditorNode");

            this.style.left = position.x;
            this.style.top = position.y;

            this.AddManipulator(new NodeDragger());
            this.AddManipulator(new EditorNodeSelector(this));
            this.AddManipulator(new ContextualMenuManipulator(ContextMenu));

            TreeElement = wrappedElement;
            ElementObject = new SerializedObject(wrappedElement);

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
                      "/" + menuStrings.Last(), OnMenuAction);
            }
        }

        void OnMenuAction(DropdownMenuAction action)
        {
            string selectedName = action.name.Split('/').Last();
            var typeName = from type in typeof(BehaviorTreeElement).Assembly.GetTypes()
                           where type.Name.Contains(selectedName)
                           select type;

            var treeElement = (BehaviorTreeElement)ScriptableObject.CreateInstance(typeName.First());
            treeElement.ID = 0;
            treeElement.Name = selectedName;
            treeElement.ElementType = typeName.First().ToString();
            this.TreeElement = treeElement;
            ElementObject = new SerializedObject(TreeElement);
            ReBindAllProperties();
        }

        internal void ReBindAllProperties()
        {
            this.Bind(ElementObject);

            var nodeContainer = this.Q<VisualElement>("NodeContainer");
            foreach(var child in nodeContainer.Children())
            {
                Debug.Log("Removing 1 element");
                nodeContainer.Remove(child);
            }

            foreach (var element in GetAllPropertyFields())
            {
                Debug.Log("Adding one");
                nodeContainer.Add(element);
                element.Bind(ElementObject);
            }

            var childContainer = this.Q<VisualElement>("ChildrenConnectorContainer");
            if (this.TreeElement.CanHaveChildren)
            {
                outPoint.AddToClassList("NodeButton");
                childContainer.Add(outPoint);
            }
            else if(childContainer.Contains(outPoint))
            {
                Debug.Log("Removing connections");
                var connections = outPoint.connections;
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].inPoint.Equals(this.inPoint) || connections[i].outPoint.Equals(this.outPoint))
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                {
                    var connection = connectionsToRemove[i];
                    connections.Remove(connection);
                    if (connection.inPoint.connections.Contains(connection))
                    {
                        connection.inPoint.connections.Remove(connection);
                    }

                    if (connection.outPoint.connections.Contains(connection))
                    {
                        connection.outPoint.connections.Remove(connection);
                    }
                }
                childContainer.Remove(outPoint);
            }
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