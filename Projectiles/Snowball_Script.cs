//Written by Eric Hollas
//
//This script controls the snowball weapon object. 
//   When it explodes on collision it checks for 
//   colliders with an area of effect and applies 
//   damage to all colliders within this area of 
//   effect.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowball_Script : MonoBehaviour 
{
    public ParticleSystem OnDeathAffect;   //play on collision
    public UIManager_Script GameState;     //to call function if paused or gameover

    private Rigidbody physics_body;        //to set the velocity

    public float area_of_effect;           //determines the grenade’s area of effect

    private Vector3 displacement;          //to set the velocity, especially the direction of the object
    private float time;                    //to calculate velocity

    public void Start()
    {
        physics_body    = this.GetComponent<Rigidbody>();
        GameState       = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        this.tag        = "weapon_snowball";

        area_of_effect  = 25.0f;
        time            = 0.0f;
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            //calculates the current velocity using basic kinematic equations, 
            //   v_y - g * t, v_y is vertical velocity, g is gravity, and t is time change.
            //   Given how Unity calculates the horizontal position, gravity needs to be 
            //   more than 9.81 to achieve desired effect.
            time += Time.deltaTime;
            displacement.y = 10.0f - 15.0f * time;

            physics_body.velocity = displacement;
        }
        else
        {
            //freezes the velocity when paused or gameover
            physics_body.velocity = Vector3.zero;
        }
    }


    public void Release(Vector3 frwrd)
    {
        //Called in MainPlayer_Script to get direction the ball is thrown
        displacement = frwrd.normalized * 40.0f;
    }

    public void OnTriggerEnter(Collider other)
    {
        //to ensure the player’s collider does not trigger a collision
        if(!other.CompareTag("Player"))
        {
            Vector3 pos = this.transform.position;
            Vector3 rot = this.transform.forward;
            Destroy(this.gameObject);

            //calls the CollisionDetection method to get all of the colliders within the area of effect
            List<Collider> aoe_objs = CollisionDetection(pos, area_of_effect);

            //goes through each of the collisions with the colliders
            foreach (Collider coll in aoe_objs)
            {
                //calculates the damage based on the distance from the collision to the collider
                //   the values are low since it is likely that there will be multiple colliders of
                //   each object that the collision triggers 
                float dist = Vector3.Distance(pos, coll.transform.position);
                float aoe_damage;
                if (dist > 10.0f)
                    aoe_damage = 5.0f;
                else if (dist > 2.0f)
                    aoe_damage = 15.0f;
                else
                    aoe_damage = 25.0f;

                //gets the script of the collider to register the damage to the player through its collider
                Collider_EnemySolider_Script enemy_script;
                enemy_script = coll.gameObject.GetComponent<Collider_EnemySolider_Script>();
                if (enemy_script)
                    enemy_script.HitDetected(aoe_damage);
                else
                    coll.gameObject.GetComponent<Collider_EnemySpawner_Script>().HitDetected(aoe_damage);
            }

            //plays the particle effect for the snowball explosion
            ParticleSystem death = Instantiate(OnDeathAffect, pos, Quaternion.Euler(-rot));
        }
    }

    //Returns the List<Collider> of every Collider the function hits
    private List<Collider> CollisionDetection(Vector3 pos, float radius)
    {
        List<Collider> hits = new List<Collider>();

        //calculates the interval of angle the for-loop cycles through
        //   the for-loop will run accuracy cubed times
        //   the nested for-loops will cycle through every direction varying
        //   by step degrees to test if there is a collision within the radius parameter distance
        int accuracy = 12;
        float step = 360.0f / (float)accuracy;

        float x_angle = -step;
        float y_angle, z_angle;
        for (int x_step = 0; x_step < accuracy; x_step++)
        {
            x_angle += step;
            y_angle = -step;
            for (int y_step = 0; y_step < accuracy; y_step++)
            {
                y_angle += step;
                z_angle = -step;
                for (int z_step = 0; z_step < accuracy; z_step++)
                {
                    z_angle += step;
                    Vector3 direction = Quaternion.Euler(x_angle, y_angle, z_angle) * Vector3.forward;

                    RaycastHit h;
                    if (Physics.Raycast(pos, direction, out h))     //tests if there is a collision
                        if(h.collider.CompareTag("enemy"))          //tests if collision is with an enemy
                            if ((!hits.Contains(h.collider)) &&     //tests if the collider has already been added to hits
                                h.distance < radius)                //tests if the collision is within the area of effect
                                hits.Add(h.collider);
                }
            }
        }

        return hits;
    }
}
