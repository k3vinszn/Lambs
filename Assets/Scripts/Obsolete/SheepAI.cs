using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Steer2D
{
    public class SheepAI : SteeringBehaviour
    {
        public SteeringAgent TargetAgent;

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

		public Vector2 TargetPoint = Vector2.zero;
		private Vector2 ReflectVelocity = Vector2.zero;
		private Vector2 CurrentVelocity =  Vector2.zero;
		public float SlowRadius = 1;
		public float StopRadius = 0.2f;

		private float DefaultSeparation;
		public bool DrawGizmos = false;

		private AudioSource sfx;

		private Animator anim;
		
		List<SteeringAgent> neighbouringAgents = new List<SteeringAgent>();
		Vector2 currentPosition;

//		private sheepState State = sheepState.idle;

		void Awake()
		{
			DefaultSeparation = SeperationWeight;

			sfx = GetComponent<AudioSource> ();
			anim = GetComponent<Animator> ();
		}
			
		void FixedUpdate()
		{
			//ANIMATIONS
			anim.SetBool ("moving", Moving);
			anim.SetFloat ("h", CurrentVelocity.x*10);
			anim.SetFloat ("v", CurrentVelocity.y*10);
			Debug.Log ("My speed is " + CurrentVelocity.x + "h and " + CurrentVelocity.y + "y");
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

					TargetAgent.GetComponent<Player>().Bark();
				}
				else
				{
					SeperationWeight = DefaultSeparation;
					Fleeing = false;
				}
			}

			if (CurrentVelocity != Vector2.zero && !Moving)
			{
				Moving = true;
				Game.MovingSheeps++;
				Baah ();
			}
			else if (CurrentVelocity == Vector2.zero && Moving && Game.PathComplete)
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
				sfx.pitch = Random.Range (1.0f, 1.5f);
				sfx.Play ();

				GetComponent<ParticleSystem>().Play ();
			}

			//print ("Barked with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
		}

		public override Vector2 GetVelocity()
		{

			if (Arrive == true)
			{
				float distance = Vector3.Distance(transform.position, (Vector3)TargetPoint);

				Vector2 desiredVelocity = (TargetPoint - (Vector2)transform.position).normalized;

				if (distance < StopRadius)
				{
					desiredVelocity = Vector2.zero;
				}
				else if (distance < SlowRadius)
					desiredVelocity = desiredVelocity * agent.MaxVelocity * ((distance - StopRadius) / (SlowRadius - StopRadius));
				else
					desiredVelocity = desiredVelocity * agent.MaxVelocity;

				if (agent.CurrentVelocity == Vector2.zero)
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
				Vector2 targetPoint = (Vector2)TargetAgent.transform.position + TargetAgent.CurrentVelocity * t;

				CurrentVelocity = -(((targetPoint - (Vector2)transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity);
				return CurrentVelocity;
				
			}
		
			else
			{
				//return Vector2.zero;

			//FLOCK STUFF
				currentPosition = (Vector2)transform.position;
			//	Debug.Log(Arrive+"!Arrive");

				UpdateNeighbouringAgents();

				CurrentVelocity = alignment() * AlignmentWeight + cohesion() * CohesionWeigth + seperation() * SeperationWeight;
				
				return CurrentVelocity;
			}
		}
	
//////////////////// FLOCK STUFF////////////////// ////////////////// ////////////////// ////////////////// 
		


			Vector2 alignment()
		{
			Vector2 averageDirection = Vector2.zero;
			
			if (neighbouringAgents.Count == 0)
				return averageDirection;
			
			foreach (var agent in neighbouringAgents)
				averageDirection += agent.CurrentVelocity;
			
			averageDirection /= neighbouringAgents.Count;
			return averageDirection.normalized;
		}
		
		Vector2 cohesion()
		{
			Vector2 averagePosition = Vector2.zero;
			
			foreach (var agent in neighbouringAgents)
				averagePosition += (Vector2)agent.transform.position;
			
			averagePosition /= neighbouringAgents.Count;
			
			return (averagePosition - currentPosition).normalized;
		}
		
		Vector2 seperation()
		{
			Vector2 moveDirection = Vector2.zero;
			
			foreach (var agent in neighbouringAgents)
				moveDirection += (Vector2)agent.transform.position - currentPosition;
			
			return (moveDirection * -1);
		}
		
		void UpdateNeighbouringAgents()
		{
			neighbouringAgents.Clear();
			
			foreach (var agent in SteeringAgent.AgentList)
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


		void OnCollisionEnter2D (Collision2D col)
		{
			if(col.gameObject.tag != "Sheep")
			{
				ContactPoint2D cp = col.contacts[0];

				ReflectVelocity = Vector2.Reflect(CurrentVelocity,cp.normal);
				Colliding = true;
				Fleeing = false;
				Arrive = false;
			}
		}

		void OnCollisionExit2D (Collision2D col)
		{
			if(col.gameObject.tag != "Sheep")
			{
				Colliding = false;
			}
		}

    }


}
