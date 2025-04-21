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
    // === Grid Config ===
    public float MaxMoves;
    public Vector2Int GridSize;
    public Vector2Int GridOffset;

    private Vector2Int currentGridSize = new Vector2Int(0, 0);
    private Vector2Int currentGridOffset = new Vector2Int(100, 100);

    public float GizmoSize = 0.25f;
    public List<Vector3Int> gridpoints;
    public List<GameObject> Tiles;
    public List<GameObject> TilePath;

    private GameObject Tile;
    private GameObject[] Blockers;

    // === UI ===
    public Text MovesLeftVariableObj;
    public UnityEngine.UI.Image MovesLeftImageObj;
    public UnityEngine.UI.Image DrawingOverlay;
    public GameObject[] SheepsUI;
    public GameObject LineDrawing;

    // === Player and Pathing ===
    public GameObject dog;
    public GameObject previousTileSelected;
    public bool pathBlocked = false;
    public float RecordDistance;
    private float currentMoves;

    private Camera cam;
    private Polyline line;

    // === Discs ===
    private Transform DiscStart;
    private Transform DiscEnd;

    // === Flags ===
    public bool showTutorial = false;
    public static bool startPuzzle = false;

    // ===========================
    // == MonoBehaviour Events ===
    // ===========================

    void Awake()
    {
        cam = Camera.main;
        SheepsUI = GameObject.FindGameObjectsWithTag("SheepDisc");
        Blockers = GameObject.FindGameObjectsWithTag("BLOCKER");
        dog = GameObject.FindGameObjectWithTag("Player");
        line = LineDrawing.GetComponent<Polyline>();
        DrawingOverlay = GameObject.FindGameObjectWithTag("DrawingOverlay").GetComponent<UnityEngine.UI.Image>();
        Tile = Resources.Load("GridTile") as GameObject;

        DiscStart = transform.GetChild(0);
        DiscEnd = transform.GetChild(1);

        Vector3 p = new Vector3(dog.transform.position.x, dog.transform.position.z, 0);
        line.AddPoint(p);

        DiscStart.position = new Vector3(dog.transform.position.x, DiscStart.position.y, dog.transform.position.z);
        DiscEnd.position = new Vector3(dog.transform.position.x, DiscEnd.position.y, dog.transform.position.z);

        DiscStart.gameObject.SetActive(false);
        DiscEnd.gameObject.SetActive(false);

        startPuzzle = false;
        GenerateGrid();
    }

    void Update()
    {
        if (dog.GetComponent<Doggy>().IsMoving)
            return;

        // Update moves UI
        int movesLeft = (int)(MaxMoves - currentMoves);
        MovesLeftVariableObj.text = movesLeft.ToString();
        MovesLeftImageObj.color = (movesLeft > 0)
            ? new Color(1, 0.784f, 0.196f, 1)
            : new Color(1, 0.294f, 0.392f, 1);

        if (!Game.ActiveLogic)
            return;

        if (Input.GetMouseButton(0))
            HandleMouseHold();

        if (Input.GetMouseButtonUp(0))
            HandleMouseRelease();

        if (Input.GetMouseButtonUp(1))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // =======================
    // == Mouse Interactions ==
    // =======================

    void HandleMouseHold()
    {
        if (!DiscStart.gameObject.activeSelf) DiscStart.gameObject.SetActive(true);
        if (!DiscEnd.gameObject.activeSelf) DiscEnd.gameObject.SetActive(true);

        DrawingOverlay.rectTransform.localScale = new Vector3(
            1 - (currentMoves / MaxMoves),
            DrawingOverlay.rectTransform.localScale.y,
            DrawingOverlay.rectTransform.localScale.z
        );

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject objectHit = hit.transform.gameObject;
            if (objectHit.tag == "GridTile" && objectHit != null)
            {
                HandleTileSelection(objectHit);
            }
            else if (objectHit == null)
            {
                ClearTilePath();
            }
        }
    }

    public void OnGoButtonPressed()
    {
        if (TilePath.Count > 0 && !dog.GetComponent<Doggy>().IsMoving)
        {
            dog.GetComponent<Doggy>().StartMoving(TilePath);
            startPuzzle = true;

            foreach (GameObject sheep in Game.Sheeps)
                sheep.GetComponent<Sheepy>().showGridObj = false;
        }
    }

    void HandleMouseRelease()
    {
        if (TilePath.Count <= 0)
        {
            ClearTilePath();
        }

        DrawingOverlay.rectTransform.localScale = new Vector3(
            0,
            DrawingOverlay.rectTransform.localScale.y,
            DrawingOverlay.rectTransform.localScale.z
        );
    }

    public void ExecutePath()
    {
        if (TilePath.Count > 0 && !dog.GetComponent<Doggy>().IsMoving)
        {
            dog.GetComponent<Doggy>().StartMoving(TilePath);
            startPuzzle = true;

            foreach (GameObject sheep in Game.Sheeps)
                sheep.GetComponent<Sheepy>().showGridObj = false;
        }
    }

    // ====================
    // == Tile Selection ==
    // ====================

    void HandleTileSelection(GameObject objectHit)
    {
        if (objectHit == previousTileSelected) return;

        if (TilePath.Contains(objectHit))
        {
            int hitIndex = TilePath.IndexOf(objectHit);
            int prevIndex = TilePath.IndexOf(previousTileSelected);

            if (hitIndex == (prevIndex - 1))
            {
                Vector3 p = new Vector3(previousTileSelected.transform.position.x, previousTileSelected.transform.position.z, 0);
                line.points.Remove(new PolylinePoint(p));
                line.UpdateMesh(true);

                previousTileSelected.GetComponent<GridTile>().selected = false;
                TilePath.Remove(previousTileSelected);
                previousTileSelected = objectHit;

                DiscEnd.position = new Vector3(objectHit.transform.position.x, DiscEnd.position.y, objectHit.transform.position.z);

                foreach (GameObject sheep in Game.Sheeps)
                {
                    var sheepComp = sheep.GetComponent<Sheepy>();
                    if (Vector3.Distance(objectHit.transform.position, sheep.transform.position) >= 1.42f
                        && sheepComp.reorientedOnce
                        && sheepComp.reorientedPathIndex > TilePath.Count)
                    {
                        sheepComp.reorientedOnce = false;
                        sheepComp.ResetRotation();
                    }
                }

                currentMoves--;

                if (currentMoves == 1)
                    dog.GetComponent<Doggy>().ReorientRotation(objectHit.transform.position);
            }
        }
        else
        {
            if (TilePath.Count == 0)
            {
                if (Vector3.Distance(dog.transform.position, objectHit.transform.position) < 1)
                {
                    objectHit.GetComponent<GridTile>().selected = true;
                    TilePath.Add(objectHit);
                    previousTileSelected = objectHit;
                }
                else if (showTutorial)
                {
                    StartCoroutine(ShowTutorial());
                }
            }
            else
            {
                if (Vector3.Distance(objectHit.transform.position, previousTileSelected.transform.position) <= 1 && currentMoves < MaxMoves)
                {
                    pathBlocked = false;

                    foreach (GameObject blocker in Blockers)
                    {
                        if (Vector3.Distance(objectHit.transform.position, blocker.transform.position) <= 0.5f &&
                            Vector3.Distance(previousTileSelected.transform.position, blocker.transform.position) <= 0.5f)
                        {
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

                        foreach (GameObject sheep in Game.Sheeps)
                        {
                            var sheepComp = sheep.GetComponent<Sheepy>();
                            if (Vector3.Distance(objectHit.transform.position, sheep.transform.position) <= 1.42f && !sheepComp.reorientedOnce)
                            {
                                sheepComp.ReorientRotation(objectHit.transform.position, TilePath.Count);
                                sheepComp.reorientedOnce = true;
                            }
                        }

                        currentMoves++;

                        if (currentMoves == 1)
                            dog.GetComponent<Doggy>().ReorientRotation(objectHit.transform.position);
                    }
                    else
                    {
                        pathBlocked = false;
                    }
                }
            }
        }
    }

    // ===================
    // == Grid & Helpers ==
    // ===================

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
        foreach (GameObject tile in TilePath)
            tile.GetComponent<GridTile>().selected = false;

        TilePath.Clear();
        previousTileSelected = null;
        line.points.Clear();

        Vector3 p = new Vector3(dog.transform.position.x, dog.transform.position.z, 0);
        line.AddPoint(p);
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
        Blockers = GameObject.FindGameObjectsWithTag("BLOCKER");

        for (int i = -size.x; i <= size.x; i++)
        {
            for (int k = -size.y; k <= size.y; k++)
            {
                Vector3Int gridPoint = new Vector3Int(pos.x + i, pos.y, pos.z + k);

                if (Mathf.Abs(gridPoint.x) <= size.x && Mathf.Abs(gridPoint.z) <= size.y)
                {
                    gridPoint += new Vector3Int(GridOffset.x, 0, GridOffset.y);
                    bool blocked = false;

                    foreach (GameObject blocker in Blockers)
                    {
                        if (blocker.transform.position == gridPoint)
                        {
                            blocked = true;
                            break;
                        }
                    }

                    if (!blocked)
                        gridPoints.Add(gridPoint);
                }
            }
        }

        return gridPoints;
    }

    void OnDrawGizmos()
    {
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
        }
    }
}
