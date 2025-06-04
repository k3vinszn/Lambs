using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Steer2D;

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
    private float currentMoves;
    private Camera cam;
    private Polyline line;

    // === Flags ===
    public bool showTutorial = false;
    public static bool startPuzzle = false;

    void Awake()
    {
        cam = Camera.main;
        SheepsUI = GameObject.FindGameObjectsWithTag("SheepDisc");
        Blockers = GameObject.FindGameObjectsWithTag("BLOCKER");
        dog = GameObject.FindGameObjectWithTag("Player");
        line = LineDrawing.GetComponent<Polyline>();
        DrawingOverlay = GameObject.FindGameObjectWithTag("DrawingOverlay").GetComponent<UnityEngine.UI.Image>();
        Tile = Resources.Load("GridTile") as GameObject;

        startPuzzle = false;
        GenerateGrid();
        ClearTilePath();
    }

    void Update()
    {
        if (dog.GetComponent<Doggy>().IsMoving)
            return;

        // Update moves UI
        UpdateMovesUI();

        if (!Game.ActiveLogic)
            return;

        if (Input.GetMouseButtonUp(0))
            HandleTileClick();

        if (Input.GetMouseButtonUp(1))
            ResetLevel();
    }

    void UpdateMovesUI()
    {
        int movesLeft = (int)(MaxMoves - currentMoves);
        MovesLeftVariableObj.text = movesLeft.ToString();
        MovesLeftImageObj.color = (movesLeft > 0)
            ? new Color(1, 0.784f, 0.196f, 1)
            : new Color(1, 0.294f, 0.392f, 1);
    }

    void HandleTileClick()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("GridTile"))
            {
                ProcessTileSelection(hit.transform.gameObject);
            }
        }
    }

    void ProcessTileSelection(GameObject tile)
    {
        // If tile is in path, handle removal
        if (TilePath.Contains(tile))
        {
            HandlePathTileClick(tile);
            return;
        }

        // Otherwise handle adding new tile
        if (currentMoves < MaxMoves)
        {
            TryAddTileToPath(tile);
        }
    }

    void HandlePathTileClick(GameObject tile)
    {
        int index = TilePath.IndexOf(tile);

        // If clicking last tile, just remove it
        if (index == TilePath.Count - 1)
        {
            RemoveLastTile();
            return;
        }

        // Otherwise remove this tile and all after it
        RemoveTilesFromIndex(index);
    }

    void TryAddTileToPath(GameObject tile)
    {
        // First tile must be adjacent to dog
        if (TilePath.Count == 0)
        {
            if (IsAdjacentToDog(tile))
            {
                AddTileToPath(tile);
            }
            else if (showTutorial)
            {
                StartCoroutine(ShowTutorial());
            }
            return;
        }

        // Subsequent tiles must be adjacent to last tile and not blocked
        if (IsAdjacentToLastTile(tile) && !IsPathBlocked(TilePath[^1], tile))
        {
            AddTileToPath(tile);
        }
    }

    bool IsAdjacentToDog(GameObject tile)
    {
        Vector2Int dogPos = new Vector2Int(Mathf.RoundToInt(dog.transform.position.x), Mathf.RoundToInt(dog.transform.position.z));
        Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z));
        Vector2Int delta = tilePos - dogPos;

        return (Mathf.Abs(delta.x) == 1 && delta.y == 0) || (delta.x == 0 && Mathf.Abs(delta.y) == 1);
    }

    bool IsAdjacentToLastTile(GameObject tile)
    {
        GameObject lastTile = TilePath[^1];
        Vector2Int lastPos = new Vector2Int(Mathf.RoundToInt(lastTile.transform.position.x), Mathf.RoundToInt(lastTile.transform.position.z));
        Vector2Int tilePos = new Vector2Int(Mathf.RoundToInt(tile.transform.position.x), Mathf.RoundToInt(tile.transform.position.z));
        Vector2Int delta = tilePos - lastPos;

        return (Mathf.Abs(delta.x) == 1 && delta.y == 0) || (delta.x == 0 && Mathf.Abs(delta.y) == 1);
    }



    bool IsPathBlocked(GameObject fromTile, GameObject toTile)
    {
        foreach (GameObject blocker in Blockers)
        {
            if (Vector3.Distance(toTile.transform.position, blocker.transform.position) <= 0.5f &&
                Vector3.Distance(fromTile.transform.position, blocker.transform.position) <= 0.5f)
            {
                return true;
            }
        }
        return false;
    }

    void AddTileToPath(GameObject tile)
    {
        tile.GetComponent<GridTile>().selected = true;
        TilePath.Add(tile);

        // Add to line renderer
        line.AddPoint(new Vector3(tile.transform.position.x, tile.transform.position.z, 0));

        currentMoves++;
        UpdateSheepRotations(tile);
    }

    void RemoveLastTile()
    {
        if (TilePath.Count == 0) return;

        GameObject lastTile = TilePath[^1];
        lastTile.GetComponent<GridTile>().selected = false;
        TilePath.RemoveAt(TilePath.Count - 1);

        line.points.RemoveAt(line.points.Count - 1);
        line.UpdateMesh(true);

        currentMoves--;
    }

    void RemoveTilesFromIndex(int index)
    {
        for (int i = TilePath.Count - 1; i >= index; i--)
        {
            TilePath[i].GetComponent<GridTile>().selected = false;
            TilePath.RemoveAt(i);
            line.points.RemoveAt(i + 1); // +1 for dog's starting point
        }
        line.UpdateMesh(true);
        currentMoves = TilePath.Count;
    }

    void UpdateSheepRotations(GameObject tile)
    {
        foreach (GameObject sheep in Game.Sheeps)
        {
            var sheepComp = sheep.GetComponent<Sheepy>();
            if (Vector3.Distance(tile.transform.position, sheep.transform.position) <= 1.42f)
            {
                sheepComp.ReorientRotation(tile.transform.position, TilePath.Count);
            }
        }
    }

    public void OnGoButtonPressed()
    {
        if (TilePath.Count > 0 && !dog.GetComponent<Doggy>().IsMoving)
        {
            dog.GetComponent<Doggy>().StartMoving(TilePath);
            startPuzzle = true;
            SetSheepGridVisibility(false);
        }
    }

    void SetSheepGridVisibility(bool visible)
    {
        foreach (GameObject sheep in Game.Sheeps)
            sheep.GetComponent<Sheepy>().showGridObj = visible;
    }

    void ResetLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator ShowTutorial()
    {
        yield return new WaitForSeconds(0.2f);
        if (Game.ActiveLogic)
        {
            Game.ActiveLogic = false;
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
        line.points.Clear();

        // Start with dog's position
        line.AddPoint(new Vector3(dog.transform.position.x, dog.transform.position.z, 0));
        currentMoves = 0;
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
                    if (!IsPositionBlocked(gridPoint))
                        gridPoints.Add(gridPoint);
                }
            }
        }
        return gridPoints;
    }

    bool IsPositionBlocked(Vector3Int position)
    {
        foreach (GameObject blocker in Blockers)
        {
            if (blocker.transform.position == (Vector3)position)
                return true;
        }
        return false;
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
        Gizmos.DrawWireCube(transform.position + new Vector3(GridOffset.x, 0, GridOffset.y),
                           new Vector3(GridSize.x * 2 + 1, 0, GridSize.y * 2 + 1));

        foreach (Vector3Int point in gridpoints)
        {
            Gizmos.color = new Color(0, 1, 0.5f, 0.25f);
            Gizmos.DrawCube(point, new Vector3(GizmoSize, 0, GizmoSize));
        }
    }
}