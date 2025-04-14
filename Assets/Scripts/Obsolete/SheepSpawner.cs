using UnityEngine;
using System.Collections;

public class SheepSpawner : MonoBehaviour {

	public int Amount = 25;
	private int currentNumber = 0;

	// Use this for initialization
	void Awake ()
	{
		SpawnSheeps();
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	void SpawnSheeps()
	{
		while(currentNumber < Amount)
		{
			Instantiate( (GameObject)Resources.Load("Sheep"), new Vector3(Random.Range(transform.position.x - (transform.localScale.x / 2), transform.position.x + (transform.localScale.x / 2)),
			                                                              transform.position.y,
			                                                              Random.Range(transform.position.z - (transform.localScale.z / 2), transform.position.z + (transform.localScale.z / 2))),
			            Quaternion.identity);
			currentNumber++;
		}
	}

	void OnDrawGizmos()
	{
		BoxCollider collider = this.GetComponent<Collider>() as BoxCollider;
		
		Gizmos.matrix = transform.localToWorldMatrix;
		Gizmos.color = new Color(0.25f, 0.5f, 1.0f, 0.1f);
		Gizmos.DrawCube(new Vector3(collider.center.x, collider.center.y,collider.center.z),
		                new Vector3(collider.size.x, collider.size.y, collider.size.z));
	}
}
