using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor
{
    public class Connection : VisualElement
    {
        public ConnectionPoint inPoint;
        public ConnectionPoint outPoint;
        public Action<Connection> OnClickRemoveConnection;

        readonly Button removeButton;

        public float lineWidth = 3;

        public Connection(ConnectionPoint inPoint, ConnectionPoint outPoint, Action<Connection> onClickRemoveConnection)
        {
            this.inPoint = inPoint;
            this.outPoint = outPoint;
            this.OnClickRemoveConnection = onClickRemoveConnection;

            this.AddToClassList("Connection");

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Connection/Connection.uss");
            this.styleSheets.Add(styleSheet);

            removeButton = new Button();
            removeButton.AddToClassList("ConnectionButton");
            removeButton.clicked += OnClicked;

            this.Add(removeButton);

            this.generateVisualContent += OnGenerateVisualContent;
        }

        void OnClicked()
        {
            OnClickRemoveConnection?.Invoke(this);
        }

        void OnGenerateVisualContent(MeshGenerationContext cxt)
        {
            MeshWriteData mesh = cxt.Allocate(4, 6);
            Vertex[] vertices = new Vertex[4];

            float inX = inPoint.worldBound.center.x;
            float leftIn = inX - lineWidth;
            float rightIn = inX + lineWidth;

            float outX = outPoint.worldBound.center.x;
            float leftOut = outX - lineWidth;
            float rightOut = outX + lineWidth;
            float top = inPoint.worldBound.center.y - 59;
            float bottom = outPoint.worldBound.center.y - 59;

            removeButton.style.top = ((top + bottom) / 2) - removeButton.layout.height/2;
            removeButton.style.left = ((inX + outX) / 2) - (removeButton.layout.width / 2) - 2;

            vertices[0].position = new Vector3(leftIn, top, Vertex.nearZ);
            vertices[1].position = new Vector3(rightIn, top, Vertex.nearZ);
            vertices[2].position = new Vector3(leftOut, bottom, Vertex.nearZ);
            vertices[3].position = new Vector3(rightOut, bottom, Vertex.nearZ);

            var color = this.resolvedStyle.color;

            vertices[0].tint = color;
            vertices[1].tint = color;
            vertices[2].tint = color;
            vertices[3].tint = color;
            mesh.SetAllVertices(vertices);
            if(top > bottom)
                mesh.SetAllIndices(new ushort[] { 0, 2, 1, 2, 3, 1});
            else
                mesh.SetAllIndices(new ushort[] { 2, 0, 3, 0, 1, 3 });

        }
    }
}
