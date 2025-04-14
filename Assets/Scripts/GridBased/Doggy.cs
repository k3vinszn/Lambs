using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UIElements;

public class Doggy : MonoBehaviour
{
    private AudioSource sfx;
    private Animator dogAnimator;
    private float speed;
    private int currentPathIndex = 0;
    private List<GameObject> pathTiles;
    private bool isMoving = false;

    public bool IsMoving { get { return isMoving; } }
    void Awake()
    {
        //round positions to the nearest int to avoid weird behaviour
        transform.position = new Vector3(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z));

        sfx = GetComponent<AudioSource>();
        dogAnimator = transform.GetChild(0).GetComponent<Animator>();
        this.name = this.name + "  " + this.transform.position.ToString();

        RandomizeRotation();
        speed = Game.AnimalSpeed;
    }

    public void StartMoving(List<GameObject> path)
    {
        pathTiles = path;
        currentPathIndex = 0;
        isMoving = true;
        dogAnimator.SetBool("isMoving", true);
    }

    void Update()
    {
        if (isMoving)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        if (currentPathIndex < pathTiles.Count)
        {
            // Calculate direction and move
            Vector3 targetPosition = pathTiles[currentPathIndex].transform.position;

            // Move the dog
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

            //Calculate direction CORRECTLY for animation
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;

            // Only rotate if actually moving (direction isn't zero)
            if (directionToTarget != Vector3.zero)
            {
                //Use the direction directly without inverting
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.LookRotation(directionToTarget),
                    25 * Time.deltaTime
                );
            }

            // Check if reached current waypoint
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

    private bool CanContinueMoving()
    {
        if (currentPathIndex < pathTiles.Count - 1)
        {
            RaycastHit hit;
            Vector3 nextTilePosition = pathTiles[currentPathIndex + 1].transform.position;

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

    private void StopMoving()
    {
        isMoving = false;
        dogAnimator.SetBool("isMoving", false);
        Game.PathComplete = true;
    }

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