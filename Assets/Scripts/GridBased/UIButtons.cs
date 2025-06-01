using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
//using UnityEngine.UIElements;

	public class UIButtons : MonoBehaviour {


    // Reference to GridManager
    private GridManager gridManager;

    void Start()
    {
        // Find the GridManager in the scene
        gridManager = FindObjectOfType<GridManager>();
    }


    public void ExecutePath()
    {
        if (gridManager != null)
        {
        }
        else
        {
            Debug.LogError("GridManager not found in scene!");
        }
    }

    public void Restart()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void NextLevel()
	{
		if (SceneManager.GetActiveScene().buildIndex < SceneManager.sceneCountInBuildSettings)
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
