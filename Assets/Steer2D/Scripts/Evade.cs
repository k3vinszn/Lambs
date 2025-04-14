using System;
using UnityEngine;

namespace Steer2D
{
    public class Evade : SteeringBehaviour
    {
        public SteeringAgent TargetAgent;
        public float FleeRadius = 1.0f;
        public bool DrawGizmos = false;

        public override Vector2 GetVelocity()
        {
            float distance = Vector3.Distance(transform.position, TargetAgent.transform.position);

			if (distance < FleeRadius )

			{

                float t = distance / TargetAgent.MaxVelocity;
                Vector2 targetPoint = (Vector2)TargetAgent.transform.position + TargetAgent.CurrentVelocity * t;

                return -(((targetPoint - (Vector2)transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity);
            }
			else if ( (transform.position.y < 4.8f && transform.position.y > 4.4f) 
			         || (transform.position.y > -4.8f && transform.position.y < -4.4f) 
			         || (transform.position.x < 8.7f && transform.position.x > 8.3f)
			         || (transform.position.x > -8.7f && transform.position.x < -8.3f))
			{
				return ( (Vector2)transform.position) * -1;
			}
			else
                return Vector2.zero;
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
