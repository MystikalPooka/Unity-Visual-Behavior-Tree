using Assets.Scripts.AI.Components;
using Assets.Scripts.AI.Tree;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Assets.Scripts.AI
{
    public class BehaviorManager : MonoBehaviour
    {
        public string FileName = "";
        /// <summary>
        /// The file to actually save/load to/from.
        /// </summary>
        [JsonIgnore]
        public BehaviorTreeManagerAsset BehaviorTreeFile;

        /// <summary>
        /// Primary Runner for this manager. 
        /// Runs all sub-behaviors/trees at the same time using the specified parallelrunner attributes.
        /// </summary>
        private ParallelRunner runner = new ParallelRunner("Main Root", -1, -1);
        public ParallelRunner Runner
        {
            get
            {
                return runner;
            }

            set
            {
                runner = value;
            }
        }

        /// <summary>
        /// Seconds between every tick. At "0" this will tick every frame (basically an update loop)
        /// </summary>
        [SerializeField]
        [Description("Seconds between every tick. At 0 this will tick every frame")]
        public float SecondsBetweenTicks = 0.1f;

        /// <summary>
        /// Number of times to tick the full trees. Set to a negative number to make an infinitely running behavior tree.
        /// </summary>
        [SerializeField]

        public int TimesToTick = 10;


        public bool spliceNewIntoTree = false;
        /// <summary>
        /// A list of trees to splice into the current tree. These are not directly editable.
        /// </summary>
        [JsonIgnore]
        public List<BehaviorTreeManagerAsset> SpliceList;

        private bool initialized = false;

        void OnEnable()
        {
            InitIfNeeded();
        }

        public void InitIfNeeded()
        {
            if (initialized == false)
            {
                Reinitialize();
            }
        }

        public void Reinitialize()
        {
            //TODO: Change to runner extension (?)
            Runner = BehaviorTreeFile.LoadFromJSON(this);

            
            if(spliceNewIntoTree) SpliceIntoRunner();
            initialized = true;
        }

        //TODO: Add ILogger *(perhaps Observer pattern? This is our "singleton")*
        //Dispatch messages to observed classes and receive that information here...
        //How to store? List? Dictionary? My face? Cat Pictures?

        /// <summary>
        /// Ticks on the aggregate ParallelRunner then continues ticking for as long as the runner is in running state
        /// </summary>
        /// <returns></returns>
        IEnumerator Start()
        {
            WaitForSeconds wfs = new WaitForSeconds(SecondsBetweenTicks);

            Debug.Log("Starting ticks on Runner: \n\t" + Runner.ToString());
            yield return Runner.Tick();
            while (Runner.CurrentState == BehaviorState.Running && (TimesToTick != 0))
            {
                yield return StartCoroutine(Runner.Tick(wfs));
                --TimesToTick;
            }

            Debug.Log("All Coroutines Should be DONE now! Ending all to make sure....");
            StopAllCoroutines();
        }

        /// <summary>
        /// Splice all trees in the "splice" area of the editor and return "true" if new trees were spliced.
        /// </summary>
        /// <returns></returns>
        public bool SpliceIntoRunner()
        {
            if (SpliceList != null)
            {
                foreach (var behaviorAsset in SpliceList)
                {
                    if (behaviorAsset == null) return false;

                    var spliceTree = behaviorAsset.LoadFromJSON();
                    
                    foreach (var behavior in spliceTree.Children)
                    {
                        if (behavior.Depth == -1 || behavior.Name == "root") continue;

                        dynamic newBehavior = Activator.CreateInstance(Type.GetType(((BehaviorTreeElement)behavior).ElementType),
                                                                        behavior.Name, behavior.Depth, behavior.ID);
                        newBehavior.BehaviorTreeManager = this;
                        Runner.AddChild(newBehavior);
                    }
                }

                return true;
            }
            else return false;
        }
    }
}
