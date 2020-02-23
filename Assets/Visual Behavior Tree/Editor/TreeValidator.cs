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
        private List<BehaviorEditorNode> checkNodes;
        private GUIStyle errorHighlightStyle;

        public TreeValidator(GUIStyle errorStyle)
        {
            errorHighlightStyle = errorStyle;
        }

        public bool IsValidTreeByNodes(List<BehaviorEditorNode> nodes)
        {
            checkNodes = nodes;
            return HasExactlyOneRootNode() && NoChildHasTwoParents();
        }

        private bool HasExactlyOneRootNode()
        {
            var rootNodes = checkNodes.FindAll(node => node.inPoint.connections.Count() == 0);

            if(rootNodes.Count > 1)
            {
                Debug.LogError("Behavior Tree Is INVALID! You cannot have more than one root node (node with no inConnections)!  Check Highlighted nodes.");
                HighlightErrorNodes(rootNodes);
                return false;
            }

            return true;
        }

        private bool NoChildHasTwoParents()
        {
            var errorChildNodes = checkNodes.FindAll(node => node.inPoint.connections.Count() > 1);

            if(errorChildNodes.Count > 0)
            {
                Debug.LogError("Behavior Tree Is INVALID! You cannot have more than one parent node for a child! Check Highlighted nodes.");

                HighlightErrorNodes(errorChildNodes);
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
