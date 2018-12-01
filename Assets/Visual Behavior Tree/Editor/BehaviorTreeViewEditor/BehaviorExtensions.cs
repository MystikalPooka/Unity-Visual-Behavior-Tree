using Assets.Scripts.AI;
using Assets.Scripts.AI.Components;
using Assets.Scripts.AI.Tree;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor.BehaviorTreeViewEditor
{
    public static class BehaviorExtensions
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }

        public static void CreateTypeMenu<T>(this GenericMenu menu, GenericMenu.MenuFunction2 func) where T : class
        {
            foreach (Type type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                var menuStrings = type.ToString().Split('.');
                menu.AddItem(new GUIContent(menuStrings[menuStrings.Length - 2] +
                      "/" + menuStrings.Last()), false, func, type.ToString());
            }
        }

        public static void CreateManagerMenu(this GenericMenu menu, GenericMenu.MenuFunction2 func)
        {
            var managers = UnityEngine.Object.FindObjectsOfType<BehaviorManager>();
            foreach (BehaviorManager manager in managers)
            {
                string menuName = manager.BehaviorLogger.Name;
                menu.AddItem(new GUIContent(menuName), false, func, menuName);
            }
        }

        public static Color GetBehaviorStateColor(this BehaviorState state)
        {
            switch (state)
            {
                case BehaviorState.Fail:
                    return Color.red;
                case BehaviorState.Running:
                    return Color.blue;
                case BehaviorState.Success:
                    return new Color(0.1f, 0.9f, 0.2f);
                case BehaviorState.Null:
                    return Color.grey;
                default:
                    return Color.black;
            }
        }


        /// <summary>
        /// Saves a scriptable object behavior tree and sets the active asset back to the behavior manager
        /// </summary>
        /// <param name="behaviorManager"></param>
        /// <param name="filePath"></param>
        /// <param name="asset"></param>
        public static void SaveBehaviorAsset(this BehaviorManager behaviorManager, string filePath, 
            BehaviorTreeManagerAsset asset, ParallelRunner root = null)
        {
            if (asset == null)
                asset = ScriptableObject.CreateInstance<BehaviorTreeManagerAsset>();

            var runnerElementList = new List<BehaviorTreeElement>();

            Debug.Log("Attempting save at path: " + filePath);

            int indexS = filePath.LastIndexOf("/") + 1;
            int indexD = filePath.LastIndexOf(".") - indexS;

            asset.name = filePath.Substring(indexS, indexD);

            var json = asset.RunnerElementsJSON;

            if(behaviorManager != null)
            {
                behaviorManager.Reinitialize();
                asset.SecondsBetweenTicks = behaviorManager.SecondsBetweenTicks;
                asset.TimesToTick = behaviorManager.TimesToTick;

                TreeElementUtility.TreeToList(behaviorManager.Runner, runnerElementList);  
            }

            if(root != null)
            {
                TreeElementUtility.TreeToList(root, runnerElementList);
            }

            if(json == "" || runnerElementList.Count == 0)
            {
                var runner = new ParallelRunner("Extension Root", -1, -1);
                runnerElementList.Add(runner);

                json = JsonConvert.SerializeObject(runnerElementList, Formatting.Indented);
            }
            json = JsonConvert.SerializeObject(runnerElementList, Formatting.Indented);
            asset.RunnerElementsJSON = json;

            Debug.Log("JSON Saved: " + asset.RunnerElementsJSON);

            var curPath = AssetDatabase.GetAssetPath(asset);

            if(curPath == null || curPath == "")
            {
                Debug.Log("Creating asset: " + filePath);
                AssetDatabase.CreateAsset(asset, filePath);
            }
                
            //AssetDatabase.Refresh();
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }
    }
}
