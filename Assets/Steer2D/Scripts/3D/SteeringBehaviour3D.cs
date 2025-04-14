using UnityEngine;
using System.Collections;

namespace Steer2D
{
	[RequireComponent(typeof(SteeringAgent3D))]
	public abstract class SteeringBehaviour3D : MonoBehaviour {

        public float Weight = 1;

        protected SteeringAgent3D agent;

        public abstract Vector3 GetVelocity();

		void Start () {
            agent = GetComponent<SteeringAgent3D>();
            agent.RegisterSteeringBehaviour(this);
		}

		void OnDestroy()
		{
            if (agent != null)
                agent.DeregisterSteeringBehaviour(this);
		}   
	}
}