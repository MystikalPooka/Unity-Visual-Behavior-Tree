using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Manipulators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Node.EditorResizer
{
    public class EditorResizer : VisualElement
    {
        internal EditorNode Node;

        public EditorResizer(EditorNode node)
        {
            Node = node;

            this.AddToClassList("Resizer");

            if(this.visible == true)
            {
                this.AddManipulator(new EditorNodeResizer(Node));
            }

            this.generateVisualContent = OnGenerateVisualContent;
        }

        void OnGenerateVisualContent(MeshGenerationContext cxt)
        {
            MeshWriteData mesh = cxt.Allocate(3, 3);
            Vertex[] vertices = new Vertex[3];

            Vector3 corner = new Vector3(resolvedStyle.width, resolvedStyle.height, Vertex.nearZ);

            vertices[0].position = corner;
            vertices[1].position = corner - (Vector3.up * resolvedStyle.height);
            vertices[2].position = corner + (Vector3.left * resolvedStyle.width);

            var color = this.resolvedStyle.color;

            vertices[0].tint = color;
            vertices[1].tint = color;
            vertices[2].tint = color;
            mesh.SetAllVertices(vertices);

            mesh.SetAllIndices(new ushort[] { 0, 2, 1});

        }
    }
}
