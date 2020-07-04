using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor
{
    public enum ConnectionPointType { In, Out }

    public class ConnectionPoint : Button, IEquatable<ConnectionPoint>
    {
        public ConnectionPointType Type;

        public Action<ConnectionPoint> OnClickConnectionPoint;
        internal List<Connection> connections = new List<Connection>();
        internal EditorNode node;

        public ConnectionPoint() 
        {
            VisualElement root = this;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/ConnectionPoint/ConnectionPoint.uss");
            root.styleSheets.Add(styleSheet);
        }

        public ConnectionPoint(EditorNode node, ConnectionPointType type, Action<ConnectionPoint> onClickConnectionPoint)
        {
            this.node = node;

            VisualElement root = this;

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/ConnectionPoint/ConnectionPoint.uss");
            root.styleSheets.Add(styleSheet);

            OnClickConnectionPoint = onClickConnectionPoint;
            this.clicked += OnClicked;

            connections = new List<Connection>();
        }

        private void OnClicked()
        {
            OnClickConnectionPoint?.Invoke(this);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ConnectionPoint);
        }

        public bool Equals(ConnectionPoint other)
        {
            return other != null &&
                   Type == other.Type &&
                   EqualityComparer<Action<ConnectionPoint>>.Default.Equals(OnClickConnectionPoint, other.OnClickConnectionPoint) &&
                   EqualityComparer<List<Connection>>.Default.Equals(connections, other.connections) &&
                   EqualityComparer<EditorNode>.Default.Equals(node, other.node);
        }

        public override int GetHashCode()
        {
            int hashCode = 97315638;
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Action<ConnectionPoint>>.Default.GetHashCode(OnClickConnectionPoint);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Connection>>.Default.GetHashCode(connections);
            hashCode = hashCode * -1521134295 + EqualityComparer<EditorNode>.Default.GetHashCode(node);
            return hashCode;
        }


    }
}