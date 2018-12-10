//Written by Eric Hollas
//
//This script manages the camera. The camera 
//   will follow the player and have a fixed 
//   height (for the look at point) so the 
//   player will bob up and down running over 
//   the hills but the camera will not. There
//   is also a min and max amount of rotation.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Script : MonoBehaviour 
{
    public GameObject player;
    public UIManager_Script GameState;
    private GameObject azimuth_plane;
    private GameObject camera_boom;

    public float min_vertical_constraint;
    public float max_vertical_constraint;
    public float camera_speed_vertical;
    public float camera_speed_horizontal;

    private Vector3 pos;
    private float tilt;
    private float pan;

    public void Start()
    {
        azimuth_plane           = GameObject.Find("/CameraRig/AzimuthPlane");
        camera_boom             = GameObject.Find("/CameraRig/AzimuthPlane/BoomPivot");
        GameState               = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        min_vertical_constraint = -50.0f;
        max_vertical_constraint = 60.0f;
        camera_speed_vertical   = 2.0f;
        camera_speed_horizontal = 2.0f;

        tilt                    = 0.0f;
        pan                     = 0.0f;
        pos                     = player.transform.position;
        pos.y                   = 12.5f;

        this.transform.position = pos;
    }

    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            //Fixes the camera height and follows the player.
            //   Ensures that the camera does not bob up and down.
            pos = player.transform.position;
            pos.y = 12.5f;

            this.transform.position = pos;

            //Gets the user input for the camera rotation around the player
            tilt -= camera_speed_vertical * Input.GetAxis("Mouse Y");
            pan += camera_speed_horizontal * Input.GetAxis("Mouse X");

            //sets the min and max for the vertical rotation
            tilt = Mathf.Clamp(tilt, min_vertical_constraint, max_vertical_constraint);

            //sets the rotation around the player
            azimuth_plane.transform.localEulerAngles = new Vector3(0.0f, pan, 0.0f);
            camera_boom.transform.localEulerAngles = new Vector3(tilt, 0.0f, 0.0f);
        }
    }

}