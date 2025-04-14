using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

public class Doggy : MonoBehaviour {
	
	private AudioSource sfx;

	void Awake()
	{
        //round positions to the nearest int to avoid weird behaviour
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

        sfx = GetComponent<AudioSource> ();
        this.name = this.name + "  " + this.transform.position.ToString();

        RandomizeRotation();

    }

    void Start()
	{

	}
	
	void Update()
	{

	}

	public void Bark()
	{
		if (!sfx.isPlaying)
		{
			sfx.clip = (AudioClip)Resources.Load ("SFX/dog/dog" + Random.Range (2, 7));
			sfx.pitch = Random.Range (1.25f, 1.5f);
			sfx.Play ();

			GetComponent<ParticleSystem>().Play ();
		}

		//print ("Barked with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
	}

    public void RandomizeRotation()
    {
        transform.eulerAngles = new Vector3(transform.rotation.x, Random.Range(0, 360), transform.rotation.z);
    }
    public void ReorientRotation(Vector3 position)
    {
        Vector3 direction = Vector3.Normalize(position - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(direction), 5000 * Time.deltaTime);
    }


}
