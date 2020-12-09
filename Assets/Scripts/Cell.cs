using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapTileGridCreator.Core;

/*
 * Linked to tile objects in Unity
 */
namespace MyUtility
{
    public class Cell : MonoBehaviour
    {
        //The x and y co-ordinate of the tile
        public int tileX;
        public int tileY;

        public GameObject unitOnTile;  // The unit on the tile
                                       //public tileMapScript map;
    }
}

