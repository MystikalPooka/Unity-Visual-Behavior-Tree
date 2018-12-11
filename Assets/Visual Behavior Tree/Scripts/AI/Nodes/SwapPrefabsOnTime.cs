using Assets.Scripts.AI;
using Assets.Scripts.AI.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Scripts.AI.Nodes
{   
    /// <summary>
    /// waits a specified time (or a number of frames) then swaps from one given prefab to the next
    /// </summary>
    [Serializable]
    class SwapPrefabsOnTime : BehaviorNode
    {
        [SerializeField]
        [Description("prefabs to select from in sequence.")]
        public GameObject[] prefabsToSwapBetween;

        [SerializeField]
        [Description("Number of received ticks before prefab goes to the next in the list.")]
        public int changeEveryTickNum = 10;

        public SwapPrefabsOnTime(string name, int depth, int id) : base(name, depth, id)
        {
        }

        public override BehaviorState Tick()
        {
            return base.Tick();
        }
    }
}
