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
            rect = new Rect(0, 0, 10f, 20f);
        }

        public void Draw()
        {
            rect.y = node.fullRect.y + (node.fullRect.height * 0.5f) - rect.height * 0.5f;

            switch (type)
            {
                case ConnectionPointType.In:
                    rect.x = node.fullRect.x - rect.width + 8f;
                    break;

                case ConnectionPointType.Out:
                    rect.x = node.fullRect.x + node.fullRect.width - 8f;
                    break;
            }

            if (GUI.Button(rect, "", style))
            {
                OnClickConnectionPoint?.Invoke(this);
            }
        }
    }
}