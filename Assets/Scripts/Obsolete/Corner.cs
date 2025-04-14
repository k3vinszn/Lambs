using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Steer2D
{

	public class Corner : MonoBehaviour {

		private Vector2 DesiredPosition;

		// Use this for initialization
		void Start () 
		{
			Transform mychildtransform = transform.parent.Find("BarrierB");
			DesiredPosition = mychildtransform.position;
		}
		
		void OnTriggerEnter2D(Collider2D other)
		{
			//Debug.Log ("hit " + other.name);


			if(other.tag == "Sheep")
			{
				if (!other.gameObject.GetComponent<SheepAI>().Arrive && !other.gameObject.GetComponent<SheepAI>().Fleeing)
				{
					other.gameObject.GetComponent<SheepAI>().Arrive = true;
					other.gameObject.GetComponent<SheepAI>().Fleeing = false;
					other.gameObject.GetComponent<SheepAI>().TargetPoint = DesiredPosition;

				}
			}

		}
	}
}
