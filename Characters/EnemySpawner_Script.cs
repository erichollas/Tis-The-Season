//Written by Eric Hollas
//
//This script controls the blue enemy spawner 
//   character and keeps track of it health. 
//   It also spawns SpawnOrb objects, which 
//   spawn SpawnAnim objects, which spawn green 
//   enemies. Thus the blue enemy is essentially 
//   a moving spawn point. The blue enemy also 
//   flees when the player is within line of 
//   sight.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySpawner_Script : MonoBehaviour 
{
    public enum spawner_state
    {
        hit,
        throwing,
        fleeing,
        idle
    }


    public GameObject orb;
    public GameObject orb_projectile;
    public ParticleSystem DeathEffect;
    public Image health_bar;
    public UIManager_Script GameState;

    public int spawner_id;

    private spawner_state curr_state;
    private spawner_state prev_state;

    private Animator anim;
    private GameObject player;
    private GameObject right_hand;
    private GameObject curr_orb;
    private SoliderReferences_Script spawners_refs;

    private float trans_time;

    private float health;
    private float spawn_timer;
    private float speed;
    private float rotation_speed;
    private float sight_line_dist;
    private float sight_line_range;
    private float flee_dist;
    private float dist_fled;

    private Vector3 location;
    private Vector3 target_rotation;
    private float x_min;
    private float x_max;
    private float tripped_distance;

    public void Start()
    {
        this.tag            = "enemy";
        GameState           = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();
        curr_state          = spawner_state.idle;
        prev_state          = spawner_state.idle;

        trans_time          = 0.0f;
        health              = 100.0f;;
        spawn_timer         = 7.0f;
        speed               = 4.0f;
        rotation_speed      = 7.5f;
        sight_line_dist     = 100.0f;
        sight_line_range    = 60.0f;
        flee_dist           = 35.0f;
        dist_fled           = 0.0f;

        location            = this.transform.position;
        x_min               = 117.5f;
        x_max               = 445.0f;

        int dist            = (int)location.z;
        dist               /= 500;
        tripped_distance    = 57.0f + (dist * 500.0f);

        anim                = this.GetComponent<Animator>();
        player              = GameObject.Find("MainPlayer");
        spawners_refs       = GameObject.Find("SoliderReferences").GetComponent<SoliderReferences_Script>();
        right_hand          = 
            this.transform.Find("Armature/root/abdomen/chest/clavicle_R/scapula_R/humerus_R/forearm_R/hand_socket").gameObject;

        curr_orb = Instantiate(orb, right_hand.transform.position, this.transform.rotation);
        curr_orb.transform.SetParent(right_hand.transform);
        curr_orb.transform.localPosition = new Vector3(0.0f, 0.6f, 0.0f);

    }
    public void Update()
    {
        if(!GameState.IsGamePausedOrLost())
        {
            if (player.transform.position.z > (location.z + 550.0f))
                Destroy(this.gameObject);

            switch (curr_state)
            {
                case spawner_state.hit:
                    if (anim.IsInTransition(0))
                    {
                        trans_time += Time.deltaTime;
                        if (trans_time > 0.25f)
                        {
                            target_rotation = this.transform.eulerAngles;
                            dist_fled = 20.0f;
                            anim.SetBool("is_fleeing", true);

                            curr_state = spawner_state.fleeing;
                            prev_state = spawner_state.hit;
                        }
                    }
                    break;
                case spawner_state.fleeing:
                    CheckThrowTime();
                    Flee();
                    break;
                case spawner_state.idle:
                    CheckThrowTime();
                    Idle();
                    break;
                default:
                    break;
            }
        }
    }

    public void FixedUpdate()
    {
        this.transform.position = location;
    }

    public void SetID(int identity)
    {
        spawner_id = identity;
        spawners_refs = GameObject.Find("SoliderReferences").GetComponent<SoliderReferences_Script>();
        spawners_refs.AddSpawner(spawner_id);
    }

    public void HitDetected(float hurt)
    {
        if(health > 0.0f)
        {
            health -= hurt;
            health_bar.transform.localScale = new Vector3((Mathf.Clamp(health, 0.0f, health)) / 100.0f, 1.0f, 1.0f);
            trans_time = 0.0f;
            anim.SetTrigger("hit");

            curr_state = spawner_state.hit;

            if (health <= 0.0f)
            {
                Destroy(this.gameObject);

                ParticleSystem death = Instantiate(DeathEffect,
                                                   this.transform.position + new Vector3(0.0f, 2.0f, 0.0f),
                                                   Quaternion.identity);

                GameObject.Find("HUD").GetComponent<UI_Script>().IncBluesScore();
            }
        }
    }

    public void SpawnEvent()
    {
        Vector3 curr_pos = curr_orb.transform.position;
        Destroy(curr_orb);

        GameObject proj_orb = Instantiate(orb_projectile, curr_pos, Quaternion.identity);
        proj_orb.GetComponent<SpawnOrb_Script>().InitOrb(spawner_id, this.transform.forward);
    }

    public void ThrowEnd()
    {
        this.transform.RotateAround(this.transform.position,
                                    Vector3.up,
                                    UnityEngine.Random.Range(0.0f, 360.0f));
        dist_fled = 20.0f;
        anim.SetBool("is_fleeing", true);

        prev_state = spawner_state.throwing;
        curr_state = spawner_state.fleeing;
    }

    private void CheckThrowTime()
    {
        if (spawn_timer > 5.0f &&
            !curr_orb)
        {
            curr_orb = Instantiate(orb, right_hand.transform.position, this.transform.rotation);
            curr_orb.transform.SetParent(right_hand.transform);

            curr_orb.transform.localPosition = new Vector3(0.0f, 0.6f, 0.0f);
        }

        if (spawn_timer > 10.0f)
        {
            if (spawners_refs.GetSpawnerCount(spawner_id) <= 2)
            {
                anim.SetTrigger("throw");

                prev_state = curr_state;
                curr_state = spawner_state.throwing;
            }
            spawn_timer = UnityEngine.Random.Range(0.0f, 6.0f);
        }
        spawn_timer += Time.deltaTime;
    }
    private void Flee()
    {
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                   Quaternion.Euler(target_rotation),
                                                   rotation_speed * Time.deltaTime);

        Vector3 direction = (this.transform.forward).normalized;
        direction *= speed * Time.deltaTime;
        location += new Vector3(direction.x, 0.0f, direction.z);

        location = new Vector3(Mathf.Clamp(location.x, x_min, x_max),
                               location.y,
                               Mathf.Clamp(location.z, tripped_distance, location.z));

        RaycastHit hit;
        if ((Physics.Raycast(location, Vector3.down, out hit, 512)) ||
            Physics.Raycast(location, Vector3.up, out hit, 512))
            location.y = hit.point.y;

        dist_fled += (speed * Time.deltaTime);

        if (dist_fled > flee_dist)
        {
            this.transform.RotateAround(location, Vector3.up, 180.0f);
            anim.SetBool("is_fleeing", false);

            prev_state = spawner_state.fleeing;
            curr_state = spawner_state.idle;
        }
    }
    private void Idle()
    {
        Vector3 solider_to_player = player.transform.position - location;
        float sight_angle = Vector3.Angle(solider_to_player, this.transform.forward);

        if (solider_to_player.magnitude < sight_line_dist &&
            sight_angle < sight_line_range)
        {
            dist_fled = 0.0f;
            anim.SetBool("is_fleeing", true);
            target_rotation = this.transform.eulerAngles - new Vector3(0.0f, sight_angle + 180.0f, 0.0f);

            prev_state = spawner_state.idle;
            curr_state = spawner_state.fleeing;
        }
    }
}