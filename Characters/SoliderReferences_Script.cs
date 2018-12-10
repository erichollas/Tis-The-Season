//Written by Eric Hollas
//
//This script will be on an empty GameObject. 
//   It will keep track of how many green 
//   enemies are spawned per each blue enemy 
//   spawner.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoliderReferences_Script : MonoBehaviour 
{
    private class spawn_ref
    {
        public int ident;
        public int count;
    }
    List<spawn_ref> spawners_id_refs = new List<spawn_ref>();


    public void Start()
    {
        List<spawn_ref> init_list = new List<spawn_ref>();
        for (int i = -3; i < 3; i++)
        {
            spawn_ref temp = new spawn_ref();
            temp.ident = i;
            temp.count = 0;
            init_list.Add(temp);
        }
        spawners_id_refs = init_list;
    }

    public void AddSpawner(int id)
    {
        spawn_ref spawner = new spawn_ref();
        spawner.ident = id;
        spawner.count = 0;

        spawners_id_refs.Add(spawner);
    }
    public void RemoveSpawner(int id)
    {
        List<spawn_ref> keepers = new List<spawn_ref>();

        foreach (spawn_ref spawner in spawners_id_refs)
            if (spawner.ident != id)
                keepers.Add(spawner);
        
        spawners_id_refs = keepers;
    }

    public void AddToSpawnCount(int id)
    {
        foreach(spawn_ref spawner in spawners_id_refs)
        {
            if(spawner.ident == id)
            {
                if (spawner.count < 5)
                    spawner.count++;
                break;
            }
        }
    }
    public void RemoveFromSpawnCount(int id)
    {
        foreach (spawn_ref spawner in spawners_id_refs)
        {
            if (spawner.ident == id)
            {
                if (spawner.count > 0)
                    spawner.count--;
                break;
            }
        }
    }

    public int GetSpawnerCount(int id)
    {
        foreach(spawn_ref spawner in spawners_id_refs)
            if(spawner.ident == id)
                return spawner.count;
        
        return 0;
    }
    public int GetSpawnerNumber()
    {
        return spawners_id_refs.Count;
    }
}
