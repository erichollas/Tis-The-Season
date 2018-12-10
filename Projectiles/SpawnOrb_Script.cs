//Written by Eric Hollas
//
//The SpawnOrb is spawned by the blue enemy. This 
//   script controls the SpawnOrb as a projectile. 
//   When it collides it spawns the SpawnAnim 
//   object. 
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOrb_Script : MonoBehaviour 
{
    public GameObject spawn_anim;         //the spawn object the spawns the enemy solider and plays a particle effect
    public UIManager_Script GameState;    //the ui_manager to keep track of the game being paused or gameover

    private Rigidbody physics_body;       //to apply the velocity of the object

    private int spawn_id;                 //to log the EnemySolider to the correct EnemySpawner within the SoliderReferences_Script

    private Vector3 displacement;         //used to calculate the velocity
    private float time;                   //used to calculate the velocity
    private float x_min;                  //clamps the orb to the playing area between the trees and ocean
    private float x_max;                  //clamps the orb to the playing area between the trees and ocean
    private float z_min;                  //clamps the orb to the playing area between the trees and ocean

    public void Start()
    {
        physics_body        = this.GetComponent<Rigidbody>();
        GameState           = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        time                = 0.0f;
        x_min               = 117.5f;
        x_max               = 445.0f;

        int dist            = (int)this.transform.position.z;
        dist               /= 500;
        z_min               = 57.0f + (dist * 500.0f);
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            //in case the collision is missed by the physics engine, delete the object to ensure that the memory isn’t kept
            if (this.transform.position.y < 0.0f)
                Destroy(this.gameObject);
            
            //calculates the current velocity using basic kinematic equations, 
            //   v_y - g * t, v_y is vertical velocity, g is gravity, and t is time change.
            //   Given how Unity calculates the horizontal position, gravity needs to be 
            //   more than 9.81 to achieve desired effect.
            time += Time.deltaTime;
            displacement.y = 10.0f - 9.81f * time;

            physics_body.velocity = displacement;

            //if the orb reaches the clamp values triggers the Spawn() event
            Vector3 pos = this.transform.position;
            if (pos.x < x_min || pos.x > x_max || pos.z < z_min)
                Spawn();
        }
        else
        {
            //freezes the object when the game state is paused or gameover
            physics_body.velocity = Vector3.zero;
        }
    }

    public void InitOrb(int id, Vector3 trajectory)
    {
        //called in the EnemySpawner_Script, sets the spawner’s id and the direction this orb is thrown
        spawn_id = id;
        displacement = trajectory.normalized * 30.0f;
    }

    public void OnTriggerEnter()
    {
        Spawn();
    }

    private void Spawn()
    {
        //initializes the spawn object
        GameObject spawn = Instantiate(spawn_anim, this.transform.position, Quaternion.identity);
        spawn.GetComponent<SpawnAnim_Script>().SetID(spawn_id);

        Destroy(this.gameObject);
    }
}
