using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public int levelBuildIndex; // Set this in the Inspector
    public GameObject star1;    // Drag the UI objects in the Inspector
    public GameObject star2;
    public GameObject star3;

    void Start()
    {
        string key = "Level_" + levelBuildIndex + "_Stars";
        int starsEarned = PlayerPrefs.GetInt(key, 0);

        if (star1 != null) star1.SetActive(starsEarned >= 1);
        if (star2 != null) star2.SetActive(starsEarned >= 2);
        if (star3 != null) star3.SetActive(starsEarned >= 3);
    }

    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Load by name (e.g., "Level1")
    }

    public void LoadSceneByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex); // Load by index (e.g., 0)
    }
}
