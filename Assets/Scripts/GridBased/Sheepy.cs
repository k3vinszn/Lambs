using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Sheepy : MonoBehaviour
{
    // ====================================
    // == SHEEP PROPERTIES & CONFIGURATION ==
    // ====================================
    // Sheep properties
    public bool BlackSheep = false;
    public GameObject AfraidOfTarget;
    public float distanceToTarget;
    public float FleeRadius = 1.1f;
    private float speed;
    public bool reorientedOnce = false;
    public int reorientedPathIndex = 0;

    // Movement tracking variables
    private float stucktimer = 0;
    private Vector3 stuckPos = Vector3.zero;
    public bool ReachedGoal = false;
    public bool ExitedGrid = false;

    // Destination and direction vectors
    public Vector3 nextDestination = Vector3.zero;
    public Vector3 FleeDirection = Vector3.zero;
    public Vector3 GoalDirection = Vector3.zero;

    // Debug and visualization
    public bool DrawGizmos = false;
    private Quaternion currentRotation;

    // Game objects references
    public GameObject modelObj;
    public GameObject gridObj;
    public GameObject directionArrowObj;
    public bool showGridObj = true;

    // Components
    private AudioSource sfx;
    private Rigidbody rb;
    private Animator anim;

    // Neighbor detection
    public float NeighbourFleeRadius = 1.25f;
    public GameObject[] Goals;

    // Grid properties
    public Vector2 GridSize;
    public Vector2 GridOffset;

    // Sheep states
    public enum State
    {
        Idle,
        Moving,
        Goal,
        Dead
    }
    public State sheepState = State.Idle;
    public bool bleed = false;

    // Visual colors
    public Color goalColor = new Color(0, 1, 0, 1);
    public Color startingColor = new Color(1, 1, 1, 1);
    public Color blackColor = new Color(0.25f, 0.25f, 0.25f, 1);

    // =====================
    // == INITIALIZATION ==
    // =====================
    void Awake()
    {
        // Initialize sheep position and properties
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

        // Get grid properties from GridManager
        GridSize = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().GridSize;
        GridOffset = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().GridOffset;

        // Set up model and grid objects
        modelObj = transform.GetChild(0).gameObject;
        gridObj = transform.GetChild(1).gameObject;
        startingColor = modelObj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color;

        // Initialize movement properties
        speed = Game.AnimalSpeed;
        sheepState = State.Idle;
        currentRotation = modelObj.transform.rotation;

        // Get components
        rb = GetComponent<Rigidbody>();
        sfx = GetComponent<AudioSource>();
        anim = modelObj.GetComponent<Animator>();

        // Find important game objects
        Goals = GameObject.FindGameObjectsWithTag("Goal");
        AfraidOfTarget = GameObject.FindGameObjectWithTag("Player");

        // Initialize state
        ReachedGoal = false;
        nextDestination = transform.position;
        IsInGoal(nextDestination);
    }

    // ===================
    // == PHYSICS UPDATE ==
    // ===================
    void FixedUpdate()
    {
        // Physics updates would go here
    }

    // =================
    // == MAIN UPDATE ==
    // =================
    void Update()
    {
        if (Game.ActiveLogic)
        {
            // Handle UI visibility based on mouse input
            // Handle UI visibility
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

            // Calculate distance to target (usually the dog)
            distanceToTarget = Vector3.Distance(transform.position, AfraidOfTarget.transform.position);

            // State machine for sheep behavior
            // Handle sheep states
            if (sheepState == State.Idle)
            {
                // Flee if dog is too close
                if (distanceToTarget < FleeRadius)
                {
                    UpdateFleeDirection();

                    if (CheckIfCanKeepMoving(FleeDirection))
                    {
                        AfraidOfTarget.GetComponent<Doggy>().Bark();
                        StartMoving(FleeDirection);
                    }
                }

                // Move with neighboring sheep if they're moving (unless this is a black sheep)
                // Handle neighbor sheep movement influence
                foreach (GameObject sheep in Game.Sheeps)
                {
                    if (sheep != null
                    && sheep.gameObject != this.gameObject
                    && sheep.GetComponent<Sheepy>().sheepState == State.Moving
                    && CheckIfCanKeepMoving(sheep.GetComponent<Sheepy>().FleeDirection)
                    && !BlackSheep && sheepState != State.Moving)
                    {
                        if (!ReachedGoal && Vector3.Distance(transform.position, sheep.transform.position) <= 1.0f)
                        {
                            FleeDirection = sheep.GetComponent<Sheepy>().FleeDirection;
                            StartMoving(FleeDirection);
                            Instantiate((GameObject)Resources.Load("EFX/ExclamationEFX"), transform.position, Quaternion.identity, transform);
                        }
                        else if (ReachedGoal && Vector3.Distance(transform.position, sheep.transform.position) <= 0.95f)
                        {
                            FleeDirection = sheep.GetComponent<Sheepy>().FleeDirection;
                            StartMoving(FleeDirection);
                            Instantiate((GameObject)Resources.Load("EFX/ExclamationEFX"), transform.position, Quaternion.identity, transform);
                        }
                    }
                }
            }

            if (sheepState == State.Moving)
            {
                // Handle leaving a goal position
                if (ReachedGoal)
                {
                    anim.SetBool("isGoal", true);
                    ReachedGoal = false;
                    modelObj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = GetComponent<Sheepy>().startingColor;
                }

                // Move towards destination
                if (Vector3.Distance(transform.position, nextDestination) >= Mathf.Epsilon)
                {
                    CheckIfStuck();
                    transform.position = Vector3.MoveTowards(transform.position, nextDestination, speed * Time.deltaTime);
                    modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(FleeDirection), 10 * Time.deltaTime);
                    anim.SetBool("isGoal", false);
                }
                else
                {
                    // Check if can continue moving or stop
                    if (CheckIfCanKeepMoving(FleeDirection) && !ReachedGoal)
                    {
                        IsInGoal(nextDestination);
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
                // Handle reaching a goal
                ReachedGoal = true;
                modelObj.transform.GetChild(1).GetComponent<SkinnedMeshRenderer>().material.color = GetComponent<Sheepy>().goalColor;
                StopMoving();
            }

            if (sheepState == State.Dead)
            {
                // Handle death state
                DeathBaah();
                if (!bleed)
                {
                    bleed = true;
                    Instantiate((GameObject)Resources.Load("EFX/BloodSplat"), transform.position + new Vector3(0, 0.1f, 0), Quaternion.Euler(new Vector3(90, 0, 0)));
                }
            }
        }
    }

    // ======================
    // == MOVEMENT METHODS ==
    // ======================
    // Calculate flee direction away from the target (dog)
    public void UpdateFleeDirection()
    {
        Vector3 DogAbsTransformPos = new Vector3(Mathf.RoundToInt(AfraidOfTarget.transform.position.x),
                                             Mathf.RoundToInt(AfraidOfTarget.transform.position.y),
                                             Mathf.RoundToInt(AfraidOfTarget.transform.position.z));

        FleeDirection = Vector3.Normalize(transform.position - DogAbsTransformPos);

        // Adjust for diagonal movement
        if (FleeDirection.x != 0 && FleeDirection.z != 0)
        {
            FleeDirection = new Vector3(FleeDirection.x * 1.4142f, 0, FleeDirection.z * 1.4142f);
        }
    }

    // Calculate next destination based on current direction
    private void GetNextDestination(Vector3 direction)
    {
        Vector3 destination = transform.position + direction;
        nextDestination = new Vector3(Mathf.RoundToInt(destination.x), 0, Mathf.RoundToInt(destination.z));

        // Check if destination is outside grid bounds
        if (nextDestination.x > GridSize.x + GridOffset.x
        || nextDestination.x < -GridSize.x + GridOffset.x
        || nextDestination.z > GridSize.y + GridOffset.y
        || nextDestination.z < -GridSize.y + GridOffset.y)
        {
            ExitedGrid = true;
        }
    }

    // Start movement in specified direction
    public void StartMoving(Vector3 direction)
    {
        Game.MovingSheeps++;
        anim.SetBool("isMoving", true);

        // Update direction if dog is very close
        if (Vector3.Distance(transform.position, AfraidOfTarget.transform.position) < 1.415f)
        {
            UpdateFleeDirection();
        }

        GetNextDestination(direction);
        sheepState = State.Moving;
        Baah();
    }

    // Stop movement and reset state
    public void StopMoving()
    {
        rb.linearVelocity = Vector3.zero;
        anim.SetBool("isMoving", false);
        sheepState = State.Idle;

        if (Game.MovingSheeps > 0)
        {
            Game.MovingSheeps--;
        }
    }

    // =====================
    // == COLLISION CHECKS ==
    // =====================
    // Check if movement in current direction is possible
    public bool CheckIfCanKeepMoving(Vector3 movingdirection)
    {
        RaycastHit hit;
        Vector3 myMidPosition = transform.position + new Vector3(0, 0.25f, 0);

        if (Physics.Raycast(myMidPosition, movingdirection, out hit, 1.42f) && hit.collider.gameObject != null)
        {
            if (hit.collider.gameObject.tag == "BLOCKER")
            {
                modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(movingdirection), 5000 * Time.deltaTime);
                return false;
            }
            else if (hit.collider.gameObject.tag == "Sheep")
            {
                if (!hit.collider.gameObject.GetComponent<Sheepy>().CheckIfCanKeepMoving(movingdirection)
                    || hit.collider.gameObject.GetComponent<Sheepy>().BlackSheep)
                {
                    hit.collider.gameObject.GetComponent<Sheepy>().modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(movingdirection), 5000 * Time.deltaTime);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }
        else
        {
            return true;
        }
    }

    // Check if sheep is stuck and not moving
    public void CheckIfStuck()
    {
        if (stucktimer == 0)
        {
            stuckPos = transform.position;
        }

        stucktimer += 1 * Time.deltaTime;

        if (stucktimer > 0.5f)
        {
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

    // =================
    // == GOAL LOGIC ==
    // =================
    // Check if sheep is in a goal position
    public void IsInGoal(Vector3 currentposition)
    {
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

    // =====================
    // == ROTATION METHODS ==
    // =====================
    // Randomize sheep model rotation
    public void RandomizeRotation()
    {
        modelObj.transform.eulerAngles = new Vector3(modelObj.transform.rotation.x, Random.Range(0, 360), modelObj.transform.rotation.z);
    }

    // Reset sheep model to original rotation
    public void ResetRotation()
    {
        modelObj.transform.rotation = currentRotation;
        showGridObj = true;
        reorientedPathIndex = 0;
    }

    // Reorient sheep model to face away from a position
    public void ReorientRotation(Vector3 position, int tilePathIndex)
    {
        showGridObj = false;
        reorientedPathIndex = tilePathIndex;

        if (!reorientedOnce)
        {
            Vector3 direction = Vector3.Normalize(transform.position - position);
            modelObj.transform.rotation = Quaternion.Lerp(modelObj.transform.rotation, Quaternion.LookRotation(direction), 5000 * Time.deltaTime);
        }
    }

    // =================
    // == AUDIO/SOUNDS ==
    // =================
    // Play sheep sound effect
    public void Baah()
    {
        if (!sfx.isPlaying)
        {
            sfx.clip = (AudioClip)Resources.Load("SFX/sheep/sheep" + Random.Range(1, 6));
            sfx.pitch = Random.Range(0.9f, 1.5f);
            sfx.Play();
            GetComponent<ParticleSystem>().Play();
        }
    }

    // Play death sound effect
    public void DeathBaah()
    {
        if (!sfx.isPlaying)
        {
            sfx.clip = (AudioClip)Resources.Load("SFX/sheep/sheepDeath" + Random.Range(1, 5));
            sfx.pitch = Random.Range(0.75f, 1.0f);
            sfx.Play();
            GetComponent<ParticleSystem>().Play();
        }
    }

    // =================
    // == DEBUG TOOLS ==
    // =================
    void OnDrawGizmos()
    {
        if (DrawGizmos)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, FleeRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, NeighbourFleeRadius);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, FleeDirection);
    }
}