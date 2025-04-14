using UnityEngine;
using System.Collections.Generic;

namespace Steer2D
{
    public class Arrive3D : SteeringBehaviour3D
    {
        public Vector3 TargetPoint = Vector3.zero;
        public float SlowRadius = 1;
        public float StopRadius = 0.2f;
        public bool DrawGizmos = false;

        public override Vector3 GetVelocity()
        {
            float distance = Vector3.Distance(transform.position, TargetPoint);
            Vector3 desiredVelocity = (TargetPoint - transform.position).normalized;

            if (distance < StopRadius)
                desiredVelocity = Vector3.zero;
            else if (distance < SlowRadius)
                desiredVelocity = desiredVelocity * agent.MaxVelocity * ((distance - StopRadius) / (SlowRadius - StopRadius));
            else
                desiredVelocity = desiredVelocity * agent.MaxVelocity;

            return desiredVelocity - agent.CurrentVelocity;
        }

        void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere((Vector3)TargetPoint, SlowRadius);

                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere((Vector3)TargetPoint, StopRadius);
            }
        }
    }
}