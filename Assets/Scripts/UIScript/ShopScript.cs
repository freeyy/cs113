using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MapTileGridCreator.Core;
using UnityEngine.UI;

public class ShopScript : MonoBehaviour
{
    public GameObject shopUI;
    public gameManagerScript GMS;
    public tileMapScript map;

    public GameObject Team1;
    public GameObject Team2;

    public GameObject[] units;
    public bool unitOnSpawnPoint;

    private int posX;
    private int posY;
    [Header("Unit1")]
    public GameObject Board1;
    public GameObject Unit1;
    public GameObject Unit1_2;
    public int unit1cost;
    [Header("Unit2")]
    public GameObject Board2;
    public GameObject Unit2;
    public GameObject Unit2_2;
    public int unit2cost;
    [Header("Unit3")]
    public GameObject Board3;
    public GameObject Unit3;
    public GameObject Unit3_2;
    public int unit3cost;
    [Header("Unit4")]
    public GameObject Board4;
    public GameObject Unit4;
    public GameObject Unit4_2;
    public int unit4cost;


    // Start is called before the first frame update
    void Start()
    {
        units = GameObject.FindGameObjectsWithTag("Unit");

    }

    // Update is called once per frame
    void Update()
    {
        setUnitInfo();
    }

    // set information for four unit
    public void setUnitInfo()
    {
        if (GMS.currentTeam == 0)
        {
            setEachUnitInfo(Board1, Unit1, unit1cost);
            setEachUnitInfo(Board2, Unit2, unit2cost);
            setEachUnitInfo(Board3, Unit3, unit3cost);
            setEachUnitInfo(Board4, Unit4, unit4cost);
        }
        else
        {
            setEachUnitInfo(Board1, Unit1_2, unit1cost);
            setEachUnitInfo(Board2, Unit2_2, unit2cost);
            setEachUnitInfo(Board3, Unit3_2, unit3cost);
            setEachUnitInfo(Board4, Unit4_2, unit4cost);
        }

    }

    //In: board of each unit (ex unitinfo), unit, cost of unit
    //Out: void
    //Desc: set information for each unit
    // note: the image of the board is taken from unit's 2dModel's sprite
    public void setEachUnitInfo(GameObject Board, GameObject unit, int unitcost)
    {

        Board.transform.GetChild(0).GetComponent<Image>().sprite = unit.transform.GetChild(1).GetComponent<SpriteRenderer>().sprite;
        Board.transform.GetChild(1).GetComponentsInChildren<TMP_Text>()[0].SetText(unit.GetComponent<UnitScript>().unitName.ToString());
        Board.transform.GetChild(2).GetComponentsInChildren<TMP_Text>()[0].SetText(unit.GetComponent<UnitScript>().maxHealthPoints.ToString());
        Board.transform.GetChild(3).GetComponentsInChildren<TMP_Text>()[0].SetText(unit.GetComponent<UnitScript>().attackDamage.ToString());
        Board.transform.GetChild(4).GetComponentsInChildren<TMP_Text>()[0].SetText(unit.GetComponent<UnitScript>().attackRange.ToString());
        Board.transform.GetChild(5).GetComponentsInChildren<TMP_Text>()[0].SetText(unit.GetComponent<UnitScript>().moveSpeed.ToString());
        Board.transform.GetChild(6).GetComponentsInChildren<TMP_Text>()[0].SetText(unitcost.ToString());
    }

    // function for close button
    public void OnClickCloseButton()
    {
        shopUI.SetActive(false);
    }

    // find whether there is a unit standing on the spawn point
    public bool SpawnPointOccupied(int x, int y)
    {
        foreach (var u in units)
        {
            if (u.GetComponent<UnitScript>().x == x && u.GetComponent<UnitScript>().y == y)
            {
                return true;
            }
        }
        return false;
    }

