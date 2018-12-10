//Written by Eric Hollas
//
//This script controls the green enemies. The 
//   green enemy pursues the player when the 
//   player is within the line of sight. The 
//   green enemy attacks the player if within 
//   a certain a distance.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemySolider_Script : MonoBehaviour 
{
    public enum solider_state
    {
        hit,
        pursue,
        attack,
        idle
    }

    public UIManager_Script GameState;
    public ParticleSystem DeathEffect;
    public Image health_bar;

    private Animator anim;
    private GameObject player;
    private SoliderReferences_Script spawner_refs;

    private solider_state curr_stage;

    private int spawner_id;
    private float trans_time;

    private float health;
    private float speed;
    private float rotation_speed;
    private float sight_line_dist;
    private float sight_line_range;
    private float attack_dist;

    private Vector3 solider_to_player;
    private float dist_to_player;
    private float angle_to_player;

    private Vector3 location;
    private float x_min;
    private float x_max;
    private float tripped_distance;

    public void Start()
    {
        this.tag            = "enemy";
        curr_stage          = solider_state.idle;
        GameState           = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        health              = 100.0f;
        speed               = 8.0f;
        rotation_speed      = 10.0f;
        sight_line_dist     = 100.0f;
        sight_line_range    = 60.0f;
        attack_dist         = 2.0f;
        trans_time          = 0.0f;

        location            = this.transform.position;
        x_min               = 117.5f;
        x_max               = 445.0f;

        int dist            = (int)location.z;
        dist               /= 500;
        tripped_distance    = 57.0f + (dist * 500.0f);

        anim                = this.GetComponent<Animator>();
        player              = GameObject.Find("MainPlayer");
        spawner_refs        = GameObject.Find("SoliderReferences").GetComponent<SoliderReferences_Script>();
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            if (player.transform.position.z > (location.z + 550.0f))
                Destroy(this.gameObject);

            solider_to_player = player.transform.position - location;
            dist_to_player = Vector3.Distance(player.transform.position, location);
            angle_to_player = Vector3.Angle(solider_to_player, this.transform.forward);

            switch (curr_stage)
            {
                case solider_state.hit:
                    if (anim.IsInTransition(0))
                    {
                        trans_time += Time.deltaTime;
                        if (trans_time > 0.25f)
                            curr_stage = solider_state.idle;
                    }
                    break;
                case solider_state.attack:
                    if (dist_to_player < attack_dist && angle_to_player < sight_line_range)
                        anim.SetTrigger("attack");
                    else
                        curr_stage = solider_state.idle;
                    break;
                case solider_state.pursue:
                    if (dist_to_player < attack_dist)
                        curr_stage = solider_state.attack;
                    else if (dist_to_player < sight_line_dist && angle_to_player < sight_line_range)
                        Pursue();
                    else
                        curr_stage = solider_state.idle;
                    break;
                default:
                    anim.SetBool("is_pursuing", false);
                    if (dist_to_player < attack_dist)
                        curr_stage = solider_state.attack;
                    else if (dist_to_player < sight_line_dist && angle_to_player < sight_line_range)
                        Pursue();
                    break;
            }
        }
    }

    public void FixedUpdate()
    {
        this.transform.position = location;
    }


    public void SetID(int id)
    {
        spawner_id = id;
    }

    public void HitDetected(float dam)
    {
        if(health > 0.0f)
        {
            anim.SetTrigger("hit_front");
            curr_stage = solider_state.hit;
            trans_time = 0.0f;

            health -= dam;

            health_bar.transform.localScale = new Vector3((Mathf.Clamp(health, 0.0f, health)) / 100.0f, 1.0f, 1.0f);

            if (health <= 0.0f)
            {
                spawner_refs.RemoveFromSpawnCount(spawner_id);

                ParticleSystem sys = Instantiate(DeathEffect,
                                                 this.transform.position + new Vector3(0.0f, 2.0f, 0.0f),
                                                 Quaternion.identity);

                Destroy(this.gameObject);

                GameObject.Find("HUD").GetComponent<UI_Script>().IncGreensScore();
            }
        }
    }

    private void Pursue()
    {
        curr_stage = solider_state.pursue;
        anim.SetBool("is_pursuing", true);

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                   Quaternion.LookRotation(solider_to_player),
                                                   (Time.deltaTime * rotation_speed));

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
    }
}