using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public int levelBuildIndex; // Set this in the Inspector
    public GameObject star1;    // Drag the UI objects in the Inspector
    public GameObject star2;
    public GameObject star3;

    private Button levelButton; // The button component

    void Start()
    {
        levelButton = GetComponent<Button>(); // Get the button on this object

        // Show stars based on saved data
        string key = "Level_" + levelBuildIndex + "_Stars";
        int starsEarned = PlayerPrefs.GetInt(key, 0);

        if (star1 != null) star1.SetActive(starsEarned >= 1);
        if (star2 != null) star2.SetActive(starsEarned >= 2);
        if (star3 != null) star3.SetActive(starsEarned >= 3);

        // ✅ Lock logic: disable button if previous level has 0 stars
        if (levelBuildIndex > 1) // Level 1 is always unlocked
        {
            string prevKey = "Level_" + (levelBuildIndex - 1) + "_Stars";
            int prevStars = PlayerPrefs.GetInt(prevKey, 0);

            if (prevStars == 0 && levelButton != null)
            {
                levelButton.interactable = false;
            }
        }
    }

    public void LoadSceneByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex); // Load by index (e.g., 0)
    }
}
