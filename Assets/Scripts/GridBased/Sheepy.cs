using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Sheepy : MonoBehaviour
{
    public bool BlackSheep = false;

    public GameObject AfraidOfTarget;
	public float distanceToTarget;

    public float FleeRadius = 1.1f;
	private float speed;
    public bool reorientedOnce = false;
    public int reorientedPathIndex = 0;


    private float stucktimer = 0;
	private Vector3 stuckPos = Vector3.zero;

	public bool ReachedGoal = false;
    public bool ExitedGrid = false;

    public Vector3 nextDestination = Vector3.zero;
    public Vector3 FleeDirection = Vector3.zero;
	public Vector3 GoalDirection = Vector3.zero;

	public bool DrawGizmos = false;

    private Quaternion currentRotation;

    public GameObject modelObj;
    public GameObject gridObj;
    public GameObject directionArrowObj;
    public bool showGridObj = true;

    private AudioSource sfx;
	private Rigidbody rb;
	private Animator anim;

	public float NeighbourFleeRadius = 1.25f;

    public GameObject[] Goals;

    public Vector2 GridSize;
    public Vector2 GridOffset;


    public enum State
	{
		Idle,
		Moving,
		Goal,
		Dead
	}

	public State sheepState = State.Idle;
    public bool bleed = false;

    public Color goalColor = new Color(0, 1, 0, 1);
    public Color startingColor = new Color(1, 1, 1, 1);
    public Color blackColor = new Color(0.25f, 0.25f, 0.25f, 1);

    void Awake()
	{
        //round positions to the nearest int to avoid weird behaviour
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

        GridSize = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().GridSize;
        GridOffset = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().GridOffset;

        modelObj = transform.GetChild(0).gameObject;
        gridObj = transform.GetChild(1).gameObject;

        startingColor = modelObj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color;

        speed = Game.AnimalSpeed;
		sheepState = State.Idle;

		//RandomizeRotation();
        currentRotation = modelObj.transform.rotation;

        rb = GetComponent<Rigidbody>();
		sfx = GetComponent<AudioSource>();
		anim = modelObj.GetComponent<Animator>();

        Goals = GameObject.FindGameObjectsWithTag("Goal");
        AfraidOfTarget = GameObject.FindGameObjectWithTag("Player");

        ReachedGoal = false;
        nextDestination = transform.position;
        IsInGoal(nextDestination);
    }

    //FOR PHYSICS
    void FixedUpdate()
    {
        //ANIMATIONS
        /*anim.SetBool ("moving", Moving);
		anim.SetFloat ("h", CurrentVelocity.x*10);
		anim.SetFloat ("v", CurrentVelocity.z*10);
		Debug.Log ("My speed is " + CurrentVelocity.x + "h and " + CurrentVelocity.z + "y");*/
    }


    void Update()
	{
        /*
        //test barking sfx
        if (Input.GetKeyDown(KeyCode.Space))
		{
			Baah();
			StopMoving();
		}*/

        


        if (Game.ActiveLogic)
		{
            //UI STUFF
            if (Input.GetMouseButton(0) && !GridManager.startPuzzle)
            {
                if (showGridObj)
                {
                    if (gridObj.activeSelf == false)
                    {
                        gridObj.SetActive(true);
                    }

                    if (directionArrowObj.activeSelf == true)
                    {
                        directionArrowObj.SetActive(false);
                    }
                }
                else
                {
                    if (gridObj.activeSelf == true)
                    {
                        gridObj.SetActive(false);
                    }

                    if (directionArrowObj.activeSelf == false)
                    {
                        directionArrowObj.SetActive(true);
                    }
                }
            }
            else
            {
                if (gridObj.activeSelf == true)
                {
                    gridObj.SetActive(false);
                }

                if (directionArrowObj.activeSelf == true)
                {
                    directionArrowObj.SetActive(false);
                }
            }

            distanceToTarget = Vector3.Distance(transform.position, AfraidOfTarget.transform.position);

            if (sheepState == State.Idle)
			{
				if (distanceToTarget < FleeRadius)
				{
                    UpdateFleeDirection();

                    if(CheckIfCanKeepMoving(FleeDirection))
                    {
                        AfraidOfTarget.GetComponent<Doggy>().Bark();
                        StartMoving(FleeDirection);
                    }
                    
                }

                //MOVE WITH PASSING SHEEP IF IM IDLE,
                //DONT IF IM A BLACK SHEEP
                foreach (GameObject sheep in Game.Sheeps)
                {
                    if (sheep != null
                    && sheep.gameObject != this.gameObject
                    && sheep.GetComponent<Sheepy>().sheepState == State.Moving
                    && CheckIfCanKeepMoving(sheep.GetComponent<Sheepy>().FleeDirection)
                    && !BlackSheep && sheepState != State.Moving)
                    {
                        //check if the sheep you want to move is in a goal, if so, only move this one, not the rest
                        if (!ReachedGoal && Vector3.Distance(transform.position, sheep.transform.position) <= 1.0f)
                        {
                            //Debug.Log(sheep.name + " started to move cause next to " + name);
                            FleeDirection = sheep.GetComponent<Sheepy>().FleeDirection;
                            StartMoving(FleeDirection);
                            Instantiate((GameObject)Resources.Load("EFX/ExclamationEFX"), transform.position, Quaternion.identity, transform);
                        }
                        else if(ReachedGoal && Vector3.Distance(transform.position, sheep.transform.position) <= 0.95f)
                        {
                            //Debug.Log(sheep.name + " started to move on a goal cause next to " + name);
                            FleeDirection = sheep.GetComponent<Sheepy>().FleeDirection;
                            StartMoving(FleeDirection);
                            Instantiate((GameObject)Resources.Load("EFX/ExclamationEFX"), transform.position, Quaternion.identity, transform);
                        }
                        
                    }
                }
            }

			if (sheepState == State.Moving)
			{
				//if we move out of a goal position do this
				if(ReachedGoal)
				{
                    anim.SetBool("isGoal", true);
                    ReachedGoal = false;
                    //Game.Score = Game.Score - 1;

                    //change sheep color
                    modelObj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = GetComponent<Sheepy>().startingColor;
                }


                if (Vector3.Distance(transform.position, nextDestination) >= Mathf.Epsilon)
                {
                    //call a failsafe check for edge cases where sheep gets stuck in geometry
                    CheckIfStuck();

                    //FLEE FROM WHATS MAKING ME MOVE
                    transform.position = Vector3.MoveTowards(transform.position, nextDestination, speed * Time.deltaTime);
                    modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(FleeDirection), 10 * Time.deltaTime);

                    anim.SetBool("isGoal", false);
                }
                else
                {
                    

                    //check if it can continue moving or else stop
                    if (CheckIfCanKeepMoving(FleeDirection) && !ReachedGoal)
                    {
                        IsInGoal(nextDestination);
                        //Debug.Log(name + " kept going");
                        GetNextDestination(FleeDirection);
                    }
                    else
                    {
                        StopMoving();
                        IsInGoal(nextDestination);
                        
                    }

                    
                }

            }

			if (sheepState == State.Goal)
			{
                //Debug.Log(name + " REACHED A GOAL POINT");
                ReachedGoal = true;

                //change sheep color
                modelObj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = GetComponent<Sheepy>().goalColor;

                StopMoving();
            }

			if (sheepState == State.Dead)
			{
                DeathBaah();
                if(!bleed)
                {
                    bleed = true;
                    Instantiate((GameObject)Resources.Load("EFX/BloodSplat"), transform.position + new Vector3(0,0.1f,0), Quaternion.Euler(new Vector3(90, 0, 0)));
                }
                    
            }

		}
	}

    public void UpdateFleeDirection()
    {
        Vector3 DogAbsTransformPos = new Vector3(Mathf.RoundToInt(AfraidOfTarget.transform.position.x),
                                                 Mathf.RoundToInt(AfraidOfTarget.transform.position.y),
                                                 Mathf.RoundToInt(AfraidOfTarget.transform.position.z));

		//Debug.Log("updated " + name + " flee direction!");
        FleeDirection = Vector3.Normalize(transform.position - DogAbsTransformPos);

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
        Game.MovingSheeps++;
        anim.SetBool("isMoving", true);

        //check to see if the Dog is 1 unit apart from this before it moves, if yes, update direction because Dog trumps all
        if (Vector3.Distance(transform.position, AfraidOfTarget.transform.position) < 1.415f)
		{
			//Debug.Log(name + " corrected direction cause Dog is close");
            UpdateFleeDirection();
        }

        GetNextDestination(direction);
        //Debug.Log("added " + name + " to the moving sheep var");
        sheepState = State.Moving;
        Baah();
	}

    public void StopMoving()
    {
        //Debug.Log(name + " is stopping");
        rb.linearVelocity = Vector3.zero;
        anim.SetBool("isMoving", false);
        sheepState = State.Idle;

        if(Game.MovingSheeps > 0)
        {
            Game.MovingSheeps--;
        }
    }

    public bool CheckIfCanKeepMoving(Vector3 movingdirection)
    {
        RaycastHit hit;

		Vector3 myMidPosition = transform.position + new Vector3(0, 0.25f, 0);
        

        if (Physics.Raycast(myMidPosition, movingdirection, out hit, 1.42f) && hit.collider.gameObject != null)
        {
            if (hit.collider.gameObject.tag == "BLOCKER")
			{
                modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(movingdirection), 5000 * Time.deltaTime);
                //Debug.Log(name + " HIT A BLOCKER!");
                return (false);
			}
			else if(hit.collider.gameObject.tag == "Sheep")
			{
                if (!hit.collider.gameObject.GetComponent<Sheepy>().CheckIfCanKeepMoving(movingdirection)
                    || hit.collider.gameObject.GetComponent<Sheepy>().BlackSheep)
                {
                    hit.collider.gameObject.GetComponent<Sheepy>().modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(movingdirection), 5000 * Time.deltaTime);
                    //Debug.Log(name + " HIT A SHEEP THAT CANT MOVE IN THIS DIRECTION");
                    return (false);
                }
                else
                {
                    /*
                    if (hit.collider.gameObject.GetComponent<Sheepy>().BlackSheep)
                    {
                        hit.collider.gameObject.GetComponent<Sheepy>().StartMoving(movingdirection);
                    }*/

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

    public void IsInGoal(Vector3 currentposition)
    {
        //Debug.Log("checking if in goal");

        //check if this sheep is inside any of the valid goal positions
        foreach (GameObject goal in Goals)
        {
            if (goal != null && Vector3.Distance(currentposition, goal.transform.position) <= Mathf.Epsilon)
            {
                anim.SetBool("isGoal", true);
                goal.GetComponent<Goal>().SheepInGoal();
                sheepState = State.Goal;
            }
        }
    }

    public void CheckIfStuck()
    {
        if (stucktimer == 0)
        {
            stuckPos = transform.position;
        }

        stucktimer += 1 * Time.deltaTime;

        if (stucktimer > 0.5f)
        {
            //Debug.Log(Vector3.Distance(transform.position, stuckPos));

            if (sheepState == State.Moving && Vector3.Distance(transform.position, stuckPos) <= 0.25f)
            {
                Debug.Log(name + " is STUCK!");
                rb.linearVelocity = Vector3.zero;
                Game.MovingSheeps--;
                StopMoving();
            }

            stucktimer = 0;
        }
    }

	public void RandomizeRotation()
	{
        modelObj.transform.eulerAngles = new Vector3(modelObj.transform.rotation.x, Random.Range(0, 360), modelObj.transform.rotation.z);
	}

    public void ResetRotation()
    {
        modelObj.transform.rotation = currentRotation;
        showGridObj = true;
        reorientedPathIndex = 0;
    }

    public void ReorientRotation(Vector3 position, int tilePathIndex)
    {
        showGridObj = false;
        reorientedPathIndex = tilePathIndex;

        if (!reorientedOnce)
        {
            //Debug.Log("reoriented " + name);
            Vector3 direction = Vector3.Normalize(transform.position - position);
            modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(direction), 5000 * Time.deltaTime);
        }
    }

    public void Baah()
	{
		if (!sfx.isPlaying)
		{
			sfx.clip = (AudioClip)Resources.Load("SFX/sheep/sheep" + Random.Range(1, 6));
			sfx.pitch = Random.Range(0.9f, 1.5f);
			sfx.Play();

			GetComponent<ParticleSystem>().Play();
		}

		//print ("Baahed with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
	}

    public void DeathBaah()
    {
        if (!sfx.isPlaying)
        {
            sfx.clip = (AudioClip)Resources.Load("SFX/sheep/sheepDeath" + Random.Range(1, 5));
            sfx.pitch = Random.Range(0.75f, 1.0f);
            sfx.Play();

            GetComponent<ParticleSystem>().Play();
        }

        //print ("Baahed with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
    }



    ////////////////////////////////////////////////////////////////////////////////////////////// 

    void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
	            Gizmos.color = Color.white;
	            Gizmos.DrawWireSphere(transform.position, FleeRadius);

				Gizmos.color = Color.blue;
				Gizmos.DrawWireSphere(transform.position, NeighbourFleeRadius);
        }

		//Gizmos.color = Color.white;
		//Gizmos.DrawRay(transform.position, GoalDirection);
		//Gizmos.DrawSphere(GoalDirection, 0.25f);

		Gizmos.color = Color.blue;
		Gizmos.DrawRay(transform.position, FleeDirection);
	}
}

