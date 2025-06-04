using Shapes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Steer2D;

public class GridManager : MonoBehaviour
{
    // ====================================
    // == COMPONENT REFERENCES & SETTINGS ==
    // ====================================
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

    // =====================
    // == UI REFERENCES ==
    // =====================
    public Text MovesLeftVariableObj;
    public UnityEngine.UI.Image MovesLeftImageObj;
    public UnityEngine.UI.Image DrawingOverlay;
    public GameObject[] SheepsUI;
    public GameObject LineDrawing;

    // =====================
    // == PLAYER & PATHING ==
    // =====================
    public GameObject dog;
    private float currentMoves;
    private Camera cam;
    private Polyline line;
    private GameObject currentTileForInput;

    // =========================
    // == DOG START POSITION  ==
    // =========================
    private Vector3 dogStartPosition;

    // =====================
    // == GAME STATE FLAGS ==
    // =====================
    public bool showTutorial = false;
    public static bool startPuzzle = false;

    // =====================
    // == SHEEP ROTATION STATE ==
    // =====================
    private readonly HashSet<Sheepy> rotatedSheep = new HashSet<Sheepy>();

    // =====================
    // == INITIALIZATION ==
    // =====================
    void Awake()
    {
        // Get references to core components
        cam = Camera.main;
        SheepsUI = GameObject.FindGameObjectsWithTag("SheepDisc");
        Blockers = GameObject.FindGameObjectsWithTag("BLOCKER");
        dog = GameObject.FindGameObjectWithTag("Player");
        line = LineDrawing.GetComponent<Polyline>();
        DrawingOverlay = GameObject.FindGameObjectWithTag("DrawingOverlay").GetComponent<UnityEngine.UI.Image>();
        Tile = Resources.Load("GridTile") as GameObject;
        dog = GameObject.FindGameObjectWithTag("Player");

        // NEW — remember where the dog begins (rounded to whole grid units)
        dogStartPosition = new Vector3(
            Mathf.RoundToInt(dog.transform.position.x),
            Mathf.RoundToInt(dog.transform.position.y),
            Mathf.RoundToInt(dog.transform.position.z));
        
        // Initialize game state
                startPuzzle = false;
        GenerateGrid();
        ClearTilePath();
    }

    // =================
    // == MAIN UPDATE ==
    // =================
    void Update()
    {
        // Skip input processing if dog is moving
        if (dog.GetComponent<Doggy>().IsMoving)
            return;

        // Update moves UI display
        UpdateMovesUI();

        // Only process input when game logic is active
        if (!Game.ActiveLogic)
            return;

        // Handle mouse input
        if (Input.GetMouseButtonUp(0))
            HandleTileClick();

        // Right click resets the level
        if (Input.GetMouseButtonUp(1))
            ResetLevel();

        // Handle keyboard input for path creation
        HandleKeyboardInput();
    }

    // =====================
    // == UI MANAGEMENT ==
    // =====================
    void UpdateMovesUI()
    {
        int movesLeft = (int)(MaxMoves - currentMoves);
        MovesLeftVariableObj.text = movesLeft.ToString();
        MovesLeftImageObj.color = (movesLeft > 0)
            ? new Color(1, 0.784f, 0.196f, 1)  // Yellow when moves available
            : new Color(1, 0.294f, 0.392f, 1); // Red when no moves left
    }

