using System;
using UnityEngine;

namespace Steer2D
{
    public class Seek3D : SteeringBehaviour3D
    {
        public Vector3 TargetPoint = Vector3.zero;

        public override Vector3 GetVelocity()
        {
            return ((TargetPoint - transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity;   
        }
    }
}
