using Assets.Scripts.AI;
using Assets.Scripts.AI.Tree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

    public static class AssetExtensions
    {
        public static BehaviorTreeElement LoadRoot(this TreeNodeAsset asset)
        {
            var elements = JsonConvert.DeserializeObject<List<dynamic>>(asset.treeElements);

            var behaviorElements = new List<BehaviorTreeElement>();
            foreach (dynamic el in elements)
            {
                string typeName = el.ElementType;
                Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(typeName);
                dynamic newBehavior = Activator.CreateInstance(type, (string)el.Name, (int)el.Depth, (int)el.ID);
                JsonConvert.PopulateObject(JsonConvert.SerializeObject(el), newBehavior);
                behaviorElements.Add(newBehavior);
            }
            return TreeElementUtility.ListToTree(behaviorElements);
        }
    }
}
