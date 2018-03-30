using Assets.Scripts.AI.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.AI.ReactiveTEST
{
    public class ReactiveBehaviorTreeElement : TreeElement
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


    }
}
