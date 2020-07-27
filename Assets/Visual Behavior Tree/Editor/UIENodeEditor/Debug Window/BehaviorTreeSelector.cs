using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Debug_Window
{
    public class BehaviorTreeSelector : VisualElement
    {
        public BehaviorManager RootManager;
        public BehaviorTreeSelector(BehaviorManager manager, Func<BehaviorTreeElement, string> onSelectedChanged)
        {
            this.AddToClassList("Selector");
            RootManager = manager;
            var dropdown = new PopupField<BehaviorTreeElement>(GetDropdownList(),0);
            dropdown.label = "Tree";
            this.Add(dropdown);
            dropdown.formatSelectedValueCallback += onSelectedChanged;
            dropdown.formatListItemCallback += FormatItem;
        }

        private List<BehaviorTreeElement> GetDropdownList()
        {
            var menuItems = new List<BehaviorTreeElement>();

            var trees = from tree in RootManager.rootList
                           select tree.Key;

            menuItems.AddRange(trees);

            return menuItems;
        }

        private string FormatItem(BehaviorTreeElement element)
        {
            return element.GetInstanceID() + "-" + element.Name;
        }
    }
}