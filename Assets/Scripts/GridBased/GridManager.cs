using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Steer2D;
using UnityEditor;
using UnityEngine.UIElements;



public class GridManager : MonoBehaviour
{
    private GameObject Tile;

    public float MaxMoves;

    public Vector2Int GridSize;
    public Vector2Int GridOffset;
    private Vector2Int currentGridSize = new Vector2Int(0,0);
    private Vector2Int currentGridOffset = new Vector2Int(100, 100);

    public List<Vector3Int> gridpoints;
    public float GizmoSize = 0.25f;

    public List<GameObject> Tiles;
    public List<GameObject> TilePath;

    public GameObject previousTileSelected;
    public bool pathBlocked = false;

    private Camera cam;

    //private GameObject[] Goals;
    private GameObject[] Blockers;
    public GameObject[] SheepsUI;

    public GameObject dog;
    public Animator dogAnimator;
    public GameObject LineDrawing;

    private Transform DiscStart;
    private Transform DiscEnd;
    public bool showTutorial = false;
    private UnityEngine.UI.Image DrawingOverlay;

    public static bool startPuzzle = false;
    public bool Moving = false;
    private int currentIndex = 0;
    private float Speed;

    public Text MovesLeftVariableObj;
    public UnityEngine.UI.Image MovesLeftImageObj;
    private float currentMoves;

    public float RecordDistance;

    private Polyline line;

    // Start is called before the first frame update
    void Awake()
    {
        Speed = Game.AnimalSpeed;

        //Find and Load Stuff
        cam = Camera.main;
        SheepsUI = GameObject.FindGameObjectsWithTag("SheepDisc");
        //Goals = GameObject.FindGameObjectsWithTag("GoalPosition");
        Blockers = GameObject.FindGameObjectsWithTag("BLOCKER");
        dog = GameObject.FindGameObjectWithTag("Player");
        line = LineDrawing.GetComponent<Polyline>();
        DrawingOverlay = GameObject.FindGameObjectWithTag("DrawingOverlay").GetComponent<UnityEngine.UI.Image>();
        Tile = Resources.Load("GridTile") as GameObject;

        dogAnimator = dog.transform.GetChild(0).GetComponent<Animator>();

        //Add Discs
        DiscStart = transform.GetChild(0);
        DiscEnd = transform.GetChild(1);

        //Add line first point
        Vector3 p = new Vector3(dog.transform.position.x, dog.transform.position.z, 0);
        line.AddPoint(p);
        DiscStart.position = new Vector3(dog.transform.position.x, DiscStart.position.y, dog.transform.position.z);
        DiscEnd.position = new Vector3(dog.transform.position.x, DiscEnd.position.y, dog.transform.position.z);

        if (DiscStart.gameObject.activeSelf == true)
            DiscStart.gameObject.SetActive(false);

        if (DiscEnd.gameObject.activeSelf == true)
            DiscEnd.gameObject.SetActive(false);

        GridManager.startPuzzle = false;
        GenerateGrid();
    }

    void RotateGameObject(Vector3 to, Vector3 from, float RotationSpeed, float offset)
    {
        
        
    }

