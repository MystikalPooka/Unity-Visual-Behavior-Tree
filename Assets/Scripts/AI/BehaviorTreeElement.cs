using Assets.Scripts.AI.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI
{
    [Serializable]
    public class BehaviorTreeElement : TreeElement
    {
        private string _ElementType;
        public string ElementType
        {
            get
            {
                return _ElementType;
            }

            set
            {
                _ElementType = value;
            }
        }

        [Newtonsoft.Json.JsonIgnore]
        [SerializeField]
        private BehaviorManager _BehaviorTreeManager;
        [Newtonsoft.Json.JsonIgnore]
        public BehaviorManager BehaviorTreeManager
        {
            get
            {
                return _BehaviorTreeManager;
            }

            set
            {
                _BehaviorTreeManager = value;
            }
        }

        public BehaviorTreeElement(string name, int depth, int id) 
            : base(name, depth, id)
        {
            ElementType = this.GetType().ToString();
            CurrentState = (BehaviorState.Null);
            Children = new List<TreeElement>();
        }


        [Newtonsoft.Json.JsonIgnore]
        public BehaviorState CurrentState;

        

        public virtual IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            if (delayStart != null)
            {
                yield return delayStart;
            }
        }

        public override string ToString()
        {
            var depthPad = "";
            for (int d = 0; d < this.Depth +1; ++d)
            {
                depthPad += "     ";
            }
            var retString = depthPad + "ID: " + ID + "\n" +
                            depthPad + "Name: " + this.Name + "\n" +
                            depthPad + "Depth: " + Depth + "\n" +
                            depthPad + "Type: " + ElementType.ToString() + "\n" +
                            depthPad + "NumChildren: " + (HasChildren ? Children.Count : 0) + "\n";

            if (Children != null)
            {
                retString += depthPad + "Children: \n";
                foreach (var child in Children)
                {
                    retString += child.ToString();
                }
            }

            return retString;

        }
    }
}