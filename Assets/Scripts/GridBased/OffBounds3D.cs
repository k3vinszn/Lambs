using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffBounds3D : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter(Collider other)
	{
		//Debug.Log ("hit " + other.name);


		if(other.tag == "Sheep")
		{
			Game.DestroyedSheeps++;
			Game.MovingSheeps--;
			Game.Sheeps.Remove(other.gameObject);
			Destroy (other.gameObject);
		}

	}
}
