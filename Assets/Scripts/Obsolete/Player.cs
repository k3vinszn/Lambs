using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Player : MonoBehaviour {
	
	//public Steer2D.Seek AgentSeek;
	//public Steer2D.Arrive AgentArrive;
	public bool Moving = false;
	public int currentIndex = 0;
	public int MaxIndex = 0;
	public float constantVelocity = 0;
	public float moveSpeed = 1;

	private LineRenderer line;
	public List<Vector3> pointsList;
	private Vector3 mousePos;
	public float currentDistance;
	public float totalDistance;
	public float MaxDistance;
	public float RecordDistance;

	private AudioSource sfx;

	private Image DrawingOverlay;

	public int controlMode = 1;
	private string controlMsg = "Clicking";

	// Structure for line points
	struct myLine
	{
		public Vector3 StartPoint;
		public Vector3 EndPoint;
	};
	
	void Awake()
	{
		sfx = GetComponent<AudioSource> ();

		line = GetComponent<LineRenderer>();
		pointsList = new List<Vector3>();
		//renderer.material.SetTextureOffset(
	}

	void Start()
	{
		DrawingOverlay = GameObject.FindGameObjectWithTag ("DrawingOverlay").GetComponent<Image>();

		//if (AgentSeek != null)
		//	AgentSeek.TargetPoint = transform.position;
		
		//if (AgentArrive != null)
		//	AgentArrive.TargetPoint = transform.position;
	}
	
	void Update()
	{

		if (Game.ActiveLogic)
		{
			switch(controlMode)
			{
			case 1:
				Drawing();
				break;

			case 2:
				Clicking();
				break;

			case 3:
				Keyboard();
				break;

			default:
				Clicking();
				break;
			}
		}
	}

// DRAW A PATH TO MOVE
	void Drawing()
	{
		if (Input.GetMouseButtonUp(0))
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			position.z = 0;

			//overlay reset
			DrawingOverlay.rectTransform.localScale = new Vector3 (0, DrawingOverlay.rectTransform.localScale.y, DrawingOverlay.rectTransform.localScale.z);
			
			//transform.position = position; OC
			
			//if (AgentSeek != null)
			//	AgentSeek.TargetPoint = position;
			
			//if (AgentArrive != null)
			//	AgentArrive.TargetPoint = position;
		}

		if (Moving)
		{
			Move ();
		}
		
		if(Input.GetMouseButton(0))
		{
			mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			mousePos.z = 0;

			if (pointsList.Count > 0 || Vector3.Distance(transform.position,mousePos) <= 1)
			{
				SavePath();
			}
			else
			{
				StartCoroutine (ShowTutorial ());
				Debug.LogWarning ("Please select a point near the dog radius to start the path");
			}

			//decrease the overlay when you draw on the screen
			if(!Moving)
			DrawingOverlay.rectTransform.localScale = new Vector3 (1-(totalDistance/MaxDistance), DrawingOverlay.rectTransform.localScale.y, DrawingOverlay.rectTransform.localScale.z);

		}

		//clear drawn path
		if(Input.GetMouseButtonDown(1))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
		//	ClearPath ();
		}
		else if(Input.GetMouseButtonUp(0))
		{
			Moving = true;
			MaxIndex = pointsList.Count;
			CheckPath();
			//Debug.Log(pointsList.Count);

			//overlay reset
			DrawingOverlay.rectTransform.localScale = new Vector3 (0, DrawingOverlay.rectTransform.localScale.y, DrawingOverlay.rectTransform.localScale.z);

			if (pointsList.Count > 0 )
				RecordDistance = MaxDistance;
		}
	}

	IEnumerator ShowTutorial()
	{
		yield return new WaitForSeconds (0.2f);
		if (Game.ActiveLogic)
		{
			Game.ActiveLogic = false;
			Instantiate ((GameObject)Resources.Load ("How2MoveUI"), transform.position, Quaternion.identity);
			yield return new WaitForSeconds (4.5f);
			Game.ActiveLogic = true;
		}
	}

	void CheckPath ()
	{
		//Debug.Log(pointsList.Count); 
		for (int i = 0; i < pointsList.Count-1; i++)
		{
			//Vector3.Distance(pointsList[i],pointsList[i + 1])
			double RoundedDistance = System.Math.Round(Vector3.Distance(pointsList[i],pointsList[i + 1]),2);
			if (RoundedDistance <0.8f)
			{
			pointsList.RemoveAt(i+1);
			}
			Debug.Log(RoundedDistance) ;
		}
		//Debug.Log(pointsList.Count); 
	}

	void SavePath ()
	{

		if (!pointsList.Contains (mousePos) && totalDistance <= MaxDistance) 
		{
			if (pointsList.Count == 0) 
			{
				pointsList.Add (transform.position);

			//	line.SetVertexCount (pointsList.Count);
				line.positionCount = pointsList.Count;
				line.SetPosition (0, transform.position);
			}
			else
			{
				if (pointsList.Count == 1) 
				{
					currentDistance = Vector3.Distance(transform.position,mousePos);
				}
				else
				{
					currentDistance = Vector3.Distance((Vector3)pointsList [pointsList.Count - 1], mousePos);

				}

				if (currentDistance >= RecordDistance)
				{	
					if (totalDistance >= MaxDistance)
					{
						// Calculate 3rd point using two other vectors
						Vector3 A = pointsList [pointsList.Count - 1 ];
						Vector3 B = mousePos;
					 	float wantedDistance = totalDistance - MaxDistance;

						Vector3 V = B - A;
						Vector3 Vnormalized = V.normalized;

						Vector2 wantedPoint = A + wantedDistance * Vnormalized;

						pointsList.Add (wantedPoint);
						//Debug.Log("Reached the end" + midpoint + " mousePos" + mousePos );
						ReturnTotalDistance ();
						totalDistance += currentDistance;
					}
					else
					{
					pointsList.Add (mousePos);
					totalDistance += currentDistance;
					}

					line.positionCount = pointsList.Count;
					line.SetPosition (pointsList.Count - 1, (Vector3)pointsList [pointsList.Count - 1]);
				}
			}

			/// Debugs
			//	Debug.Log ("Total Distance: " + totalDistance + "  Current Distance: " + currentDistance);
		}

	}

	void ReturnTotalDistance ()
	{
		totalDistance = 0;

		for (int i = 0; i < pointsList.Count - 1; i++)
		{
			currentDistance =  Vector3.Distance((Vector3)pointsList [i],pointsList [i + 1]);
			totalDistance+= currentDistance;
		}
		Debug.Log("Total Distance is:" + totalDistance);
	}

	void ClearPath ()
	{
		line.positionCount = 0;
		pointsList.RemoveRange(0,pointsList.Count);
		totalDistance = 0;
		currentDistance = 0;
		Moving = false;
		currentIndex = 0;
		Game.PathComplete = false;
	} 

	void Move () 
	{
		if (currentIndex < MaxIndex && currentIndex < pointsList.Count)
		{
	//		currentDistance = Vector3.Distance(transform.position, pointsList[currentIndex]);

			//FIGURE OUT DIRECTION OF THE SPRITE
			/*
			Vector3 dir = transform.position - pointsList[currentIndex];
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle+180);*/

			transform.position = Vector3.MoveTowards(transform.position, pointsList[currentIndex], Time.deltaTime * constantVelocity);

			//Debug.Log("currentDistance" + currentDistance +" "+ "velocity" + (0.5f*currentDistance));
		}
		//	print(pointsList[0]);
		if ( currentIndex < pointsList.Count && currentIndex < MaxIndex && transform.position == pointsList[currentIndex] )
		{
			currentIndex += 1;
		}

		if (currentIndex != 0 && currentIndex == pointsList.Count)
		{
			Moving = false;
			Game.PathComplete = true;
		}
	}

	void CheckSheeps()
	{

	}

	public void Bark()
	{
		if (!sfx.isPlaying)
		{
			sfx.clip = (AudioClip)Resources.Load ("SFX/dog/dog" + Random.Range (2, 7));
			sfx.pitch = Random.Range (1.25f, 1.5f);
			sfx.Play ();

			GetComponent<ParticleSystem>().Play ();
		}

		//print ("Barked with "+sfx.clip+" and at the "+sfx.pitch+" pitch!");
	}

