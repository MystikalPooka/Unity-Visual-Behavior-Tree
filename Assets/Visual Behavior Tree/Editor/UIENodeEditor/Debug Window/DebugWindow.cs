using Assets.Scripts.AI;
using Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Debug_Window;
using Assets.Visual_Behavior_Tree.Scripts;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor
{
    public class DebugWindow : EditorWindow
    {
        List<EditorNode> nodes = new List<EditorNode>();

        List<Connection> connections = new List<Connection>();

        BehaviorManager selectedManager;

        [MenuItem("Testing/Behavior Debug Window")]
        public static void Open()
        {
            DebugWindow wnd = GetWindow<DebugWindow>();
            wnd.titleContent = new GUIContent("Behavior Node Debugger");
        }

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Debug Window/DebugWindow.uxml");
            VisualElement uxmlRoot = visualTree.CloneTree();

            var toolbar = uxmlRoot.Q<VisualElement>("Toolbar");
            var dropdown = new BehaviorManagerSelector(OnSelectedManagerChanged);

            toolbar.Add(dropdown);

            root.Add(uxmlRoot);

            var container = rootVisualElement.Q<VisualElement>("TabContainer");

            //container.AddManipulator(new BehaviorEditorDragger(this));

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Visual Behavior Tree/Editor/UIENodeEditor/Debug Window/DebugWindow.uss");
            root.styleSheets.Add(styleSheet);

            this.SetAntiAliasing(4);
        }

        private string OnSelectedManagerChanged(string selected)
        {
            var toolbar = rootVisualElement.Q<VisualElement>("Toolbar");
            var selector = rootVisualElement.Q<VisualElement>("TreeSelector");
            if (selector != null) toolbar.Remove(selector);
     

            var idAndName = selected.Split('-');
            if (idAndName.Length > 1)
            {
                var matchedManager = from manager in UnityEngine.Object.FindObjectsOfType<BehaviorManager>()
                                     where manager.GetInstanceID() == int.Parse(idAndName[0])
                                     select manager;
                selectedManager = matchedManager.First();

                var treeDropdown = new BehaviorTreeSelector(selectedManager, OnSelectedTreeChanged);
                treeDropdown.name = "TreeSelector";

                toolbar.Add(treeDropdown);
            }

            return selected;
        }

        private string OnSelectedTreeChanged(BehaviorTreeElement selected)
        {
            Debug.Log(selected.name);
            var asset = selectedManager.rootList[selected];
            LoadNodesFromAssetAndRoot(asset, selected);
            
            return selected.GetInstanceID() + "-" + selected.Name;
        }

        private void LoadNodesFromAssetAndRoot(TreeNodeAsset asset, BehaviorTreeElement root)
        {
            var container = rootVisualElement.Q<VisualElement>("TabContainer");
            if (nodes != null)
                foreach (var node in nodes)
                {
                    container.Remove(node);
                }

            if (connections != null)
                foreach (var connection in connections)
                {
                    container.Remove(connection);
                    connection.MarkDirtyRepaint();
                }

            UIETreeLoader loader = new UIETreeLoader();
            var rootNode = loader.LoadFromAssetAndRoot(asset, root);
            nodes = loader.GetNodes(OnClickInPoint, OnClickOutPoint, OnClickAddNode, OnClickRemoveNode);
            connections = loader.GetConnectionsFromRoot(rootNode, OnClickRemoveConnection);

            foreach (var node in nodes)
            {
                container.Add(node);
            }

            foreach (var connection in connections)
            {
                container.Add(connection);
                connection.MarkDirtyRepaint();
            }
        }

        private void OnClickRemoveNode(EditorNode obj)
        {}

        private void OnClickAddNode(EditorNode node)
        {}

        private void OnGUI()
        {
            rootVisualElement.Q<VisualElement>("TabContainer").style.height = new StyleLength(position.height);
        }

        private void OnClickInPoint(ConnectionPoint inPoint)
        {}

        private void OnClickOutPoint(ConnectionPoint outPoint)
        {}

        private void OnClickRemoveConnection(Connection obj)
        {}
    }
}