//Written by Eric Hollas
//
//This script creates and manages the HUD. It 
//   includes the methods for updating the 
//   current score and the user’s health bar.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Script : MonoBehaviour 
{
    public GameObject player;

    public Text greens_score;
    public Text blues_score;
    public Text dist_score;

    public Image health_bar;

    private MainPlayer_Script player_script;

    private int g_score;
    private int b_score;

    private void Start()
    {
        player_script = player.GetComponent<MainPlayer_Script>();
        g_score = 0;
        b_score = 0;
    }

    public void Update()
    {
        //Since the colliders are only called once, the update for distance traveled must
        //    be called constantly since the player may turn around and the score can count down
        int dist = (((int)player.transform.position.z / (int)500.0f));
        dist_score.text = dist.ToString();
    }

    //calculates the player’s health bar
    public void PlayerHit(float curr_health)
    {
        health_bar.transform.localScale = new Vector3((curr_health / 100.0f), 1.0f, 1.0f);
    }

    //Called from SoliderReferences_Script to tick the score when a blue/green enemy is destroyed
    public void IncGreensScore()
    {
        g_score++;
        greens_score.text = g_score.ToString();
    }
    public void IncBluesScore()
    {
        b_score++;
        blues_score.text = b_score.ToString();
    }

    //the next three methods return the values for the scores
    public int GetBluesScore()
    {
        return b_score;
    }
    public int GetGreensScore()
    {
        return g_score;
    }
    public int GetTerrainScore()
    {
        return (((int)player.transform.position.z / (int)500.0f));
    }
}
