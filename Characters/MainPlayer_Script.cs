//Written by Eric Hollas
//
//This is the script for the Main Player. 
//   This will control the player object 
//   and keep track of the player’s health. 
//   There are also 3 events for the 
//   player’s throw animation, BeginThrowEvent(), 
//   ThrowEventPeak(), ThrowEventRelease().
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayer_Script : MonoBehaviour 
{
    public UIManager_Script GameState;
    public GameObject candy_cane;
    public GameObject candy_cane_projectile;
    public GameObject snowball;
    public GameObject snowball_projectile;

    public ParticleSystem OnDeathEffect;

    private CandyCane_Script projectile_script_candy;
    private Snowball_Script projectile_script_snow;

    private Animator anim;
    private GameObject camera_rig;
    private GameObject throwing_hand;
    private GameObject weapon;

    private bool is_throwing = false;
    private float anim_speed = 0.0f;

    private Vector3 location;
    private bool equipped_cane;
    private float rotation_speed;
    private float speed;
    private float sprint_multiplier;
    private float health;
    private float x_min;
    private float x_max;
    private float tripped_distance;

    public void Start()
    {
        this.tag            = "Player";
        GameState           = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        location            = this.transform.position;
        equipped_cane       = false;
        rotation_speed      = 5.0f;
        speed               = 4.5f;
        sprint_multiplier   = 3.0f;
        health              = 100.0f;
        x_min               = 117.5f;
        x_max               = 445.0f;
        tripped_distance    = 57.0f;

        anim                = this.GetComponent<Animator>();
        camera_rig          = GameObject.Find("/CameraRig/AzimuthPlane");
        throwing_hand       =
            this.transform.Find("Armature/root/abdomen/chest/clavicle_R/scapula_R/humerus_R/forearm_R/hand_socket").gameObject;
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (anim_speed > 2.0f)
                    this.transform.rotation = Quaternion.Euler(0.0f, 15.0f, 0.0f) * this.transform.rotation;
                anim.SetTrigger("throw");
                is_throwing = true;
            }

            float frame_rot = 0.0f;
            int directions_num = 0;
            if (Input.GetKey(KeyCode.D))
            {
                frame_rot += 90.0f;
                directions_num++;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                frame_rot += 270.0f;
                directions_num++;
            }

            if (Input.GetKey(KeyCode.W))
            {
                frame_rot += ((frame_rot < 270.0f) ? (0.0f) : (360.0f));
                directions_num++;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                frame_rot += 180.0f;
                directions_num++;
            }

            if (directions_num > 0 && !is_throwing)
                Move(frame_rot / (float)directions_num);
            else
                anim_speed = 0.0f;

            anim.SetFloat("speed", anim_speed);
        }
    }

    public void FixedUpdate()
    {
        this.transform.position = location;
    }

    public void HitDetected(float dam)
    {
        if (!GameState.IsGamePausedOrLost())
        {
            health -= dam;
            anim.SetTrigger("hit");

            GameObject.Find("HUD").GetComponent<UI_Script>().PlayerHit(health);

            if (health <= 0.0f)
            {
                GameState.GameOver();

                Vector3 pos = this.transform.position;
                Destroy(this.gameObject);

                ParticleSystem boom = Instantiate(OnDeathEffect,
                                                  this.transform.position + new Vector3(0.0f, 2.0f, 0.0f),
                                                  Quaternion.Euler(Vector3.up));
            }
        }
    }

    public void EquipCandyCane()
    {
        if(!equipped_cane)
        {
            equipped_cane = true;
            weapon = Instantiate(candy_cane,
                                 throwing_hand.transform.position,
                                 Quaternion.identity);
            weapon.transform.SetParent(throwing_hand.transform);

            weapon.transform.localPosition = new Vector3(0.0f, 2.25f, -2.0f);
            weapon.transform.localEulerAngles = new Vector3(130.0f, 0.0f, 0.0f);
        }
    }
    public void TripDistance(float dist)
    {
        tripped_distance = dist - 493.0f;
    }

    public void BeginThrowEvent()
    {
        if(!equipped_cane)
        {
            weapon = Instantiate(snowball,
                                 throwing_hand.transform.position,
                                 Quaternion.identity);
            weapon.transform.SetParent(throwing_hand.transform);
            weapon.transform.localPosition += new Vector3(0.0f, 0.6f, 0.0f);
        }
    }
    public void ThrowEventPeak()
    {
        if(equipped_cane)
        {
            Vector3 pos = weapon.transform.position;
            Vector3 rot = weapon.transform.eulerAngles;

            GameObject projectile;

            projectile = Instantiate(candy_cane_projectile,
                                     pos,
                                     Quaternion.Euler(rot));
            projectile_script_candy = projectile.GetComponent<CandyCane_Script>();
            projectile_script_candy.SetThrowingHand(weapon.transform.parent.gameObject);

            Destroy(weapon);
        }
    }
    public void ThrowEventRelease()
    {
        Vector3 proj_direction = Quaternion.Euler(0.0f, -3.75f, 0.0f) * this.transform.forward;
        
        is_throwing = false;
        if (equipped_cane)
        {
            projectile_script_candy.Release(proj_direction);
            equipped_cane = false;
        }
        else
        {
            Vector3 pos = weapon.transform.position;

            Destroy(weapon);
            GameObject projectile;
            projectile = Instantiate(snowball_projectile,
                                     pos,
                                     Quaternion.identity);
            projectile_script_snow = projectile.GetComponent<Snowball_Script>();
            projectile_script_snow.Release(proj_direction);
        }
    }

    private void Move(float relative_rot)
    {
        float camera_rotation = camera_rig.transform.rotation.eulerAngles.y + relative_rot;

        float velocity = speed;
        if (Input.GetKey(KeyCode.Space))
        {
            anim_speed = 3.0f;
            velocity *= sprint_multiplier;
            camera_rotation -= 15.0f;
        }
        else
        {
            anim_speed = 1.0f;
        }

        this.transform.rotation = Quaternion.Slerp(this.transform.rotation,
                                                   Quaternion.Euler(0.0f, camera_rotation, 0.0f),
                                                   Time.deltaTime * rotation_speed);
        Vector3 direction = (this.transform.forward).normalized;

        direction *= velocity * Time.deltaTime;
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