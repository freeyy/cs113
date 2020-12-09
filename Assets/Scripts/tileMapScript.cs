using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MapTileGridCreator.Core;

/*
 * Script for gameManager:
 * 1. Do Path Finding
 * 2. Handle player input
 * 3. Manage GameObjects: 
 *  - Tiles Generation
 *  - MapUI Display
 *  - Selected Unit
 */


public class tileMapScript : MonoBehaviour    
{
    [Header("Map Size")]
    public int mapSizeX;
    public int mapSizeY;

    //Reference holders for the other two scripts that are currently running
    [Header("Manager Scripts")]
    public battleManagerScript BMS;
    public gameManagerScript GMS;

    //List of tile data structure  (corresponding to Tile game objects)
    [Header("Tiles(config)")]
    public Tile[,] tiles;

    // appears when a unit is selected
    // layer: mapCursorUI -> mapPathUI -> mapRangeUI -> tile
    public GameObject mapRangeUI;               // highlight move range
    public GameObject mapCursorUI;              // highlight the mouse location
    public GameObject mapPathUI;                // highlight the path (route arrow)

    //quad objects contained by mapUIs
    public GameObject[,] quadOnMapRange;
    public GameObject[,] quadOnMapCursor;
    public GameObject[,] quadOnMapPath;

    //Nodes along the path of shortest path from the pathfinding
    public List<Node> currentPath = null;

    //Node graph for pathfinding purposes
    public Node[,] graph;

    // This is used to map pre-set units on to the board
    // Called when game starts
    [Header("Units on the board")]
    public GameObject unitsOnBoard;

    //containers (parent gameObjects) for UI objects
    [Header("Containers")]
    public GameObject tileContainer;                    // contains all the tile objects
    public GameObject UIMapRangeContainer;              // contains all the mapRangeUI objects
    public GameObject UIMapCursorContainer;             // contains all the map
    public GameObject UIMapPathContainer;               // the route arrow
    

    //In the update() function mouse down raycast sets this unit
    [Header("Selected Unit Info")]
    public GameObject selectedUnit;
    //These two are set in the highlightUnitRange() function
    //They are used for other things as well, mainly to check for movement, or finalize function
    public HashSet<Node> selectedUnitTotalRange;
    public HashSet<Node> selectedUnitMoveRange;

    public bool unitSelected = false;

    public int unitSelectedPreviousX;
    public int unitSelectedPreviousY;

    public GameObject previousOccupiedTile;


    //public AudioSource selectedSound;
    //public AudioSource unselectedSound;
    // Quad materials
    [Header("Materials")]
    public Material greenUIMat;
    public Material redUIMat;
    public Material blueUIMat;


    public float rightClickTime = -1;

    private void Awake()
    {
        graph = new Node[mapSizeX, mapSizeY];
        tiles = new Tile[mapSizeX, mapSizeY];
        quadOnMapRange = new GameObject[mapSizeX, mapSizeY];
        quadOnMapPath = new GameObject[mapSizeX, mapSizeY];
        quadOnMapCursor = new GameObject[mapSizeX, mapSizeY];
    }


    private void Start()
    {
        //With the generated info this function will read the info and produce the map
        createTileObjectsFromPrefab();
        createGridUIObjects();
        //Generate pathfinding graph
        generatePathFindingGraph();
        //Put pre-set units on board
        bindTileWithUnits();
    }

