using Assets.Scripts.AI;
using Assets.Scripts.AI.Tree;
using Assets.Visual_Behavior_Tree.Editor.NodeEditor;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor;
using Assets.Visual_Behavior_Tree.Scripts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Editor
{
    public class TreeSaver
    {
        private List<Rect> positions;

        public void SaveTree(List<BehaviorEditorNode> nodes, string path)
        {
            BehaviorEditorNode rootNode = nodes.Find(node => node.inPoint.connections.Count == 0);

            rootNode.treeElement.Depth = -1;

            positions = new List<Rect>();
            positions.Add(rootNode.fullRect);

            RecursiveAddChildren(rootNode);

            var json = GetJsonSaveDataFromRoot(rootNode.treeElement);

            Debug.Log(json);

            SaveAssetToDatabase(path, positions, json);
        }

        private void RecursiveAddChildren(BehaviorEditorNode rootNode)
        {
            rootNode.treeElement.Children = new List<TreeElement>();
            var rootNodeConnections = rootNode.outPoint.connections;
            rootNodeConnections.ForEach(outNode =>
            {
                var outInpointNode = outNode.inPoint.node;
                if (outInpointNode != rootNode)
                {
                    var treeElement = outInpointNode.treeElement;
                    treeElement.Parent = rootNode.treeElement;
                    treeElement.Depth = rootNode.treeElement.Depth + 1;
                    rootNode.treeElement.Children.Add(treeElement);
                    var pos = outInpointNode.fullRect;
                    positions.Add(pos);
                    RecursiveAddChildren(outInpointNode);
                }
            });
        }

        public void SaveTree(List<EditorNode> nodes, string path)
        {
            EditorNode rootNode = nodes.Find(node => node.inPoint.connections.Count == 0);

            rootNode.TreeElement.Depth = -1;

            positions = new List<Rect>
            {
                rootNode.layout
            };

            RecursiveAddChildren(rootNode);

            var json = GetJsonSaveDataFromRoot(rootNode.TreeElement);

            SaveAssetToDatabase(path, positions, json);
        }

        private void RecursiveAddChildren(EditorNode rootNode)
        {
            rootNode.TreeElement.Children = new List<TreeElement>();
            var rootOutNode = rootNode.outPoint;
            if (rootOutNode == null) return;
            var rootNodeConnections = rootOutNode.connections;
            rootNodeConnections.ForEach(outNode =>
            {
                var outInpointNode = outNode.inPoint.node;
                if (outInpointNode != rootNode)
                {
                    var treeElement = outInpointNode.TreeElement;
                    treeElement.Parent = rootNode.TreeElement;
                    treeElement.Depth = rootNode.TreeElement.Depth + 1;
                    rootNode.TreeElement.Children.Add(treeElement);
                    var pos = outInpointNode.layout;
                    positions.Add(pos);
                    RecursiveAddChildren(outInpointNode);
                }
            });
        }

        private string GetJsonSaveDataFromRoot(BehaviorTreeElement root)
        {
            var elementList = new List<BehaviorTreeElement>();
            TreeElementUtility.TreeToList(root, elementList);

            return JsonConvert.SerializeObject(elementList, Formatting.Indented);
        }

        private void SaveAssetToDatabase(string path, List<Rect> positions, string json)
        {
            TreeNodeAsset asset = ScriptableObject.CreateInstance<TreeNodeAsset>();
            asset.positions = positions;
            asset.treeElements = json;

            AssetDatabase.CreateAsset(asset, path);
        }
    }
}
