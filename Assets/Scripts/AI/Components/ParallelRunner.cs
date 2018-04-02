using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using UniRx;
using UniRx.Diagnostics;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    [Serializable]
    [Description("Runs all children at same time. Fails if NumFailures are >0 and children failed reach that number. Succeeds otherwise.")]
    public class ParallelRunner : BehaviorComponent
    {
       
        /// <summary>
        /// Number of times the children return fail before the parallel runner returns in a fail state.
        /// 0 means ignore number of failures.
        /// 0 for both succeed and fail means loops infinitely
        /// </summary>
        [SerializeField] public int NumberOfFailuresBeforeFail = 0;
        protected IntReactiveProperty NumberOfFailures = new IntReactiveProperty(0);

        /// <summary>
        /// Number of times the children return success before the parallel runner returns in a success state.
        /// 0 means ignore number of sucesses.
        /// 0 for both succeed and fail means loops infinitely
        /// </summary>
        [SerializeField] public int NumberOfSuccessBeforeSucceed = 0;
        protected IntReactiveProperty NumberOfSuccesses = new IntReactiveProperty(0);

        public ParallelRunner(string name, int depth, int id)
            : base(name, depth, id) { }

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            //Initialize and start tick
            base.Tick().ToObservable()
                //.Do(_ => BehaviorLogger.Log("Subscribed to ParallelRunner at start (base.tick()"))
                .Subscribe();
            CurrentState = BehaviorState.Running;

            if (Children == null || Children.Count == 0)
            {
                Debug.LogWarning("Children Null in parallel runner");
                CurrentState = BehaviorState.Fail;
                yield break;
            }

            foreach(var ch in Children)
            {
                ((BehaviorTreeElement)ch).Tick()
                    .ToObservable()
                    .Do(_ =>
                    {
                        //BehaviorLogger.Log(Name + ": Num Succeed: " + NumberOfSuccesses.Value);
                        //BehaviorLogger.Log(Name + ": Num Fail: " + NumberOfFailures.Value);
                        if (NumberOfFailures.Value >= NumberOfFailuresBeforeFail && NumberOfFailuresBeforeFail > 0)
                        {
                            CurrentState = BehaviorState.Fail;
                            return;
                        }

                        if (NumberOfSuccesses.Value >= NumberOfSuccessBeforeSucceed && NumberOfSuccessBeforeSucceed > 0)
                        {
                            CurrentState = BehaviorState.Success;
                            return;
                        }
                    })
                    .Subscribe().AddTo(Disposables);
            }
            if (CurrentState == BehaviorState.Running) CurrentState = BehaviorState.Success;
        }

        public override void Initialize()
        {
            ObservableLogger.Listener.LogToUnityDebug();

            var allChildrenToRun = from x in Children
                                   select x as BehaviorTreeElement;

            foreach (var ch in allChildrenToRun)
            {
                //TODO: will be changed to an actual debugger instead of just unity logs. Issue #3
                ch.ObserveEveryValueChanged(x => x.CurrentState).Subscribe(x =>
                {
                    //BehaviorLogger.Warning(ElementType + " state changed: " + x);
                    if (x == BehaviorState.Fail)
                    {
                        NumberOfFailures.SetValueAndForceNotify(NumberOfFailures.Value + 1);
                    }
                    else if (x == BehaviorState.Success)
                    {
                        NumberOfSuccesses.SetValueAndForceNotify(NumberOfSuccesses.Value + 1);
                    }
                }).AddTo(Disposables);
            }

            Initialized = true;
        }
    }
}
