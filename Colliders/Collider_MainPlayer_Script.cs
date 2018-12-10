//Written by Eric Hollas
//
//This collider script is used mostly to pass 
//   damage from the weapon to the parenting 
//   character. It is done this way because it 
//   will be the collider not the player when 
//   the weapon’s (snowball, cnadycane, or 
//   enemy fists) script calls OnTriggerEnter.
//This script will also call the function to 
//   equip a candy cane javelin when the 
//   player overlaps with a candy cane patch.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collider_MainPlayer_Script : MonoBehaviour 
{
    public GameObject player;

    private MainPlayer_Script scrpt;

    public void Start()
    {
        this.tag = "Player";

        scrpt = player.GetComponent<MainPlayer_Script>();
    }

    //Function when colliding with candy cane patch and equips 
    //   the player with a candy cane.
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("candy"))
            player.GetComponent<MainPlayer_Script>().EquipCandyCane();
    }

    //HitDetected method is called in the weapon script to pass damage
    //   to the character’s script. The weapon script will call the correct
    //   HitDetected method via their weapon OnTriggerEnter Collider parameter.
    public void HitDetected(float damage)
    {
        scrpt.HitDetected(damage);
    }
}