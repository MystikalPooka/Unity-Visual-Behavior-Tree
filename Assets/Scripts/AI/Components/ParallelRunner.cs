using System;
using System.Collections;
using System.ComponentModel;
using UniRx;
using UnityEngine;

namespace Assets.Scripts.AI.Components
{
    [Serializable]
    [Description("Runs all children at the same time.")]
    public class ParallelRunner : BehaviorComponent
    {
        /// <summary>
        /// Number of times the children return fail before the parallel runner returns in a fail state.
        /// 0 means ignore number of failures.
        /// 0 for both succeed and fail means loops infinitely
        /// </summary>
        [SerializeField] public int NumberOfFailuresBeforeFail = 0;
        private int NumberOfFailures = 0;

        /// <summary>
        /// Number of times the children return success before the parallel runner returns in a success state.
        /// 0 means ignore number of sucesses.
        /// 0 for both succeed and fail means loops infinitely
        /// </summary>
        [SerializeField] public int NumberOfSuccessBeforeSucceed = 0;
        private int NumberOfSuccesses = 0;

        public ParallelRunner(string name, int depth, int id)
            : base(name, depth, id) { }

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            base.Tick().ToObservable().Subscribe(xb => Debug.Log("Subscribed to ParallelRunner at start (base.tick()"));
            CurrentState = (BehaviorState.Running);
            if (Children == null || Children.Count <=0)
            {
                Debug.LogWarning("Children Null in parallel runner");
                CurrentState = (BehaviorState.Null);
                yield return null;
            }

            foreach(BehaviorTreeElement behavior in Children)
            {
                BehaviorTreeManager.StartCoroutine(behavior.Tick());

                if(NumberOfFailuresBeforeFail != 0 && behavior.CurrentState == BehaviorState.Fail)
                {
                    Debug.LogWarning("    Child returned fail: " + behavior.ToString());
                    ++NumberOfFailures;
                    if(NumberOfFailures >= NumberOfFailuresBeforeFail)
                    {
                        CurrentState = (BehaviorState.Fail);
                        yield break;
                    }
                }

                if (NumberOfSuccessBeforeSucceed != 0 && behavior.CurrentState == BehaviorState.Success)
                {
                    Debug.LogWarning("    Child returned success: " + behavior.ToString());
                    ++NumberOfSuccesses;
                    if (NumberOfSuccesses >= NumberOfSuccessBeforeSucceed)
                    {
                        CurrentState = (BehaviorState.Success);
                        yield break;
                    }
                }
                Debug.LogWarning("Ending Parallel Tick in Run State.");
                CurrentState = (BehaviorState.Running);
                yield return null;
            }
        }
    }
}
