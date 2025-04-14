using UnityEngine;
using System.Collections.Generic;

namespace Steer2D
{
    public class SteeringAgent : MonoBehaviour
    {
        public float MaxVelocity = 1;
        public float Mass = 10;
        public float Friction = .05f;
        public bool RotateSprite = true;

        [HideInInspector]
        public Vector2 CurrentVelocity;

        public static List<SteeringAgent> AgentList = new List<SteeringAgent>();

        List<SteeringBehaviour> behaviours = new List<SteeringBehaviour>();

        public void RegisterSteeringBehaviour(SteeringBehaviour behaviour)
        {
            behaviours.Add(behaviour);
        }

        public void DeregisterSteeringBehaviour(SteeringBehaviour behaviour)
        {
            behaviours.Remove(behaviour);
        }

        void Start()
        {
            AgentList.Add(this);
        }

        void Update()
        {
			if (Game.ActiveLogic)
			{
	            Vector2 acceleration = Vector2.zero;

	            foreach (SteeringBehaviour behaviour in behaviours)
	            {
	                if (behaviour.enabled)
	                    acceleration += behaviour.GetVelocity() * behaviour.Weight;
			/*		print ("Accel" + acceleration);
					print ("Velocity" + behaviour.GetVelocity());
					print ("Weight" + behaviour.Weight); */
	            }
				//print ("Accel" + acceleration);
	            CurrentVelocity += acceleration / Mass;

	            CurrentVelocity -= CurrentVelocity * Friction;

	            if (CurrentVelocity.magnitude > MaxVelocity)
	                CurrentVelocity = CurrentVelocity.normalized * MaxVelocity;

	            transform.position = transform.position + (Vector3)CurrentVelocity * Time.deltaTime;

				/*// Clamp Sheep to visible Screen.
				float transX;
				float transY;
				transX = Mathf.Clamp(transform.position.x, -8.7f, 8.7f);
				transY = Mathf.Clamp(transform.position.y, -4.8f, 4.8f);
				transform.position = new Vector3(transX,transY,0);*/

	        
	            if (RotateSprite && CurrentVelocity.magnitude > 0.0001f)
	            {
	                float angle = Mathf.Atan2(CurrentVelocity.y, CurrentVelocity.x) * Mathf.Rad2Deg;

	                transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
	            }
			}
        }

        void OnDestroy()
        {
            AgentList.Remove(this);
        }
    }
}