using Assets.Editor;
using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Assets.Visual_Behavior_Tree.Editor.NodeEditor
{
    public class BehaviorEditorNode
    {
        public Rect fullRect;
        public Rect subInspectorRect;

        public string title;
        public int titleSize = 10;

        public bool isDragged;
        public bool isSelected;

        [SerializeField]
        public BehaviorTreeElement treeElement;

        public SerializedObject elementObject;

        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;

        private GUIStyle inPointStyle;
        private GUIStyle outPointStyle;

        public GUIStyle style;
        public GUIStyle defaultNodeStyle;
        public GUIStyle selectedNodeStyle;

        public Action<BehaviorEditorNode> OnRemoveNode;

        public BehaviorEditorNode(Vector2 position, float width, float height, BehaviorTreeElement element, Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<BehaviorEditorNode> OnClickRemoveNode)
        {
            fullRect = new Rect(position.x, position.y, width, height);

            style = new GUIStyle();
            style.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1.png") as Texture2D;
            style.border = new RectOffset(12, 12, 12, 12);

            defaultNodeStyle = style;

            selectedNodeStyle = new GUIStyle();
            selectedNodeStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/node1 on.png") as Texture2D;
            selectedNodeStyle.border = new RectOffset(12, 12, 12, 12);

            inPointStyle = new GUIStyle();
            inPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left.png") as Texture2D;
            inPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn left on.png") as Texture2D;
            inPointStyle.border = new RectOffset(4, 4, 12, 12);

            outPointStyle = new GUIStyle();
            outPointStyle.normal.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right.png") as Texture2D;
            outPointStyle.active.background = EditorGUIUtility.Load("builtin skins/darkskin/images/btn right on.png") as Texture2D;
            outPointStyle.border = new RectOffset(4, 4, 12, 12);

            inPoint = new ConnectionPoint(this, ConnectionPointType.In, inPointStyle, OnClickInPoint);
            outPoint = new ConnectionPoint(this, ConnectionPointType.Out, outPointStyle, OnClickOutPoint);

            Vector2 inPointSize = inPoint.rect.size;
            Vector2 outPointSize = outPoint.rect.size;

            subInspectorRect = new Rect(position.x + inPointSize.x, position.y + titleSize, width - inPointSize.x - outPointSize.x, height - (titleSize * 2));

            treeElement = element;
            elementObject = new SerializedObject(treeElement);

            title = treeElement.Name;

            OnRemoveNode = OnClickRemoveNode;
        }

        public void AddInConnection(Connection connection)
        {
            inPoint.connections.Add(connection);
        }

        public void RemoveInConnection(Connection connection)
        {
            inPoint.connections.Remove(connection);
        }

        public void AddOutConnection(Connection connection)
        {
            outPoint.connections.Add(connection);
        }

        public void RemoveOutConnection(Connection connection)
        {
            outPoint.connections.Remove(connection);
        }

        public void Drag(Vector2 delta)
        {
            fullRect.position += delta;
            subInspectorRect.position += delta;
        }

        public void Draw()
        {
            inPoint.Draw();
            outPoint.Draw();

            GUI.Box(fullRect, title, style);
            GUI.Box(subInspectorRect, "", selectedNodeStyle);

            Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(treeElement.ElementType);
            FieldInfo[] fields = type.GetFields();

            var items = new List<string>();
            foreach (FieldInfo field in fields)
            {
                items.Add(field.Name);
            }

            var objStyle = EditorStyles.objectField;
            var boxes = EditorGUIUtility.GetFlowLayoutedRects(subInspectorRect, objStyle, 2, 0, items);
            for (int i = 0; i < items.Count; ++i)
            {
                var box = boxes[i];
                box.x = box.x + 7;
                box.y = box.y + 7;
                box.xMax = box.xMax - 15;
                using (new GUILayout.AreaScope(box, ""))
                {
                    var prop = elementObject.FindProperty(items[i]);

                    GUILayout.BeginHorizontal();
                    GUILayout.Label(prop.displayName, EditorStyles.whiteLabel, GUILayout.MinWidth(50), GUILayout.ExpandWidth(true));
                    EditorGUILayout.PropertyField(prop, GUIContent.none, true, GUILayout.MinWidth(100), GUILayout.ExpandWidth(true));
                    GUILayout.EndHorizontal();
                }
            }
            elementObject.ApplyModifiedProperties();
        }

        public bool ProcessEvents(Event e)
        {
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0)
                    {
                        if (fullRect.Contains(e.mousePosition))
                        {
                            isDragged = true;
                            GUI.changed = true;
                            isSelected = true;
                            style = selectedNodeStyle;
                        }
                        else
                        {
                            GUI.changed = true;
                            isSelected = false;
                            style = defaultNodeStyle;
                        }
                    }

                    if (e.button == 1 && isSelected && fullRect.Contains(e.mousePosition))
                    {
                        ProcessContextMenu();
                        e.Use();
                    }
                    break;

                case EventType.MouseUp:
                    isDragged = false;
                    break;

                case EventType.MouseDrag:
                    if (e.button == 0 && isDragged)
                    {
                        Drag(e.delta);
                        e.Use();
                        return true;
                    }
                    break;
            }

            return false;
        }

        private void ProcessContextMenu()
        {
            GenericMenu genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Remove node"), false, OnClickRemoveNode);
            genericMenu.ShowAsContext();
        }

        private void OnClickRemoveNode()
        {
            OnRemoveNode?.Invoke(this);
        }
    }


}
