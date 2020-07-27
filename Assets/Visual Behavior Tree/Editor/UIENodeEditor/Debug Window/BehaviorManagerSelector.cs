using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Assets.Visual_Behavior_Tree.Editor.UIENodeEditor.Debug_Window
{
    public class BehaviorManagerSelector : VisualElement
    {
        public BehaviorManagerSelector(Func<string,string> onSelectedChanged)
        {
            this.AddToClassList("Selector");
            var dropdown = new PopupField<string>(GetDropdownList(),0);
            dropdown.label = "Manager";
            this.Add(dropdown);
            dropdown.formatSelectedValueCallback += onSelectedChanged;
        }

        private List<string> GetDropdownList()
        {
            var menuItems = new List<string>();
            menuItems.Add("None");

            var managers = from manager in UnityEngine.Object.FindObjectsOfType<BehaviorManager>()
                           select manager.GetInstanceID() + "-" + manager.name;

            menuItems.AddRange(managers);

            return menuItems;
        }
    }
}