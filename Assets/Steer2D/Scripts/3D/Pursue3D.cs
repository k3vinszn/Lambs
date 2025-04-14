using System;
using UnityEngine;

namespace Steer2D
{
    public class Pursue3D : SteeringBehaviour3D
    {
        public SteeringAgent3D TargetAgent;

        public override Vector3 GetVelocity()
        {
            float t = Vector3.Distance(transform.position, TargetAgent.transform.position) / TargetAgent.MaxVelocity;
            Vector3 targetPoint = TargetAgent.transform.position + TargetAgent.CurrentVelocity * t;

            return ((targetPoint - transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity;
        }
    }
}