    void Move()
    {
        //FIGURE OUT DIRECTION THE DOG IS FACING
        Vector3 dogFacingDirection = dog.transform.position - TilePath[currentIndex].transform.position;

        if (currentIndex < TilePath.Count)
        {
            dog.transform.position = Vector3.MoveTowards(dog.transform.position, TilePath[currentIndex].transform.position, Time.deltaTime * Speed);
            dog.transform.rotation = Quaternion.Lerp(dog.transform.rotation, Quaternion.LookRotation(-dogFacingDirection), 25 * Time.deltaTime);

            dogAnimator.SetBool("isMoving", true);
        }
        //	print(TilePath[0]);
        if (currentIndex < TilePath.Count && dog.transform.position == TilePath[currentIndex].transform.position)
        {
            if(CheckIfDogCanKeepMoving())
            {
                //Debug.Log("all good, proceed");
                currentIndex += 1;
            }
            else
            {
                dogAnimator.SetBool("isMoving", false);
                Moving = false;
                Game.PathComplete = true;
            }
            
        }

        if (currentIndex != 0 && currentIndex == TilePath.Count)
        {
            dogAnimator.SetBool("isMoving", false);
            Moving = false;
            Game.PathComplete = true;
        }
    }
    public bool CheckIfDogCanKeepMoving()
    {
        RaycastHit hit;

        Vector3 dogMidPosition = dog.transform.position + new Vector3(0, 0.25f, 0);

        if(TilePath[currentIndex] != TilePath[TilePath.Count-1])
        {
            //Debug.Log("we're NOT in the last tile");

            if(Physics.Raycast(TilePath[currentIndex+1].transform.position, Vector3.up, out hit, 1.5f)
                && hit.collider.gameObject != null)
            {
                if (hit.collider.gameObject.tag == "Sheep")
                {
                    hit.collider.gameObject.GetComponent<Sheepy>().UpdateFleeDirection();

                    if(!hit.collider.gameObject.GetComponent<Sheepy>().CheckIfCanKeepMoving(hit.collider.gameObject.GetComponent<Sheepy>().FleeDirection))
                    {
                        //Debug.Log("DOG IS NEXT TO A SHEEP THAT CANT MOVE IN THIS DIRECTION");
                        return (false);
                    }

                    //Debug.Log("DOG CAN MOVE FREELY && went through sheep test!");
                    return (true);
                }

                //Debug.Log("DOG CAN MOVE FREELY!");
                return (true);
            }
            else
            {
                //Debug.Log("DOG CAN MOVE FREELY cause raycast hit nothing");
                return (true);
            }
        }
        else
        {
            //Debug.Log("we're in the last tile");
            return (true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //update UI moves left
        MovesLeftVariableObj.text = "" + (MaxMoves - currentMoves);

        if ((MaxMoves - currentMoves) > 0)
        {
            //update UI moves left color
            MovesLeftImageObj.color = new Color(1, 0.784f, 0.196f, 1);
        }
        else
        {
            //update UI moves left color
            MovesLeftImageObj.color = new Color(1, 0.294f, 0.392f, 1);
        }
        

        //IF GAME IS NOT PAUSED
        if (Game.ActiveLogic)
        {
            if (Moving)
            {
                Move();
            }

            if (Input.GetMouseButton(0) && !Moving)
            {

                if (DiscStart.gameObject.activeSelf == false)
                    DiscStart.gameObject.SetActive(true);

                if (DiscEnd.gameObject.activeSelf == false)
                    DiscEnd.gameObject.SetActive(true);

                //decrease the overlay when you draw on the screen
                if (!Moving)
                    DrawingOverlay.rectTransform.localScale = new Vector3(1 - (currentMoves / MaxMoves), DrawingOverlay.rectTransform.localScale.y, DrawingOverlay.rectTransform.localScale.z);

                RaycastHit hit;
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit))
                {
                    GameObject objectHit = hit.transform.gameObject;

                    if (objectHit.tag == "GridTile" && objectHit != null)
                    {
                        if (objectHit != previousTileSelected)
                        {
                            //Debug.Log(objectHit.name + " hit teste");

                            if (TilePath.Contains(objectHit))
                            {
                                //Debug.Log("HIT OBJECT " + TilePath.IndexOf(objectHit) + " AND THE STORED INDEX-1 WAS " + (TilePath.IndexOf(previousTileSelected) - 1));

                                if (TilePath.IndexOf(objectHit) == (TilePath.IndexOf(previousTileSelected) - 1))
                                {
                                    Vector3 p = new Vector3(previousTileSelected.transform.position.x, previousTileSelected.transform.position.z, 0);
                                    line.points.Remove(new PolylinePoint(p));

                                    line.UpdateMesh(true);

                                    previousTileSelected.GetComponent<GridTile>().selected = false;
                                    TilePath.Remove(previousTileSelected);
                                    previousTileSelected = objectHit;

                                    DiscEnd.position = new Vector3(objectHit.transform.position.x, DiscEnd.position.y, objectHit.transform.position.z);

                                    //make the sheep return from the direction they will go to default state
                                    foreach (GameObject sheep in Game.Sheeps)
                                    {
                                        if (Vector3.Distance(objectHit.transform.position, sheep.transform.position) >= 1.42f
                                            && sheep.GetComponent<Sheepy>().reorientedOnce
                                            && sheep.GetComponent<Sheepy>().reorientedPathIndex > TilePath.Count)
                                        {
                                            //Debug.Log("reorient possible");
                                            sheep.GetComponent<Sheepy>().reorientedOnce = false;
                                            sheep.GetComponent<Sheepy>().ResetRotation();
                                        }
                                    }

                                    currentMoves--;

                                    //make the dog turn on the first path tile
                                    if (currentMoves == 1)
                                    {
                                        dog.GetComponent<Doggy>().ReorientRotation(objectHit.transform.position);
                                    }

                                    /*line.positionCount = TilePath.Count;
                                    line.SetPosition(TilePath.Count - 1, TilePath[TilePath.Count - 1].transform.position);*/
                                }
                                else
                                {
                                    //Debug.Log(TilePath.IndexOf(objectHit) + " TILE IS PART OF THE PATH BUT WASN'T SELECTED LAST");
                                }

                            }
                            else
                            {
                                //FIRST MOVE/CLICK CONDITIONS
                                if (TilePath.Count == 0)
                                {
                                    

                                    if (Vector3.Distance(dog.transform.position, objectHit.transform.position) < 1)
                                    {
                                        objectHit.GetComponent<GridTile>().selected = true;
                                        TilePath.Add(objectHit);
                                        previousTileSelected = objectHit;

                                    }
                                    else
                                    {
                                        if (showTutorial)
                                            StartCoroutine(ShowTutorial());

                                    }

                                }
                                else
                                {
                                    //REST OF THE MOVES AFTER YOU START A PATH
                                    if (Vector3.Distance(objectHit.transform.position, previousTileSelected.transform.position) <= 1 && currentMoves < MaxMoves)
                                    {

                                        //check if there's any blocker between the last grid tile and the selected tile
                                        foreach (GameObject blocker in Blockers)
                                        {
                                            if (Vector3.Distance(objectHit.transform.position, blocker.transform.position) <= 0.5f
                                                && Vector3.Distance(previousTileSelected.transform.position, blocker.transform.position) <= 0.5f)
                                            {
                                                //Debug.Log("there's a blocker between these two points here");
                                                pathBlocked = true;
                                            }
                                        }

                                        if (!pathBlocked)
                                        {
                                            objectHit.GetComponent<GridTile>().selected = true;
                                            TilePath.Add(objectHit);
                                            previousTileSelected = objectHit;

                                            DiscEnd.position = new Vector3(objectHit.transform.position.x, DiscEnd.position.y, objectHit.transform.position.z);

                                            Vector3 p = new Vector3(objectHit.transform.position.x, objectHit.transform.position.z, 0);
                                            line.AddPoint(p);

                                            //make the sheep turn into the direction they will go
                                            foreach (GameObject sheep in Game.Sheeps)
                                            {
                                                if (Vector3.Distance(objectHit.transform.position, sheep.transform.position) <= 1.42f
                                                    && !sheep.GetComponent<Sheepy>().reorientedOnce)
                                                {
                                                    sheep.GetComponent<Sheepy>().ReorientRotation(objectHit.transform.position, TilePath.Count);
                                                    sheep.GetComponent<Sheepy>().reorientedOnce = true;
                                                }
                                            }

                                            /*
                                            line.positionCount = TilePath.Count;
                                            line.SetPosition(TilePath.Count - 1, TilePath[TilePath.Count - 1].transform.position);*/

                                            currentMoves++;

                                            //make the dog turn on the first path tile
                                            if (currentMoves == 1)
                                            {
                                                dog.GetComponent<Doggy>().ReorientRotation(objectHit.transform.position);
                                            }
                                        }
                                        else
                                        {
                                            //reset pathBlocked so the loop runs again and rechecks
                                            pathBlocked = false;
                                        }

                                    }
                                    else
                                    {
                                        //Debug.Log("invalid move");
                                    }

                                    
                                }



                            }
                        }

                    }
                    else if (objectHit == null)
                    {
                        ClearTilePath();
                    }

                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                if(TilePath.Count <= 0)
                {
                    ClearTilePath();

                }
                else if(TilePath.Count > 0)
                {
                    //Debug.Log("LEFT MOUSE IS UP");
                    Moving = true;
                    GridManager.startPuzzle = true;

                    //make the sheep grid turn off when simulation starts
                    foreach (GameObject sheep in Game.Sheeps)
                    {
                        sheep.GetComponent<Sheepy>().showGridObj = false;
                    }

                    /*
                        if (DiscStart.gameObject.activeSelf == true)
                            DiscStart.gameObject.SetActive(false);

                        if (DiscEnd.gameObject.activeSelf == true)
                            DiscEnd.gameObject.SetActive(false);

                        if(LineDrawing.activeSelf == true)
                            LineDrawing.SetActive(false);*/

                    DrawingOverlay.rectTransform.localScale = new Vector3(0, DrawingOverlay.rectTransform.localScale.y, DrawingOverlay.rectTransform.localScale.z);

                    if (TilePath.Count > 0)
                        RecordDistance = MaxMoves;

                    //ClearTilePath();
                }

            }

            if (Input.GetMouseButtonUp(1))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                //ClearTilePath();
            }

        }
    }

