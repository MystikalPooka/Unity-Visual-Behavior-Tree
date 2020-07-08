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
        private TreeNodeAsset asset;

        private List<BehaviorTreeElement> behaviorElements = new List<BehaviorTreeElement>();

        private BehaviorTreeElement root;

        private List<EditorNode> nodes;
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
                //dynamic newBehavior = Activator.CreateInstance(type, (string)el.Name, (int)el.Depth, (int)el.ID);
                dynamic newBehavior = ScriptableObject.CreateInstance(type);
                JsonConvert.PopulateObject(JsonConvert.SerializeObject(el), newBehavior);
                behaviorElements.Add(newBehavior);
            }
            root = TreeElementUtility.ListToTree(behaviorElements);
            return root;
        }

        public List<EditorNode> GetNodes(Action<ConnectionPoint> onClickInPoint, Action<ConnectionPoint> onClickOutPoint, Action<EditorNode> onClickAddNode, Action<EditorNode> onClickRemoveNode)
        {
            nodes = new List<EditorNode>();

            for(int i = 0; i < behaviorElements.Count; ++i)
            {
                var nodeRect = asset.positions[i];
                var newNode = new EditorNode(behaviorElements[i], nodeRect.position, onClickInPoint, onClickOutPoint, onClickAddNode, onClickRemoveNode);

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
