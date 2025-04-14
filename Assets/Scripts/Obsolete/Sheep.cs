using UnityEngine;
using System.Collections;

public class Sheep : MonoBehaviour {

	public GameObject Player;

	public float speed;
	public Vector3 direction;
	public Vector3 randomFactor;
	public float distance;

	public bool isIn = false;

	// Use this for initialization
	void Awake ()
	{
		Player = GameObject.FindGameObjectWithTag("Player");

        
	}
	
	// Update is called once per frame
	void Update ()
	{
		//calculate distance between the player and this object, ignoring Y
		distance = Vector3.Distance(new Vector3(Player.transform.position.x, 0, Player.transform.position.z),
		                            new Vector3(transform.position.x, 0, transform.position.z));

		randomFactor = new Vector3(Random.Range(0.1f,1),0,Random.Range(0.1f,1));

		if(!isIn)
		{
			

			if(distance < 2)
			{

				direction = (transform.position - Player.transform.position).normalized;

				speed = (0.01f / distance * 200) - (distance / 10);

				GetComponent<Rigidbody>().linearVelocity = direction * speed;
			}
		}
		else
		{
			GetComponent<Renderer>().material.color = new Color(0.5f, 1, 0.5f);

			//slow down sheep
			if(GetComponent<Rigidbody>().linearVelocity.z > 0)
			{
				GetComponent<Rigidbody>().linearVelocity = new Vector3 (0, 0, GetComponent<Rigidbody>().linearVelocity.z - 0.01f); 
			}
			else
			{
				GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			}
		}

	}
}
