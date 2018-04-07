using Assets.Scripts.AI.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.ComponentModel;
using UniRx;
using Assets.Scripts.AI.Tree;
using System.Linq;
using Assets.Scripts.AI.Behavior_Logger;

namespace Assets.Scripts.AI
{
    public class BehaviorManager : MonoBehaviour, IDisposable
    {
        private BehaviorLogger behaviorLogger;
        public BehaviorLogger BehaviorLogger
        {
            get
            {
                if(behaviorLogger == null)
                {
                    behaviorLogger = new BehaviorLogger(gameObject.name + ": " + BehaviorTreeFile.name + " Tree");
                }
                return behaviorLogger;
            }

            private set
            {
                behaviorLogger = value;
            }
        }

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

        public IObservable<BehaviorTreeElement> TreeStream { get; private set; }
        public void Reinitialize()
        {
            //TODO: Change to runner extension
            Runner = BehaviorTreeFile.LoadFromJSON(this);

            if(spliceNewIntoTree) SpliceIntoRunner();

            List<BehaviorTreeElement> treeList = new List<BehaviorTreeElement>();

            TreeElementUtility.TreeToList(Runner, treeList);

            var treeQuery = from el in treeList
                            select el;

            TreeStream =
                treeQuery
                .ToObservable()
                .Do(xr =>
                {
                    xr.ObserveEveryValueChanged(x => x.NumberOfTicksReceived)
                    .Do(_ =>
                    {
                        var logEntry = new BehaviorLogEntry(
                                loggerName: BehaviorLogger.Name,
                                logType: LogType.Log,
                                timestamp: DateTime.Now,
                                message: "Ticked!",
                                newState: xr.CurrentState,
                                ticknum: xr.NumberOfTicksReceived.Value,
                                context: this,
                                state: xr);
                        BehaviorLogger.Raw(logEntry);
                    })
                    .Subscribe()
                    .AddTo(this);
                });

            TreeStream.Subscribe().AddTo(this);

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
                TreeStream.Subscribe().AddTo(this);
                yield return new WaitForSeconds(SecondsBetweenTicks);
                if (TimesToTick >= 1) --TimesToTick;
            }
        }

        /// <summary>
        /// Splice all trees in the "splice" area of the editor and return "true" if new trees were spliced.
        /// </summary>
        /// <returns></returns>
        /// 
        //TODO: Swap this to a reactive approach.
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

        public void Dispose()
        {
            Runner.Dispose();
        }
    }
}
