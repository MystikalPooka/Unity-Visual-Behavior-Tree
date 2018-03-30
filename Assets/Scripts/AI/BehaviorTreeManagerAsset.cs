using Assets.Scripts.AI.Components;
using Assets.Scripts.AI.Tree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.AI
{
    [System.Serializable]
    public class BehaviorTreeManagerAsset : ScriptableObject
    {
        public int TimesToTick;
        public double SecondsBetweenTicks = 10;

        public string RunnerElementsJSON;
    }

    public static class AssetExtensions
    {
        public static ParallelRunner LoadFromJSON(this BehaviorTreeManagerAsset asset, BehaviorManager manager = null)
        {
            //TODO: Reload button
            //TODO: Confirm reload from json
            if (asset == null)
            {
                Debug.Log("Asset is null when loading");
                return new ParallelRunner("Empty Root", -1, -1);
            }
            else
            {
                //Elements should be a list of dynamic objects
                var elements =  JsonConvert.DeserializeObject<List<dynamic>>(asset.RunnerElementsJSON);

                var newElements = new List<BehaviorTreeElement>();
                foreach(dynamic el in elements)
                {
                    string typeName = el.ElementType;
                    Type type = Assembly.GetAssembly(typeof(BehaviorTreeElement)).GetType(typeName);
                    dynamic newBehavior = Activator.CreateInstance(type, (string)el.Name, (int)el.Depth, (int)el.ID);

                    JsonConvert.PopulateObject(JsonConvert.SerializeObject(el), newBehavior);
                    newElements.Add(newBehavior);
                }
                var str = "";
                foreach(var e in newElements)
                {
                    e.BehaviorTreeManager = manager;
                    str += e.Name + "\n";
                }

                var tree = TreeElementUtility.ListToTree(newElements);

                return (ParallelRunner)tree;
            }
        }
    }

}
