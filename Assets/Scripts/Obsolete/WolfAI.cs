using UnityEngine;
using System.Collections.Generic;

namespace Steer2D
{
public class WolfAI : SteeringBehaviour {

	public SteeringAgent TargetAgent;
	
	public SteeringAgent PersuedAgent;
	public float StopRadius = 0.1f;
	public float SlowRadius = 0.12f;

	public float AttackRadius = 1.0f;
	public float FleeRadius = 1.0f;
	public float distance;
	public bool Persue = false;
	public bool Fleeing = false;

	public Vector3 TargetPosition;

	public float NeighbourRadius = 0.3f;
	public float AlignmentWeight = 1;
	public float CohesionWeigth = 0.1f;
	public float SeperationWeight = 3;

	public bool DrawGizmos = false;

	public List<SteeringAgent> neighbouringAgents = new List<SteeringAgent>();
	
	Vector2 currentPosition;

	// Use this for initialization
	void Awake () 
	{
		// PersuedAgent = null;
	}
	
	// Update is called once per frame
	void Update () 
	{
			if (Game.ActiveLogic)
			{
				currentPosition = (Vector2)transform.position;
				distance = Vector3.Distance(currentPosition, TargetAgent.transform.position);

				if (distance < FleeRadius)
				{
					Fleeing = true;
				}
				else
				{
					Fleeing = false;
					if (PersuedAgent == null)
						FindTarget();
				}
					
			}
	}
	
	
	
	public override Vector2 GetVelocity()
	{
		if (Persue)
		{

				float distance = Vector3.Distance(transform.position, PersuedAgent.transform.position);
				Vector2 desiredVelocity = ((Vector2)PersuedAgent.transform.position - (Vector2)transform.position).normalized;

				if (distance <= 0.45f)
				{
					//print ("GOTCHA!");
					Instantiate (Resources.Load ("EFX/BloodSplat"), PersuedAgent.transform.position, Quaternion.identity);

					Persue = false;

					//kill the sheep
					Destroy (PersuedAgent.gameObject);
					//SteeringAgent.Destroy(PersuedAgent);

					SteeringAgent.AgentList.Remove(PersuedAgent);
					//subtract the killed sheep from the end level condition
					Game.MaxGoal = Game.MaxGoal - 1;
				}

				if (distance < StopRadius)
					desiredVelocity = Vector2.zero;
				else if (distance < SlowRadius)
					desiredVelocity = desiredVelocity * agent.MaxVelocity * ((distance - StopRadius) / (SlowRadius - StopRadius));
				else
					desiredVelocity = desiredVelocity * agent.MaxVelocity;

				return desiredVelocity - agent.CurrentVelocity;
			
		
		}
				
			if(Fleeing)
			{
				Persue = false;
				float t = distance / TargetAgent.MaxVelocity;
				Vector2 targetPoint = (Vector2)TargetAgent.transform.position + TargetAgent.CurrentVelocity * t;

				return -(((targetPoint - (Vector2)transform.position).normalized * agent.MaxVelocity) - agent.CurrentVelocity);

			}
			else
			{

				//FLOCK STUFF
			
				//	Debug.Log(Arrive+"!Arrive");

				UpdateNeighbouringAgents();
				return Vector2.zero - agent.CurrentVelocity;
			//	return alignment() * AlignmentWeight + cohesion() * CohesionWeigth + seperation() * SeperationWeight;
			}
	}

		void FindTarget()
		{

			foreach (var agent in SteeringAgent.AgentList)
			{
					if ( (Vector3.Distance(agent.transform.position, currentPosition) < AttackRadius) && agent.tag != "Wolf" && !Persue)
					{
						PersuedAgent = agent;
						agent.GetComponent<SheepAI>().Fleeing = true;
						Debug.Log("Target Found" + agent.name);
					TargetPosition = PersuedAgent.transform.position;
						Persue = true;
				//	transform.rotation = Quaternion.LookRotation(Vector3.forward, agent.transform.position - transform.position);
					}
			}
		}


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
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(TargetAgent.transform.position, FleeRadius);

				Gizmos.color = Color.cyan;
				Gizmos.DrawWireSphere(transform.position, AttackRadius);
			}

		}

	}
}
