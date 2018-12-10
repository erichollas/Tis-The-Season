//Written by Eric Hollas
//
//This collider script is the trigger for the 
//   ContinuousTerrain_Script to create a new 
//   terrain.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderCreate_Script : MonoBehaviour
{
    private Collider coll;
    private ContinuousTerrain_Script cont_terr_script;

    private int terrain_id;
    private bool already_triggered;

    public void Start()
    {
        coll = GetComponent<Collider>();
        coll.isTrigger = true;

        already_triggered = false;

        cont_terr_script = GameObject.Find("ContinuousTerrain").GetComponent<ContinuousTerrain_Script>();

    }

    //Called when the player collides with this box collider. Starts the process of starting a new Terrain object with
    //   the ContinuousTerrain object’s script. Also, already_triggered is set to true so that this object is only called
    //   once. This also ensures that this collider is only deleted in one place (the ContinuousTerrain_Script).
    public void OnTriggerEnter(Collider other)
    {
        if (!already_triggered && other.gameObject.CompareTag("Player"))
        {
            already_triggered = true;
            cont_terr_script.TriggerCreateTerrain((int)this.transform.position.z);
        }
    }

}