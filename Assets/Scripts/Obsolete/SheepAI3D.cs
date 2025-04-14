using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Steer2D
{
    public class SheepAI3D : SteeringBehaviour3D
    {
        public SteeringAgent3D TargetAgent;

        public float FleeRadius = 1.0f;
		public float distance;
		public bool Fleeing = false;
		public bool Arrive = false;
		public bool Colliding = false;
		public bool Moving = false;

        public float NeighbourRadius = 0.3f;
		public float AlignmentWeight = 1;
		public float CohesionWeigth = 0.1f;
		public float SeperationWeight = 0;
		public float SeperationMultiplier;

		public Vector3 TargetPoint = Vector3.zero;
		private Vector3 ReflectVelocity = Vector3.zero;
		private Vector3 CurrentVelocity =  Vector3.zero;
		public float SlowRadius = 1;
		public float StopRadius = 0.2f;

		private float DefaultSeparation;
		public bool DrawGizmos = false;

		private AudioSource sfx;

		private Animator anim;
		
		List<SteeringAgent3D> neighbouringAgents = new List<SteeringAgent3D>();
		Vector3 currentPosition;

//		private sheepState State = sheepState.idle;

		void Awake()
		{
			DefaultSeparation = SeperationWeight;

			sfx = GetComponent<AudioSource> ();
			anim = GetComponent<Animator> ();

            transform.eulerAngles = new Vector3(transform.rotation.x, Random.Range(0, 360), transform.rotation.z);
        }
			
		void FixedUpdate()
		{
			//ANIMATIONS
			/*anim.SetBool ("moving", Moving);
			anim.SetFloat ("h", CurrentVelocity.x*10);
			anim.SetFloat ("v", CurrentVelocity.z*10);
			Debug.Log ("My speed is " + CurrentVelocity.x + "h and " + CurrentVelocity.z + "y");*/
		}

		void Update()
		{ 
			//test barking sfx
			if (Input.GetKeyDown (KeyCode.Space))
			{
				Baah ();
			}

			if (Game.ActiveLogic)
			{
				distance = Vector3.Distance(transform.position, TargetAgent.transform.position);
				
				if (distance < FleeRadius )
				{
					SeperationWeight = (SeperationWeight + (FleeRadius - distance)) * SeperationMultiplier;
					Fleeing = true;
					Arrive = false;

					TargetAgent.GetComponent<Doggy>().Bark();
				}
				else
				{
					SeperationWeight = DefaultSeparation;
					Fleeing = false;
				}
			}

			if (CurrentVelocity != Vector3.zero && !Moving)
			{
				Moving = true;
				Game.MovingSheeps++;
				Baah ();
			}
			else if (CurrentVelocity == Vector3.zero && Moving && Game.PathComplete)
			{
				Moving = false;
				Game.MovingSheeps--;
			}
		}

		public void Baah()
		{
			if (!sfx.isPlaying)
			{
				sfx.clip = (AudioClip)Resources.Load ("SFX/sheep/sheep" + Random.Range (1, 6));
				sfx.pitch = Random.Range (0.9f, 1.5f);
				sfx.Play ();

				GetComponent<ParticleSystem>().Play ();
			}

			//print ("Barked with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
		}

		public override Vector3 GetVelocity()
		{

			if (Arrive == true)
			{
				float distance = Vector3.Distance(transform.position, TargetPoint);

				Vector3 desiredVelocity = (TargetPoint - transform.position).normalized;

				if (distance < StopRadius)
				{
					desiredVelocity = Vector3.zero;
				}
				else if (distance < SlowRadius)
					desiredVelocity = desiredVelocity * agent.MaxVelocity * ((distance - StopRadius) / (SlowRadius - StopRadius));
				else
					desiredVelocity = desiredVelocity * agent.MaxVelocity;

				if (agent.CurrentVelocity == Vector3.zero)
					Arrive = false;

				CurrentVelocity = desiredVelocity - agent.CurrentVelocity;
				
				return CurrentVelocity; 
					// * (FleeRadius - distance)* SeperationMultiplier;
			}

			if (Colliding == true)
			{
	
				CurrentVelocity = ReflectVelocity * 10;
				Debug.DrawRay(transform.position, ReflectVelocity * 2, Color.magenta);
				return ReflectVelocity;

			}

			if(Fleeing)
			{
				
				float t = distance / TargetAgent.MaxVelocity;
				Vector3 targetPoint = TargetAgent.transform.position + TargetAgent.CurrentVelocity * t;

				CurrentVelocity = -(((targetPoint - transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity);
				return CurrentVelocity;
				
			}
		
			else
			{
				//return Vector3.zero;

			//FLOCK STUFF
				currentPosition = transform.position;
			//	Debug.Log(Arrive+"!Arrive");

				UpdateNeighbouringAgents();

				CurrentVelocity = alignment() * AlignmentWeight + cohesion() * CohesionWeigth + seperation() * SeperationWeight;
				
				return CurrentVelocity;
			}
		}
	
//////////////////// FLOCK STUFF////////////////// ////////////////// ////////////////// ////////////////// 
		


			Vector3 alignment()
		{
			Vector3 averageDirection = Vector3.zero;
			
			if (neighbouringAgents.Count == 0)
				return averageDirection;
			
			foreach (var agent in neighbouringAgents)
				averageDirection += agent.CurrentVelocity;
			
			averageDirection /= neighbouringAgents.Count;
			return averageDirection.normalized;
		}
		
		Vector3 cohesion()
		{
			Vector3 averagePosition = Vector3.zero;
			
			foreach (var agent in neighbouringAgents)
				averagePosition += agent.transform.position;
			
			averagePosition /= neighbouringAgents.Count;
			
			return (averagePosition - currentPosition).normalized;
		}
		
		Vector3 seperation()
		{
			Vector3 moveDirection = Vector3.zero;
			
			foreach (var agent in neighbouringAgents)
				moveDirection += agent.transform.position - currentPosition;
			
			return (moveDirection * -1);
		}
		
		void UpdateNeighbouringAgents()
		{
			neighbouringAgents.Clear();
			
			foreach (var agent in SteeringAgent3D.AgentList)
			{
				if (Vector3.Distance(agent.transform.position, currentPosition) < NeighbourRadius)
					neighbouringAgents.Add(agent);
			}
		}
////////////////////// ////////////////// ////////////////// ////////////////// ////////////////// 

        void OnDrawGizmos()
        {
            if (DrawGizmos)
            {
	                Gizmos.color = Color.gray;
	                Gizmos.DrawWireSphere(TargetAgent.transform.position, FleeRadius);

					Gizmos.color = Color.white;
					Gizmos.DrawWireSphere(transform.position, NeighbourRadius);
            }

        }


		void OnCollisionEnter (Collision col)
		{
			if(col.gameObject.tag != "Sheep")
			{
				ContactPoint cp = col.contacts[0];

				ReflectVelocity = Vector3.Reflect(CurrentVelocity,cp.normal);
				Colliding = true;
				Fleeing = false;
				Arrive = false;
			}
		}

		void OnCollisionExit (Collision col)
		{
			if(col.gameObject.tag != "Sheep")
			{
				Colliding = false;
			}
		}

    }


}
