#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEditor;
using UnityEditorInternal;

public class PlayerMovement : MonoBehaviour {
    

    public enum typeMovement { Arrows, GridMove, Click, Draw };
    public typeMovement TypeOfMovement;

    public bool RoundToGrid = false;

    public bool Teste = false;
    public int Valor = 10;

    //Keyboard Arrow Movement Variables
    public float ArrowsSpeed = 25;

    //Grid Movement Variables
    public float gridMoveSpeed = 3f;
    public float gridSize = 1f;

    public bool allowDiagonals = false;
    public bool correctDiagonalSpeed = true;

    private enum Orientation
    {
        Horizontal,
        Vertical
    };
    private Orientation gridOrientation = Orientation.Horizontal;
    private Vector3 input;
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float t;
    private float factor;

    //Click to Move Variables
    public LayerMask clickableLayers;
    public NavMeshAgent myAgent;
    public float clickMoveSpeed = 5;

    //Drawing Path Variables
    public float MaxDistance;
    public float RecordDistance;
    public float drawMoveSpeed = 10;

    private LineRenderer line;
    private List<Vector3> pointsList;
    private Vector3 mousePos;
    private float currentDistance;
    private float totalDistance;
    private bool Moving = false;
    private int currentIndex = 0;
    private int MaxIndex = 0;

    

    /*
    // Structure for line points
    struct myLine
    {
        public Vector3 StartPoint;
        public Vector3 EndPoint;
    };*/

    void Awake()
    {
        //myAgent = gameObject.GetComponent<NavMeshAgent>();

        line = GetComponent<LineRenderer>();
        pointsList = new List<Vector3>();  
    }

