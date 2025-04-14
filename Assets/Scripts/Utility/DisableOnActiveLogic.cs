using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisableOnActiveLogic : MonoBehaviour {
	
	// Use this for initialization
	void Update () {

		if (!Game.ActiveLogic)
			GetComponent<Button> ().interactable = false;
		else
			GetComponent<Button> ().interactable = true;
	}
}
