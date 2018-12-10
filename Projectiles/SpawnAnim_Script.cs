//Written by Eric Hollas
//
//The SpawnAnim object is spawned by the 
//   SpawnOrb. This object plays a particle 
//   effect and spawns a green enemy.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAnim_Script : MonoBehaviour 
{
    public GameObject solider;            //the enemy solider to spawn
    public UIManager_Script GameState;    //the reference to the ui_manager script to determine if the game is paused or over

    private ParticleSystem burst;         //the particle effect to be played for spawn events

    private int spawner_id;               //the id of the spawner to keep track of how many soldiers are active per enemy spawner

    private float time;                   //used to delete the object when the particle effect has finished playing
    private bool already_spawned;         //ensures that this object spawn only one enemy solider

    public void Start()
    {
        burst = this.GetComponent<ParticleSystem>();
        GameState = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();
        already_spawned = false;
        time = 0.0f;
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            time += Time.deltaTime;

            if (time >= 0.85f &&
               !already_spawned)
            {
                //make already_spawned to make sure only one solider is spawned
                already_spawned = true;

                //spawns the enemy at this game object’s location with a random orientation
                GameObject enemy = Instantiate(solider, this.transform.position, Quaternion.identity);
                enemy.transform.RotateAround(enemy.transform.position,
                                             Vector3.up,
                                             UnityEngine.Random.Range(0.0f, 360.0f));

                //Logs the new spawned enemy solider with the SoliderReferences_Script and keeps the spawner_id with
                //   the solider’s script to be called when the solider dies
                EnemySolider_Script enemy_script = enemy.GetComponent<EnemySolider_Script>();
                SoliderReferences_Script solider_refs =
                    GameObject.Find("SoliderReferences").GetComponent<SoliderReferences_Script>();

                enemy_script.SetID(spawner_id);
                solider_refs.AddToSpawnCount(spawner_id);
            }

            //the spawn particle effect is 2 seconds long, so delete if true
            if (time > 2.1f)
                Destroy(this.gameObject);
        }
    }

    //to be called in the SpawnOrb_Script
    public void SetID(int id)
    {
        spawner_id = id;
    }
}