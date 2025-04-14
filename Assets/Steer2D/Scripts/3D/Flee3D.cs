using System;
using UnityEngine;

namespace Steer2D
{
    public class Flee3D : SteeringBehaviour3D
    {
        public Vector3 TargetPoint = Vector3.zero;
        public float FleeRadius = 1;
        public bool DrawGizmos = false;

        public override Vector3 GetVelocity()
        {
            float distance = Vector3.Distance(transform.position, TargetPoint);

            if (distance < FleeRadius)
                return -(((TargetPoint - transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity);
            else
                return Vector3.zero;
        }

        void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(TargetPoint, FleeRadius);
            }
        }
    }
}
