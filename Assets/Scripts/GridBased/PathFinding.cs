using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

public class PathFinding : MonoBehaviour {

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    public List<GameObject> TileList;

    public List<GridTile> openList;
    public HashSet<GridTile> closedList;

    public bool performanceDebug = false;


    void Awake()
	{
		TileList = GameObject.FindGameObjectWithTag("GridManager").GetComponent<GridManager>().Tiles;
    }

    void Start()
	{
        
	}
	
	void Update()
	{
        
    }

    public List<GameObject> FindPath(Vector3 startingPos, Vector3 destinyPos)
    {
        Stopwatch sw = new Stopwatch();

        if (performanceDebug)
        {
            sw.Start();
        }

        GridTile startTile = GetTile(startingPos).GetComponent<GridTile>();
        GridTile endTile = GetTile(destinyPos).GetComponent<GridTile>();

        openList = new List<GridTile> { startTile };
        closedList = new HashSet<GridTile>();

        foreach (GameObject gridTile in TileList)
        {
            gridTile.GetComponent<GridTile>().gCost = int.MaxValue;
            gridTile.GetComponent<GridTile>().CalculateFCost();
            gridTile.GetComponent<GridTile>().cameFromTile = null;
        }

        startTile.gCost = 0;
        startTile.hCost = CalculateDistanceCost(startingPos, destinyPos);
        startTile.CalculateFCost();

        while(openList.Count > 0)
        {
            GridTile currentTile = GetLowestFCostTile(openList);

            if (endTile == null)
                return null;

            if(currentTile == endTile)
            {
                if (performanceDebug)
                {
                    sw.Stop();
                    print("path found: " + sw.ElapsedMilliseconds + " ms");
                }

                //reached final tile
                return CalculatePath(endTile);
            }

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach(GameObject neighbourTile in GetNeighbourList(currentTile.gameObject))
            {
                if(closedList.Contains(neighbourTile.GetComponent<GridTile>()))
                {
                    continue;
                }

                int tentativeGCost = currentTile.gCost + CalculateDistanceCost(currentTile.gameObject.transform.position, neighbourTile.transform.position);
                if(tentativeGCost < neighbourTile.GetComponent<GridTile>().gCost)
                {
                    neighbourTile.GetComponent<GridTile>().cameFromTile = currentTile.gameObject;
                    neighbourTile.GetComponent<GridTile>().gCost = tentativeGCost;
                    neighbourTile.GetComponent<GridTile>().hCost = CalculateDistanceCost(neighbourTile.transform.position, endTile.gameObject.transform.position);
                    neighbourTile.GetComponent<GridTile>().CalculateFCost();

                    if(!openList.Contains(neighbourTile.GetComponent<GridTile>()))
                    {
                        openList.Add(neighbourTile.GetComponent<GridTile>());
                    }
                }

            }
        }

        //out of tiles on the openList
        return null;
    }

    private List<GameObject> CalculatePath(GridTile endTile)
    {
        List<GameObject> path = new List<GameObject>();

        path.Add(endTile.gameObject);
        GridTile currentTile = endTile;

        while(currentTile.cameFromTile != null)
        {
            path.Add(currentTile.cameFromTile);
            currentTile = currentTile.cameFromTile.GetComponent<GridTile>();
        }

        path.Reverse();
        return path;
    }

    private List<GameObject> GetNeighbourList(GameObject currentTileOBJ)
    {
        List<GameObject> neighbourList = new List<GameObject>();

        foreach (GameObject gridTile in TileList)
        {
            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(1,0,0)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(-1,0,0)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(0,0,1)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(0,0,-1)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(1,0,1)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(-1,0,1)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(1,0,-1)))
            {
                neighbourList.Add(gridTile);
            }

            if (gridTile.transform.position == (currentTileOBJ.transform.position + new Vector3(-1,0,-1)))
            {
                neighbourList.Add(gridTile);
            }
        }

        return neighbourList;
    }

    private GridTile GetLowestFCostTile(List<GridTile> gridtilesList)
    {
        GridTile lowestFCostTile = gridtilesList[0];

        for(int i = 1; i < gridtilesList.Count; i++)
        {
            if (gridtilesList[i].fCost == lowestFCostTile.fCost)
            {
                lowestFCostTile = gridtilesList[i];
            }
        }
        return lowestFCostTile;
    }

    private int CalculateDistanceCost(Vector3 startingPos, Vector3 destinyPos)
    {
        int xDistance = (int)Mathf.Abs(startingPos.x - destinyPos.x);
        int zDistance = (int)Mathf.Abs(startingPos.z - destinyPos.z);
        int remaining = (int)Mathf.Abs(xDistance - zDistance);

        return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + MOVE_STRAIGHT_COST * remaining;
    }

    public GameObject GetTile(Vector3 gridTileposition)
    {
        Vector3 IntGridTilePosition = new Vector3(Mathf.RoundToInt(gridTileposition.x), 0, Mathf.RoundToInt(gridTileposition.z));

        foreach (GameObject gridTile in TileList)
        {
            if (gridTile.transform.position == IntGridTilePosition)
            {
                return gridTile;
            }
        }

        return null;
    }




}
