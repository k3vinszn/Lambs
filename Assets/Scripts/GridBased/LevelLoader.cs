using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName); // Load by name (e.g., "Level1")
    }

    public void LoadSceneByIndex(int buildIndex)
    {
        SceneManager.LoadScene(buildIndex); // Load by index (e.g., 0)
    }
}