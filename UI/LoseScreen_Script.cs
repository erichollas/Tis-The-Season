//Written by Eric Hollas
//
//This script gets the info from the HUD 
//   and displays the lose screen for 
//   the end of the game.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoseScreen_Script : MonoBehaviour 
{
    public UI_Script hud_script;
    public Text greens_score;
    public Text blues_score;
    public Text terrain_score;

    public void Start()
    {
        //gets the info from the HUD and displays the lose screen
        greens_score.text = hud_script.GetGreensScore().ToString();
        blues_score.text = hud_script.GetBluesScore().ToString();
        terrain_score.text = hud_script.GetTerrainScore().ToString();
    }

}
