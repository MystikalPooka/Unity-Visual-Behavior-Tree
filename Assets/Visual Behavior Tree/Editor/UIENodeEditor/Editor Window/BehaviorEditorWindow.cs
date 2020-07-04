using Assets.Scripts.AI;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Manipulators;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Util;
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
    public class BehaviorEditorWindow : EditorWindow
    {
        public List<EditorNode> nodes =  new List<EditorNode>();
        public List<Connection> connections = new List<Connection>();

        private ConnectionPoint selectedInPoint;
        private ConnectionPoint selectedOutPoint;

        [MenuItem("Testies/Behavior Editor Window")]
        public static void ShowExample()
        {
            BehaviorEditorWindow wnd = GetWindow<BehaviorEditorWindow>();
            wnd.titleContent = new GUIContent("Behavior Node Editor");
        }

        public void OnEnable()
        {
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Editor Window/BehaviorEditorWindow.uxml");
            VisualElement uxmlRoot = visualTree.CloneTree();

            root.Add(uxmlRoot);

            var container = rootVisualElement.Q<VisualElement>("GridContainer");

            container.AddManipulator(new ContextualMenuManipulator(ContextMenu));
            container.AddManipulator(new BehaviorEditorDragger(this));

            var saveButton = (Button)rootVisualElement.Q<VisualElement>("SaveButton");
            saveButton.clicked += SaveAllNodesToFile;

            var loadButton = (Button)rootVisualElement.Q<VisualElement>("LoadButton");
            loadButton.clicked += LoadNodesFromFile;

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Editor Window/BehaviorEditorWindow.uss");
            root.styleSheets.Add(styleSheet);

            this.SetAntiAliasing(4);
        }

        private void ContextMenu(ContextualMenuPopulateEvent evt)
        {
            foreach (Type type in
                Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(BehaviorTreeElement))))
            {
                var menuStrings = type.ToString().Split('.');
                evt.menu.AppendAction(menuStrings[menuStrings.Length - 2] +
                      "/" + menuStrings.Last(), OnMenuAction);
            }
        }

        void OnMenuAction(DropdownMenuAction action)
        {
            if (nodes == null)
            {
                nodes = new List<EditorNode>();
            }

            DeselectAllNodes();

            string selectedName = action.name.Split('/').Last();
            var typeName = from type in typeof(BehaviorTreeElement).Assembly.GetTypes()
                           where type.Name.Contains(selectedName)
                           select type;

            var treeElement = (BehaviorTreeElement)CreateInstance(typeName.First());
            treeElement.ID = 0;
            treeElement.Name = selectedName;
            treeElement.ElementType = typeName.First().ToString();

            EditorNode item = new EditorNode(treeElement, action.eventInfo.localMousePosition, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            OnClickAddNode(item);
        }

        void OnClickAddNode(EditorNode node)
        {
            nodes.Add(node);
            rootVisualElement.Q<VisualElement>("GridContainer").Add(node);
        }

        private void OnGUI()
        {
            rootVisualElement.Q<VisualElement>("GridContainer").style.height = new StyleLength(position.height);
        }

        protected void OnClickInPoint(ConnectionPoint inPoint)
        {
            selectedInPoint = inPoint;

            if (selectedOutPoint != null)
            {
                if (!selectedOutPoint.node.Equals(selectedInPoint.node))
                {
                    CreateConnection();
                }
                ClearConnectionSelection();
            }
        }

        private void OnClickOutPoint(ConnectionPoint outPoint)
        {
            selectedOutPoint = outPoint;

            if (selectedInPoint != null)
            {
                if (!selectedOutPoint.node.Equals(selectedInPoint.node))
                {
                    CreateConnection();
                }
                ClearConnectionSelection();
            }
        }

        private void OnClickRemoveConnection(Connection connection)
        {
            connection.inPoint.connections.Remove(connection);
            connection.outPoint.connections.Remove(connection);
            rootVisualElement.Q<VisualElement>("GridContainer").Remove(connection);
            connections.Remove(connection);
        }

        private void CreateConnection()
        {
            if (connections == null)
            {
                connections = new List<Connection>();
            }

            var connection = new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection);
            var container = rootVisualElement.Q<VisualElement>("GridContainer");
            connection.MarkDirtyRepaint();
            container.Add(connection);

            selectedInPoint.connections.Add(connection);
            selectedOutPoint.connections.Add(connection);
            connections.Add(connection);
        }

        private void ClearConnectionSelection()
        {
            selectedInPoint = null;
            selectedOutPoint = null;
        }

        private void SaveAllNodesToFile()
        {
            DeselectAllNodes();

            if (IsValidTree())
            {
                var path = EditorUtility.SaveFilePanelInProject(
                "Save behavior tree",
                "New Behavior Tree.asset",
                "asset",
                "Save behavior tree asset");

                TreeSaver saver = new TreeSaver();
                saver.SaveTree(nodes, path);
            }
        }

        protected void LoadNodesFromFile()
        {
            var container = rootVisualElement.Q<VisualElement>("GridContainer");
            if(nodes != null)
                foreach (var node in nodes)
                {
                    container.Remove(node);
                }

            if(connections != null)
                foreach (var connection in connections)
                {
                    container.Remove(connection);
                    connection.MarkDirtyRepaint();
                }

            var path = EditorUtility.OpenFilePanel(
                "Load behavior tree",
                "",
                "asset");

            UIETreeLoader loader = new UIETreeLoader();
            var root = loader.LoadFromAsset(path);
            nodes = loader.GetNodes(OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            connections = loader.GetConnectionsFromRoot(root, OnClickRemoveConnection);

            foreach(var node in nodes)
            {
                container.Add(node);
            }

            foreach(var connection in connections)
            {
                container.Add(connection);
                connection.MarkDirtyRepaint();
            }
        }

        private bool IsValidTree()
        {
            return new UIETreeValidator().IsValidTreeByNodes(nodes);
        }

        private void OnClickRemoveNode(EditorNode node)
        {
            if (connections != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].inPoint.Equals(node.inPoint) || connections[i].outPoint.Equals(node.outPoint))
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
                        rootVisualElement.Q<VisualElement>("GridContainer").Remove(connection);

                    }

                    if (connection.outPoint.connections.Contains(connection))
                    {
                        connection.outPoint.connections.Remove(connection);
                        rootVisualElement.Q<VisualElement>("GridContainer").Remove(connection);
                    }
                }

                connectionsToRemove = null;
            }

            nodes.Remove(node);
        }

        private void DeselectAllNodes() 
        {
            foreach(var node in nodes)
            {
                node.RemoveFromClassList("Selected");
                node.RemoveFromClassList("Error");
            }
        }
    }
}