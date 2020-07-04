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
using Assets.Visual_Behavior_Tree.Scripts;

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
                    behaviorLogger = new BehaviorLogger(gameObject.name + ": " + BehaviorTreeFiles.Count + " Trees");
                }
                return behaviorLogger;
            }

            private set
            {
                behaviorLogger = value;
            }
        }

        public List<Subject<BehaviorTreeElement>> Debuggers;

        /// <summary>
        /// The file to actually save/load to/from.
        /// </summary>
        [JsonIgnore]
        [Description("The currently loaded tree assets that will be run concurrently.")]
        public List<TreeNodeAsset> BehaviorTreeFiles;

        public BehaviorTreeElement Runner { get; set; }

        /// <summary>
        /// Seconds between every tick. At "0" this will tick every frame (basically an update loop)
        /// </summary>
        [SerializeField]
        [Description("Milliseconds between every tick. At 0 this will tick every frame")]
        public double MilliSecondsBetweenTicks = 100;

        /// <summary>
        /// Number of times to tick the full tree. Set to a negative number to make an infinitely running behavior tree.
        /// </summary>
        [SerializeField]
        [Description("Times to tick this tree before stopping. Negative values indicate infinitely running behavior.")]
        public int TimesToTick = 10;

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

            if (BehaviorTreeFiles.Count > 1)
            {
                Runner = ScriptableObject.CreateInstance<Merge>();
                Runner.Depth = -1;
                Runner.ID = -1;
                foreach (var asset in BehaviorTreeFiles)
                {
                    var childRoot = asset.LoadRoot();
                    Debug.Log("child root: " + childRoot);
                    ((Merge)Runner).AddChild(childRoot);
                }
            }
            else Runner = BehaviorTreeFiles.First().LoadRoot();

            var logStream = ObservableBehaviorLogger.Listener.Subscribe(log => Debug.Log("Manager: " + log));

            Runner.Manager = this;
            Runner.Initialize();

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
                    Runner.Start().Subscribe();
                })
                .Subscribe()
                .AddTo(this);
        }

        public void Dispose()
        {
            Runner.Dispose();
        }
    }
}
