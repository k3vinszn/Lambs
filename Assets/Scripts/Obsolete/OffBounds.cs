﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffBounds : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		//Debug.Log ("hit " + other.name);


		if(other.tag == "Sheep")
		{
			Game.DestroyedSheeps++;
			Game.MovingSheeps--;
			Destroy (other.gameObject);
		}

	}
}
