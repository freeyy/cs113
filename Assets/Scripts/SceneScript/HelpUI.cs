using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpUI : MonoBehaviour
{
    public GameObject helpUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnclickhelpIcon()
    {
        helpUI.SetActive(true);
    }

    public void Onclickclose()
    {
        helpUI.SetActive(false);
    }

}
