//Written by Eric Hollas
//
//This is for the colliders on the solider’s 
//   hands for when the solider attacks the 
//   user.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Fist_Script : MonoBehaviour 
{
    public float damage;
    private MainPlayer_Script scrpt;


    public void Start()
    {
        this.tag = "enemy_fist_collider";

        scrpt    = GameObject.Find("MainPlayer").GetComponent<MainPlayer_Script>();
        damage   = 10.0f;
    }

    //Called when the EnemySolider’s hands collide with the player. It
    //   then logs the damage to the player’s script.
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            scrpt.HitDetected(damage);
    }
}
