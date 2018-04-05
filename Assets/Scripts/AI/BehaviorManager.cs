using Assets.Scripts.AI.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.ComponentModel;
using UniRx.Triggers;
using UniRx;
using Assets.Scripts.AI.Tree;
using System.Linq;
using Assets.Scripts.AI.Behavior_Logger;

namespace Assets.Scripts.AI
{
    public class BehaviorManager : MonoBehaviour
    {
        public BehaviorLogger BehaviorLogger { get; private set;}

        /// <summary>
        /// The file to actually save/load to/from.
        /// </summary>
        [JsonIgnore]
        [Description("The currently loaded tree asset that will be run.")]
        public BehaviorTreeManagerAsset BehaviorTreeFile;
        public ParallelRunner Runner { get; set; } = new ParallelRunner("Main Root", -1, -1);

        /// <summary>
        /// Seconds between every tick. At "0" this will tick every frame (basically an update loop)
        /// </summary>
        [SerializeField]
        [Description("Seconds between every tick. At 0 this will tick every frame")]
        public float SecondsBetweenTicks = 0.1f;

        /// <summary>
        /// Number of times to tick the full tree. Set to a negative number to make an infinitely running behavior tree.
        /// </summary>
        [SerializeField]
        [Description("Times to tick this tree before stopping. Negative values indicate infinitely running behavior.")]
        public int TimesToTick = 10;

        [Description("Open a list to splice other trees into this tree.")]
        public bool spliceNewIntoTree = false;
        /// <summary>
        /// A list of trees to splice into the current tree. These trees are not directly editable from here.
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


        public IObservable<BehaviorTreeElement> treeStream { get; private set; }
        public void Reinitialize()
        {
            //TODO: Change to runner extension (?)
            Runner = BehaviorTreeFile.LoadFromJSON(this);

            if(spliceNewIntoTree) SpliceIntoRunner();

            List<BehaviorTreeElement> treeList = new List<BehaviorTreeElement>();

            TreeElementUtility.TreeToList(Runner, treeList);

            var treeQuery = from el in treeList
                            select el;


            BehaviorLogger = new BehaviorLogger(gameObject.name + " Logger");
            treeStream =
                treeQuery
                .ToObservable()
                .Do(xr =>
                {
                    xr.ObserveEveryValueChanged(x => x.NumberOfTicksReceived)
                    .Do(_ =>
                    {
                        BehaviorLogger.Debug(xr + " Ticked " + xr.NumberOfTicksReceived.Value + " times");
                    })
                    .Subscribe()
                    .AddTo(this);
                });

            treeStream.Subscribe().AddTo(this);

            initialized = true;
        }

        /// <summary>
        /// Ticks on the aggregate ParallelRunner then continues ticking for as long as the runner is in running state
        /// </summary>
        /// <returns></returns>
        IEnumerator Start()
        {
            while(TimesToTick != 0)
            {
                yield return Runner.Tick()
                                   .ToObservable(true)
                                   .Subscribe(_ => { }, e => Debug.LogError("Error: " + e))
                                   .AddTo(this);
                treeStream.Subscribe().AddTo(this);
                yield return new WaitForSeconds(SecondsBetweenTicks);
                if (TimesToTick > 1) --TimesToTick;
            }
        }

        /// <summary>
        /// Splice all trees in the "splice" area of the editor and return "true" if new trees were spliced.
        /// </summary>
        /// <returns></returns>
        /// 
        //TODO: Swap this to a better and/or reactive approach.
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