    private void Update()
    {
        UnitScript selectedUnitScript = selectedUnit ? selectedUnit.GetComponent<UnitScript>() : null;

        //If input is left mouse down then select the unit
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedUnitScript == null)
            {
                //mouseClickToSelectUnit();
                mouseClickToSelectUnitV2();
            }
            //After a unit has been selected then if we get a mouse click, we need to check if the unit has entered the selection state (1) 'Selected'
            //Move the unit
            else if (selectedUnitScript.isSelected() && selectedUnitScript.movementQueue.Count == 0)
            {
                if (selectTileToMoveTo())
                {
                    //selectedSound.Play();
                    Debug.Log("movement path has been located");
                    unitSelectedPreviousX = selectedUnitScript.x;
                    unitSelectedPreviousY = selectedUnitScript.y;
                    previousOccupiedTile = selectedUnitScript.tileBeingOccupied;
                    selectedUnitScript.setWalkingAnimation();
                    moveUnit();
                    
                    StartCoroutine(moveUnitAndFinalize());
                    //The moveUnit function calls a function on the unitScriptm when the movement is completed the finalization is called from that script.
                }

            }
            //Finalize the movement
            else if(selectedUnitScript.isMoved())
            {
                finalizeOption();
            }
        }

        // Undo move operation of a unit with right-click
        // Note that we need to seperate this operation with holding-right-mouse-button
        if (Input.GetMouseButtonDown(1))
        {
            rightClickTime = Time.time;
        }

        if (Input.GetMouseButtonUp(1)) { 
            if (selectedUnitScript != null && Time.time - rightClickTime < .2)
            {
                // if unit not in combat or movement, undo the last movement
                if (selectedUnitScript.movementQueue.Count == 0 && selectedUnitScript.combatQueue.Count == 0 
                    && !selectedUnitScript.isWaiting())
                { 
                    //unselectedSound.Play();
                    selectedUnitScript.setIdleAnimation();
                    deselectUnit();
                }
                // if unit is in movement, right-click will speed up the process
                else if (selectedUnitScript.movementQueue.Count == 1)
                {
                    selectedUnitScript.visualMovementSpeed = 0.5f;
                }
            }
        }
       
        
    }
    

    //Creates the graph for the pathfinding, it sets up the neighbours
    //This is also from Quill18Create's tutorial
    public void generatePathFindingGraph()
    {
        //initialize graph 
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                graph[x, y] = new Node();
                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }
        //calculate neighbours
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {              
                //X is not 0, then we can add left (x - 1)
                if (x > 0)
                {                   
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                }
                //X is not mapSizeX - 1, then we can add right (x + 1)
                if (x < mapSizeX-1)
                {                   
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                }
                //Y is not 0, then we can add downwards (y - 1 ) 
                if (y > 0)
                {
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                }
                //Y is not mapSizeY -1, then we can add upwards (y + 1)
                if (y < mapSizeY - 1)
                {
                    graph[x, y].neighbours.Add(graph[x, y + 1]);
                }
               
               
            }
        }
    }

    //Desc: This instantiates all the information for the map, the UI Quads and the map tiles, 
    //      from the prefab scene.
    public void createTileObjectsFromPrefab()
    {
        // generate tiles from existing 3D Objects
        Cell[] cs = tileContainer.GetComponentsInChildren<Cell>();
        foreach (Cell cell in cs)
        {
            tiles[cell.tileX, cell.tileY] = new Tile(cell);
        }
    }

    public void createGridUIObjects()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                GameObject gridUI = Instantiate(mapRangeUI, new Vector3(x, 0.501f, y), Quaternion.Euler(90f, 0, 0));
                gridUI.transform.SetParent(UIMapRangeContainer.transform);
                quadOnMapRange[x, y] = gridUI;

                GameObject gridUIForPathfindingDisplay = Instantiate(mapPathUI, new Vector3(x, 0.502f, y), Quaternion.Euler(90f, 0, 0));
                gridUIForPathfindingDisplay.transform.SetParent(UIMapPathContainer.transform);
                quadOnMapPath[x, y] = gridUIForPathfindingDisplay;

                GameObject gridUICursor = Instantiate(mapCursorUI, new Vector3(x, 0.503f, y), Quaternion.Euler(90f, 0, 0));
                gridUICursor.transform.SetParent(UIMapCursorContainer.transform);
                quadOnMapCursor[x, y] = gridUICursor;
            }
        }
    }


    //Moves the unit
    public void moveUnit()
    {
        if (selectedUnit != null)
        {
            selectedUnit.GetComponent<UnitScript>().MoveNextTile();
        }
    }

    //In: the x and y of a tile
    //Out: vector 3 of the tile in world space, theyre .75f off of zero
    //Desc: returns a vector 3 of the tile in world space, theyre .75f off of zero
    public Vector3 tileCoordToWorldCoord(int x, int y)
    {
        return new Vector3(x, 0.75f, y);
    }



    //In: 
    //Out: void
    //Desc: bind tiles with units placed on them (directly from thier axis [x, y]!)
    public void bindTileWithUnits()
    {
        foreach (Transform team in unitsOnBoard.transform)
        {
            foreach (Transform unitOnTeam in team) { 
                int unitX = unitOnTeam.GetComponent<UnitScript>().x;
                int unitY = unitOnTeam.GetComponent<UnitScript>().y;
                unitOnTeam.GetComponent<UnitScript>().tileBeingOccupied = tiles[unitX, unitY].tileOnMap;
                tiles[unitX, unitY].tileOnMap.GetComponent<Cell>().unitOnTile = unitOnTeam.gameObject;
            }
            
        }
    }
    //In: x and y position of the tile to move to
    //Out: void
    //Desc: generates the path for the selected unit
    //Think this one is also partially from Quill18Create's tutorial
    public void generatePathTo(int x, int y)
    {

        if (selectedUnit.GetComponent<UnitScript>().x == x && selectedUnit.GetComponent<UnitScript>().y == y){
            Debug.Log("clicked the same tile that the unit is standing on");
            currentPath = new List<Node>();
            selectedUnit.GetComponent<UnitScript>().path = currentPath;
            
            return;
        }
        if (unitCanEnterTile(x, y) == false)
        {
            //cant move into something so we can probably just return
            //cant set this endpoint as our goal

            return;
        }

        selectedUnit.GetComponent<UnitScript>().path = null;
        currentPath = null;
        //from wiki dijkstra's
        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();
        Node source = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        Node target = graph[x, y];
        dist[source] = 0;
        prev[source] = null;
        //Unchecked nodes
        List<Node> unvisited = new List<Node>();

        //Initialize
        foreach (Node n in graph)
        {

            //Initialize to infite distance as we don't know the answer
            //Also some places are infinity
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }
            unvisited.Add(n);
        }
        //if there is a node in the unvisited list lets check it
        while (unvisited.Count > 0)
        {
            //u will be the unvisited node with the shortest distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }


            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node n in u.neighbours)
            {

                //float alt = dist[u] + u.DistanceTo(n);
                float alt = dist[u] + costToEnterTile(n.x, n.y);
                if (alt < dist[n])
                {
                    dist[n] = alt;
                    prev[n] = u;
                }
            }
        }
        //if were here we found shortest path, or no path exists
        if (prev[target] == null)
        {
            //No route;
            return;
        }
        currentPath = new List<Node>();
        Node curr = target;
        //Step through the current path and add it to the chain
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }
        //Now currPath is from target to our source, we need to reverse it from source to target.
        currentPath.Reverse();
        selectedUnit.GetComponent<UnitScript>().path = currentPath;
    }

    //In: tile's x and y position
    //Out: cost that is requiredd to enter the tile
    //Desc: checks the cost of the tile for a unit to enter
    public float costToEnterTile(int x, int y)
    {
        if (unitCanEnterTile(x, y) == false)
        {
            return Mathf.Infinity;
        }
        return tiles[x, y].movementCost;
    }

    //change this when we add movement types
    //In:  tile's x and y position
    //Out: true or false if the unit can enter the tile that was entered
    //Desc: if the tile is not occupied by another team's unit, then you can walk through and if the tile is walkable 
    public bool unitCanEnterTile(int x, int y)
    {
        if (tiles[x, y].tileOnMap.GetComponent<Cell>().unitOnTile != null)
        {
            if (tiles[x, y].tileOnMap.GetComponent<Cell>().unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
            {
                return false;
            }
        }
        return tiles[x, y].isWalkable;
    }

    
    //In:  
    //Out: void
    //Desc: uses a raycast to see where the mouse is pointing, this is used to select units
    public void mouseClickToSelectUnit()
    {
        GameObject tempSelectedUnit;
        
        RaycastHit hit;       
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        
       
        if (Physics.Raycast(ray, out hit))
        {


            //Debug.Log(hit.transform.tag);
            if (unitSelected == false)
            {
               
                if (hit.transform.gameObject.CompareTag("Tile"))
                {
                    if (hit.transform.GetComponent<Cell>().unitOnTile != null)
                    {


                        tempSelectedUnit = hit.transform.GetComponent<Cell>().unitOnTile;
                        if (tempSelectedUnit.GetComponent<UnitScript>().isUnselected()
                            && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                            )
                        {
                            disableHighlightUnitRange();
                            selectedUnit = tempSelectedUnit;
                            selectedUnit.GetComponent<UnitScript>().map = this;
                            selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                            unitSelected = true;
                            
                            highlightUnitRange();
                        }
                    }
                }

                else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
                {
                    
                    tempSelectedUnit = hit.transform.parent.gameObject;
                    if (tempSelectedUnit.GetComponent<UnitScript>().isUnselected()
                            && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                        )
                    {

                        disableHighlightUnitRange();
                        selectedUnit = tempSelectedUnit;
                        selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                        //These were here before I don't think they do anything the unit location is set beforehand
                        //selectedUnit.GetComponent<UnitScript>().x = (int)selectedUnit.transform.position.x;
                        // selectedUnit.GetComponent<UnitScript>().y = (int)selectedUnit.transform.position.z;
                        selectedUnit.GetComponent<UnitScript>().map = this;
                        unitSelected = true;
                       
                        highlightUnitRange();
                    }
                }
            }

            }
    }



    //In:  
    //Out: void
    //Desc: finalizes the movement, sets the tile the unit moved to as occupied, etc
    public void finalizeMovementPosition()
    {
        int x = selectedUnit.GetComponent<UnitScript>().x;
        int y = selectedUnit.GetComponent<UnitScript>().y;
        tiles[x, y].tileOnMap.GetComponent<Cell>().unitOnTile = selectedUnit;
        //After a unit has been moved we will set the unitMoveState to (2) the 'Moved' state
        selectedUnit.GetComponent<UnitScript>().setMovementState(2);
       
        highlightUnitAttackOptionsFromPosition();
        highlightTileUnitIsOccupying();
    }



    //In:  
    //Out: void
    //Desc: selects a unit based on the cursor from the other script
    public void mouseClickToSelectUnitV2()
    {
        
        if (unitSelected == false && GMS.tileBeingDisplayed!=null)
        {

            if (GMS.tileBeingDisplayed.GetComponent<Cell>().unitOnTile != null)
            {
                GameObject tempSelectedUnit = GMS.tileBeingDisplayed.GetComponent<Cell>().unitOnTile;
                if (tempSelectedUnit.GetComponent<UnitScript>().isUnselected()
                                && tempSelectedUnit.GetComponent<UnitScript>().teamNum == GMS.currentTeam
                                )
                {
                    disableHighlightUnitRange();
                    //selectedSound.Play();
                    selectedUnit = tempSelectedUnit;
                    selectedUnit.GetComponent<UnitScript>().map = this;
                    selectedUnit.GetComponent<UnitScript>().setMovementState(1);
                    selectedUnit.GetComponent<UnitScript>().setSelectedAnimation();
                    unitSelected = true;
                    highlightUnitRange();
                   
                }
            }
        }
        
}
    //In:  
    //Out: void
    //Desc: finalizes the player's option, wait or attack
    public void finalizeOption()
    {
    
    RaycastHit hit;
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    HashSet<Node> attackableTiles = getUnitAttackOptionsFromPosition();

    if (Physics.Raycast(ray, out hit))
    {

        //This portion is to ensure that the tile has been clicked
        //If the tile has been clicked then we need to check if there is a unit on it
        if (hit.transform.gameObject.CompareTag("Tile"))
        {
            if (hit.transform.GetComponent<Cell>().unitOnTile != null)
            {
                GameObject unitOnTile = hit.transform.GetComponent<Cell>().unitOnTile;
                int unitX = unitOnTile.GetComponent<UnitScript>().x;
                int unitY = unitOnTile.GetComponent<UnitScript>().y;

                if (unitOnTile == selectedUnit)
                {
                    disableHighlightUnitRange();
                    Debug.Log("ITS THE SAME UNIT JUST WAIT");
                    selectedUnit.GetComponent<UnitScript>().setWaitIdleAnimation();
                    selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                    deselectUnit();


                    }
                    else if (unitOnTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX,unitY]))
                {
                        if (unitOnTile.GetComponent<UnitScript>().currentHealthPoints > 0)
                        {
                            Debug.Log("We clicked an enemy that should be attacked");
                            Debug.Log(selectedUnit.GetComponent<UnitScript>().currentHealthPoints);
                            StartCoroutine(BMS.doAttack(selectedUnit, unitOnTile));

                            
                            StartCoroutine(deselectAfterMovements(selectedUnit, unitOnTile));
                        }
                }                                     
            }
        }
        else if (hit.transform.parent != null && hit.transform.parent.gameObject.CompareTag("Unit"))
        {
            GameObject unitClicked = hit.transform.parent.gameObject;
            int unitX = unitClicked.GetComponent<UnitScript>().x;
            int unitY = unitClicked.GetComponent<UnitScript>().y;

            if (unitClicked == selectedUnit)
            {
                disableHighlightUnitRange();
                Debug.Log("ITS THE SAME UNIT JUST WAIT");
                selectedUnit.GetComponent<UnitScript>().setWaitIdleAnimation();
                selectedUnit.GetComponent<UnitScript>().setMovementState(3);
                deselectUnit();

            }
            else if (unitClicked.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum && attackableTiles.Contains(graph[unitX, unitY]))
            {
                    if (unitClicked.GetComponent<UnitScript>().currentHealthPoints > 0)
                    {
                        
                        Debug.Log("We clicked an enemy that should be attacked");
                        Debug.Log("Add Code to Attack enemy");
                        //selectedUnit.GetComponent<UnitScript>().setAttackAnimation();
                        StartCoroutine(BMS.doAttack(selectedUnit, unitClicked));

                        // selectedUnit.GetComponent<UnitScript>().wait();
                        //Check if soemone has won
                        //GMS.checkIfUnitsRemain();
                        StartCoroutine(deselectAfterMovements(selectedUnit, unitClicked));
                    }
            }

        }
    }
    
}

    //In:  
    //Out: void
    //Desc: de-selects the unit
    public void deselectUnit()
    {
        
        if (selectedUnit != null)
        {
            if (selectedUnit.GetComponent<UnitScript>().isSelected())
            {
            disableHighlightUnitRange();
            disableUnitUIRoute();
            selectedUnit.GetComponent<UnitScript>().setMovementState(0);

       
            selectedUnit = null;
            unitSelected = false;
            }
            else if (selectedUnit.GetComponent<UnitScript>().isWaiting())
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                unitSelected = false;
                selectedUnit = null;
            }
            else
            {
                disableHighlightUnitRange();
                disableUnitUIRoute();
                int x = selectedUnit.GetComponent<UnitScript>().x;
                int y = selectedUnit.GetComponent<UnitScript>().y;

                tiles[x, y].tileOnMap.GetComponent<Cell>().unitOnTile = null;
                tiles[unitSelectedPreviousX, unitSelectedPreviousY].tileOnMap.GetComponent<Cell>().unitOnTile = selectedUnit;

                selectedUnit.GetComponent<UnitScript>().x = unitSelectedPreviousX;
                selectedUnit.GetComponent<UnitScript>().y = unitSelectedPreviousY;
                selectedUnit.GetComponent<UnitScript>().tileBeingOccupied = previousOccupiedTile;
                selectedUnit.transform.position = tileCoordToWorldCoord(unitSelectedPreviousX, unitSelectedPreviousY);
                selectedUnit.GetComponent<UnitScript>().setMovementState(0);
                selectedUnit = null;
                unitSelected = false;
            }
        }
    }


    //In:  
    //Out: void
    //Desc: highlights the units range options (this is the portion shown in the video)
    public void highlightUnitRange()
    {
       
       
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        HashSet<Node> finalEnemyUnitsInMovementRange = new HashSet<Node>();
      
        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;


        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        finalMovementHighlight = getUnitMovementOptions();
        totalAttackableTiles = getUnitTotalAttackableTiles(finalMovementHighlight, attRange, unitInitialNode);
        //Debug.Log("There are this many available tiles for the unit: "+finalMovementHighlight.Count);

        foreach (Node n in totalAttackableTiles)
        {

            if (tiles[n.x, n.y].tileOnMap.GetComponent<Cell>().unitOnTile != null)
            {
                GameObject unitOnCurrentlySelectedTile = tiles[n.x, n.y].tileOnMap.GetComponent<Cell>().unitOnTile;
                if (unitOnCurrentlySelectedTile.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
                {
                    finalEnemyUnitsInMovementRange.Add(n);
                }
            }
        }

        
        highlightEnemiesInRange(totalAttackableTiles);
        //highlightEnemiesInRange(finalEnemyUnitsInMovementRange);
        highlightMovementRange(finalMovementHighlight);
        //Debug.Log(finalMovementHighlight.Count);
        selectedUnitMoveRange = finalMovementHighlight;

        //This final bit sets the selected Units tiles, which can be accessible in other functions
        //Probably bad practice, but I'll need to get things to work for now (2019-09-30)
        selectedUnitTotalRange = getUnitTotalRange(finalMovementHighlight, totalAttackableTiles);
        //Debug.Log(unionTiles.Count);
        
        //Debug.Log("exiting the while loop");
        //This will for each loop will highlight the movement range of the units
       

    }


    //In:  
    //Out: void
    //Desc: disables the quads that are being used to highlight position
    public void disableUnitUIRoute()
    {
        foreach(GameObject quad in quadOnMapPath)
        {
            if (quad.GetComponent<Renderer>().enabled == true)
            {
                
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    //In:  
    //Out: HashSet<Node> of the tiles that can be reached by unit
    //Desc: returns the hashSet of nodes that the unit can reach from its position
    public HashSet<Node> getUnitMovementOptions()
    {
        float[,] cost = new float[mapSizeX, mapSizeY];
        HashSet<Node> UIHighlight = new HashSet<Node>();
        HashSet<Node> tempUIHighlight = new HashSet<Node>();
        HashSet<Node> finalMovementHighlight = new HashSet<Node>();      
        int moveSpeed = selectedUnit.GetComponent<UnitScript>().moveSpeed;
        Node unitInitialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];

        ///Set-up the initial costs for the neighbouring nodes
        finalMovementHighlight.Add(unitInitialNode);
        foreach (Node n in unitInitialNode.neighbours)
        {
            cost[n.x, n.y] = costToEnterTile(n.x, n.y);
            //Debug.Log(cost[n.x, n.y]);
            if (moveSpeed - cost[n.x, n.y] >= 0)
            {
                UIHighlight.Add(n);
            }
        }

        finalMovementHighlight.UnionWith(UIHighlight);

        while (UIHighlight.Count != 0)
        {
            foreach (Node n in UIHighlight)
            {
                foreach (Node neighbour in n.neighbours)
                {
                    if (!finalMovementHighlight.Contains(neighbour))
                    {
                        cost[neighbour.x, neighbour.y] = costToEnterTile(neighbour.x, neighbour.y) + cost[n.x, n.y];
                        //Debug.Log(cost[neighbour.x, neighbour.y]);
                        if (moveSpeed - cost[neighbour.x, neighbour.y] >= 0)
                        {
                            //Debug.Log(cost[neighbour.x, neighbour.y]);
                            tempUIHighlight.Add(neighbour);
                        }
                    }
                }

            }

            UIHighlight = tempUIHighlight;
            finalMovementHighlight.UnionWith(UIHighlight);
            tempUIHighlight = new HashSet<Node>();
           
        }
        Debug.Log("The total amount of movable spaces for this unit is: " + finalMovementHighlight.Count);
        Debug.Log("We have used the function to calculate it this time");
        return finalMovementHighlight;
    }

    //In:  finalMovement highlight and totalAttackabletiles
    //Out: a hashSet of nodes that are the combination of the two inputs
    //Desc: returns the unioned hashSet
    public HashSet<Node> getUnitTotalRange(HashSet<Node> finalMovementHighlight, HashSet<Node> totalAttackableTiles)
    {
        HashSet<Node> unionTiles = new HashSet<Node>();
        unionTiles.UnionWith(finalMovementHighlight);
        //unionTiles.UnionWith(finalEnemyUnitsInMovementRange);
        unionTiles.UnionWith(totalAttackableTiles);
        return unionTiles;
    }
    //In:  finalMovement highlight, the attack range of the unit, and the initial node that the unit was standing on
    //Out: hashSet Node of the total attackable tiles for the unit
    //Desc: returns a set of nodes that represent the unit's total attackable tiles
    public HashSet<Node> getUnitTotalAttackableTiles(HashSet<Node> finalMovementHighlight, int attRange, Node unitInitialNode)
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        HashSet<Node> totalAttackableTiles = new HashSet<Node>();
        foreach (Node n in finalMovementHighlight)
        {
            neighbourHash = new HashSet<Node>();
            neighbourHash.Add(n);
            for (int i = 0; i < attRange; i++)
            {
                foreach (Node t in neighbourHash)
                {
                    foreach (Node tn in t.neighbours)
                    {
                        tempNeighbourHash.Add(tn);
                    }
                }

                neighbourHash = tempNeighbourHash;
                tempNeighbourHash = new HashSet<Node>();
                if (i < attRange - 1)
                {
                    seenNodes.UnionWith(neighbourHash);
                }

            }
            neighbourHash.ExceptWith(seenNodes);
            seenNodes = new HashSet<Node>();
            totalAttackableTiles.UnionWith(neighbourHash);
        }
        totalAttackableTiles.Remove(unitInitialNode);
        
        //Debug.Log("The unit node has this many attack options" + totalAttackableTiles.Count);
        return (totalAttackableTiles);
    }


    //In:  
    //Out: hashSet of nodes get all the attackable tiles from the current position
    //Desc: returns a set of nodes that are all the attackable tiles from the units current position
    public HashSet<Node> getUnitAttackOptionsFromPosition()
    {
        HashSet<Node> tempNeighbourHash = new HashSet<Node>();
        HashSet<Node> neighbourHash = new HashSet<Node>();
        HashSet<Node> seenNodes = new HashSet<Node>();
        Node initialNode = graph[selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y];
        int attRange = selectedUnit.GetComponent<UnitScript>().attackRange;


        neighbourHash = new HashSet<Node>();
        neighbourHash.Add(initialNode);
        for (int i = 0; i < attRange; i++)
        {
            foreach (Node t in neighbourHash)
            {
                foreach (Node tn in t.neighbours)
                {
                    tempNeighbourHash.Add(tn);
                }
            }
            neighbourHash = tempNeighbourHash;
            tempNeighbourHash = new HashSet<Node>();
            if (i < attRange - 1)
            {
                seenNodes.UnionWith(neighbourHash);
            }
        }
        neighbourHash.ExceptWith(seenNodes);
        neighbourHash.Remove(initialNode);
        return neighbourHash;
    }

    //In:  
    //Out: hashSet node that the unit is currently occupying
    //Desc: returns a set of nodes of the tile that the unit is occupying
    public HashSet<Node> getTileUnitIsOccupying()
    {
       
        int x = selectedUnit.GetComponent<UnitScript>().x;
        int y = selectedUnit.GetComponent<UnitScript>().y;
        HashSet<Node> singleTile = new HashSet<Node>();
        singleTile.Add(graph[x, y]);
        return singleTile;
        
    }

    //In:  
    //Out: void
    //Desc: highlights the selected unit's options
    public void highlightTileUnitIsOccupying()
    {
        if (selectedUnit != null)
        {
            highlightMovementRange(getTileUnitIsOccupying());
        }
    }

    //In:  
    //Out: void
    //Desc: highlights the selected unit's attackOptions from its position
    public void highlightUnitAttackOptionsFromPosition()
    {
        if (selectedUnit != null)
        {
            highlightEnemiesInRange(getUnitAttackOptionsFromPosition());
        }
    }

    //In:  Hash set of the available nodes that the unit can range
    //Out: void - it changes the quadUI property in the gameworld to visualize the selected unit's movement
    //Desc: This function highlights the selected unit's movement range
    public void highlightMovementRange(HashSet<Node> movementToHighlight)
    {
        foreach (Node n in movementToHighlight)
        {
            quadOnMapRange[n.x, n.y].GetComponent<Renderer>().material = blueUIMat;
            quadOnMapRange[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }



    //In:  Hash set of the enemies in range of the selected Unit
    //Out: void - it changes the quadUI property in the gameworld to visualize an enemy
    //Desc: This function highlights the enemies in range once they have been added to a hashSet
    public void highlightEnemiesInRange(HashSet<Node> enemiesToHighlight)
    {
        foreach (Node n in enemiesToHighlight)
        {
            quadOnMapRange[n.x, n.y].GetComponent<Renderer>().material = redUIMat;
            quadOnMapRange[n.x, n.y].GetComponent<MeshRenderer>().enabled = true;
        }
    }


    //In:  
    //Out: void 
    //Desc: disables the highlight
    public void disableHighlightUnitRange()
    {
        foreach(GameObject quad in quadOnMapRange)
        {
            if(quad.GetComponent<Renderer>().enabled == true)
            {
                quad.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    //In:  
    //Out: void 
    //Desc: moves the unit then finalizes the movement
    public IEnumerator moveUnitAndFinalize()
    {
        disableHighlightUnitRange();
        disableUnitUIRoute();
        while (selectedUnit.GetComponent<UnitScript>().movementQueue.Count != 0)
        {
            yield return new WaitForEndOfFrame();
        }
        finalizeMovementPosition();
        selectedUnit.GetComponent<UnitScript>().setSelectedAnimation();
    }


    //In:  both units engaged in a battle
    //Out:  
    //Desc: deselects the selected unit after the action has been taken
    public IEnumerator deselectAfterMovements(GameObject unit, GameObject enemy)
    {
        //selectedSound.Play();
        selectedUnit.GetComponent<UnitScript>().setMovementState(3);
        disableHighlightUnitRange();
        disableUnitUIRoute();
        //If i dont have this wait for seconds the while loops get passed as the coroutine has not started from the other script
        //Adding a delay here to ensure that it all works smoothly. (probably not the best idea)
        yield return new WaitForSeconds(.25f);
        while (unit.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        while (enemy.GetComponent<UnitScript>().combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
          
        }
        Debug.Log("All animations done playing");
       
        deselectUnit();


    }

    //In:  
    //Out: true if there is a tile that was clicked that the unit can move to, false otherwise 
    //Desc: checks if the tile that was clicked is move-able for the selected unit
    public bool selectTileToMoveTo()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
           
            if (hit.transform.gameObject.CompareTag("Tile")){
               
                int clickedTileX = hit.transform.GetComponent<Cell>().tileX;
                int clickedTileY = hit.transform.GetComponent<Cell>().tileY;
                Node nodeToCheck = graph[clickedTileX, clickedTileY];
                //var unitScript = selectedUnit.GetComponent<UnitScript>();

                if (selectedUnitMoveRange.Contains(nodeToCheck)) {
                    if ((hit.transform.gameObject.GetComponent<Cell>().unitOnTile == null || hit.transform.gameObject.GetComponent<Cell>().unitOnTile == selectedUnit) && (selectedUnitMoveRange.Contains(nodeToCheck)))
                    {
                        Debug.Log("We have finally selected the tile to move to");
                        generatePathTo(clickedTileX, clickedTileY);
                        return true;
                    }
                }
            }
            else if (hit.transform.gameObject.CompareTag("Unit"))
            {
              
                if (hit.transform.parent.GetComponent<UnitScript>().teamNum != selectedUnit.GetComponent<UnitScript>().teamNum)
                {
                    Debug.Log("Clicked an Enemy");
                }
                else if(hit.transform.parent.gameObject == selectedUnit)
                {
                   
                    generatePathTo(selectedUnit.GetComponent<UnitScript>().x, selectedUnit.GetComponent<UnitScript>().y);
                    return true;
                }
            }

        }
        return false;
    }
}

