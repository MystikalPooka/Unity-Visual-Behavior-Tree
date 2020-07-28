using Assets.Scripts.AI;
using Assets.Scripts.AI.Tree;
using Assets.Visual_Behavior_Tree.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor
{
    public class UIETreeLoader
    {
        private TreeNodeAsset Asset;

        private List<BehaviorTreeElement> behaviorElements = new List<BehaviorTreeElement>();

        private List<EditorNode> nodes;

        public BehaviorTreeElement LoadFromPath(string path)
        {
            var filename = path.Substring(path.LastIndexOf("Assets/"));
            return LoadFromAsset(AssetDatabase.LoadAssetAtPath<TreeNodeAsset>(filename));
        }

        public BehaviorTreeElement LoadFromAsset(TreeNodeAsset asset)
        {
            var elements = JsonConvert.DeserializeObject<List<dynamic>>(asset.treeElements);
            Asset = asset;
            behaviorElements = new List<BehaviorTreeElement>();
            foreach (dynamic el in elements)
            {
                string typeName = el.ElementType;
                Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(typeName);
                dynamic newBehavior = ScriptableObject.CreateInstance(type);
                JsonConvert.PopulateObject(JsonConvert.SerializeObject(el), newBehavior);
                behaviorElements.Add(newBehavior);
            }
            return TreeElementUtility.ListToTree(behaviorElements);
        }

        public BehaviorTreeElement LoadFromAssetAndRoot(TreeNodeAsset asset, BehaviorTreeElement root)
        {
            Asset = asset;
            behaviorElements = new List<BehaviorTreeElement>();
            TreeElementUtility.TreeToList(root, behaviorElements);
            return root;
        }

        public List<EditorNode> GetNodes(Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint, Action<EditorNode> onClickAddNode, Action<EditorNode> onClickRemoveNode)
        {
            nodes = new List<EditorNode>();

            for(int i = 0; i < behaviorElements.Count; ++i)
            {
                var nodeRect = Asset.positions[i];
                var newNode = new EditorNode(behaviorElements[i], nodeRect, onClickInPoint, onClickOutPoint, onClickAddNode, onClickRemoveNode);

                nodes.Add(newNode);
            }

            return nodes;
        }

        internal List<Connection> GetConnectionsFromRoot(BehaviorTreeElement root, Action<Connection> onClickRemoveConnection)
        {
            List<Connection> connections = new List<Connection>();

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
                        var connection = new Connection(inPoint, nodes[parentIndex].outPoint, onClickRemoveConnection);
                        inPoint.connections.Add(connection);
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
