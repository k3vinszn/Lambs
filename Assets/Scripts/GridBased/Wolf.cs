﻿using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using Steer2D;
using UnityEditor;

public class Wolf : MonoBehaviour {
	
    public PathFinding PathFinder;
    public List<GameObject> TilePath;

    public GameObject AfraidOfTarget;
    public float distanceToTarget;

    public GameObject closestSheep;

    public float FleeRadius = 1.1f;
    private int currentIndex = 0;
    private float speed;

    private float stucktimer = 0;
    private Vector3 stuckPos = Vector3.zero;

    public bool ReachedGoal = false;
    public bool ExitedGrid = false;

    public Vector3 nextDestination = Vector3.zero;
    public Vector3 FleeDirection = Vector3.zero;
    public Vector3 GoalDirection = Vector3.zero;

    public bool DrawGizmos = false;

    public enum State
    {
        Idle,
        Fleeing,
        Moving,
        Eating
    }

    public State wolfState = State.Idle;

    private AudioSource sfx;
    private Rigidbody rb;
    private Animator anim;

    private GridManager gridManager;
    public Vector2 GridSize;
    public Vector2 GridOffset;

    void Awake()
    {
        gridManager = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>();
        GridSize = gridManager.GridSize;
        GridOffset = gridManager.GridOffset;

        speed = Game.AnimalSpeed;
        RandomizeRotation();

        rb = GetComponent<Rigidbody>();
        sfx = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();

        AfraidOfTarget = GameObject.FindGameObjectWithTag("Player");

        ReachedGoal = false;
        nextDestination = transform.position;
    }

    void Start()
	{
		sfx = GetComponent<AudioSource> ();
        this.name = this.name + "  " + this.transform.position.ToString();

        PathFinder = GetComponent<PathFinding>();
        AfraidOfTarget = GameObject.FindGameObjectWithTag("Player");

        UpdateTargetToChase();
        UpdatePathfinderTarget();
    }

    

    void Update()
	{

        if (Game.ActiveLogic && GridManager.startPuzzle)
        {
            distanceToTarget = Vector3.Distance(transform.position, AfraidOfTarget.transform.position);

            if (wolfState == State.Idle)
            {
                if (distanceToTarget < FleeRadius)
                {
                    UpdateFleeDirection();

                    if (CheckIfCanKeepMoving(FleeDirection))
                    {
                        AfraidOfTarget.GetComponent<Doggy>().Bark();
                        StartMoving(FleeDirection);
                    }

                }
                else
                {
                    wolfState = State.Moving;
                    Howl();
                }
            }

            if (wolfState == State.Moving)
            {
                if (currentIndex < TilePath.Count)
                {
                    if (Vector3.Distance(transform.position, TilePath[currentIndex].transform.position) >= Mathf.Epsilon)
                    {
                        //FIGURE OUT DIRECTION THE DOG IS FACING
                        Vector3 wolfFacingDirection = transform.position - TilePath[currentIndex].transform.position;
                        UpdatePathfinderTarget();

                        transform.position = Vector3.MoveTowards(transform.position, TilePath[currentIndex].transform.position, Time.deltaTime * speed);
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(-wolfFacingDirection), 10 * Time.deltaTime);

                        //transform.LookAt(TilePath[currentIndex].transform.position);
                    }
                    else
                    {
                        if(currentIndex < TilePath.Count)
                        {
                            //get new tile after arriving to this one
                            currentIndex += 1;
                        }
                        else
                        {
                            Debug.Log("entered tilepath index count stopped state");
                        }
                        
                    }

                }
                
                if (currentIndex != 0 && currentIndex == TilePath.Count)
                {
                    Growl();
                    closestSheep.GetComponent<Sheepy>().sheepState = Sheepy.State.Dead;

                    TilePath.Clear();
                    UpdateTargetToChase();
                }
            }

            if (wolfState == State.Fleeing)
            {
                //if we move out of a goal position do this
                if (ReachedGoal)
                {
                    ReachedGoal = false;
                    //Game.Score = Game.Score - 1;

                    //change sheep color
                    transform.GetChild(1).GetComponent<MeshRenderer>().material.color = GetComponent<Sheepy>().startingColor;
                    transform.GetChild(2).GetComponent<MeshRenderer>().material.color = GetComponent<Sheepy>().startingColor;
                }


                if (Vector3.Distance(transform.position, nextDestination) >= Mathf.Epsilon)
                {

                    //FLEE FROM WHATS MAKING ME MOVE
                    transform.position = Vector3.MoveTowards(transform.position, nextDestination, speed * Time.deltaTime);
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(FleeDirection), 10 * Time.deltaTime);
                }
                else
                {
                    //check if it can continue moving or else stop
                    if (CheckIfCanKeepMoving(FleeDirection))
                    {
                        //Debug.Log(name + " kept going");
                        GetNextDestination(FleeDirection);
                    }
                    else
                    {
                        StopMoving();
                    }
                }

            }

            
        }
    }

    public void UpdateTargetToChase()
    {
        closestSheep = GetClosestTarget(Game.Sheeps);
        currentIndex = 0;
    }
    public void UpdatePathfinderTarget()
    {
        if (closestSheep != null && !closestSheep.GetComponent<Sheepy>().ExitedGrid && !ExitedGrid)
        {
            TilePath = PathFinder.FindPath(transform.position, closestSheep.transform.position);
        }
    }

