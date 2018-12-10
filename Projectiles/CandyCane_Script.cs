//Written by Eric Hollas
//
//This script controls the candy cane javelin 
//   weapon. It sticks in the terrain and 
//   disappears after three seconds. Or if it 
//   hits an enemy it spawns a simple candy 
//   cane mesh that does not have a collider 
//   or script at the point of contact with 
//   the same rotation.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandyCane_Script : MonoBehaviour 
{
    public GameObject simple_cane;             //when sticking to player, a simple mesh with no collider or script
    public UIManager_Script GameState;         //to call function if paused or gameover

    private Rigidbody physics_body;            //to set the velocity
    private GameObject player_throwing_hand;   //gameobject the cane will follow while still possessed

    private Vector3 prev_hand_pos;             //used to calculate position when still possessed

    private bool still_possessed;              //boolean flag to determine if cane is still possessed
    private bool moving;                       //boolean flag to determine is cane isn’t stuck in ground
    private float damage;                      //sets the damage value

    private Vector3 displacement;              //to set the velocity, especially the direction of the object
    private float time;                        //to calculate velocity
    private float x_min;                       //used to clamp the object between the trees and ocean
    private float x_max;                       //used to clamp the object between the trees and ocean
    private float z_min;                       //used to clamp the object between the trees and ocean

    public void Start()
    {
        this.tag         = "weapon_candy";
        GameState        = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        physics_body     = this.GetComponent<Rigidbody>();

        moving           = true;
        still_possessed  = true;
        damage           = 80.0f;

        time             = 0.0f;
        x_min            = 117.5f;
        x_max            = 445.0f;

        int dist         = (int)this.transform.position.z;
        dist            /= 500;
        z_min            = 57.0f + (dist * 500.0f);
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            //the time variable ensures the candy cane disappears after 3 seconds when the 
            //    candycane is stuck in the ground.
            time += Time.deltaTime;

            //The orientation of the candycane should be set when the player is all the way
            //   wound in the throw animation. So the still_possessed ensures the cnadycane 
            //   still follows the player’s hand’s position.
            if (still_possessed)
            {
                Vector3 delta_pos = player_throwing_hand.transform.position - prev_hand_pos;
                prev_hand_pos = player_throwing_hand.transform.position;

                this.transform.position = this.transform.position + delta_pos;
            }
            else if (moving)
            {
                //calculates the current velocity using basic kinematic equations, 
                //   v_y - g * t, v_y is vertical velocity, g is gravity, and t is time change.
                //   Given how Unity calculates the horizontal position, gravity needs to be 
                //   more than 9.81 to achieve desired effect.
                time += Time.deltaTime;
                displacement.y = 10.0f - 15.0f * time;

                physics_body.velocity = displacement;

                //clamps the pos to stay away from trees or the ocean.
                Vector3 pos = this.transform.position;
                if (pos.x < x_min || pos.x > x_max || pos.z < z_min)
                    moving = false;

                //rotates the candycane
                this.transform.RotateAround(pos, Vector3.right, Time.deltaTime * 50.0f);
            }
            else if (time > 3.0f)
            {
                Destroy(this.gameObject);
            }
        }
        else
        {
            //freezes the object in mid air when paused
            physics_body.velocity = Vector3.zero;
        }
    }

    //gets the throwing hand object so that the candy cane can follow its position
    public void SetThrowingHand(GameObject bone)
    {
        player_throwing_hand = bone;
        if (!player_throwing_hand)
            player_throwing_hand =
                GameObject.Find("MainPlayer/Armature/root/abdomen/chest/clavicle_R/scapula_R/humerus_R/forearm_R/hand_socket").gameObject;
        
        prev_hand_pos = player_throwing_hand.transform.position;
    }

    //called in the MainPlayer_Script when the player finishes the throw animation
    public void Release(Vector3 frwrd)
    {
        time = 0.0f;
        still_possessed = false;

        //sets the direction the candy is thrown
        displacement = frwrd.normalized * 40.0f;
    }


    public void OnTriggerEnter(Collider other)
    {
        //the if freezes the candy cane as if it were stuck in the snow and
        //   sets the time to zero so that it vanishes after 3 seconds
        if (other.CompareTag("Terrain"))
        {
            moving = false;
            physics_body.velocity = Vector3.zero;
            time = 0.0f;
        }
        else if(other.CompareTag("enemy"))
        {
            Vector3 pos = this.transform.position;
            Vector3 rot = this.transform.eulerAngles;
            Destroy(this.gameObject);

            //makes a candycane object without a collider or script sticks in the enemy it hits
            //   with the correct position and rotation
            GameObject cane_strike = Instantiate(simple_cane, pos, Quaternion.Euler(rot));
            cane_strike.transform.parent = other.gameObject.transform.parent;

            //gets the correct script of the enemy and calls the correct HitDetected method for the damage
            Collider_EnemySolider_Script enemy_script;
            enemy_script = other.gameObject.GetComponent<Collider_EnemySolider_Script>();
            if (enemy_script)
                enemy_script.HitDetected(damage);
            else
                other.gameObject.GetComponent<Collider_EnemySpawner_Script>().HitDetected(damage);
        }
    }
}
