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
using UniRx.Diagnostics;

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

        public Merge Runner { get; set; } = new Merge("Main Root", -1, -1);

        /// <summary>
        /// Seconds between every tick. At "0" this will tick every frame (basically an update loop)
        /// </summary>
        [SerializeField]
        
        [Description("Seconds between every tick. At 0 this will tick every frame")]
        public double MilliSecondsBetweenTicks = 100;

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

        public void InitIfNeeded()
        {
            if (initialized == false)
            {
                Reinitialize();
            }
        }

        public Subject<BehaviorTreeElement> TreeSubject { get; private set; }
        List<BehaviorTreeElement> treeList = new List<BehaviorTreeElement>();
        public void Reinitialize()
        {
            //TODO: Change to runner extension
            Runner = BehaviorTreeFile.LoadFromJSON(this);

            if(spliceNewIntoTree) SpliceIntoRunner();

            TreeElementUtility.TreeToList(Runner, treeList);

            var treeQuery = treeList.AsEnumerable();
            TreeSubject = new Subject<BehaviorTreeElement>();
            TreeSubject.Subscribe(xr =>
            {
                var logEntry = new BehaviorLogEntry(
                loggerName: BehaviorLogger.Name,
                logType: LogType.Log,
                timestamp: DateTime.Now,
                message: "Ticked!",
                behaviorID: xr.ID,
                newState: xr.CurrentState,
                ticknum: xr.NumberOfTicksReceived.Value,
                context: this,
                state: xr);
                BehaviorLogger.Raw(logEntry);
                Debug.Log("xr debug initialize");
            }).AddTo(this);

            initialized = true;
        }

        /// <summary>
        /// Ticks on the aggregate ParallelRunner then continues ticking for as long as the runner is in running state
        /// </summary>
        /// <returns></returns>
        //TODO: CHANGE TO Zip() instead of loop
        void Start()
        {
            InitIfNeeded();

            var timeStep = Observable.Interval(TimeSpan.FromMilliseconds(MilliSecondsBetweenTicks))
                .TakeWhile((_) => TimesToTick != 0)
                .Do(cx =>
                {
                    if (TimesToTick > 0) --TimesToTick;
                    Debug.Log(TimesToTick);
                    Runner.Start();
                    TreeSubject.OnNext(Runner);
                })
                .Debug("")
                .Subscribe()
                .AddTo(this);
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
