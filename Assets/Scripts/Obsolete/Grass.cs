using System;
using UnityEngine;
using System.Collections.Generic;

namespace Steer2D
{
	public class Grass : MonoBehaviour {

		private SteeringAgent TargetAgent;
		public SteeringAgent PlayerAgent;

		public float ArriveRadius = 2.0f;
		public float distance;

//		List<SteeringAgent> neighbouringAgents = new List<SteeringAgent>();


		// Use this for initialization
		void Awake () {

		}

		// Update is called once per frame
		void Update () 
		{
			if (Game.ActiveLogic)
				FindTarget();
		}
			
	
		void FindTarget()
		{

			foreach (var agent in SteeringAgent.AgentList)
			{
				if (Vector3.Distance(transform.position, PlayerAgent.transform.position) > ArriveRadius)
				{
					if ( (Vector3.Distance(agent.transform.position, transform.position) < ArriveRadius) && agent.tag == "Sheep")
					{
						if (!agent.GetComponent<SheepAI>().Arrive && !agent.GetComponent<SheepAI>().Fleeing)
						{
							agent.GetComponent<SheepAI>().Arrive = true;
							agent.GetComponent<SheepAI>().Fleeing = false;
							agent.GetComponent<SheepAI>().TargetPoint = transform.position;
							TargetAgent = agent;
				
						}
					}
				}
			}
		}
	}
}