    IEnumerator ShowTutorial()
    {
        yield return new WaitForSeconds(0.2f);
        if (Game.ActiveLogic)
        {
            Game.ActiveLogic = false;
            Debug.LogWarning("Please select a point near the dog radius to start the path");
            Instantiate((GameObject)Resources.Load("How2MoveUI_GRID"), transform.position, Quaternion.identity);
            yield return new WaitForSeconds(4.5f);
            Game.ActiveLogic = true;
        }
    }

    void ClearTilePath()
    {
        foreach (GameObject tileobj in TilePath)
        {
            tileobj.GetComponent<GridTile>().selected = false;
        }
        previousTileSelected = null;
        TilePath.Clear();

        line.points.Clear();

        //Add line first point
        Vector3 p = new Vector3(dog.transform.position.x, dog.transform.position.z, 0);
        line.AddPoint(p);
        //ClearAllPoints();
    }

    void GenerateGrid()
    {
        gridpoints = GetGridPoints(Vector3Int.RoundToInt(transform.position), GridSize);

        foreach (Vector3Int gpoint in gridpoints)
        {
            
            GameObject tile = Instantiate(Tile, gpoint, Tile.transform.rotation, transform);
            Tiles.Add(tile);
            tile.name = "Tile_" + Tiles.IndexOf(tile);
        }
    }

