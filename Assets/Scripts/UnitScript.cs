using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MapTileGridCreator.Core;

public class UnitScript : MonoBehaviour
{
    public int teamNum;
    public int x;
    public int y;

    //This is a low tier idea, don't use it 
    public bool coroutineRunning;

    //Meta defining play here
    public Queue<int> movementQueue;    // to avoid coroutine conflicts
    public Queue<int> combatQueue;      // to avoid coroutine conflicts
    public float visualMovementSpeed = .15f; // used to increase the units movementSpeed when travelling on the board

    //Animator
    public Animator animator;


    public GameObject tileBeingOccupied;

    public GameObject damagedParticle;

    [Header("Unit States")]
    public string unitName;
    public int moveSpeed = 2;    
    public int attackRange = 1;
    public int attackDamage = 1;
    public int maxHealthPoints = 5;
    public int currentHealthPoints;
    public Sprite unitSprite;

    [Header("UI Elements")]
    //Unity UI References
    public Canvas healthBarCanvas;
    public TMP_Text hitPointsText;
    public Image healthBar;

    public Canvas damagePopupCanvas;
    public TMP_Text damagePopupText;
    public Image damageBackdrop;
    

    //This may change in the future if 2d sprites are used instead
    //public Material unitMaterial;
    //public Material unitWaitMaterial;

    public tileMapScript map;

    //Location for positional update
    public Transform startPoint;
    public Transform endPoint;
    public float moveSpeedTime = 1f;
    
    //3D Model or 2D Sprite variables to check which version to use
    //Make sure only one of them are enabled in the inspector
    //public GameObject holder3D;
    public GameObject holder2D;
    // Total distance between the markers.
    private float journeyLength;

    //Boolean to startTravelling
    public bool unitInMovement;


    //Enum for unit states
    public enum movementStates
    {
        Unselected,
        Selected,
        Moved,
        Wait
    }
    public movementStates unitMoveState;
   
    //Pathfinding

    public List<Node> path = null;

    //Path for moving unit's transform
    public List<Node> pathForMovement = null;
    public bool completedMovement = false;

    private void Awake()
    {
        animator = holder2D.GetComponent<Animator>();
        movementQueue = new Queue<int>();
        combatQueue = new Queue<int>();
       
        x = (int)transform.position.x;
        y = (int)transform.position.z;
        unitMoveState = movementStates.Unselected;
        currentHealthPoints = maxHealthPoints;
        hitPointsText.SetText(currentHealthPoints.ToString());
    }

    public void LateUpdate()
    {
        // let 2D units always face to the camera
        healthBarCanvas.transform.forward = Camera.main.transform.forward;
        //damagePopupCanvas.transform.forward = Camera.main.transform.forward;
        holder2D.transform.forward = Camera.main.transform.forward;
    }

    // Move the unit to the selected tile along the path
    public void MoveNextTile()
    {
        if (path.Count == 0)
        {
            return;
        }
        else
        {
            StartCoroutine(moveOverSeconds(transform.gameObject, path[path.Count - 1]));
        }
        
     }

    // Reset the unit to be movable again
    public void moveAgain()
    {
        path = null;
        setMovementState(0);
        completedMovement = false;
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.white;
        setIdleAnimation();
        //gameObject.GetComponentInChildren<Renderer>().material = unitMaterial;
    }

    public void setMovementState(int i)
    {
        switch (i)
        {
            case 0:
                unitMoveState = movementStates.Unselected;  break;
            case 1:
                unitMoveState = movementStates.Selected; break;
            case 2:
                unitMoveState = movementStates.Moved; break;
            case 3:
                unitMoveState = movementStates.Wait; break;
            default:
                break;
        }
    }


    /**
     * Unit Damages
     */

    public void dealDamage(int x)
    {
        currentHealthPoints -= x;
        updateHealthUI();
    }

    public IEnumerator displayDamage(int damageTaken)
    {
        combatQueue.Enqueue(1);

        damagePopupText.SetText(damageTaken.ToString());
        damagePopupCanvas.enabled = true;

        Color backDrop = damageBackdrop.GetComponent<Image>().color;
        Color damageValue = damagePopupText.color;

        // gradually reduce transparency of text and backdrop (fade out)
        for (float f = 1f; f >= -0.01f; f -= 0.01f)
        {
            backDrop.a = f;
            damageValue.a = f;
            damageBackdrop.GetComponent<Image>().color = backDrop;
            damagePopupText.color = damageValue;
            yield return new WaitForEndOfFrame();
        }
        
        combatQueue.Dequeue();
    }

