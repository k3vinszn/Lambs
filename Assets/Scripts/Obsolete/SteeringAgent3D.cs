using UnityEngine;
using System.Collections.Generic;

namespace Steer2D
{
    public class SteeringAgent3D : MonoBehaviour
    {
        public float MaxVelocity = 1;
        public float Mass = 10;
        public float Friction = .05f;
        public bool RotateObject = true;
        public bool reachedGoal = false;
        public Vector3 goal = new Vector3(10, 0, 10);

        [HideInInspector]
        public Vector3 CurrentVelocity;

        public static List<SteeringAgent3D> AgentList = new List<SteeringAgent3D>();

        List<SteeringBehaviour3D> behaviours = new List<SteeringBehaviour3D>();

        public void RegisterSteeringBehaviour(SteeringBehaviour3D behaviour)
        {
            behaviours.Add(behaviour);
        }

        public void DeregisterSteeringBehaviour(SteeringBehaviour3D behaviour)
        {
            behaviours.Remove(behaviour);
        }

        void Start()
        {
            AgentList.Add(this);
        }

        public void ReachGoal(Vector3 gtarget)
        {
            reachedGoal = true;
            goal = gtarget;
        }

        void Update()
        {
			if (Game.ActiveLogic && !reachedGoal)
			{
	            Vector3 acceleration = Vector3.zero;

	            foreach (SteeringBehaviour3D behaviour in behaviours)
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

	            transform.position = transform.position + CurrentVelocity * Time.deltaTime;

				//transform.right = CurrentVelocity;

				/*// Clamp Sheep to visible Screen.
				float transX;
				float transY;
				transX = Mathf.Clamp(transform.position.x, -8.7f, 8.7f);
				transY = Mathf.Clamp(transform.position.y, -4.8f, 4.8f);
				transform.position = new Vector3(transX,transY,0);*/

	        
	            if (RotateObject && CurrentVelocity.magnitude > 0.0001f)
	            {
					transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(CurrentVelocity), 10*Time.deltaTime);
	            }
			}
            else if (Game.ActiveLogic && reachedGoal)
            {
                transform.position = Vector3.MoveTowards(transform.position, goal, MaxVelocity * Time.deltaTime);

                if (RotateObject)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(goal), MaxVelocity * Time.deltaTime);
                }
                
            }
        }

		void FixedUpdate()
		{
			/*Quaternion deltaRotation = Quaternion.Euler(CurrentVelocity * Time.deltaTime);
			GetComponent<Rigidbody>().MoveRotation(GetComponent<Rigidbody>().rotation * deltaRotation);*/
		}

        void OnDestroy()
        {
            AgentList.Remove(this);
        }
    }
}