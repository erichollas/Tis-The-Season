//Written by Eric Hollas
//
//This script keeps track of all the terrains and 
//   their corresponding scenery objects, with the 
//   exception of the first terrain. This script 
//   also creates and deletes terrains. The terrains 
//   are created using coroutines to ensure no 
//   sudden drops in frame rate. Terrains are also 
//   created using the values for the first terrain 
//   object by referencing its script to ensure that 
//   the new terrains match up.
//


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousTerrain_Script : MonoBehaviour
{
    public enum DevStage
    {
        Blank,
        Deleting,
        SculptHills,
        AddMountains,
        AddTextures,
        SculptCoast,
        CreateTerrainObj,
        AddScenicObjs,
        AddEnemies,
        Finished
    }
    public class terrain_object
    {
        public int z_pos;                       //makes sure that the terrain is at the correct location
        public GameObject terrain;              //the terrain’s gameobject
        public TerrainData terrain_data;        //the terrain’s data

        public GameObject collider_create;      //holds the reference to the create collide when it needs to be deleted
        public GameObject collider_delete;      //holds the reference to the delete collide when it needs to be deleted

        public List<GameObject> tree_objects;   //holds the reference to the tree objects when they needed to deleted
        public List<GameObject> candy_patches;  //holds the reference to the candy patches when they needed to deleted
    }
    public List<GameObject> first_layer_trees;          //holds the references to the trees that exist before run time
    public List<GameObject> first_layer_candy;          //holds the references to the candy patches that exist before run time

    public UIManager_Script GameState;                  //to get wether the game is paused or over
    public GameObject collider_create_terrain;          //to get the prototype for the create collider object
    public GameObject collider_delete_terrain;          //to get the prototype for the delete collider object
    public SoliderReferences_Script enemy_references;   //used to determine the spawner_ids of new enemyspawners

    private int frame_count = 0;                        //used to coordinate the coroutines with new terrains

    private int create_collider_offset = 100;           //used to correctly calculate the z_pos of the terrain_object
    private int delete_collider_offset = 50;            //used to correctly calculate the z_pos of the terrain_object

    private DevStage curr_stage = DevStage.Blank;       //keeps track of the current stage of terrain generation

    private terrain_object curr_terr_obj;               //used to keep track of the new terrain while being created before 
                                                        //   being added to the curr_terrains list

    private Terrain_Script first_terrain_script;        //references the script of the first terrain needed for generating 
                                                        //   new terrains
    private List<terrain_object> curr_terrains;         //holds the references to the active terrain_objects which references
                                                        //   the gameobjects associated with each terrain

    public void Start()
    {
        first_terrain_script = GameObject.Find("/FirstTerrain").GetComponent<Terrain_Script>();
        GameState = GameObject.Find("UI Manager").GetComponent<UIManager_Script>();

        curr_terrains = new List<terrain_object>();

        //gets the references to the first terrain level's trees and candy patches
        first_layer_trees = new List<GameObject>();
        first_layer_candy = new List<GameObject>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("tree"))
            first_layer_trees.Add(obj);
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("candy"))
            first_layer_candy.Add(obj);
        
        //starts the first terrain creation
        TriggerCreateTerrain(create_collider_offset);
    }
    public void Update()
    {
        if (!GameState.IsGamePausedOrLost())
        {
            frame_count++;
            if (frame_count == 40)
            {
                frame_count = 0;
                switch (curr_stage)
                {
                    case DevStage.SculptHills:
                        StartCoroutine(HillsRoutine(curr_terr_obj.z_pos / 500, 
                                                    curr_terr_obj.terrain_data));
                        curr_stage = DevStage.AddMountains;
                        break;
                    case DevStage.AddMountains:
                        StartCoroutine(MountainsRoutine(curr_terr_obj.terrain_data));
                        curr_stage = DevStage.AddTextures;
                        break;
                    case DevStage.AddTextures:
                        StartCoroutine(TexturesRoutine(curr_terr_obj.terrain_data));
                        curr_stage = DevStage.SculptCoast;
                        break;
                    case DevStage.SculptCoast:
                        StartCoroutine(CoastRoutine(curr_terr_obj.terrain_data));
                        curr_stage = DevStage.CreateTerrainObj;
                        break;
                    case DevStage.CreateTerrainObj:
                        CreateTerrainObj();
                        break;
                    case DevStage.AddScenicObjs:
                        ScenicObjsStageNewTerrain();
                        break;
                    case DevStage.AddEnemies:
                        SpawnEnemiesNewTerrain();
                        break;
                    case DevStage.Finished:
                        FinishedStageNewTerrain();
                        break;
                }
            }
        }
    }

    //to be called in the delete terrain collider
    //   the parameter is the z position of the collider
    public void TriggerDeleteTerrain(int pos)
    {
        //the first part of the if for the first delete collider to
        //    delete the first terrain's trees and candy patches
        if(pos == (1000 + delete_collider_offset))
        {
            foreach (GameObject obj in first_layer_trees)
                Destroy(obj);
            foreach (GameObject obj in first_layer_candy)
                Destroy(obj);
            first_layer_trees.Clear();
            first_layer_candy.Clear();
        }
        else
        {
            //find the right terrain reference in curr_terrains
            terrain_object temp = new terrain_object();
            foreach(terrain_object to in curr_terrains)
            {
                if(to.z_pos == (pos - (1000 + delete_collider_offset)))
                {
                    temp = to;
                    break;
                }
            }
            //delete the tree and candy patch objects
            foreach (GameObject patch in temp.candy_patches)
                Destroy(patch);
            foreach (GameObject tr in temp.tree_objects)
                Destroy(tr);

            //delete the terrain and remove the reference from curr_terrains
            Destroy(temp.terrain);
            curr_terrains.Remove(temp);
        }

        //the next part adds trees to the furthest behind terrain to show a barrier
        //   the player cannot pass

        //find the terrain we are looking for
        terrain_object behind = new terrain_object();
        foreach(terrain_object to in curr_terrains)
        {
            if(to.z_pos == (pos - (500 + delete_collider_offset)))
            {
                behind = to;
                break;
            }
        }

        // get the tree references and set the trees
        List<GameObject> barr_trees = new List<GameObject>();
        first_terrain_script.ApplyBarrierTrees(behind.terrain_data,
                                               behind.z_pos / 500,
                                               ref barr_trees);

        //keep the references to the new trees to the terrain_object
        foreach (GameObject tr in barr_trees)
            behind.tree_objects.Add(tr);
    }

    //to be called in the create terrain collider
    //   the parameter is the z position of the collider
    public void TriggerCreateTerrain(int pos)
    {
        //creates the terrain and initializes the associated TerrainData
        terrain_object terr = new terrain_object();
        terr.terrain_data = new TerrainData();
        terr.terrain_data.heightmapResolution = 513;
        terr.terrain_data.size = new Vector3(500.0f, 600.0f, 500.0f);
        terr.terrain_data.baseMapResolution = 1024;
        
        //sets the correct z position
        terr.z_pos = pos + (500 - create_collider_offset);

        //sets the curr_terr_obj and stage for the next phase of terrain creation
        curr_terr_obj = terr;
        curr_stage = DevStage.SculptHills;
    }


    private void ScenicObjsStageNewTerrain()
    {
        //creates the trees and candy patches using functions defined in the first_terrain_script
        //   keeps the references to these new objects in the curr_terr_obj.tree_objects and curr_terr_obj.candy_patches
        first_terrain_script.PlantTrees((curr_terr_obj.z_pos / 500),
                                        curr_terr_obj.terrain_data,
                                        ref curr_terr_obj.tree_objects);
        first_terrain_script.PlantCandy((curr_terr_obj.z_pos / 500),
                                        curr_terr_obj.terrain_data,
                                        ref curr_terr_obj.candy_patches);

        //creates a delete collider object and sets the position if the terrain is not the first created terrain
        if(curr_terr_obj.z_pos != 500)
        {
            curr_terr_obj.collider_delete = Instantiate(collider_delete_terrain,
                                                        this.transform.position,
                                                        Quaternion.identity);
            curr_terr_obj.collider_delete.transform.position = new Vector3(280.0f,
                                                                           10.0f,
                                                                           (float)delete_collider_offset + (curr_terr_obj.z_pos));
        }
        //creates a create collider object and sets the position
        curr_terr_obj.collider_create = Instantiate(collider_create_terrain,
                                                    this.transform.position,
                                                    Quaternion.identity);
        curr_terr_obj.collider_create.transform.position = new Vector3(280.0f,
                                                                       10.0f,
                                                                       (float)create_collider_offset + (curr_terr_obj.z_pos));
        //sets the new stage to the next phase of the terrain creation process
        curr_stage = DevStage.AddEnemies;
    }
    private void SpawnEnemiesNewTerrain()
    {
        //creates the enemy spawner characters via the SpawnSpawners function defined in the first_terrain_script
        first_terrain_script.SpawnSpawners(curr_terr_obj.terrain_data,
                                           (curr_terr_obj.z_pos / 500),
                                           3,
                                           enemy_references.GetSpawnerNumber());
        //sets the new stage of the last phase of the terrain creation process
        curr_stage = DevStage.Finished;
    }
    private void FinishedStageNewTerrain()
    {
        //changes the curr_stage to blank and adds the reference of the new terrain_object to curr_terrains
        curr_stage = DevStage.Blank;
        curr_terrains.Add(curr_terr_obj);
    }

    private void DeleteObjectReferences(int terr_id)
    {
        //takes the parameter to delete the tree and candy patch objects of the curr_terrains[terr_id]
        foreach (GameObject patch in curr_terrains[terr_id].candy_patches)
            Destroy(patch);
        foreach (GameObject tr in curr_terrains[terr_id].tree_objects)
            Destroy(tr);

        //destroys the terrain object referenced by curr_terrains[terr_id] and removes the reference from curr_terrains
        Destroy(curr_terrains[terr_id].terrain);
        curr_terrains.RemoveAt(terr_id);
    }



