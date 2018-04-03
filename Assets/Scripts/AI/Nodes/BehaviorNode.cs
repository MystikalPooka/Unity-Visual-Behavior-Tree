using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.Nodes
{
    public abstract class BehaviorNode : BehaviorTreeElement
    {
        public BehaviorNode(string name, int depth, int id) : base(name, depth, id)
        {}

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            yield return base.Tick(delayStart);
        }
    }
}