    //debug show sphere on mousepos
    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(mousePos, 0.25f);
    }*/

    void Update()
    {

        switch(TypeOfMovement)
        {
            case typeMovement.Arrows:

                //Debug.Log("KEYBOARD ARROWS TO MOVE");
                myAgent.enabled = false;

                transform.position += new Vector3(Input.GetAxis("Horizontal") * ArrowsSpeed * Time.deltaTime, 0, Input.GetAxis("Vertical") * ArrowsSpeed * Time.deltaTime);

                //round position to int so it sticks to the grid
                if(RoundToGrid)
                    transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, Mathf.Round(transform.position.z));

                break;

            case typeMovement.GridMove:

                //Debug.Log("KEYS TO MOVE IN A GRID FASHION");
                myAgent.enabled = false;

                if (!isMoving)
                {
                    input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                    transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, Mathf.Round(transform.position.z));

                    if (!allowDiagonals)
                    {
                        if (Mathf.Abs(input.x) > Mathf.Abs(input.z))
                        {
                            input.z = 0;
                        }
                        else
                        {
                            input.x = 0;
                        }
                    }

                    if (input != Vector3.zero)
                    {
                        StartCoroutine(gridmove(transform));
                    }
                }

                break;

            case typeMovement.Click:

                //Debug.Log("CLICK TO MOVE");
                myAgent.enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    Ray myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;

                    if(Physics.Raycast(myRay, out hitInfo, 100, clickableLayers))
                    {
                        //Debug.Log("I hit something!");
                        myAgent.acceleration = clickMoveSpeed;
                        myAgent.speed = clickMoveSpeed;

                        if (RoundToGrid)
                        {
                            hitInfo.point = new Vector3(Mathf.Round(hitInfo.point.x), hitInfo.point.y, Mathf.Round(hitInfo.point.z));
                        }

                        myAgent.SetDestination(hitInfo.point);
                    }
                }



                break;

            case typeMovement.Draw:

                //Debug.Log("DRAW TO MOVE");
                myAgent.enabled = true;

                if (Moving)
                {
                    Move();
                }

                if (Input.GetMouseButton(0))
                {
                    

                    Ray myRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hitInfo;

                    if (Physics.Raycast(myRay, out hitInfo, 100, clickableLayers))
                    {
                        //Debug.Log("I hit something!");
                     
                        if (RoundToGrid)
                        {
                            mousePos.x = Mathf.Round(hitInfo.point.x);
                            mousePos.z = Mathf.Round(hitInfo.point.z);
                        }
                        else
                        {
                            mousePos = hitInfo.point;
                        }

                        mousePos.y = 0;
                    }

                    /*if (pointsList.Count > 0 || Vector3.Distance(transform.position, mousePos) <= 10)
                    {
                        SavePath();
                    }*/

                    
                    SavePath();

                }

                //clear drawn path
                if (Input.GetMouseButtonDown(1))
                {
                    //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    ClearPath ();
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    Moving = true;
                    //Debug.Log(Moving);

                    MaxIndex = pointsList.Count;
                    CheckPath();
                    //Debug.Log(pointsList.Count);
                }

                break;

            default:

                Debug.Log("NOTHING");

                break;
        }
    }

    


    void CheckPath()
    {
        for (int i = 0; i < pointsList.Count - 1; i++)
        {
            //Vector3.Distance(pointsList[i],pointsList[i + 1])
            double RoundedDistance = System.Math.Round(Vector3.Distance(pointsList[i], pointsList[i + 1]), 2);
            if (RoundedDistance < 0.8f)
            {
                pointsList.RemoveAt(i + 1);
            }
            //Debug.Log(RoundedDistance);
;        }
    }

    void SavePath()
    {

        if (!pointsList.Contains(mousePos) && totalDistance <= MaxDistance)
        {
            if (pointsList.Count == 0)
            {
                pointsList.Add(transform.position);

                //	line.SetVertexCount (pointsList.Count);
                line.positionCount = pointsList.Count;
                line.SetPosition(0, transform.position);
            }
            else
            {
                if (pointsList.Count == 1)
                {
                    currentDistance = Vector3.Distance(transform.position, mousePos);
                }
                else
                {
                    currentDistance = Vector3.Distance((Vector3)pointsList[pointsList.Count - 1], mousePos);

                }

                if (currentDistance >= RecordDistance)
                {
                    if (totalDistance >= MaxDistance)
                    {
                        // Calculate 3rd point using two other vectors
                        Vector3 A = pointsList[pointsList.Count - 1];
                        Vector3 B = mousePos;
                        float wantedDistance = totalDistance - MaxDistance;

                        Vector3 V = B - A;
                        Vector3 Vnormalized = V.normalized;

                        Vector2 wantedPoint = A + wantedDistance * Vnormalized;

                        pointsList.Add(wantedPoint);
                        //Debug.Log("Reached the end" + midpoint + " mousePos" + mousePos );
                        ReturnTotalDistance();
                        totalDistance += currentDistance;
                    }
                    else
                    {
                        pointsList.Add(mousePos);
                        totalDistance += currentDistance;
                    }

                    line.positionCount = pointsList.Count;
                    line.SetPosition(pointsList.Count - 1, (Vector3)pointsList[pointsList.Count - 1]);
                }
            }

            /// Debugs
            //	Debug.Log ("Total Distance: " + totalDistance + "  Current Distance: " + currentDistance);
        }

    }

    void ReturnTotalDistance()
    {
        totalDistance = 0;

        for (int i = 0; i < pointsList.Count - 1; i++)
        {
            currentDistance = Vector3.Distance((Vector3)pointsList[i], pointsList[i + 1]);
            totalDistance += currentDistance;
        }
        Debug.Log("Total Distance is:" + totalDistance);
    }

    void ClearPath()
    {
        line.positionCount = 0;
        pointsList.RemoveRange(0, pointsList.Count);
        totalDistance = 0;
        currentDistance = 0;
        Moving = false;
        currentIndex = 0;
        MaxIndex = 0;
    }

    void Move()
    {
        if (currentIndex < MaxIndex && currentIndex < pointsList.Count)
        {
            currentDistance = Vector3.Distance(transform.position, pointsList[currentIndex]);

            //FIGURE OUT DIRECTION OF THE SPRITE
            /*
			Vector3 dir = transform.position - pointsList[currentIndex];
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle+180);*/

            transform.position = Vector3.MoveTowards(transform.position, pointsList[currentIndex], Time.deltaTime * drawMoveSpeed);

            //Debug.Log("currentDistance" + currentDistance +" "+ "velocity" + (0.5f*currentDistance));
        }
        //	print(pointsList[0]);
        if (currentIndex < pointsList.Count && currentIndex < MaxIndex && transform.position == pointsList[currentIndex])
        {
            currentIndex += 1;
        }

        if (currentIndex != 0 && currentIndex == pointsList.Count)
        {
            Moving = false;
            //Debug.Log("finished moving and my point is count is " + pointsList.Count);

            ClearPath(); 
        }
    }

    //movement with keyboard gridmove mode
    public IEnumerator gridmove(Transform transform)
    {
        isMoving = true;
        startPosition = transform.position;
        t = 0;

        if (gridOrientation == Orientation.Horizontal)
        {
            endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * gridSize,
                startPosition.y, startPosition.z + System.Math.Sign(input.z) * gridSize);
        }
        else
        {
            endPosition = new Vector3(startPosition.x + System.Math.Sign(input.x) * gridSize,
                startPosition.y + System.Math.Sign(input.z) * gridSize, startPosition.z);
        }

        if (allowDiagonals && correctDiagonalSpeed && input.x != 0 && input.z != 0)
        {
            factor = 0.7071f;
        }
        else
        {
            factor = 1f;
        }

        while (t < 1f)
        {
            t += Time.deltaTime * (gridMoveSpeed / gridSize) * factor;
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        isMoving = false;
        yield return 0;
    }

}

