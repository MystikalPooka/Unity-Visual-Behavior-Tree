using Assets.Scripts.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Visual_Behavior_Tree.Scripts
{
    public class TreeNodeAsset : ScriptableObject
    {
        public List<Rect> positions;

        public string treeElements;
    }
}
