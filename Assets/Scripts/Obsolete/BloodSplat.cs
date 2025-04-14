using UnityEngine;
using System.Collections;

public class BloodSplat : MonoBehaviour {

	private Vector2 minScale;
	private Vector2 maxScale;

	// Use this for initialization
	void Awake ()
	{
		maxScale = transform.localScale;
		minScale = maxScale / 10;

		transform.localScale = minScale;
	}
	
	// Update is called once per frame
	void FixedUpdate ()
	{
		transform.localScale = Vector2.Lerp (minScale, maxScale, 2 * Time.deltaTime);
	}
}
