//Written by Eric Hollas
//
//This is for the collider of the candy cane patch object to equip the candy cane object to the player 
//   when the player walks into the area of the candy cane patch.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyCanePatch_Script : MonoBehaviour 
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            GameObject.Find("MainPlayer").GetComponent<MainPlayer_Script>().EquipCandyCane();
    }
}
