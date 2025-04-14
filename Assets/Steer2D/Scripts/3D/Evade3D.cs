using System;
using UnityEngine;

namespace Steer2D
{
    public class Evade3D : SteeringBehaviour3D
    {
        public SteeringAgent3D TargetAgent;
        public float FleeRadius = 1.0f;
        public bool DrawGizmos = false;

        public override Vector3 GetVelocity()
        {
            float distance = Vector3.Distance(transform.position, TargetAgent.transform.position);

			if (distance < FleeRadius )

			{
                float t = distance / TargetAgent.MaxVelocity;
                Vector3 targetPoint = TargetAgent.transform.position + TargetAgent.CurrentVelocity * t;

                return -(((targetPoint - transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity);
            }
			else
                return Vector3.zero;
        }

        void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(TargetAgent.transform.position, FleeRadius);
            }
        }
    }
}
