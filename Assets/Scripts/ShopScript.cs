using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MapTileGridCreator.Core;

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
    public TMP_Text unit1_name;
    public TMP_Text hp1_text;
    public TMP_Text attack1_text;
    public TMP_Text range1_text;
    public TMP_Text move1_text;
    public TMP_Text unit1cost_text;
    public GameObject Unit1;
    public GameObject Unit1_2;
    public int unit1cost;
    [Header("Unit2")]
    public TMP_Text unit2_name;
    public TMP_Text hp2_text;
    public TMP_Text attack2_text;
    public TMP_Text range2_text;
    public TMP_Text move2_text;
    public TMP_Text unit2cost_text;
    public GameObject Unit2;
    public GameObject Unit2_2;
    public int unit2cost;
    [Header("Unit3")]
    public TMP_Text unit3_name;
    public TMP_Text hp3_text;
    public TMP_Text attack3_text;
    public TMP_Text range3_text;
    public TMP_Text move3_text;
    public TMP_Text unit3cost_text;
    public GameObject Unit3;
    public GameObject Unit3_2;
    public int unit3cost;

    // Start is called before the first frame update
    void Start()
    {
        units = GameObject.FindGameObjectsWithTag("Unit");
        setUnitInfo();

    }

    // Update is called once per frame
    void Update()
    {
    }

    public void setUnitInfo()
    {

        setEachUnitInfo(unit1_name, hp1_text, attack1_text, range1_text, move1_text, unit1cost_text, Unit1, unit1cost);
        setEachUnitInfo(unit2_name, hp2_text, attack2_text, range2_text, move2_text, unit2cost_text, Unit2, unit2cost);
        setEachUnitInfo(unit3_name, hp3_text, attack3_text, range3_text, move3_text, unit3cost_text, Unit3, unit3cost);

    }

    public void setEachUnitInfo(TMP_Text name, TMP_Text hp, TMP_Text attack, TMP_Text range, TMP_Text move, TMP_Text cost,
    GameObject unit, int unitcost)
    {
        name.SetText(unit.GetComponent<UnitScript>().unitName.ToString());
        hp.SetText(unit.GetComponent<UnitScript>().maxHealthPoints.ToString());
        attack.SetText(unit.GetComponent<UnitScript>().attackDamage.ToString());
        range.SetText(unit.GetComponent<UnitScript>().attackRange.ToString());
        move.SetText(unit.GetComponent<UnitScript>().moveSpeed.ToString());
        cost.SetText(unitcost.ToString());
    }

    public void OnClickCloseButton()
    {
        shopUI.SetActive(false);
    }


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

    public void OccupyBuilding()
    {
        

    }

    public void OnClickBuyButton1()
    {
        SpawnUnit(GMS.Gold1, unit1cost, GMS.Gold2, unit1cost, Unit1, Unit1, GMS.currentGoldUI);
    }

    public void OnClickBuyButton2()
    {
        SpawnUnit(GMS.Gold1, unit1cost, GMS.Gold2, unit1cost, Unit2, Unit2, GMS.currentGoldUI);
    }

    public void OnClickBuyButton3()
    {
        SpawnUnit(GMS.Gold1, unit1cost, GMS.Gold2, unit1cost, Unit3, Unit3, GMS.currentGoldUI);
    }

    public void SpawnUnit(int gold1, int cost1, int gold2, int cost2, GameObject unit1, GameObject unit2, TMP_Text goldUI)
    {
        posX = GMS.building.GetComponent<Cell>().spawnX;
        posY = GMS.building.GetComponent<Cell>().spawnY;
        units = GameObject.FindGameObjectsWithTag("Unit");
        unitOnSpawnPoint = SpawnPointOccupied(posX, posY);
        if (!unitOnSpawnPoint)
        {
            if (GMS.currentTeam == 0)
            {
                if (gold1 >= cost1)
                {
                    gold1 -= cost1;
                    goldUI.SetText("GOLD: " + gold1.ToString());
                    GameObject newUnit = Instantiate(unit1, new Vector3(posX, 0.75f, posY), Quaternion.identity);
                    newUnit.GetComponent<UnitScript>().tileBeingOccupied = map.tiles[posX, posY].tileOnMap;
                    newUnit.transform.parent = Team1.transform;
                    newUnit.GetComponent<UnitScript>().changeHealthBarColour(0);
                    newUnit.GetComponent<UnitScript>().setMovementState(3);
                    newUnit.GetComponent<UnitScript>().holder2D.GetComponent<SpriteRenderer>().color = Color.gray;
                    map.tiles[posX, posY].tileOnMap.GetComponent<Cell>().unitOnTile = newUnit;
                }
            }
            else if (GMS.currentTeam == 1)
            {
                if (gold2 >= cost2)
                {
                    gold2 -= cost2;
                    goldUI.SetText("GOLD: " + gold2.ToString());
                    GameObject newUnit = Instantiate(unit2, new Vector3(posX, 0.75f, posY), Quaternion.identity);
                    newUnit.GetComponent<UnitScript>().teamNum = 1;
                    newUnit.GetComponent<UnitScript>().tileBeingOccupied = map.tiles[posX, posY].tileOnMap;
                    newUnit.transform.parent = Team2.transform;
                    newUnit.GetComponent<UnitScript>().changeHealthBarColour(0);
                    newUnit.GetComponent<UnitScript>().holder2D.GetComponent<SpriteRenderer>().flipX = true;
                    newUnit.GetComponent<UnitScript>().setMovementState(3);
                    newUnit.GetComponent<UnitScript>().holder2D.GetComponent<SpriteRenderer>().color = Color.gray;
                    map.tiles[posX, posY].tileOnMap.GetComponent<Cell>().unitOnTile = newUnit;
                }
            }
        }
    }




}
