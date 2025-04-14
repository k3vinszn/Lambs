using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Goal : MonoBehaviour
{
    private AudioSource a;

    public GameObject door;
    public bool MoveDoor = false;

    void Awake()
    {
        a = GetComponent<AudioSource>();
        //Instantiate((GameObject)Resources.Load("Grass/GRASS"+Random.Range(1,3)), transform.position, Quaternion.Euler(0, Random.Range(0, 360),0));
    }

    void Update()
    {
        if (MoveDoor)
        {
            if (door.transform.position.y > 0.25f)
                door.transform.position -= new Vector3(0, 2, 0) * Time.deltaTime;
        }
    }

    public void SheepInGoal()
    {
        a.pitch = Random.Range(0.75f, 1.25f);
        a.Play();

        if (door != null)
        {
            MoveDoor = true;
        }
    }
}