    // =====================
    // == INPUT HANDLING ==
    // =====================
    void HandleKeyboardInput()
    {
        // WASD keys for directional path building
        if (Input.GetKeyDown(KeyCode.W)) TryAddTileInDirection(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.S)) TryAddTileInDirection(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.A)) TryAddTileInDirection(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.D)) TryAddTileInDirection(Vector2Int.right);
    }

    void TryAddTileInDirection(Vector2Int direction)
    {
        if (currentTileForInput == null) return;

        // Calculate current and target positions
        Vector2Int currentPos = new Vector2Int(
            Mathf.RoundToInt(currentTileForInput.transform.position.x),
            Mathf.RoundToInt(currentTileForInput.transform.position.z)
        );

        Vector2Int nextPos = currentPos + direction;
        GameObject nextTile = GetTileAtPosition(nextPos);

        // Process tile if valid
        if (nextTile != null)
        {
            ProcessTileSelection(nextTile);
            if (TilePath.Contains(nextTile))
                currentTileForInput = nextTile;
        }
    }

    GameObject GetTileAtPosition(Vector2Int pos)
    {
        // Find tile at specified grid position
        foreach (GameObject tile in Tiles)
        {
            Vector2Int tilePos = new Vector2Int(
                Mathf.RoundToInt(tile.transform.position.x),
                Mathf.RoundToInt(tile.transform.position.z)
            );
            if (tilePos == pos)
                return tile;
        }
        return null;
    }

    void HandleTileClick()
    {
        // Raycast to detect tile clicks
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.CompareTag("GridTile"))
            {
                ProcessTileSelection(hit.transform.gameObject);
            }
        }
    }

    // =====================
    // == PATH MANAGEMENT ==
    // =====================
    void ProcessTileSelection(GameObject tile)
    {
        // Prevent selecting the tile the dog starts on
        if (Vector3.Distance(tile.transform.position, dogStartPosition) < 0.01f)
            return;

        // Ignore clicks on tiles already in the path
        if (TilePath.Contains(tile))
            return;

        // Only add tiles if moves are available
        if (currentMoves < MaxMoves)
        {
            TryAddTileToPath(tile);
        }
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
        // Check if path between tiles is blocked
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
        // Mark tile as selected and add to path
        tile.GetComponent<GridTile>().selected = true;
        TilePath.Add(tile);

        // Update line renderer
        line.AddPoint(new Vector3(tile.transform.position.x, tile.transform.position.z, 0));

        currentMoves++;
        UpdateSheepRotations(tile);
    }

    void UpdateSheepRotations(GameObject tile)
    {
        // Rotate sheep that are near the newly added tile
        foreach (GameObject sheep in Game.Sheeps)
        {
            var sheepComp = sheep.GetComponent<Sheepy>();

            // Skip if already rotated this sheep
            if (rotatedSheep.Contains(sheepComp))
                continue;

            // Rotate if close enough to new tile
            if (Vector3.Distance(tile.transform.position, sheep.transform.position) <= 1.42f)
            {
                sheepComp.ReorientRotation(tile.transform.position, TilePath.Count);
                rotatedSheep.Add(sheepComp);
            }
        }
    }

    // =====================
    // == GAME ACTIONS ==
    // =====================
    public void OnGoButtonPressed()
    {
        // Start dog movement if path exists and dog isn't already moving
        if (TilePath.Count > 0 && !dog.GetComponent<Doggy>().IsMoving)
        {
            dog.GetComponent<Doggy>().StartMoving(TilePath);
            startPuzzle = true;
            SetSheepGridVisibility(false);
        }
    }

    void SetSheepGridVisibility(bool visible)
    {
        // Toggle sheep grid visibility
        foreach (GameObject sheep in Game.Sheeps)
            sheep.GetComponent<Sheepy>().showGridObj = visible;
    }

    void ResetLevel()
    {
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator ShowTutorial()
    {
        // Display tutorial popup
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
        // Reset all tiles in current path
        foreach (GameObject tile in TilePath)
            tile.GetComponent<GridTile>().selected = false;

        // Clear path data
        TilePath.Clear();
        line.points.Clear();
        rotatedSheep.Clear();

        // Reset line renderer to dog's position
        line.AddPoint(new Vector3(dog.transform.position.x, dog.transform.position.z, 0));
        currentMoves = 0;

        // Set current input tile to dog's position
        currentTileForInput = GetTileAtPosition(new Vector2Int(
            Mathf.RoundToInt(dog.transform.position.x),
            Mathf.RoundToInt(dog.transform.position.z)
        ));
    }

    // =====================
    // == GRID GENERATION ==
    // =====================
    void GenerateGrid()
    {
        // Create grid points and instantiate tiles
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
        // Generate grid points within specified size, skipping blocked positions
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
        // Check if position is occupied by a blocker
        foreach (GameObject blocker in Blockers)
        {
            if (blocker.transform.position == (Vector3)position)
                return true;
        }
        return false;
    }

    // =====================
    // == EDITOR TOOLS ==
    // =====================
    void OnDrawGizmos()
    {
        // Update grid visualization when parameters change
        if (GridSize != currentGridSize || GridOffset != currentGridOffset)
        {
            currentGridSize = GridSize;
            currentGridOffset = GridOffset;
            gridpoints = GetGridPoints(Vector3Int.RoundToInt(transform.position), GridSize);
        }

        // Draw grid boundary
        Gizmos.color = new Color(0, 0.5f, 0.25f, 1);
        Gizmos.DrawWireCube(transform.position + new Vector3(GridOffset.x, 0, GridOffset.y),
                           new Vector3(GridSize.x * 2 + 1, 0, GridSize.y * 2 + 1));

        // Draw grid points
        foreach (Vector3Int point in gridpoints)
        {
            Gizmos.color = new Color(0, 1, 0.5f, 0.25f);
            Gizmos.DrawCube(point, new Vector3(GizmoSize, 0, GizmoSize));
        }
    }
}