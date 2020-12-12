using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BacktoMenu : MonoBehaviour
{
    public GameObject backUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void onClickBackUI()
    {
        backUI.SetActive(true);
    }
    public void onClickYes()
    {
        SceneManager.LoadScene(0);
    }

    public void onClickNo()
    {
        backUI.SetActive(false);
    }
}
