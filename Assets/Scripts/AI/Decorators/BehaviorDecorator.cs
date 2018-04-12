using System.Collections;
using System.Collections.Generic;

namespace Assets.Scripts.AI.Decorators
{
    public abstract class BehaviorDecorator : BehaviorTreeElement
    {
        public BehaviorDecorator(string name, int depth, int id) 
            : base(name, depth, id)
        {
            Children = new List<Tree.TreeElement>();
        }

        public override IEnumerator Tick(UnityEngine.WaitForSeconds delayStart = null)
        {
            return base.Tick(delayStart);
        }
    } 
}