[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var PlayerMovement = target as PlayerMovement;


        PlayerMovement.TypeOfMovement = (PlayerMovement.typeMovement) EditorGUILayout.EnumPopup("Movement Control", PlayerMovement.TypeOfMovement);

        EditorGUILayout.Space();

        switch (PlayerMovement.TypeOfMovement)
        {
            case PlayerMovement.typeMovement.Arrows:

                
                PlayerMovement.ArrowsSpeed = EditorGUILayout.FloatField("Movement Speed:", PlayerMovement.ArrowsSpeed);

                EditorGUILayout.Space();
                PlayerMovement.RoundToGrid = EditorGUILayout.Toggle("Clamp to Grid Points", PlayerMovement.RoundToGrid);

                /*PlayerMovement.Teste = GUILayout.Toggle(PlayerMovement.Teste, "Teste Booleano");
                if (PlayerMovement.Teste)
                    PlayerMovement.Valor = EditorGUILayout.IntSlider("Range do Valor:", PlayerMovement.Valor, 1, 100);*/

                break;

            case PlayerMovement.typeMovement.GridMove:

                PlayerMovement.gridMoveSpeed = EditorGUILayout.FloatField("Grid Move Speed:", PlayerMovement.gridMoveSpeed);
                PlayerMovement.gridSize = EditorGUILayout.FloatField("Grid Size:", PlayerMovement.gridSize);

                PlayerMovement.allowDiagonals = EditorGUILayout.Toggle("Allow Diagonal Movement", PlayerMovement.allowDiagonals);
                PlayerMovement.correctDiagonalSpeed = EditorGUILayout.Toggle("Correct Diagonal Speed", PlayerMovement.correctDiagonalSpeed);

                break;

            case PlayerMovement.typeMovement.Click:

                //work around for layer masks (stupid Unity devs forgot about this)
                LayerMask tempMask = EditorGUILayout.MaskField("Clickable Layers", InternalEditorUtility.LayerMaskToConcatenatedLayersMask(PlayerMovement.clickableLayers), InternalEditorUtility.layers);
                PlayerMovement.clickableLayers = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
                
                PlayerMovement.myAgent = (NavMeshAgent) EditorGUILayout.ObjectField("Object to Control", PlayerMovement.myAgent, typeof(NavMeshAgent), true);
                PlayerMovement.clickMoveSpeed = EditorGUILayout.FloatField("Click Move Speed:", PlayerMovement.clickMoveSpeed);

                EditorGUILayout.Space();
                PlayerMovement.RoundToGrid = EditorGUILayout.Toggle("Clamp to Grid Points", PlayerMovement.RoundToGrid);

                break;

            case PlayerMovement.typeMovement.Draw:

                //Debug.Log("DRAW TO MOVE");

                PlayerMovement.MaxDistance = EditorGUILayout.FloatField("Max Drawing:", PlayerMovement.MaxDistance);
                PlayerMovement.RecordDistance = EditorGUILayout.FloatField("Point Margin:", PlayerMovement.RecordDistance);
                PlayerMovement.drawMoveSpeed = EditorGUILayout.FloatField("Moving Speed:", PlayerMovement.drawMoveSpeed);

                EditorGUILayout.Space();
                PlayerMovement.RoundToGrid = EditorGUILayout.Toggle("Clamp to Grid Points", PlayerMovement.RoundToGrid);

                break;

            default:

                Debug.Log("NOTHING");

                break;
        }


        
    }
}
#endif