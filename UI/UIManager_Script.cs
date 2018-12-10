//Written by Eric Hollas
//
//This script keeps track of the game’s state, wether that be
//   paused, playing, or game over. It also ensures that the 
//   correct UI screen(s) is currently showing
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager_Script : MonoBehaviour 
{
    public enum game_state
    {
        paused,
        playing,
        lost
    }

    public Canvas hud_screen;
    public Canvas pause_screen;
    public Canvas lose_screen;

    private game_state curr_state;

    public void Start()
    {
        curr_state = game_state.paused;
        hud_screen.gameObject.SetActive(false);
        pause_screen.gameObject.SetActive(true);
        lose_screen.gameObject.SetActive(false);
    }

    public void Update()
    {
        //the first if handles the paused state, the else if handles user input if the game is over
        if(Input.GetKeyDown(KeyCode.P))
        {
            //this nested if-else if statement is to handle paused or not paused states
            if (curr_state == game_state.paused)
            {
                curr_state = game_state.playing;
                hud_screen.gameObject.SetActive(true);
                pause_screen.gameObject.SetActive(false);
            }
            else if (curr_state == game_state.playing)
            {
                curr_state = game_state.paused;
                hud_screen.gameObject.SetActive(true);
                pause_screen.gameObject.SetActive(true);
            }
        }
        else if(Input.GetKeyDown(KeyCode.L))
        {
            if(curr_state == game_state.lost)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }

    //called in the MainPlayer_Script to end the game
    public void GameOver()
    {
        curr_state = game_state.lost;
        hud_screen.gameObject.SetActive(false);
        pause_screen.gameObject.SetActive(false);
        lose_screen.gameObject.SetActive(true);
    }

    //returns if the game is currently paused or if the player has lost
    //    this function will be called to freeze the game if paused and 
    //    to ensure no exceptions will be thrown when the MainPlayer is null
    public bool IsGamePausedOrLost()
    {
        return curr_state != game_state.playing;
    }
}
