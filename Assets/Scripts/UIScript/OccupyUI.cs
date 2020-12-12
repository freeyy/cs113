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

    // set the text that how much time does the unit require to occupy the building
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

    // function for close button
    public void OnClickNoButton()
    {
        occupyUI.SetActive(false);
    }

    // function for yes button
    public void OnClickYesButton()
    {
        // disable unit (standig on the spawn point) 's ability to attack
        map.disableHighlightUnitRange();
        GMS.occupier.GetComponent<UnitScript>().setWaitIdleAnimation();
        GMS.occupier.GetComponent<UnitScript>().setMovementState(3);
        map.deselectUnit();

        GMS.building.GetComponent<Cell>().holdtime -= 1;
        // if time need to occupy the building is 0
        if (GMS.building.GetComponent<Cell>().holdtime == 0)
        {
            // set tile (under the building) 's owner to the occupier
            GMS.building.GetComponent<Cell>().owner = GMS.occupier.GetComponent<UnitScript>().teamNum + 1;
            // set time need to occupy the building to it's max time
            GMS.building.GetComponent<Cell>().holdtime = GMS.building.GetComponent<Cell>().Maxholdtime;
            // is the building is base
            if (GMS.building.GetComponent<Cell>().isBase == true)
            {
                // show the winner window
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
        // ensure that when player click on the building in the turn again, occupy confirm window will not be shown.
        GMS.building.GetComponent<Cell>().hasHolded = true;
        // close the occupy confirm window
        occupyUI.SetActive(false);
    }
}
