using Assets.Scripts.AI;
using Assets.Scripts.AI.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Editor.BehaviorTreeViewEditor.BackendData
{
    internal class MultiColumnBehaviorTreeView : TreeViewWithTreeModel<BehaviorTreeElement>
    {
        const float kRowHeights = 20f;
        const float kToggleWidth = 18f;
        const float kTypeButtonWidth = 70f;
        public ShowParameters ShowParams;

        // All columns
        enum BTreeColumns
        {
            State,
            Name,
            Parameters
        }

        public enum SortOption
        {
            State,
            Name,
            Parameters
        }

        public enum ShowParameters
        {
            None,
            Active,
            All
        }

        // Sort options per column
        SortOption[] m_SortOptions =
        {
            SortOption.State,
            SortOption.Name,
            SortOption.Parameters
        };

        public static void TreeToList(TreeViewItem root, IList<TreeViewItem> result)
        {
            if (root == null)
                throw new NullReferenceException("root");
            if (result == null)
                throw new NullReferenceException("result");

            result.Clear();

            if (root.children == null)
                return;

            Stack<TreeViewItem> stack = new Stack<TreeViewItem>();
            for (int i = root.children.Count - 1; i >= 0; i--)
                stack.Push(root.children[i]);

            while (stack.Count > 0)
            {
                TreeViewItem current = stack.Pop();
                result.Add(current);

                if (current.hasChildren && current.children[0] != null)
                {
                    for (int i = current.children.Count - 1; i >= 0; i--)
                    {
                        stack.Push(current.children[i]);
                    }
                }
            }
        }

        public MultiColumnBehaviorTreeView(TreeViewState state, MultiColumnHeader multicolumnHeader, TreeModel<BehaviorTreeElement> model) 
            : base(state, multicolumnHeader, model)
        {
            Assert.AreEqual(m_SortOptions.Length, Enum.GetValues(typeof(BTreeColumns)).Length, "Ensure number of sort options are in sync with number of MyColumns enum values");

            // Custom setup
            columnIndexForTreeFoldouts = 1;
            
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            customFoldoutYOffset = (kRowHeights - EditorGUIUtility.singleLineHeight) * 0.5f; // center foldout in the row since we also center content. See RowGUI
            extraSpaceBeforeIconAndLabel = kToggleWidth;
            multicolumnHeader.sortingChanged += OnSortingChanged;
            
            Reload();
        }


        // Note we We only build the visible rows, only the backend has the full tree information. 
        // The treeview only creates info for the row list.
        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            var rows = base.BuildRows(root);
            SortIfNeeded(root, rows);
            return rows;
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            SortIfNeeded(rootItem, GetRows());
        }

        void SortIfNeeded(TreeViewItem root, IList<TreeViewItem> rows)
        {
            if (rows.Count <= 1)
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
            {
                return; // No column to sort for (just use the order the data are in)
            }

            // Sort the roots of the existing tree items
            SortByMultipleColumns();
            TreeToList(root, rows);
            Repaint();
        }

        void SortByMultipleColumns()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            var myTypes = rootItem.children.Cast<TreeViewItem<BehaviorTreeElement>>();
            var orderedQuery = InitialOrder(myTypes, sortedColumns);
            for (int i = 1; i < sortedColumns.Length; i++)
            {
                SortOption sortOption = m_SortOptions[sortedColumns[i]];
                bool ascending = multiColumnHeader.IsSortedAscending(sortedColumns[i]);

                switch (sortOption)
                {
                    case SortOption.Name:
                        orderedQuery = orderedQuery.ThenBy(l => l.data.Name, ascending);
                        break;
                }
            }

            rootItem.children = orderedQuery.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<TreeViewItem<BehaviorTreeElement>> InitialOrder(IEnumerable<TreeViewItem<BehaviorTreeElement>> myTypes, int[] history)
        {
            SortOption sortOption = m_SortOptions[history[0]];
            bool ascending = multiColumnHeader.IsSortedAscending(history[0]);
            switch (sortOption)
            {
                case SortOption.Name:
                    return myTypes.Order(l => l.data.Name, ascending);
                default:
                    Assert.IsTrue(false, "Unhandled enum");
                    break;
            }

            // default
            return myTypes.Order(l => l.data.Name, ascending);
        }

        protected override void ContextClickedItem(int id)
        {
            //var item = treeModel.Find(id);
            GenericMenu menu = new GenericMenu();
            menu.CreateTypeMenu<BehaviorTreeElement>(OnMenuTypeSelected);
            menu.ShowAsContext();
        }

        public void OnMenuTypeSelected(object itemTypeSelected)
        {
            object[] obj = itemTypeSelected as object[];
            BehaviorTreeElement element = obj[0] as BehaviorTreeElement;
            element.ElementType = obj[1].ToString();
            element.Name = element.ElementType.Split('.').Last() + " " + element.ID;
            Reload();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (TreeViewItem<BehaviorTreeElement>)args.item;

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, (BTreeColumns)args.GetColumn(i), ref args);
            }
        }

        void CellGUI(Rect cellRect, TreeViewItem<BehaviorTreeElement> item, BTreeColumns column, ref RowGUIArgs args)
        {
            // Center cell rect vertically (makes it easier to place controls, icons etc in the cells)
            switch (column)
            {
                case BTreeColumns.State:
                    CenterRectUsingSingleLineHeight(ref cellRect);
                    //EditorGUI.DrawRect(cellRect, GetBehaviorStateColor((int)item.data.CurrentState));
                    break;
                case BTreeColumns.Name:
                    // Do toggle
                    CenterRectUsingSingleLineHeight(ref cellRect);
                    Rect toggleRect = cellRect;
                    toggleRect.x += GetContentIndent(item);
                    toggleRect.width = kToggleWidth;

                    // Default icon and label
                    args.rowRect = cellRect;
                    base.RowGUI(args);
                    break;
                case BTreeColumns.Parameters:
                    switch(ShowParams)
                    {
                        case ShowParameters.Active:
                            if (IsSelected(item.id))
                            {
                                cellRect.height = TypeDependantDrawer.GetTotalHeightOfProperties(item.data);
                                TypeDependantDrawer.DrawAllFields(item.data, cellRect);
                            }
                            break;
                        case ShowParameters.All:
                            cellRect.height = TypeDependantDrawer.GetTotalHeightOfProperties(item.data);
                            TypeDependantDrawer.DrawAllFields(item.data, cellRect);
                            break;
                        default:
                            break;
                    }
                    args.rowRect = cellRect;
                    break;
            }
        }

        protected override void AfterRowsGUI()
        {
            base.AfterRowsGUI();
            RefreshCustomRowHeights();
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item)
        {
            float maxHeight = 0;
            if((ShowParams.HasFlag(ShowParameters.Active) && IsSelected(item.id)) ||
                ShowParams.HasFlag(ShowParameters.All))
            {
                maxHeight = TypeDependantDrawer.GetTotalHeightOfProperties((item as TreeViewItem<BehaviorTreeElement>).data);
            }
            return Math.Max(20f, maxHeight);
        }

        // Rename
        //--------

        protected override bool CanRename(TreeViewItem item)
        {
            // Only allow rename if we can show the rename overlay with a certain width (label might be clipped by other columns)
            Rect renameRect = GetRenameRect(treeViewRect, 0, item);
            return renameRect.width > 30;
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            // Set the backend name and reload the tree to reflect the new model
            if (args.acceptedRename)
            {
                var element = treeModel.Find(args.itemID);
                element.Name = args.newName;
                Reload();
            }
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
        {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        // Misc
        //--------

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("State"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 20,
                    minWidth = 10,
                    autoResize = true,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 125,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Parameters"),
                    headerTextAlignment = TextAlignment.Left,
                    width = 200,
                    minWidth = 60,
                    autoResize = true,
                    allowToggleVisibility = true
                }
        };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(BTreeColumns)).Length, "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state = new MultiColumnHeaderState(columns);
            return state;
        }
    }
}
