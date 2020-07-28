using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor.NodeEditor
{
    public enum ConnectionPointType { In, Out }

    public class ConnectionPoint
    {
        public Rect rect;

        public ConnectionPointType type;

        public BehaviorEditorNode node;

        public GUIStyle style;

        public Action<ConnectionPoint> OnClickConnectionPoint;

        public List<Connection> connections =  new List<Connection>();

        public ConnectionPoint(BehaviorEditorNode node, ConnectionPointType type, GUIStyle style, Action<ConnectionPoint> OnClickConnectionPoint)
        {
            this.node = node;
            this.type = type;
            this.style = style;
            this.OnClickConnectionPoint = OnClickConnectionPoint;
            rect = new Rect(0, 0, 20f, 10f);
        }

        public void Draw()
        {
            rect.x = node.fullRect.x + (node.fullRect.width * 0.5f) - rect.width * 0.5f;

            switch (type)
            {
                case ConnectionPointType.In:
                    rect.y = node.fullRect.y - rect.height + 4f;
                    break;

                case ConnectionPointType.Out:
                    rect.y = node.fullRect.y + node.fullRect.height - 5f;
                    break;
            }

            if (GUI.Button(rect, "", style))
            {
                OnClickConnectionPoint?.Invoke(this);
            }
        }
    }
}