//Written by Eric Hollas
//
//This script ensures that the water game object follows the user’s z-position. This will
//   use less memory instead of creating and deleting multiple water objects.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water_Script : MonoBehaviour 
{
    public GameObject player;
    public UIManager_Script GameState;

    private float pos_x;
    private float pos_y;

    public void Start()
    {
        GameState = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();
        Vector3 pos = this.transform.position;
        pos_x = pos.x;
        pos_y = pos.y;
    }

    public void Update()
    {
        //if the game state is playing this object will match the user’s z-position.
        if (!GameState.IsGamePausedOrLost())
            this.transform.position = new Vector3(pos_x, pos_y, player.transform.position.z);
    }
}