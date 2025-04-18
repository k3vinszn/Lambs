using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

public class Doggy : MonoBehaviour
{
    // ====================================
    // == COMPONENT REFERENCES & SETTINGS ==
    // ====================================
    private AudioSource sfx;
    private Animator dogAnimator;
    private float speed;
    private int currentPathIndex = 0;
    private List<GameObject> pathTiles;
    private bool isMoving = false;
    private bool hasFinishedPath = false;

    // =====================
    // == PUBLIC PROPERTIES ==
    // =====================
    public bool IsMoving { get { return isMoving; } }
    public bool HasFinishedPath => hasFinishedPath;

    // =====================
    // == INITIALIZATION ==
    // =====================
    void Awake()
    {
        // Round position to avoid floating point issues on grid
        transform.position = new Vector3(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.y),
            Mathf.RoundToInt(transform.position.z)
        );

        // Setup references
        sfx = GetComponent<AudioSource>();
        dogAnimator = transform.GetChild(0).GetComponent<Animator>();

        // Rename instance for easier debug 
        this.name = this.name + "  " + this.transform.position.ToString();

        // Randomize initial rotation and set movement speed
        RandomizeRotation();
        speed = Game.AnimalSpeed;
    }

    // =================
    // == MAIN UPDATE ==
    // =================
    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
    }

    // ======================
    // == MOVEMENT CONTROL ==
    // ======================
    // Starts the movement along the given path
    public void StartMoving(List<GameObject> path)
    {
        if (hasFinishedPath) return; // Prevent re-triggering movement

        pathTiles = path;
        currentPathIndex = 0;
        isMoving = true;
        dogAnimator.SetBool("isMoving", true);
    }

    // Handles movement along the current path
    private void MoveAlongPath()
    {
        if (currentPathIndex < pathTiles.Count)
        {
            Vector3 targetPosition = pathTiles[currentPathIndex].transform.position;

            // Move towards the current target tile
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                Time.deltaTime * speed
            );

            // Rotate dog in movement direction (for animation)
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            if (directionToTarget != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(directionToTarget),
                    25 * Time.deltaTime
                );
            }

            // Check if tile reached
            if (transform.position == targetPosition)
            {
                if (CanContinueMoving())
                {
                    currentPathIndex++;
                }
                else
                {
                    StopMoving();
                }
            }
        }
        else
        {
            StopMoving();
        }
    }

    // ========================
    // == MOVEMENT COLLISION ==
    // ========================
    // Checks if the dog can continue moving to the next tile (handles sheep logic)
    private bool CanContinueMoving()
    {
        if (currentPathIndex < pathTiles.Count - 1)
        {
            Vector3 nextTilePosition = pathTiles[currentPathIndex + 1].transform.position;
            RaycastHit hit;

            if (Physics.Raycast(nextTilePosition, Vector3.up, out hit, 1.5f))
            {
                if (hit.collider.CompareTag("Sheep"))
                {
                    Sheepy sheep = hit.collider.GetComponent<Sheepy>();
                    sheep.UpdateFleeDirection();

                    if (!sheep.CheckIfCanKeepMoving(sheep.FleeDirection))
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    // Stops movement and marks path as completed
    private void StopMoving()
    {
        isMoving = false;
        hasFinishedPath = true;

        dogAnimator.SetBool("isMoving", false);
        Game.PathComplete = true;
    }

    // =================
    // == AUDIO/SOUNDS ==
    // =================
    // Plays a random dog bark and triggers particle effect
    public void Bark()
    {
        if (!sfx.isPlaying)
        {
            sfx.clip = (AudioClip)Resources.Load("SFX/dog/dog" + Random.Range(2, 7));
            sfx.pitch = Random.Range(1.25f, 1.5f);
            sfx.Play();

            GetComponent<ParticleSystem>().Play();
        }
    }

    // =====================
    // == ROTATION METHODS ==
    // =====================
    // Randomizes the dog's initial Y-axis rotation
    public void RandomizeRotation()
    {
        transform.eulerAngles = new Vector3(
            transform.rotation.x,
            Random.Range(0, 360),
            transform.rotation.z
        );
    }

    // Smoothly reorients the dog to face a target position
    public void ReorientRotation(Vector3 position)
    {
        Vector3 direction = Vector3.Normalize(position - transform.position);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            5000 * Time.deltaTime
        );
    }
}