using Assets.Scripts.AI;
using Assets.Scripts.AI.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

namespace Assets.Visual_Behavior_Tree.Scripts.AI.Nodes
{
    public class MoveToNode : BehaviorNode
    {
        public int TimeBetweenChecks;

        [Newtonsoft.Json.JsonIgnore]
        private Transform moveToPoint;

        [Newtonsoft.Json.JsonIgnore]
        private NavMeshAgent agent;

        private Vector3 lastDestination;

        public float destReachedThreshold;

        public MoveToNode(string name, int depth, int id) : base(name, depth, id)
        {}

        public override void Initialize()
        {
            base.Initialize();
            agent = this.Manager.GetComponent<NavMeshAgent>();
            moveToPoint = GameObject.FindGameObjectWithTag("Player").transform;
        }

        public override IObservable<BehaviorState> Start()
        {
            agent.SetDestination(moveToPoint.position);
            return Observable
                .Interval(TimeSpan.FromMilliseconds(TimeBetweenChecks))
                .TakeWhile(_ => !IsDestinationReached())
                .Select(x =>
                {
                    if (agent == null)
                    {
                        Debug.LogWarning("Unable to find agent in " + Name);
                        return Observable.Return(BehaviorState.Fail);
                    }
                    if (moveToPoint.position != lastDestination)
                    {
                        agent.SetDestination(moveToPoint.position);
                        lastDestination = moveToPoint.position;
                    }
                    return Observable.Return(BehaviorState.Running);
                })
                .TakeLast(1)
                .Select(x => BehaviorState.Success)
                .Do(state => DebugLogAction(state));
        }

        bool IsDestinationReached()
        {
            return agent.remainingDistance <= destReachedThreshold;
        }
    }
}