    public List<Vector3Int> GetGridPoints(Vector3Int pos, Vector2Int size)
    {
        List<Vector3Int> gridPoints = new List<Vector3Int>();
        //Goals = GameObject.FindGameObjectsWithTag("GoalPosition");
        Blockers = GameObject.FindGameObjectsWithTag("BLOCKER");
        int sizeH = size.x;
        int sizeV = size.y;

        for (int i = -sizeH; i <= sizeH; i++)
        {
            for (int k = -sizeV; k <= sizeV; k++)
            {
                Vector3Int gridPoint = new Vector3Int(pos.x + i, pos.y, pos.z + k);

                if (Mathf.Abs(gridPoint.x) <= sizeH && Mathf.Abs(gridPoint.z) <= sizeV)
                {
                    gridPoint += new Vector3Int(GridOffset.x,0,GridOffset.y);

                    bool GoalInsideGridPoint = false;
                    bool BlockerInsideGridPoint = false;

                    /*
                    //check if there's shit in the way of the grid
                    foreach (GameObject sheep in SheepsUI)
                    {
                        if (sheep.transform.position == gridPoint)
                        {
                            GoalInsideGridPoint = true;
                            //Debug.Log("Goal " + goal.name + " is in the same place as the " + gridPoint + " grid point!");
                        }
                    }*/

                    //check if there's shit in the way of the drawable grid
                    
                    foreach (GameObject blocker in Blockers)
                    {
                        if (blocker.transform.position == gridPoint)
                        {
                            BlockerInsideGridPoint = true;
                            //Debug.Log("BLOCKER " + blocker.name + " is in the same place as the " + gridPoint + " grid point!");
                        }
                    }

                    if (!BlockerInsideGridPoint)
                    {
                        gridPoints.Add(gridPoint);
                        //Debug.Log("added " + gridPoint + " to gridpoints!");
                    }

                    /*
                    foreach (GameObject goal in Goals)
                    {
                        if (goal.transform.position == gridPoint)
                        {
                            GoalInsideGridPoint = true;
                            //Debug.Log("Goal " + goal.name + " is in the same place as the " + gridPoint + " grid point!");
                        }
                    }

                    if(!GoalInsideGridPoint)
                    {
                        gridPoints.Add(gridPoint);
                        //Debug.Log("added " + gridPoint + " to gridpoints!");
                    }*/
                    
                }
            }
        }
        return gridPoints;
    }

    void OnDrawGizmos()
    {
        //only refresh the gridpoint counting if the grid setting values change, otherwise leave it be you spammer!
        if (GridSize != currentGridSize || GridOffset != currentGridOffset)
        {
            currentGridSize = GridSize;
            currentGridOffset = GridOffset;
            gridpoints = GetGridPoints(Vector3Int.RoundToInt(transform.position), GridSize);
        }

        Gizmos.color = new Color(0, 0.5f, 0.25f, 1);
        Gizmos.DrawWireCube(transform.position + new Vector3(GridOffset.x, 0, GridOffset.y), new Vector3(GridSize.x * 2 + 1, 0, GridSize.y * 2 + 1));

        foreach (Vector3Int point in gridpoints)
        {
            Gizmos.color = new Color(0, 1, 0.5f, 0.25f);
            Gizmos.DrawCube(point, new Vector3(GizmoSize, 0, GizmoSize));
            //Gizmos.DrawSphere(point, GizmoSize);
        }

        //Gizmos.color = new Color(0,1,1,0.2f);
        //Gizmos.DrawSphere(transform.position, distance);
    }
}
