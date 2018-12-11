using System.Collections;
using System.Collections.Generic;
using UniRx;
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
            Children.Add(element);
        }

        public override System.IObservable<BehaviorState> Tick()
        {
            return Observable.Empty<BehaviorState>();
        }
        
    }
}
