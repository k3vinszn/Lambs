using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using UnityEngine.UIElements;

	public class Game : MonoBehaviour {

	public GameObject FinalScorePanel;
	public GameObject PausedPanel;
	public GameObject PauseButton;
	public GameObject NextButton;
    public GameObject PrevButton;

    public static bool Pause = false;
	public static bool ActiveLogic = true;
	private bool LevelEnded = false;

	public static float Score = 0;
	public static float MaxGoal = 0;
	public static int DestroyedSheeps = 0;
	public static bool PathComplete = false;
	public static int MovingSheeps = 0;
    public static float AnimalSpeed = 4;

    private float ScoreDEBUG = 0;
	private int DestroyedSheepsDEBUG = 0;
	private float MaxGoalDEBUG = 0;
	private bool PathCompleteDEBUG = false;
	private int MovingSheepsDEBUG = 0;

    private List<GameObject> SheepsLIST;

    public Toggle OneStar;
	public Toggle TwoStar;
	public Toggle ThreeStar;

	public float OneStarScore = 1;
	public float TwoStarScore = 2;
	public float ThreeStarScore = 3;

	public Text ScoreFinal;
	public Text LevelFailed;

	private GameObject[] StartingSheep;
    public static List<GameObject> Sheeps = new List<GameObject>();

	// Use this for initialization
	void Awake()
	{

        // Ensure game is unpaused when level loads
        Game.Pause = false;
        Time.timeScale = 1;


        StartingSheep = GameObject.FindGameObjectsWithTag("Sheep");
        UpdateSheepList();

        //CALCULATE NEEDED SCORE TO WIN
        Score = 0;
		MaxGoal = 0;

		Game.Pause = false;
		Game.ActiveLogic = true;
		DestroyedSheeps = 0;
		Game.MovingSheeps = 0;
		Game.PathComplete = false;

        Score = 0;
        MaxGoal = 0;
        Game.ActiveLogic = true;
        DestroyedSheeps = 0;
        Game.MovingSheeps = 0;
        Game.PathComplete = false;

        foreach (GameObject S in Game.Sheeps)
        {
            MaxGoal++;
        }
    }

	public void UpdateSheepList()
	{
		Game.Sheeps.Clear();

        foreach (GameObject sheep in StartingSheep)
        {
            Game.Sheeps.Add(sheep);
        }
    }

	// Update is called once per frame
	void Update ()
	{
		//assign our static vars to these ones so that we can see them in Inspector in DEBUG
		ScoreDEBUG = Score;
		DestroyedSheepsDEBUG = DestroyedSheeps;
		MaxGoalDEBUG = MaxGoal;
		PathCompleteDEBUG = PathComplete;
		MovingSheepsDEBUG = MovingSheeps;

        SheepsLIST = Sheeps;



        //check and confirm that the game was Paused
        if (PausedPanel.activeSelf)
		{
			Game.ActiveLogic = false;
		}
			
		if (MovingSheeps < 1 && !LevelEnded && PathComplete)
		{
			StartCoroutine("ActivateFinalScore");
		}
	}

    public IEnumerator ActivateFinalScore()
	{
        //Debug.Log("Game is complete");

        yield return new WaitForSeconds(0.25f);

        if (MovingSheeps < 1 && !LevelEnded && PathComplete)
		{
            LevelEnded = true;
            Game.ActiveLogic = false;

            //CALCULATE FINAL SCORE BASED ON SHEEP IN GOALS
            foreach (GameObject sheep in Sheeps)
            {
                if (sheep != null && sheep.GetComponent<Sheepy>().ReachedGoal)
                {
					Score += 1;
                }
            }

            StartCoroutine(StarCheck());
            LaunchScorePanel();

            //Show and update the score UI 
            ScoreFinal.text = Score.ToString() + " of " + MaxGoal;
            Game.Pause = true;
            FinalScorePanel.SetActive(true);

            //Debug.Log("current build level is " + SceneManager.GetActiveScene().buildIndex + " and total scene count is " + SceneManager.sceneCountInBuildSettings);

            if (SceneManager.GetActiveScene().buildIndex + 1 == SceneManager.sceneCountInBuildSettings)
            {
                
                NextButton.SetActive(false);
            }

            //return;
        }
		else
		{
			yield break;
		}

    }

	void LaunchScorePanel()
	{
		if (Score < OneStarScore)
		{
			FinalScorePanel.GetComponent<AudioSource> ().clip = (AudioClip)Resources.Load ("SFX/LevelFailed");
			//Debug.Log ("Level Failed!");
			LevelFailed.gameObject.SetActive (true);
			OneStar.gameObject.SetActive (false);
			TwoStar.gameObject.SetActive (false);
			ThreeStar.gameObject.SetActive (false);
			NextButton.SetActive(false);
		}
		else
		{
            FinalScorePanel.GetComponent<AudioSource> ().clip = (AudioClip)Resources.Load ("SFX/LevelComplete");
			//Debug.Log ("Level Completed!");
			LevelFailed.gameObject.SetActive (false);
			OneStar.gameObject.SetActive (true);
			TwoStar.gameObject.SetActive (true);
			ThreeStar.gameObject.SetActive (true);
		}

		if (SceneManager.GetActiveScene().buildIndex == 0)
		{
            PrevButton.SetActive(false);
        }
    }

	IEnumerator StarCheck ()
	{
		yield return new WaitForSeconds (0.5f);
		if (Score >= OneStarScore)
		{
			OneStar.isOn = true;

			yield return new WaitForSeconds (0.5f);
			if (Score >= TwoStarScore)
			{
				TwoStar.isOn = true;

				yield return new WaitForSeconds (0.5f);
				if (Score >= ThreeStarScore)
				{
					ThreeStar.isOn = true;
				}
			}
		}

	}

    public void PauseGame()
    {
        Game.Pause = true; // Add this line
        Game.ActiveLogic = !Game.Pause;
        Time.timeScale = Game.Pause ? 0 : 1; // Freeze/unfreeze game
        PausedPanel.SetActive(true);
        PauseButton.SetActive(false);
    }

    public void UnpauseGame()
    {
        Game.Pause = false; // Add this line
        Game.ActiveLogic = !Game.Pause;
        Time.timeScale = Game.Pause ? 0 : 1; // Freeze/unfreeze game
        PausedPanel.SetActive(false);
        PauseButton.SetActive(true);
    }

    public void Restart()
    {
        // Unpause the game before reloading
        Game.Pause = false;
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel()
	{
		if (SceneManager.GetActiveScene().buildIndex <= SceneManager.sceneCountInBuildSettings)
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
		else
		{
			Debug.LogError ("No more levels in build. Returning to first level...");
			SceneManager.LoadScene(0);
		}
	}

    public void PreviousLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex > 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
        else
        {
            Debug.LogError("No more levels in build. Returning to first level...");
            SceneManager.LoadScene(0);
        }
    }

    public void Quit()
	{
		Application.Quit ();
	}

    public void RestartFromBeginning()
    {
        SceneManager.LoadScene(0);
    }

}
