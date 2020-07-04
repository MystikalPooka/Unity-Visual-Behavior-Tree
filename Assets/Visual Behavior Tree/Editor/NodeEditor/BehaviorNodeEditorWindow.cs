using Assets.Editor;
using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor.NodeEditor
{
    public class BehaviorNodeEditorWindow : EditorWindow
    {
        private List<BehaviorEditorNode> nodes;
        private List<Connection> connections;

        private GUIStyle nodeStyle;
        private GUIStyle selectedNodeStyle;

        private ConnectionPoint selectedInPoint;
        private ConnectionPoint selectedOutPoint;

        private Vector2 offset;
        private Vector2 drag;

        [MenuItem("Window/uVBT/Node Editor")]
        private static void OpenWindow()
        {
            BehaviorNodeEditorWindow window = GetWindow<BehaviorNodeEditorWindow>();
            window.titleContent = new GUIContent("Behavior Node Editor");
        }

        private void OnEnable()
        {
            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/lightskin/images/node6 on.png") as Texture2D;
            selectedNodeStyle.border = new RectOffset(10, 10, 10, 10);
        }

        protected virtual void OnGUI()
        {
            DrawToolbar();

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);

            DrawNodes();
            DrawConnections();

            DrawConnectionLine(Event.current);

            ProcessNodeEvents(Event.current);
            ProcessEvents(Event.current);

            if (GUI.changed) Repaint();
        }

        protected void DrawToolbar()
        {
            if(GUILayout.Button("Save")) {
                SaveAllNodesToFile();
            }
            if (GUILayout.Button("Load"))
            {
                LoadNodesFromFile();
            }
        }

        protected void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            offset += drag * 0.5f;
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }

        protected void DrawNodes()
        {
            if(nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Draw();
                }
            }
        }

        protected void DrawConnections()
        {
            if (connections != null)
            {
                for (int i = 0; i < connections.Count; i++)
                {
                    connections[i].Draw();
                }
            }
        }

        protected void DrawConnectionLine(Event e)
        {
            if (selectedInPoint != null)
            {
                Handles.DrawLine(selectedInPoint.rect.center,
                                 e.mousePosition);
                GUI.changed = true;
            }

            if (selectedOutPoint != null)
            {
                Handles.DrawLine(selectedOutPoint.rect.center,
                                 e.mousePosition);

                GUI.changed = true;
            }
        }

        protected void ProcessEvents(Event e)
        {
            drag = Vector2.zero;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 1)
                    {
                        ProcessContextMenu(e.mousePosition);
                    }
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        OnDrag(e.delta);
                    }
                    break;
            }
        }

        protected void ProcessNodeEvents(Event e)
        {
            if (nodes != null)
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    bool guiChanged = nodes[i].ProcessEvents(e);

                    if (guiChanged)
                    {
                        GUI.changed = true;
                    }
                }
            }
        }

        protected void ProcessContextMenu(Vector2 mousePosition)
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.CreateTypeMenu<BehaviorTreeElement>((typeName) => OnClickAddNode(mousePosition, typeName));
            genericMenu.ShowAsContext();
        }

        protected void OnClickAddNode(Vector2 mousePosition, object itemTypeSelected)
        {
            if (nodes == null)
            {
                nodes = new List<BehaviorEditorNode>();
            }
            Debug.Log((string)itemTypeSelected);
            Type type = typeof(BehaviorTreeElement).Assembly.GetType((string)itemTypeSelected, true);

            var treeElement = (BehaviorTreeElement)CreateInstance(type);
            treeElement.ID = 0;
            treeElement.Name = type.ToString().Split('.').Last();
            treeElement.Depth = -1;
            treeElement.ElementType = type.ToString();

            nodes.Add(new BehaviorEditorNode(mousePosition, 200, 150, treeElement, OnClickInPoint, OnClickOutPoint, OnClickRemoveNode));
        }

        private void OnClickRemoveNode(BehaviorEditorNode node)
        {
            if (connections != null)
            {
                List<Connection> connectionsToRemove = new List<Connection>();

                for (int i = 0; i < connections.Count; i++)
                {
                    if (connections[i].inPoint == node.inPoint || connections[i].outPoint == node.outPoint)
                    {
                        connectionsToRemove.Add(connections[i]);
                    }
                }

                for (int i = 0; i < connectionsToRemove.Count; i++)
                {
                    var connection = connectionsToRemove[i];
                    connections.Remove(connection);
                    if(connection.inPoint.connections.Contains(connection))
                    {
                        connection.inPoint.connections.Remove(connection);
                    }

                    if (connection.outPoint.connections.Contains(connection))
                    {
                        connection.outPoint.connections.Remove(connection);
                    }
                }

                connectionsToRemove = null;
            }

            nodes.Remove(node);
        }

        private void OnDrag(Vector2 delta)
        {
            drag = delta;

            if (nodes != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Drag(delta);
                }
            }

            GUI.changed = true;
        }

        protected void OnClickInPoint(ConnectionPoint inPoint)
        {
            selectedInPoint = inPoint;

            if (selectedOutPoint != null)
            {
                if (selectedOutPoint.node != selectedInPoint.node)
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
                if (selectedOutPoint.node != selectedInPoint.node)
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
            connections.Remove(connection);
        }

        private void CreateConnection()
        {
            if (connections == null)
            {
                connections = new List<Connection>();
            }

            var connection = new Connection(selectedInPoint, selectedOutPoint, OnClickRemoveConnection);

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
            var path = EditorUtility.OpenFilePanel(
                "Save behavior tree",
                "",
                "asset");

            TreeLoader loader = new TreeLoader();
            var root = loader.LoadFromAsset(path);
            nodes = loader.GetNodes(OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);
            connections = loader.GetConnectionsFromRoot(root, OnClickRemoveConnection);
        }

        private bool IsValidTree()
        {
            DeselectAllNodes();
            return new TreeValidator(selectedNodeStyle).IsValidTreeByNodes(nodes);
        }

        private void DeselectAllNodes()
        {
            foreach(var node in nodes)
            {
                node.style = node.defaultNodeStyle;
            }
        }
    }
}