//***************************************************
//The rest of the methods are the Coroutines 
//   for Hills, Mountains, Coast, and Textures 
//   for the new terrains. They are copies of their
//   counterparts from the FirstTerrain_Script, with
//   the exception that a "yield return null;" is
//   added to break up the loops across multiple 
//   frames. For documentation of these methods,
//   please see the versions of these methods in
//   the FirstTerrain_Script file.
//These methods were the absolute last addition to
//   this project before being considered finished.
/****************************************************

    private IEnumerator HillsRoutine(int terr_num, TerrainData t_data)
    {
        float[,] height_map = new float[t_data.heightmapWidth,
                                        t_data.heightmapHeight];

        float persistance = first_terrain_script.hills_depth * 10.0f;
        float x_scale = first_terrain_script.hills_scale_width / 10.0f;
        float z_scale = first_terrain_script.hills_scale_length / 10.0f;
        float height_scale = first_terrain_script.hills_scale_height / 100.0f;

        float offset = 512.0f * terr_num;

        for (int x = 0; x < t_data.heightmapWidth; x++)
        {
            for (int z = 0; z < t_data.heightmapHeight; z++)
            {
                if (!(height_map[x, z] > 0.0f))
                {
                    float total = 0.0f;
                    float max = 0.0f;
                    float freq = 1.0f;
                    float amp = 1.0f;

                    for (int i = 0; i < first_terrain_script.hills_roughness; i++)
                    {
                        float x_val = (x + offset) * x_scale * freq;
                        float z_val = z * z_scale * freq;

                        total += amp * Mathf.PerlinNoise(x_val, z_val);

                        max += amp;
                        amp += persistance;
                        freq *= 2.0f;
                    }
                    height_map[x, z] += (total / max) * height_scale;
                }
            }
            if (x % 50 == 0)
                yield return null;
        }

        t_data.SetHeights(0, 0, height_map);
    }
    private IEnumerator MountainsRoutine(TerrainData t_data)
    {
        float[,] height_map = t_data.GetHeights(0,
                                                0,
                                                t_data.heightmapWidth,
                                                t_data.heightmapHeight);

        float width_max = first_terrain_script.mnts_width_max * 50.0f;
        float width_min = first_terrain_script.mnts_width_min * 50.0f;
        float width_half = width_min / 2.0f;
        float distance_covered = width_max;

        while (distance_covered < (t_data.heightmapWidth - (int)width_max))
        {
            float width = UnityEngine.Random.Range(width_min, width_max);
            Vector3 peak = new Vector3(UnityEngine.Random.Range((distance_covered - width_half),
                                                                (distance_covered + width_min)),
                                       UnityEngine.Random.Range(first_terrain_script.mnts_height_min,
                                                                first_terrain_script.mnts_height_max),
                                       UnityEngine.Random.Range(0.0f, (width_max / 3.0f)));

            float slope = peak.y / width;
            height_map[(int)peak.x, (int)peak.z] = peak.y;

            distance_covered = peak.x + width;

            for (int x = (int)(distance_covered - (2 * width)); x < distance_covered; x++)
            {
                if (x >= 0 && x < t_data.heightmapWidth)
                {
                    for (int z = 0; z < (int)(1.5f * width_max); z++)
                    {
                        if (!(x == (int)peak.x && z == (int)peak.z))
                        {
                            float radius = Vector2.Distance(new Vector2(peak.x, peak.z),
                                                            new Vector2(x, z));

                            float height = peak.y - (slope * radius);

                            if (height_map[x, z] < height)
                                height_map[x, z] = height;
                        }
                    }
                }
            }
            yield return null;
        }

        Vector3 peak_first = new Vector3(0, first_terrain_script.mnts_height_min, 0.0f);
        Vector3 peak_last = new Vector3(t_data.heightmapWidth, first_terrain_script.mnts_height_min, 0.0f);

        float slope_other = first_terrain_script.mnts_height_min / width_max;
        int z_width = (int)(1.3f * width_max);
        int x_width = (int)(t_data.heightmapWidth - width_max);

        for (int x = 0; x < t_data.heightmapWidth; x++)
        {
            for (int z = 0; z < z_width; z++)
            {
                if (x >= width_max &&
                   x < x_width)
                {
                    continue;
                }
                else if (!(x == (int)peak_first.x && z == (int)peak_first.z) &&
                         !(x == (int)peak_last.x && z == (int)peak_last.z))
                {
                    float radius;
                    if (x >= x_width)
                        radius = Vector2.Distance(new Vector2(peak_last.x, peak_last.z), new Vector2(x, z));
                    else
                        radius = Vector2.Distance(new Vector2(peak_first.x, peak_first.z), new Vector2(x, z));

                    float height = peak_last.y - (slope_other * radius);

                    if (height_map[x, z] < height)
                        height_map[x, z] = height;
                }
            }
            if (x % 50 == 0)
                yield return null;
        }

        t_data.SetHeights(0, 0, height_map);
    }
    private IEnumerator TexturesRoutine(TerrainData t_data)
    {
        SplatPrototype[] protos = new SplatPrototype[first_terrain_script.texture_maps.Count];
        for (int i = 0; i < first_terrain_script.texture_maps.Count; i++)
        {
            protos[i] = new SplatPrototype();
            protos[i].texture = first_terrain_script.texture_maps[i].texture;
            protos[i].tileSize = new Vector2(first_terrain_script.texture_maps[i].tile_size,
                                             first_terrain_script.texture_maps[i].tile_size);
            protos[i].tileOffset = new Vector2(0.0f, 0.0f);
            protos[i].texture.Apply(true);
        }
        t_data.splatPrototypes = protos;

        float[,] height_map = t_data.GetHeights(0,
                                                0,
                                                t_data.heightmapWidth,
                                                t_data.heightmapHeight);
        float[,,] splat_data = new float[t_data.alphamapWidth,
                                         t_data.alphamapHeight,
                                         t_data.alphamapLayers];

        for (int x = 0; x < t_data.alphamapWidth; x++)
        {
            for (int z = 0; z < t_data.alphamapHeight; z++)
            {
                float[] alphas = new float[t_data.alphamapLayers];

                for (int i = 0; i < first_terrain_script.texture_maps.Count; i++)
                    if ((height_map[x, z] >= first_terrain_script.texture_maps[i].height_min) &&
                        (height_map[x, z] <= first_terrain_script.texture_maps[i].height_max))
                        alphas[i] = 1.0f;

                int count = 0;
                for (int i = 0; i < alphas.Length; i++)
                    if (alphas[i] > 0.1f)
                        count++;
                for (int i = 0; i < alphas.Length; i++)
                    alphas[i] /= count;

                for (int i = 0; i < first_terrain_script.texture_maps.Count; i++)
                    splat_data[x, z, i] = alphas[i];
            }
            if (x % 50 == 0)
                yield return null;
        }
        t_data.SetAlphamaps(0, 0, splat_data);
    }
    private IEnumerator CoastRoutine(TerrainData t_data)
    {
        float[,] height_map = t_data.GetHeights(0,
                                                0,
                                                t_data.heightmapWidth,
                                                t_data.heightmapHeight);

        for (int x = 0; x < t_data.heightmapWidth; x++)
            for (int z = 0; z < t_data.heightmapHeight; z++)
                if (z < (first_terrain_script.coast_inland_range + 10))
                    height_map[x, z] += 0.005f;
                else if (z % 5 == 0)
                    height_map[x, z] = -0.1f;

        for (int x = 0; x < t_data.heightmapWidth; x++)
        {
            for (int z = first_terrain_script.coast_inland_range; z < t_data.heightmapHeight; z++)
            {
                List<Vector2> neighbors = new List<Vector2>();
                float avg_height = height_map[x, z];
                for (int row = -1; row < 2; row++)
                {
                    for (int col = -1; col < 2; col++)
                    {
                        Vector2 pos = new Vector2(Mathf.Clamp(x + row, 0, t_data.heightmapWidth - 1),
                                                  Mathf.Clamp(z + col, 0, t_data.heightmapHeight - 1));
                        if (!neighbors.Contains(pos))
                        {
                            neighbors.Add(pos);
                            avg_height += height_map[(int)pos.x, (int)pos.y];
                        }
                    }
                }
                height_map[x, z] = ((float)avg_height) / (((float)neighbors.Count) + 1.0f);
            }
            if (x % 50 == 0)
                yield return null;
        }
        t_data.SetHeights(0, 0, height_map);
    }
}
