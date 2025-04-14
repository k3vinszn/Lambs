using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour {
	
	public float TimeUntilDestruction = 1.0f;

	// Use this for initialization
	void Start () {

		Invoke ("DestroyNow", TimeUntilDestruction);
	}

	void DestroyNow()
	{
		Destroy(gameObject);
	}
	
}
