using System;
using System.Collections;
using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.AI.Nodes
{
    [Serializable]
    public class MoveEnemyIfCloseToPlayer : BehaviorNode
    {
        private Transform myPos;
        private GameObject[] players;
        private int numTicks = 0;

        [SerializeField]
        [Description("Move Speed. Default = 1.0")]
        public float MoveSpeed = 1;

        /// <summary>
        /// The number of ticks between each full update of player objects. At "0" this will update EVERY TICK.
        /// This can be very inefficient and typically is not needed. Default is 10.
        /// </summary>
        [SerializeField]
        [Description("Frames between updating player list in the scene. " +
            "At 0 this ticks every frame which is typically not needed and is very inefficient.")]
        public int UpdatePlayersTickInterval = 20;

        /// <summary>
        /// Number of ticks between updating current position. Default is 5.
        /// </summary>
        [SerializeField]
        [Description("Frames between updating current position in the scene. " +
            "At 0 this ticks every frame which is typically not needed and can be inefficient.")]
        public int UpdateSelfPositionTickInterval = 5;

        public MoveEnemyIfCloseToPlayer(string name, int depth, int id) : base(name, depth, id)
        { }

        public override IEnumerator Tick(WaitForSeconds delayStart = null)
        {
            ++numTicks;
            if(UpdatePlayersTickInterval % numTicks == 0 || numTicks == 0)
            {
                players = GameObject.FindGameObjectsWithTag("Player");
            }
            if (UpdateSelfPositionTickInterval % numTicks == 0 || numTicks == 0)
            {
                myPos = BehaviorTreeManager.GetComponentInParent<Transform>();
            }

            if (players != null && players.Length > 0)
            {
                var closestPlayer = GetClosestEnemy(players);

                var targetPosition = closestPlayer.position;
                var currentPosition = myPos.position;

                if (Vector3.Distance(currentPosition, targetPosition) > myPos.GetComponentInParent<BoxCollider>().size.magnitude*2)
                {
                    var directionOfTravel = targetPosition - currentPosition;
                    CurrentState = (BehaviorState.Running);
                    float step = (MoveSpeed / directionOfTravel.magnitude) * Time.fixedDeltaTime;
                    float t = 0;
                    while(t <= 1.0f)
                    {
                        t += step; // Goes from 0 to 1, incrementing by step each time
                        myPos.position = Vector3.Lerp(currentPosition, targetPosition, t); // Move objectToMove closer to b
                        yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
                    }
                }
                CurrentState = BehaviorState.Success;
                yield return null;
            }

            CurrentState = BehaviorState.Fail;
            yield return null;
        }

        private Transform GetClosestEnemy(GameObject[] enemies)
        {
            Transform bestTarget = null;
            float closestDistanceSqr = Mathf.Infinity;
            Vector3 currentPosition = myPos.position;
            foreach (var potentialTarget in enemies)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget.transform;
                }
            }

            return bestTarget;
        }

        private IEnumerator MoveTowardsPosition(Transform pos)
        {
            var targetPosition = pos.position;
            var currentPosition = myPos.position;
            var directionOfTravel = targetPosition - currentPosition;

            float step = (MoveSpeed / directionOfTravel.magnitude) * Time.fixedDeltaTime;
            float t = 0;
            while (t <= 1.0f)
            {
                t += step; // Goes from 0 to 1, incrementing by step each time
                myPos.position = Vector3.Lerp(currentPosition, targetPosition, t); // Move objectToMove closer to b
                yield return new WaitForFixedUpdate();         // Leave the routine and return here in the next frame
            }
        }
    }
}
