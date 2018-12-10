//Written by Eric Hollas
//
//This collider script is used mostly to pass 
//   damage from the weapon to the parenting 
//   character. It is done this way because it 
//   will be the collider not the player when 
//   the weapon’s (snowball, cnadycane, or 
//   enemy fists) script calls OnTriggerEnter. 
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider_EnemySolider_Script : MonoBehaviour 
{
    public GameObject Owning_Enemy;

    private EnemySolider_Script scrpt;

    public void Start()
    {
        this.tag = "enemy";
        scrpt = Owning_Enemy.GetComponent<EnemySolider_Script>();
    }


    //HitDetected method is called in the weapon script to pass damage
    //   to the character’s script. The weapon script will call the correct
    //   HitDetected method via their weapon OnTriggerEnter Collider parameter.
    public void HitDetected(float damage)
    {
        scrpt.HitDetected(damage);
    }
}
