using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using Assets.Scripts.AI.Tree;
using Assets.Visual_Behavior_Tree.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Assets.Editor.BehaviorTreeViewEditor.BackendData
{
    class MultiColumnBTreeWindow : EditorWindow
    {
        [NonSerialized] bool _Initialized;
        [SerializeField] TreeViewState _TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState _MultiColumnHeaderState;
        SearchField _SearchField;
        MultiColumnBehaviorTreeView _TreeView;
        [SerializeField] TreeNodeAsset _TreeNodeAsset;

        string FilePath = "";
        private static string FileDir = "Assets/Behaviors/";

        [MenuItem("Behavior Tree/New Tree")]
        public static MultiColumnBTreeWindow GetWindow()
        {
            var window = GetWindow<MultiColumnBTreeWindow>();
            window.titleContent = new GUIContent("Behavior Tree Builder");
            window.Focus();
            window.Repaint();
            return window;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceID, int line)
        {
            var BTreeAsset = EditorUtility.InstanceIDToObject(instanceID) as TreeNodeAsset;
            if (BTreeAsset != null)
            {
                var window = GetWindow();
                window.SetTreeAsset(BTreeAsset);
                return true;
            }
            return false; // we did not handle the open
        }

        public void SetTreeAsset(TreeNodeAsset BehaviorTreeAsset)
        {
            if(BehaviorTreeAsset == null || BehaviorTreeAsset.treeElements == "")
            {
                CreateNewTree();
            }
            _TreeNodeAsset = BehaviorTreeAsset;
            _Initialized = false;
        }

        Rect multiColumnTreeViewRect
        {
            get { return new Rect(20, 50, position.width - 40, position.height - 70); }
        }

        Rect toolbarRect
        {
            get { return new Rect(20f, 10f, position.width - 40f, 20f); }
        }

        Rect topToolbarRect
        {
            get { return new Rect(20f, 30f, position.width - 40f, 30f); }
        }

        Rect bottomToolbarRect
        {
            get { return new Rect(20f, position.height - 18f, position.width - 60f, 16f); }
        }

        public MultiColumnBehaviorTreeView treeView
        {
            get { return _TreeView; }
        }

        void InitIfNeeded()
        {
            if (!_Initialized)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (_TreeViewState == null)
                    _TreeViewState = new TreeViewState();

                bool firstInit = _MultiColumnHeaderState == null;
                var headerState = MultiColumnBehaviorTreeView.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(_MultiColumnHeaderState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(_MultiColumnHeaderState, headerState);
                _MultiColumnHeaderState = headerState;

                var multiColumnHeader = new BTreeMultiColumnHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit();

                var treeModel = new TreeModel<BehaviorTreeElement>(GetData());

                _TreeView = new MultiColumnBehaviorTreeView(_TreeViewState, multiColumnHeader, treeModel);

                _SearchField = new SearchField();
                _SearchField.downOrUpArrowKeyPressed += _TreeView.SetFocusAndEnsureSelectedItem;

                _Initialized = true;
            }
        }

        IList<BehaviorTreeElement> GetData()
        {
            if (_TreeNodeAsset == null)
            {
                CreateNewTree();
            }

            var treeRoot = _TreeNodeAsset.LoadRoot();
            if(treeRoot == null)
            {
                treeRoot = new Merge("New Root",-1, -1);
            }
            var treeList = new List<BehaviorTreeElement>();

            TreeElementUtility.TreeToList(treeRoot, treeList);

            return treeList;
        }

        void CreateNewTree()
        {
            CustomAssetUtility.CreateAsset<TreeNodeAsset>();
            _TreeNodeAsset = (TreeNodeAsset)Selection.activeObject;
            var root =new Merge("root",-1,-1);
            //BehaviorExtensions.SaveBehaviorAsset(null, AssetDatabase.GetAssetPath(_TreeNodeAsset),
            //                                    _TreeNodeAsset,(Merge)root);
        }

        void OnSelectionChange()
        {
            if (!_Initialized)
                return;

            var BehaviorTreeAsset = Selection.activeObject as TreeNodeAsset;
            if (BehaviorTreeAsset != null && BehaviorTreeAsset != _TreeNodeAsset)
            {
                _TreeNodeAsset = BehaviorTreeAsset;
                _TreeView.treeModel.SetData(GetData());
                _TreeView.Reload();
            }
        }

        void OnGUI()
        {
            InitIfNeeded();

            SearchBar(toolbarRect);
            TopToolbar(topToolbarRect);
            DoTreeView(multiColumnTreeViewRect);
            BottomToolBar(bottomToolbarRect);
        }

        void SearchBar(Rect rect)
        {
            treeView.searchString = _SearchField.OnGUI(rect, treeView.searchString);
        }

        void DoTreeView(Rect rect)
        {
            _TreeView.OnGUI(rect);
        }

        void TopToolbar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using (new EditorGUILayout.HorizontalScope())
            {
                GenericMenu menu = new GenericMenu();
                if (EditorGUILayout.DropdownButton(new GUIContent("Add Behavior"),FocusType.Passive))
                {
                    menu.CreateTypeMenu<BehaviorTreeElement>(OnTypeSelected);
                    menu.ShowAsContext();
                }

                if (GUILayout.Button("Remove Behavior"))
                {
                    var selection = _TreeView.GetSelection();
                    _TreeView.treeModel.RemoveElements(selection);
                }

                FilePath = GUILayout.TextField(FilePath);
                if (GUILayout.Button("Save Tree"))
                {
                    FilePath = EditorUtility.SaveFilePanel("", FileDir, "New Behavior Tree", "asset");
                    //BehaviorExtensions.SaveBehaviorAsset(null, FilePath, _TreeNodeAsset, (Merge)_TreeView.treeModel.Root);
                }
            }

            GUILayout.EndArea();
        }

        private void OnTypeSelected(object typeName)
        {
            var selection = _TreeView.GetSelection();
            BehaviorTreeElement parent = (selection.Count == 1 ? _TreeView.treeModel.Find(selection[0]) : null) ?? _TreeView.treeModel.Root;
            int depth = parent != null ? parent.Depth + 1 : 0;
            int id = _TreeView.treeModel.GenerateUniqueID();
            
            Type type = typeof(BehaviorTreeElement).Assembly.GetType((string)typeName, true);

            dynamic element = Activator.CreateInstance(type, type.ToString().Split('.').Last() + " " + id, depth, id);
            element.ElementType = element.GetType().ToString();
            //element.BehaviorTreeManager = parent.BehaviorTreeManager;
            _TreeView.treeModel.AddElement(element, parent, 0);

            _TreeView.SetSelection(new[] { id }, TreeViewSelectionOptions.RevealAndFrame);
            //TODO: Show there are unsaved changes
        }

        void BottomToolBar(Rect rect)
        {
            GUILayout.BeginArea(rect);

            using(new EditorGUILayout.HorizontalScope())
            {
                var style = "miniButton";
                if (GUILayout.Button("Expand All", style))
                {
                    treeView.ExpandAll();
                }

                if (GUILayout.Button("Collapse All", style))
                {
                    treeView.CollapseAll();
                }

                GUILayout.Space(10);

                _TreeView.ShowParams = (MultiColumnBehaviorTreeView.ShowParameters) 
                    EditorGUILayout.EnumPopup("Show Parameter Lists", _TreeView.ShowParams, "miniButton");
            }
            GUILayout.EndArea();
        }
    }

    internal class BTreeMultiColumnHeader : MultiColumnHeader
    {
        public BTreeMultiColumnHeader(MultiColumnHeaderState state)
            : base(state)
        { }

        protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
        {
            // Default column header gui
            base.ColumnHeaderGUI(column, headerRect, columnIndex);
        }
    }
}
