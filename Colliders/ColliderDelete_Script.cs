//Written by Eric Hollas
//
//This collider script is the trigger for the 
//   ContinuousTerrain_Script to delete a new 
//   terrain.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderDelete_Script : MonoBehaviour
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

    //Called when the player collides with this box collider. Deletes the Terrain object that is before this
    //   collider’s location. Also, already_triggered is set to true so that this object is only called
    //   once. This also ensures that this collider is only deleted in one place (the ContinuousTerrain_Script).
    public void OnTriggerEnter(Collider other)
    {
        if (!already_triggered && other.gameObject.CompareTag("Player"))
        {
            already_triggered = true;
            float z_pos = this.transform.position.z;
            cont_terr_script.TriggerDeleteTerrain((int)z_pos);
            GameObject.Find("MainPlayer").GetComponent<MainPlayer_Script>().TripDistance(z_pos);
        }
    }
}
