using System.Collections;
using System.Collections.Generic;
using MapTileGridCreator.Core;
using UnityEngine;

/*
 * Contains all the info for a tile.
 */
[System.Serializable]
public class Tile
{
    public float movementCost = 1;
    public bool isWalkable = true;
    public GameObject tileOnMap;

    public Tile(Cell cell)
    {
        movementCost = cell.moveCost;
        isWalkable = cell.isWalkable;
        tileOnMap = cell.gameObject;
    }
}
