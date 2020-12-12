using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    private bool show;
    public GameObject levels;

    // Start is called before the first frame update
    void Start()
    {
        show = false;
        levels = GameObject.Find("Levels");
        levels.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickNewGame()
    // direct to level 1
    {
        SceneManager.LoadScene(1);
    }

    public void OnClickLoadGame()
    // show level select button
    {
        show = !show;
        levels.SetActive(show);
        Debug.Log(show);
    }

    public void OnClickLvlOne()
    // direct to level 1
    {
        SceneManager.LoadScene(1);
    }


    public void OnClickQuit()
    // quit the game
    {
        Application.Quit();
    }
}