    // when click on recurit buton on the first board, a unit is spawned on the spawn point
    public void OnClickBuyButton1()
    {
        SpawnUnit(GMS.Gold1, GMS.Gold2, unit1cost, Unit1, Unit1_2, GMS.currentGoldUI);
    }

    public void OnClickBuyButton2()
    {
        SpawnUnit(GMS.Gold1, GMS.Gold2, unit2cost, Unit2, Unit2_2, GMS.currentGoldUI);
    }

    public void OnClickBuyButton3()
    {
        SpawnUnit(GMS.Gold1, GMS.Gold2, unit3cost, Unit3, Unit3_2, GMS.currentGoldUI);
    }

    public void OnClickBuyButton4()
    {
        SpawnUnit(GMS.Gold1, GMS.Gold2, unit4cost, Unit4, Unit4_2, GMS.currentGoldUI);
    }

    //In: gold of two players, cost of the unit, player 1's unit, player2's unit, gold text window
    //Out: void
    //Desc: spawn a unit on the spawn point
    public void SpawnUnit(int gold1, int gold2, int cost, GameObject unit1, GameObject unit2, TMP_Text goldUI)
    {
        posX = GMS.building.GetComponent<Cell>().spawnX;
        posY = GMS.building.GetComponent<Cell>().spawnY;
        units = GameObject.FindGameObjectsWithTag("Unit");
        unitOnSpawnPoint = SpawnPointOccupied(posX, posY);
        // if there is no unit on spawn point
        if (!unitOnSpawnPoint)
        {
            // current player is 1
            if (GMS.currentTeam == 0)
            {
                if (gold1 >= cost)
                {
                    GMS.Gold1 -= cost;
                    goldUI.SetText("GOLD: " + GMS.Gold1.ToString());
                    GameObject newUnit = Instantiate(unit1, new Vector3(posX, 0.75f, posY), Quaternion.identity);
                    // set unit's tileBeing Occupied to the tile it stands on
                    newUnit.GetComponent<UnitScript>().tileBeingOccupied = map.tiles[posX, posY].tileOnMap;
                    // add the unit to the folder of Team1
                    newUnit.transform.parent = Team1.transform;
                    // change unit hp color to blue
                    newUnit.GetComponent<UnitScript>().changeHealthBarColour(0);
                    // disable unit's action
                    newUnit.GetComponent<UnitScript>().setMovementState(3);
                    // change unit's color to gray
                    newUnit.GetComponent<UnitScript>().holder2D.GetComponent<SpriteRenderer>().color = Color.gray;
                    // bind the tile unit standing on in the tilemap to the unit
                    map.tiles[posX, posY].tileOnMap.GetComponent<Cell>().unitOnTile = newUnit;
                }
            }
            // current player is 2
            else if (GMS.currentTeam == 1)
            {
                if (gold2 >= cost)
                {
                    GMS.Gold2 -= cost;
                    goldUI.SetText("GOLD: " + GMS.Gold2.ToString());
                    GameObject newUnit = Instantiate(unit2, new Vector3(posX, 0.75f, posY), Quaternion.identity);
                    // change unit's teamNum
                    newUnit.GetComponent<UnitScript>().teamNum = 1;
                    newUnit.GetComponent<UnitScript>().tileBeingOccupied = map.tiles[posX, posY].tileOnMap;
                    newUnit.transform.parent = Team2.transform;
                    newUnit.GetComponent<UnitScript>().changeHealthBarColour(0);
                    // flip the unit
                    newUnit.GetComponent<UnitScript>().holder2D.GetComponent<SpriteRenderer>().flipX = true;
                    newUnit.GetComponent<UnitScript>().setMovementState(3);
                    newUnit.GetComponent<UnitScript>().holder2D.GetComponent<SpriteRenderer>().color = Color.gray;
                    map.tiles[posX, posY].tileOnMap.GetComponent<Cell>().unitOnTile = newUnit;
                }
            }
        }
    }




}
