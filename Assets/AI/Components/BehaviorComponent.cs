using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Assets.Scripts.AI.Components
{
    [System.Serializable]
    public abstract class BehaviorComponent : BehaviorTreeElement
    {
        public BehaviorComponent(string name, int depth, int id) 
            : base(name, depth, id)
        {
            Children = new List<Tree.TreeElement>();
        }

        public virtual void AddChild(BehaviorTreeElement element)
        {
            element.Parent = this;

            element.BehaviorTreeManager = BehaviorTreeManager;
            Children.Add(element);
        }


    }
}
