using UnityEngine;
using System.Collections;

public class SheepGoal : MonoBehaviour {

	private AudioSource a;

	void Awake ()
	{
		Game.Score = 0;
		a = GetComponent<AudioSource>();
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		//Debug.Log ("hit " + other.name);

			
			if(other.tag == "Sheep")
			{
				Game.Score = Game.Score + 1;
				Destroy (other.gameObject);
				Game.MovingSheeps--;
				a.pitch = Random.Range (0.75f, 1.25f);
				a.Play ();
			}
				
	}


}