public void UpdateFleeDirection()
    {
        //Debug.Log("updated " + name + " flee direction!");
        FleeDirection = Vector3.Normalize(transform.position - AfraidOfTarget.transform.position);

        //to compensate nextDestination and speed when going in diagonals, should be slightly modified to cover the correct distance
        if (FleeDirection.x != 0 && FleeDirection.z != 0)
        {
            //float myhypotenuse = Mathf.Sqrt(FleeDirection.x * FleeDirection.x + FleeDirection.z * FleeDirection.z);
            //FOR SOME REASON THIS FORMULA ABOVE ISNT GIVING OUT A PROPER RESULT, LETS SCRAP IT
            //"1.4142f" is a basic hipotenuse ratio of a 1x1 triangle which is the only thing we need for a grid unit system

            //Debug.Log("Im going on a diagonal!");

            FleeDirection = new Vector3(FleeDirection.x * 1.4142f, 0, FleeDirection.z * 1.4142f);
            //speed = speed * 1.4142f;
        }
    }

    private void GetNextDestination(Vector3 direction)
    {
        Vector3 destination = transform.position + direction;

        nextDestination = new Vector3(Mathf.RoundToInt(destination.x), 0, Mathf.RoundToInt(destination.z));

        if (nextDestination.x > GridSize.x + GridOffset.x
        || nextDestination.x < -GridSize.x + GridOffset.x
        || nextDestination.z > GridSize.y + GridOffset.y
        || nextDestination.z < -GridSize.y + GridOffset.y)
        {
            //Debug.Log(name + " has left the playable grid area");
            ExitedGrid = true;
        }
    }
    public void StartMoving(Vector3 direction)
    {
        //check to see if the Dog is 1 unit apart from this before it moves, if yes, update direction because Dog trumps all
        if (Vector3.Distance(transform.position, AfraidOfTarget.transform.position) < 1.415f)
        {
            //Debug.Log(name + " corrected direction cause Dog is close");
            UpdateFleeDirection();
        }

        GetNextDestination(direction);

        wolfState = State.Fleeing;
        Howl();
    }

    public void StopMoving()
    {
        rb.linearVelocity = Vector3.zero;
        wolfState = State.Idle;
        TilePath = null;
    }

    GameObject GetClosestTarget(List<GameObject> sheeps)
    {
        GameObject bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (GameObject potentialTarget in sheeps)
        {
            if (potentialTarget != null
                && !potentialTarget.GetComponent<Sheepy>().ExitedGrid
                && potentialTarget.GetComponent<Sheepy>().sheepState != Sheepy.State.Dead)
            {
                Vector3 directionToTarget = potentialTarget.transform.position - currentPosition;
                float dSqrToTarget = directionToTarget.sqrMagnitude;
                if (dSqrToTarget < closestDistanceSqr)
                {
                    closestDistanceSqr = dSqrToTarget;
                    bestTarget = potentialTarget;
                }
            }
        }

        return bestTarget;
    }

    public bool CheckIfCanKeepMoving(Vector3 movingdirection)
    {
        RaycastHit hit;

        Vector3 myMidPosition = transform.position + new Vector3(0, 0.25f, 0);

        if (Physics.Raycast(myMidPosition, movingdirection, out hit, 1) && hit.collider.gameObject != null)
        {
            if (hit.collider.gameObject.tag == "BLOCKER")
            {
                //Debug.Log(name + " HIT A BLOCKER!");
                return (false);
            }
            else if (hit.collider.gameObject.tag == "Sheep")
            {
                if (!hit.collider.gameObject.GetComponent<Sheepy>().CheckIfCanKeepMoving(FleeDirection)
                    || hit.collider.gameObject.GetComponent<Sheepy>().BlackSheep)
                {
                    //Debug.Log(name + " HIT A SHEEP THAT CANT MOVE IN THIS DIRECTION");
                    return (false);
                }
                else
                {
                    return (true);
                }
            }
            else
            {
                //Debug.Log(name + " CAN MOVE FREELY!");
                return (true);
            }
        }
        else
        {
            return (true);
        }
    }

    private void RandomizeRotation()
    {
        transform.eulerAngles = new Vector3(transform.rotation.x, Random.Range(0, 360), transform.rotation.z);
    }
    private void Howl()
	{
		if (!sfx.isPlaying)
		{
			sfx.clip = (AudioClip)Resources.Load ("SFX/wolf/wolf" + Random.Range (2, 6));
			sfx.pitch = Random.Range (0.75f, 1.0f);
			sfx.Play ();

			GetComponent<ParticleSystem>().Play ();
		}

		//print ("Barked with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
	}
    private void Growl()
    {
        if (!sfx.isPlaying)
        {
            sfx.clip = (AudioClip)Resources.Load("SFX/wolf/growl");
            sfx.pitch = Random.Range(0.75f, 1.0f);
            sfx.Play();

            GetComponent<ParticleSystem>().Play();
        }

        //print ("Barked with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0, 0.5f, 1);

        foreach (GameObject pathTile in TilePath)
        {
            
            Gizmos.DrawSphere(pathTile.transform.position, 0.1f);
        }

        for(int i = 0; i < (TilePath.Count - 1); i++)
        {
            Gizmos.DrawLine(TilePath[i].transform.position, TilePath[i+1].transform.position);
            //Gizmos.DrawRay(transform.position, transform.forward, 1);
        }
        

    }

}