// Click on a place TO MOVE
	void Clicking()
	{
		if ( Input.GetMouseButton (0) )
		{
			Vector3 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			position.z = 0;

			transform.position = Vector3.MoveTowards(transform.position, position, moveSpeed * Time.deltaTime);
		}
	}
	
// USE KEYBOARD TO MOVE
	void Keyboard()
	{
		float moveHorizontal = Input.GetAxis ("Horizontal");
		float moveVertical = Input.GetAxis ("Vertical");
		
		Vector3 movement = new Vector3 (moveHorizontal * Time.deltaTime * moveSpeed, moveVertical * Time.deltaTime * moveSpeed, 0.0f);
		
		transform.position += movement;
	}



///////////////////////////////////// GUI TEST STUFF ///////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////////////////////

	void OnGUI()
	{
		// Make a background box
		GUI.Box (new Rect (10,10,150,100), "Control Type");

		if(controlMode == 1)
		{
			controlMsg = "Drawing";

			GUI.Label (new Rect (35,70,130,20), "Max Drawing: " + Mathf.Round(MaxDistance));
			MaxDistance = GUI.HorizontalSlider(new Rect(35, 90, 100, 30), MaxDistance, 0, 100);
		}
		else if(controlMode == 2)
		{
			controlMsg = "Clicking";

			GUI.Label (new Rect (60,70,120,20), "Speed: " + Mathf.Round(moveSpeed));
			moveSpeed = GUI.HorizontalSlider(new Rect(35, 90, 100, 30), moveSpeed, 1, 10);
		}
		else if(controlMode == 3)
		{
			controlMsg = "Keyboard";
			
			GUI.Label (new Rect (25,50,130,20), "Use WASD to move");
			GUI.Label (new Rect (60,70,120,20), "Speed: " + Mathf.Round(moveSpeed));
			moveSpeed = GUI.HorizontalSlider(new Rect(35, 90, 100, 30), moveSpeed, 1, 10);

		}

		if (GUI.Button (new Rect (35,30,100,20), controlMsg))
		{
			if(controlMode == 3)
			{
				controlMode = 1;
			}
			else
			{
				controlMode++;
			}

		}
	}
}