    public void updateHealthUI()
    {
        healthBar.fillAmount = (float)currentHealthPoints / maxHealthPoints;
        hitPointsText.SetText(currentHealthPoints.ToString());
    }

    public void changeHealthBarColour(int i)
    {
        if (i == 0)
        {
            healthBar.color = Color.blue;
        }
        else if (i == 1)
        {
           
            healthBar.color = Color.red;
        }
    }

    /*
     * Unit Death
     */

    public void unitDie()
    {
        if (holder2D.activeSelf)
        {
            StartCoroutine(fadeOut());
            StartCoroutine(destoryUnit());
        }
        //Destroy(gameObject,2f);
        /*
        Renderer rend = GetComponentInChildren<SpriteRenderer>();
        Color c = rend.material.color;
        c.a = 0f;
        rend.material.color = c;
        StartCoroutine(fadeOut(rend));*/
    }

    public IEnumerator fadeOut()
    {
        combatQueue.Enqueue(1);
        //setDieAnimation();
        //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        Renderer rend = GetComponentInChildren<SpriteRenderer>();
        
        for (float f = 1f; f >= .05; f -= 0.01f)
        {
            Color c = rend.material.color;
            c.a = f;
            rend.material.color = c;
            yield return new WaitForEndOfFrame();
        }
        combatQueue.Dequeue();
    }


    /*
     * Unit Movement
     */

    // Move a unit to a new tile (endNode)
    public IEnumerator moveOverSeconds(GameObject objectToMove,Node endNode)
    {
        movementQueue.Enqueue(1);
        path.RemoveAt(0); // remove the first tile on path because we are standing on it

        // move unit along the path
        while (path.Count != 0)
        {
            Vector3 endPos = map.tileCoordToWorldCoord(path[0].x, path[0].y);
            objectToMove.transform.position = Vector3.Lerp(transform.position, endPos, visualMovementSpeed);
            if ((transform.position - endPos).sqrMagnitude < 0.001)
            {
                path.RemoveAt(0);
            }
            yield return new WaitForEndOfFrame();
        }
        visualMovementSpeed = 0.15f;
        transform.position = map.tileCoordToWorldCoord(endNode.x, endNode.y);

        // update unit axis 
        x = endNode.x;
        y = endNode.y;

        // update map
        tileBeingOccupied.GetComponent<Cell>().unitOnTile = null;    // remove unit occupation on previous tile
        tileBeingOccupied = map.tiles[x, y].tileOnMap;                                   // link current occupied tile to this unit
        movementQueue.Dequeue();
    }


    #region public helper
    public bool isSelected() { return unitMoveState == movementStates.Selected; }
    public bool isUnselected() { return unitMoveState == movementStates.Unselected; }
    public bool isWaiting() { return unitMoveState == movementStates.Wait; }
    public bool isMoved() { return unitMoveState == movementStates.Moved; }
   
    #endregion


    /*
     * Helpers
     */

    public IEnumerator destoryUnit()
    {
        // wait until damage display and fadeout done
        // then destory the unit 
        while (combatQueue.Count > 0)
        {
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
    }

    public void resetPath()
    {
        path = null;
        completedMovement = false;
    }


    public void setSelectedAnimation()
    {
        
        animator.SetTrigger("toSelected");
    }
    public void setIdleAnimation()
    {        
        animator.SetTrigger("toIdle");
    }
    public void setWalkingAnimation()
    {
        animator.SetTrigger("toWalking");
    }

    // currently unit doesn't have attack animation
    public void setAttackAnimation()
    {
       animator.SetTrigger("toAttacking");
    }
    public void setWaitIdleAnimation()
    {
        gameObject.GetComponentInChildren<SpriteRenderer>().color = Color.gray;
        animator.SetTrigger("toIdleWait");
    }
       
    public void setDieAnimation()
    {
        animator.SetTrigger("dieTrigger");
    }
}
