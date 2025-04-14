using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelID : MonoBehaviour
{
    private Text levelText;

    // Start is called before the first frame update
    void Awake()
    {
        levelText = GetComponent<Text>();
        levelText.text = "Level " + SceneManager.GetActiveScene().name;
    }

}
