using Assets.Scripts.AI;
using Assets.Scripts.AI.Tree;
using Assets.Visual_Behavior_Tree.Editor.NodeEditor;
using Assets.Visual_Behavior_Tree.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor
{
    public class TreeLoader
    {
        private TreeNodeAsset asset;

        private List<BehaviorTreeElement> behaviorElements = new List<BehaviorTreeElement>();

        private BehaviorTreeElement root;

        private List<BehaviorEditorNode> nodes;
        private List<Connection> connections = new List<Connection>();

        public BehaviorTreeElement LoadFromAsset(string path)
        {
            var filename = path.Substring(path.LastIndexOf("Assets/"));
            asset = AssetDatabase.LoadAssetAtPath<TreeNodeAsset>(filename);
            
            var elements = JsonConvert.DeserializeObject<List<dynamic>>(asset.treeElements);

            behaviorElements = new List<BehaviorTreeElement>();
            foreach (dynamic el in elements)
            {
                string typeName = el.ElementType;
                Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(typeName);
                dynamic newBehavior = Activator.CreateInstance(type, (string)el.Name, (int)el.Depth, (int)el.ID);
                JsonConvert.PopulateObject(JsonConvert.SerializeObject(el), newBehavior);
                behaviorElements.Add(newBehavior);
            }
            root = TreeElementUtility.ListToTree(behaviorElements);
            return root;
        }

        public List<BehaviorEditorNode> GetNodes(Action<ConnectionPoint> OnClickInPoint, Action<ConnectionPoint> OnClickOutPoint, Action<BehaviorEditorNode> OnClickRemoveNode)
        {
            nodes = new List<BehaviorEditorNode>();

            for(int i = 0; i < behaviorElements.Count; ++i)
            {
                var nodeRect = asset.positions[i];
                var newNode = new BehaviorEditorNode(nodeRect.position, nodeRect.width, nodeRect.height, behaviorElements[i], OnClickInPoint, OnClickOutPoint, OnClickRemoveNode);

                nodes.Add(newNode);
            }

            return nodes;
        }

        internal List<Connection> GetConnectionsFromRoot(BehaviorTreeElement root, Action<Connection> onClickRemoveConnection)
        { 
            for (int parentIndex = 0; parentIndex < behaviorElements.Count; parentIndex++)
            {
                var parent = behaviorElements[parentIndex];

                int parentDepth = parent.Depth;

                // Count children based depth value, we are looking at children until it's the same depth as this object
                for (int i = parentIndex + 1; i < behaviorElements.Count; i++)
                {
                    if (behaviorElements[i].Depth == parentDepth + 1)
                    {
                        var inPoint = nodes[i].inPoint;
                        var connection = new Connection(nodes[i].inPoint, nodes[parentIndex].outPoint, onClickRemoveConnection);
                        nodes[i].inPoint.connections.Add(connection);
                        nodes[parentIndex].outPoint.connections.Add(connection);
                        connections.Add(connection);
                    }

                    if (behaviorElements[i].Depth <= parentDepth)
                        break;
                }
            }
            return connections;
        }
    }
}
