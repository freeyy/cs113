using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
 *  This script is for the battle system.
 *  It controls the animation/HP/... of a battle between any two units.
 *  Functions are called from [tileMapScript] when user launch an attack.
 */

public class battleManagerScript : MonoBehaviour
{
    public camShakeScript CSS;
    public gameManagerScript GMS;
    
    private bool battleStatus;  // to check if battle has finished

    //In: two unit gameObjects the attacker and the receiver
    //Desc: perform attack animation and results on two units
    public IEnumerator doAttack(GameObject unit, GameObject enemy)
    {
        battleStatus = true;
        float elapsedTime = 0;
        Vector3 startingPos = unit.transform.position;
        Vector3 endingPos = enemy.transform.position;

        
        unit.GetComponent<UnitScript>().setWalkingAnimation();  // switch unit animation

        // animation: a small movement to peform the attack
        while (elapsedTime < .25f)
        {
            unit.transform.position = Vector3.Lerp(startingPos, startingPos+((((endingPos - startingPos) / (endingPos - startingPos).magnitude)).normalized*.5f), (elapsedTime / .25f));
            elapsedTime += Time.deltaTime;
            
            yield return new WaitForEndOfFrame();
        }
        
        while (battleStatus)
        {
            // shake the camera based on the damage
            StartCoroutine(CSS.camShake(.2f,unit.GetComponent<UnitScript>().attackDamage,getDirection(unit,enemy)));
            
            // diplay damage on top of each unit
            if(unit.GetComponent<UnitScript>().attackRange == enemy.GetComponent<UnitScript>().attackRange 
                && enemy.GetComponent<UnitScript>().currentHealthPoints - unit.GetComponent<UnitScript>().attackDamage > 0)
            {
                StartCoroutine(unit.GetComponent<UnitScript>().displayDamage(enemy.GetComponent<UnitScript>().attackDamage));
                StartCoroutine(enemy.GetComponent<UnitScript>().displayDamage(unit.GetComponent<UnitScript>().attackDamage));
            }
            else
            {
                StartCoroutine(enemy.GetComponent<UnitScript>().displayDamage(unit.GetComponent<UnitScript>().attackDamage));
            }
            
            battleResult(unit, enemy);  // update HP of each unit
            yield return new WaitForEndOfFrame();
        }

        // animation: unit go back to starting point before attack
        if (unit != null)   
        {
            //StartCoroutine(returnAfterAttack(unit, startingPos));
            elapsedTime = 0;
            while (elapsedTime < .30f)
            {
                unit.transform.position = Vector3.Lerp(unit.transform.position, startingPos, (elapsedTime / .25f));
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            unit.GetComponent<UnitScript>().setWaitIdleAnimation(); // switch back unit animation to idle
        }
    }


    #region Helper Func

    //In: the initiator is the unit that initiated the attack and the recipient is the receiver
    //Desc: Deal with HP and death of two units after a battle.
    public void battleResult(GameObject initiator, GameObject recipient)
    {
        battleStatus = true;
        var initiatorUnit = initiator.GetComponent<UnitScript>();
        var recipientUnit = recipient.GetComponent<UnitScript>();
        int initiatorAtt = initiatorUnit.attackDamage;
        int recipientAtt = recipientUnit.attackDamage;
        //If the two units have the same attackRange then they can trade
        if (initiatorUnit.attackRange == recipientUnit.attackRange)
        {
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitScript>().damagedParticle, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            recipientUnit.dealDamage(initiatorAtt);
            if (isDead(recipient))
            {
                //Set to null then remove, if the gameObject is destroyed before its removed it will not check properly
                //This leads to the game not actually ending because the check to see if any units remains happens before the object
                //is removed from the parent, so we need to parent to null before we destroy the gameObject.
                recipient.transform.parent = null;
                recipientUnit.unitDie();
                battleStatus = false;
                GMS.checkIfUnitsRemain(initiator, recipient);
                return;
            }

            initiatorUnit.dealDamage(recipientAtt);
            if (isDead(initiator))
            {
                initiator.transform.parent = null;
                initiatorUnit.unitDie();
                battleStatus = false;
                GMS.checkIfUnitsRemain(initiator, recipient);
                return;

            }
        }
        //if the units don't have the same attack range, like a swordsman vs an archer; the recipient cannot strike back
        else
        {
            GameObject tempParticle = Instantiate(recipientUnit.GetComponent<UnitScript>().damagedParticle, recipient.transform.position, recipient.transform.rotation);
            Destroy(tempParticle, 2f);

            recipientUnit.dealDamage(initiatorAtt);
            if (isDead(recipient))
            {
                recipient.transform.parent = null;
                recipientUnit.unitDie();
                battleStatus = false;
                GMS.checkIfUnitsRemain(initiator, recipient);

                return;
            }
        }
        battleStatus = false;
    }

    //Desc: the vector3 which the unit needs to moveTowards is returned by this function
    public Vector3 getDirection(GameObject unit, GameObject enemy)
    {
        Vector3 startingPos = unit.transform.position;
        Vector3 endingPos = enemy.transform.position;
        return (((endingPos - startingPos) / (endingPos - startingPos).magnitude)).normalized;
    }

    //Desc: return if a unit is dead. (HP <= 0)
    private bool isDead(GameObject unitToCheck)
    {
        if (unitToCheck.GetComponent<UnitScript>().currentHealthPoints <= 0)
        {
            return true;
        }
        return false;
    }

    #endregion
}
