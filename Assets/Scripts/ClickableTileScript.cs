using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Linked to the tile 3D objects in Unity
 */
public class ClickableTileScript : MonoBehaviour    
{
    //The x and y co-ordinate of the tile
    public int tileX;
    public int tileY;
    
    public GameObject unitOnTile;  // The unit on the tile
    public tileMapScript map;
}
