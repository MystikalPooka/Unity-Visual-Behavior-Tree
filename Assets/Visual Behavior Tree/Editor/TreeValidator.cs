using Assets.Visual_Behavior_Tree.Editor.NodeEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor
{
    public class TreeValidator
    {
        private GUIStyle errorHighlightStyle;

        public TreeValidator(GUIStyle errorStyle)
        {
            errorHighlightStyle = errorStyle;
        }

        public bool IsValidTreeByNodes(List<BehaviorEditorNode> nodes)
        {
            return HasExactlyOneRootNode(nodes);
        }

        private bool HasExactlyOneRootNode(List<BehaviorEditorNode> nodes)
        {
            var rootNodes = nodes.FindAll(node => node.inPoint.connections.Count() == 0);

            if(rootNodes.Count > 1)
            {
                Debug.LogError("Behavior Tree Is INVALID! You cannot have more than one root node (node with no inConnections)!  Check Highlighted nodes.");
                HighlightErrorNodes(rootNodes);
                return false;
            }

            return true;
        }

        private void HighlightErrorNodes(List<BehaviorEditorNode> errorNodes)
        {
            foreach(var node in errorNodes)
            {
                node.style = errorHighlightStyle;
            }
        }
    }
}
