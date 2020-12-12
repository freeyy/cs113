using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MapTileGridCreator.Core;

public class OccupyUI : MonoBehaviour
{
    public GameObject occupyUI;
    public gameManagerScript GMS;
    public tileMapScript map;

    public TMP_Text holdtime;
    // Start is called before the first frame update
    void Start()
    {
        setHoldTimeText();
    }

    // Update is called once per frame
    void Update()
    {
        setHoldTimeText();
    }

    public void setHoldTimeText()
    {
        int t = GMS.building.GetComponent<Cell>().holdtime;
        if (t > 1)
        {
            holdtime.SetText(("It takes " + t + " turns").ToString());
        }
        else
        {
            holdtime.SetText(("It takes " + t + " turn").ToString());
        }
    }

    public void OnClickNoButton()
    {
        occupyUI.SetActive(false);
    }

    public void OnClickYesButton()
    {
        map.disableHighlightUnitRange();
        GMS.occupier.GetComponent<UnitScript>().setWaitIdleAnimation();
        GMS.occupier.GetComponent<UnitScript>().setMovementState(3);
        map.deselectUnit();

        GMS.building.GetComponent<Cell>().holdtime -= 1;
        if (GMS.building.GetComponent<Cell>().holdtime == 0)
        {
            GMS.building.GetComponent<Cell>().owner = GMS.occupier.GetComponent<UnitScript>().teamNum + 1;
            GMS.building.GetComponent<Cell>().holdtime = GMS.building.GetComponent<Cell>().Maxholdtime;
            if (GMS.building.GetComponent<Cell>().isBase == true)
            {
                GMS.displayWinnerUI.enabled = true;
                if (GMS.building.GetComponent<Cell>().owner == 1)
                {
                    GMS.displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Player 1 has won!");
                }
                else
                {
                    GMS.displayWinnerUI.GetComponentInChildren<TextMeshProUGUI>().SetText("Player 2 has won!");
                }
            }
        }
        GMS.building.GetComponent<Cell>().hasHolded = true;
        occupyUI.SetActive(false);
    }
}
