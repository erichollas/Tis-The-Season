//Written by Eric Hollas
//
//Sets the values through the TerrainEditor.cs
//Initializes the values to sculpt the terrain and attach
//   the tree, candy patch, and enemy objects. The values
//   for this script are intended to exist within one object’s
//   script. Then the continuous terrain object will reference
//   that object’s script for its values. That way the other
//   terrain objects will match are seamlessly flow from one
//   to the other.
//


using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]

public class Terrain_Script : MonoBehaviour
{
    public enum TagType
    {
        tag, 
        layer
    }
    [System.Serializable]
    public class splatmaps
    {
        public Texture2D texture    = null;
        public bool remove          = false;
        public int tile_size        = 10;
        public float height_min     = 0.0f;
        public float height_max     = 0.0f;
    }
    [System.Serializable]
    public class trees
    {
        public GameObject mesh      = null;
        public bool remove          = false;
        public float scale_min      = 0.0f;
        public float scale_max      = 1.0f;
    }

    public Terrain terrain;
    public TerrainData terrain_data;

    public float hills_scale_length;
    public float hills_scale_width;
    public float hills_scale_height;
    public float hills_depth;
    public int hills_roughness;
   
    public float mnts_height_min;
    public float mnts_height_max;
    public float mnts_width_min;
    public float mnts_width_max;


    public float forest_height_min;
    public float forest_height_max;
    public float forest_height_scale;

    public int coast_inland_range;

    public GameObject candycane_patch;
    public int candy_num_patches;
    public int candy_tree_width;
    public int candy_tree_height;
    public int candy_coast_width;

    public GameObject enemy;
    public int num_enemies;


    public List<splatmaps> texture_maps = new List<splatmaps>();
    public List<trees> forest = new List<trees>();
    public List<GameObject> tree_objects = new List<GameObject>();
    public List<GameObject> candy_objects = new List<GameObject>();


    public void OnEnable()
    {
        terrain = this.GetComponent<Terrain>();
        terrain_data = Terrain.activeTerrain.terrainData;
    }

    [SerializeField]
    public int terrain_layer = -1;
    public void Awake()
    {
        //this function sets the tag and layer properties and the terrain_layer variable for the terrain

        SerializedObject tagger = 
            new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tag_props = tagger.FindProperty("tags");

        AddTag(tag_props, "Terrain", TagType.tag);
        tagger.ApplyModifiedProperties();

        SerializedProperty layer_props = tagger.FindProperty("layers");
        terrain_layer = AddTag(layer_props, "Terrain", TagType.layer);
        tagger.ApplyModifiedProperties();

        this.gameObject.tag = "Terrain";
        this.gameObject.layer = terrain_layer;
    }

    private int AddTag(SerializedProperty props,
                       string label,
                       TagType typ)
    {
        //finds the terrain object’s layer and returns the int reference

        for (int i = 0; i < props.arraySize; i++)
            if (props.GetArrayElementAtIndex(i).stringValue.Equals(label))
                return i;

        if (typ == TagType.layer)
        {
            for (int i = 8; i < props.arraySize; i++)
            {
                if (props.GetArrayElementAtIndex(i).stringValue == "")
                {
                    props.GetArrayElementAtIndex(i).stringValue = label;
                    return i;
                }
            }
        }

        props.InsertArrayElementAtIndex(0);
        props.GetArrayElementAtIndex(0).stringValue = label;

        return 0;
    }





    public void ApplyHills()
    {
        //sets the height map for hills for this terrain object 
        terrain_data.SetHeights(0, 0, ShapeHills(0, terrain_data));
    }
    public void ApplyMountains()
    {
        //sets the height map for mountains for this terrain object 
        terrain_data.SetHeights(0, 0, ShapeMountains(terrain_data));
    }
    public void AddTexture()
    {
        //adds another place in the GUI table for another texture
        texture_maps.Add(new splatmaps());
    }
    public void RemoveTextures()
    {
        //in the InspectorGUI deletes from the GUI table all of the
        //    textures with the remove check box ticked, unless it
        //    is the only texture left
        List<splatmaps> keepers = new List<splatmaps>();

        foreach (splatmaps map in texture_maps)
            if (!map.remove)
                keepers.Add(map);

        if (keepers.Count == 0)
            keepers.Add(texture_maps[0]);

        texture_maps = keepers;
    }
    public void ApplyTextures()
    {
        //sets the alpha map for textures for this terrain object 
        terrain_data.SetAlphamaps(0, 0, CalcSplatmaps(terrain_data));
    }
    public void AddTree()
    {
        //adds another place in the GUI table for another tree object
        //    there will be four for this project as I only have 4 
        //    different tree objects
        forest.Add(new trees());
    }
    public void RemoveTrees()
    {
        //in the InspectorGUI deletes from the GUI table all of the
        //    tree objects with the remove check box ticked, unless it
        //    is the only tree object left
        List<trees> keepers = new List<trees>();

        foreach (trees t in forest)
            if (!t.remove)
                keepers.Add(t);

        if (keepers.Count == 0)
            keepers.Add(forest[0]);

        forest = keepers;
    }
    public void ApplyTrees()
    {
        
        //deletes the tree objects referenced by tree_objects (the trees 
        //    already placed in the game)
        if (tree_objects.Count != 0)
            DeleteTreeObjects();

        //places the tree objects in the game and holds their references
        //    in the tree_objects List<GameObject>
        PlantTrees(0, terrain_data, ref tree_objects);

        //places the tree objects that create the barrier so the player 
        //    cannot go in the negative z direction. and then adds the
        //    references to tree_objects.
        List<GameObject> barr_trees = new List<GameObject>();
        ApplyBarrierTrees(terrain_data, 0, ref barr_trees);
        foreach (GameObject tr in barr_trees)
            tree_objects.Add(tr);
    }
    public void DeleteTreeObjects()
    {
        //Deletes the tree objects from the game that are referenced
        //    in the tree_objects List<GameObject>
        if (tree_objects.Count != 0)
        {
            foreach (GameObject tree in tree_objects)
                DestroyImmediate(tree, false);
            tree_objects.Clear();
        }
        else
        {
            Debug.Log("No trees to delete.");
        }
    }
    public void SmoothCoast()
    {
        //sets the height map to sculpt the coast for this terrain object 
        terrain_data.SetHeights(0, 0, ShapeCoast(terrain_data));
    }
    public void ApplyCandy()
    {
        //deletes the candy patch objects referenced by candy_objects (the candy patches 
        //    already placed in the game)
        if(candy_objects.Count != 0)
            DeleteCandy();

        //places the candy patches in the game and holds their references
        //    in the candy_objects List<GameObject>
        PlantCandy(0, terrain_data, ref candy_objects);
    }
    public void DeleteCandy()
    {
        //Deletes the candy patch objects from the game that are referenced
        //    in the candy_objects List<GameObject>
        if (candy_objects.Count != 0)
        {
            foreach (GameObject patch in candy_objects)
                DestroyImmediate(patch, false);
            candy_objects.Clear();
        }
        else
        {
            Debug.Log("No candy to be deleted.");
        }
    }
    public void SpawnEnemySpawners()
    {
        //places EnemySpawners in the game
        SpawnSpawners(terrain_data, 0, num_enemies, 0);
    }
    public void ResetTerrain()
    {
        //resets the height map for the terrain object
        float[,] height_map = new float[terrain_data.heightmapWidth, terrain_data.heightmapHeight];
        terrain_data.SetHeights(0, 0, height_map);
    }



    public float[,] ShapeHills(int terrain_number, TerrainData t_data)
    {
        float[,] height_map = new float[t_data.heightmapWidth, t_data.heightmapHeight];

        //these local variables scale the global variables so that the global variables
        //   will have values between 0.0 and 1.0
        float persistance = hills_depth * 10.0f;
        float x_scale = hills_scale_width / 10.0f;
        float z_scale = hills_scale_length / 10.0f;
        float height_scale = hills_scale_height / 100.0f;

        //this will set the position int z position
        float offset = 512.0f * terrain_number;

        //creates hills based on brownian motion
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

                    for (int i = 0; i < hills_roughness; i++)
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
        }

        return height_map;
    }

    public float[,] ShapeMountains(TerrainData t_data)
    {
        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);

        //these local variables scale the global variables so that the global variables
        //   will have values between 0.0 and 1.0
        float width_max = mnts_width_max * 50.0f;
        float width_min = mnts_width_min * 50.0f;
        float width_half = width_min / 2.0f;

        //distance_covered will be used to make sure that the entire side is covered with mountains
        float distance_covered = width_max;

        //while-loop pans over the entire side of the terrain to add mountains
        while (distance_covered < (t_data.heightmapWidth - (int)width_max))
        {
            //width and peak are random values, the peak sets the center of the mountain
            float width = UnityEngine.Random.Range(width_min, width_max);
            Vector3 peak = new Vector3(UnityEngine.Random.Range((distance_covered - width_half),
                                                                (distance_covered + width_min)),
                                       UnityEngine.Random.Range(mnts_height_min,
                                                                mnts_height_max),
                                       UnityEngine.Random.Range(0.0f, (width_max / 3.0f)));

            //sets the slope of the mountain
            float slope = peak.y / width;
            height_map[(int)peak.x, (int)peak.z] = peak.y;

            //increments the distance_covered
            distance_covered = peak.x + width;

            //sets the heights of the mountain within a radius of width of the peak
            for (int x = (int)(distance_covered - (2 * width)); x < distance_covered; x++)
            {
                //this if ensures that x and z do not throw a range exception for the height_map array
                if (x >= 0 && x < t_data.heightmapWidth)
                {
                    for (int z = 0; z < (int)(1.5f * width_max); z++)
                    {
                        if (!(x == (int)peak.x && z == (int)peak.z))
                        {
                            float radius = Vector2.Distance(new Vector2(peak.x, peak.z),
                                                            new Vector2(x, z));
     
                            //calculates the height at the point (x, z)
                            float height = peak.y - (slope * radius);

                            //sets this height if the height of this mountain at this spot 
                            //    is higher than the height that is already set at (x, z) 
                            if (height_map[x, z] < height)
                                height_map[x, z] = height;
                        }
                    }
                }
            }

        }


        //the rest of this method sculpts the first and last mountain of the terrain that
        //   way the mountains for adjacent terrain objects will match

        //this is the same as above except without the randomness

        Vector3 peak_first = new Vector3(0, mnts_height_min, 0.0f);
        Vector3 peak_last = new Vector3(t_data.heightmapWidth, mnts_height_min, 0.0f);

        float slope_other = mnts_height_min / width_max;
        int z_width = (int)(1.3f * width_max);
        int x_width = (int)(t_data.heightmapWidth - width_max);

        for (int x = 0; x < t_data.heightmapWidth; x++)
        {
            for (int z = 0; z < z_width; z++)
            {
                if(x >= width_max &&
                   x < x_width)
                {
                    //if the loop goes farther than the radius of the mountain, then continue
                    continue;
                }
                else if (!(x == (int)peak_first.x && z == (int)peak_first.z) &&
                         !(x == (int)peak_last.x && z == (int)peak_last.z))
                {
                    float radius;
      
                    //this if-else statement is for the first or last mountain peak, 
                    //   that way we only one nested for-loop for both the first and last peaks
                    if(x >= x_width)
                        radius = Vector2.Distance(new Vector2(peak_last.x, peak_last.z), new Vector2(x, z));
                    else
                        radius = Vector2.Distance(new Vector2(peak_first.x, peak_first.z), new Vector2(x, z));


                    float height = peak_last.y - (slope_other * radius);

                    if (height_map[x, z] < height)
                        height_map[x, z] = height;
                }
            }
        }

        return height_map;
    }

    public float[,,] CalcSplatmaps(TerrainData t_data)
    {
        //initializes the splatPrototypes for the TerrainData
        SplatPrototype[] protos = new SplatPrototype[texture_maps.Count];
        for (int i = 0; i < texture_maps.Count; i++)
        {
            protos[i] = new SplatPrototype();
            protos[i].texture = texture_maps[i].texture;
            protos[i].tileSize = new Vector2(texture_maps[i].tile_size, texture_maps[i].tile_size);
            protos[i].tileOffset = new Vector2(0.0f, 0.0f);
            protos[i].texture.Apply(true);
        }
        t_data.splatPrototypes = protos;


        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);
        float[,,] splat_data = new float[t_data.alphamapWidth, t_data.alphamapHeight, t_data.alphamapLayers];

        //sets the values of the TerrainData's alphamap
        for (int x = 0; x < t_data.alphamapWidth; x++)
        {
            for (int z = 0; z < t_data.alphamapHeight; z++)
            {
                float[] alphas = new float[t_data.alphamapLayers];

                //determines which texture should be applied depending on the height map
                for (int i = 0; i < texture_maps.Count; i++)
                    if ((height_map[x, z] >= texture_maps[i].height_min) &&
                        (height_map[x, z] <= texture_maps[i].height_max))
                        alphas[i] = 1.0f;

                // following two for loops blend the values of all textures that do need to be applied
                int count = 0;
                for (int i = 0; i < alphas.Length; i++)
                    if (alphas[i] > 0.1f)
                        count++;
                for (int i = 0; i < alphas.Length; i++)
                    alphas[i] /= count;

                //sets the values of each texture at point(x, z)
                for (int i = 0; i < texture_maps.Count; i++)
                    splat_data[x, z, i] = alphas[i];
            }
        }

        return splat_data;
    }

    //for appropriate results, ShapeCoast() must be called after PlantTrees()
    public float[,] ShapeCoast(TerrainData t_data)
    {
        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);

        //since the value of height_map cannot be negative and change its appearance, in order to 
        //   show a coast we raise the entire terrain and lower parts of the terrain beyond coast_inland_range
        for (int x = 0; x < t_data.heightmapWidth; x++)
            for (int z = 0; z < t_data.heightmapHeight; z++)
                if(z < (coast_inland_range + 10))
                    height_map[x, z] += 0.005f;
                else if (z % 5 == 0)
                    height_map[x, z] = -0.1f;

        //from the coast that starts at z = coast_inland_range to the end of the terrain we use these
        //   for loops to smooth out the coast portion of the terrain
        for (int x = 0; x < t_data.heightmapWidth; x++)
        {
            for (int z = coast_inland_range; z < t_data.heightmapHeight; z++)
            {
                //average out the height at point (x, z) with its neighbors' heights

                //store the neighbors points in the List<Vector2>
                List<Vector2> neighbors = new List<Vector2>();
                float avg_height = height_map[x, z];
                for (int row = -1; row < 2; row++)
                {
                    for (int col = -1; col < 2; col++)
                    {
                        //use Mathf.Clamp to ensure that the values of the neighbors don't throw 
                        //   and out of range exception
                        Vector2 pos = new Vector2(Mathf.Clamp(x + row, 0, t_data.heightmapWidth - 1),
                                                  Mathf.Clamp(z + col, 0, t_data.heightmapHeight - 1));
                        if (!neighbors.Contains(pos))
                        {
                            //if this coordinate is not in the neighbors list, add it to the list and
                            // add the height to avg_height
                            neighbors.Add(pos);
                            avg_height += height_map[(int)pos.x, (int)pos.y];
                        }
                    }
                }
           
                //calculate the average height and change height_map[x, z] to the average, 
                //   add one to the denominator because we initialize avg_height = height[x, z]
                height_map[x, z] = ((float)avg_height) / (((float)neighbors.Count) + 1.0f);
            }

        }

        return height_map;
    }

    public void PlantTrees(int terrain_number, TerrainData t_data, ref List<GameObject> tree_list)
    {
        //terrain_number is used to calculate the z-position of the trees
        //t_data is used to match the trees to the correct height on the terrain
        //tree_list will be used to keep the references to the new trees that are added

        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);
        List<GameObject> tree_update = new List<GameObject>();

        //initializes the values so that the loop starts with a z_range where the mountains end and
        //   and uses size_tree and x_range so that the forest will be three trees thick
        int size_tree = 5;
        int x_range = t_data.heightmapWidth - (3 * size_tree);
        int z_range = (int)(mnts_width_max * 125);


        for (int x = size_tree; x < x_range; x += size_tree)
        {
            for (int z = size_tree; z < z_range; z += size_tree)
            {
                if ((height_map[x, z] >= forest_height_min) &&
                    (height_map[x, z] <= forest_height_max))
                {
                    Vector3 tree_pos = new Vector3(z, 10.0f, x);
                    tree_pos += this.transform.position;
                    tree_pos += new Vector3(0.0f, 0.0f, (terrain_number * 500.0f));

                    RaycastHit hit;
                    //ensures that the physics cast only works if the tree is hitting the terrain_layer
                    int layer_mask = 1 << terrain_layer;

                    if ((Physics.Raycast(tree_pos, Vector3.down, out hit, layer_mask)) ||
                        (Physics.Raycast(tree_pos, Vector3.up, out hit, layer_mask)))
                    {
                        //makes sure the trees are not placed up the mountain and only at the 
                        //   base of the mountain
                        if (hit.point.y < 5.0f)
                        {
                            //index picks a random tree from the different tree objects 
                            //   provided in the editor
                            int index = (int)UnityEngine.Random.Range(0.0f, forest.Count);

                            //initializes the tree objects and sets the trees to a random 
                            //   scale, width and height
                            GameObject inst;
                            inst = Instantiate(forest[index].mesh,
                                               hit.point,
                                               this.transform.rotation);
                            inst.tag = "tree";
                            float scale_lateral = UnityEngine.Random.Range(forest[index].scale_min,
                                                                           forest[index].scale_max);
                            float scale_vertical = UnityEngine.Random.Range(forest[index].scale_min *
                                                                            forest_height_scale,
                                                                            forest[index].scale_max *
                                                                            forest_height_scale);
                            inst.transform.localScale += new Vector3(scale_lateral,
                                                                     scale_vertical,
                                                                     scale_lateral);

                            //adjusts the size_tree variable based on the previous tree so that 
                            //   there are not gaps between trees
                            size_tree = (int)(5 * scale_lateral);

                            //adds the new tree to the local List<GameObject>, 
                            //   eventually tree_list = tree_update
                            tree_update.Add(inst);
                        }
                    }
                }
            }
        }

        //sets global variables so that the coast and barrier trees' positions make sense
        //   thus call PlantTrees() before calling ShapeCoast() and ApplyBarrierTrees()
        candy_coast_width = coast_inland_range - size_tree;
        candy_tree_width = z_range;

        tree_list = tree_update;
    }

    // some values used here are set in the PlantTrees() method, thus call this method after PlantTrees()
    public void ApplyBarrierTrees(TerrainData t_data, int terrrain_num, ref List<GameObject>tree_list)
    {
        //t_data is used to place the trees on the terrain at the correct height
        //terrain_num is used to calculate the offset of the z-position for each of the trees
        //tree_list keeps the references to each of the new trees that are added

        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);

        List<GameObject> veggies = new List<GameObject>();

        int size_tree = 5;

        //the z for loop goes from the forest of trees planted in PlantTrees() to the coast (but this method
        //    does not necessarily need to be called before ShapeCoast())
        //the x for loop ensures that this layer of trees is 3 trees deep
        for (int x = size_tree; x < candy_tree_height; x += size_tree)
        {
            for (int z = candy_tree_width; z < (candy_coast_width + 5); z += size_tree)
            {
                //calculates the position of the tree
                Vector3 tree_pos = new Vector3(z, 10.0f, x);
                tree_pos.z += terrrain_num * 500.0f;

                RaycastHit hit;
                //ensures that the physics cast only works if the tree is hitting the terrain_layer
                int layer_mask = 1 << terrain_layer;

                if ((Physics.Raycast(tree_pos, Vector3.down, out hit, layer_mask)) ||
                    (Physics.Raycast(tree_pos, Vector3.up, out hit, layer_mask)))
                {
                    //index picks a random tree from the different tree objects 
                    //   provided in the editor
                    int index = (int)UnityEngine.Random.Range(0.0f, forest.Count);

                    //initializes the tree objects and sets the trees to a random 
                    //   scale, width and height
                    GameObject inst;
                    inst = Instantiate(forest[index].mesh, hit.point, Quaternion.identity);
                    inst.tag = "tree";

                    float scale_lateral = UnityEngine.Random.Range(forest[index].scale_min,
                                                                   forest[index].scale_max);
                    float scale_vertical = UnityEngine.Random.Range(forest[index].scale_min * forest_height_scale,
                                                                    forest[index].scale_max * forest_height_scale);
                    inst.transform.localScale += new Vector3(scale_lateral,
                                                             scale_vertical,
                                                             scale_lateral);

                    //adjusts the size_tree variable based on the previous tree so that 
                    //   there are no gaps between trees
                    size_tree = (int)(5 * scale_lateral);

                    veggies.Add(inst);
                }
            }
        }

        tree_list = veggies;
    }

    public void PlantCandy(int terrain_number, TerrainData t_data, ref List<GameObject> patch_list)
    {
        //terrain_number is used to calculate the z-position of the candy patches
        //t_data is used to match the candy patches to the correct height on the terrain
        //patch_list will be used to keep the references to the new candy patches that are added

        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);

        List<GameObject> candy_list = new List<GameObject>();

        //x_step and z_step are used to create the spacing between candy patches
        int x_step = 3 * ((t_data.heightmapWidth - candy_tree_height) / candy_num_patches);
        int z_step = 6 * ((candy_coast_width - candy_tree_width) / candy_num_patches);
        
        for (int x = (candy_tree_height + (z_step / 4)); x < (t_data.heightmapWidth - x_step); x += x_step)
        {
            for (int z = (candy_tree_width + (z_step / 4)); z < (candy_coast_width - z_step); z += z_step)
            {
                //picks the position randomly between (z, x) and (z + z_step, x + x_step)
                Vector3 pos = new Vector3(UnityEngine.Random.Range(z, (z + z_step)),
                                          100.0f,
                                          UnityEngine.Random.Range(x, (x + x_step)));
                pos += this.transform.position;
                pos += new Vector3(0.0f, 0.0f, (terrain_number * 500.0f));

                RaycastHit hit;
                //ensures that the physics cast only works if the tree is hitting the terrain_layer
                int layer_mask = 1 << terrain_layer;

                if ((Physics.Raycast(pos, Vector3.down, out hit, layer_mask)) ||
                    (Physics.Raycast(pos, Vector3.up, out hit, layer_mask)))
                {
                    //initializes the candy patch game object and adds the reference to candy_list
                    GameObject inst;
                    inst = Instantiate(candycane_patch,
                                       hit.point,
                                       this.transform.rotation);
                    inst.tag = "candy";

                    inst.transform.localScale += new Vector3(0.0f, 3.0f, 0.0f);

                    Vector3 size = inst.GetComponent<Collider>().bounds.size;
                    inst.transform.position += new Vector3(0.0f, (size.y / 2.0f), 0.0f);

                    candy_list.Add(inst);
                }
            }
        }

        //keeps the references to the candy patch objects
        patch_list = candy_list;
    }

    public void SpawnSpawners(TerrainData t_data, int terrain_num, int num_spawns, int first_spawn_id)
    {
        //t_data is used to match the spawners to the correct height on the terrain
        //terrain_number is used to calculate the z-position of the spawners
        //num_spawns is the number of enemies to be spawned for this terrain
        //first_spawn_id is used to set the id's of the newly spawned enemies

        float[,] height_map = t_data.GetHeights(0, 0, t_data.heightmapWidth, t_data.heightmapHeight);

        //sets the local variables for the for-loop to run to and set the first spawner to 
        //   the middle of the terrain
        int num_insts = first_spawn_id + num_spawns;
        float x_coord = (t_data.heightmapHeight / 2.0f);
        float z_coord = (t_data.heightmapWidth / 2.0f) + (terrain_num * 500.0f);

        for (int id = first_spawn_id; id < num_insts; id++, z_coord += 90.0f)
        {
            Vector3 pos = new Vector3(x_coord, 100.0f, z_coord);

            //initialize a new spawner with a random orientation
            GameObject spawner = Instantiate(enemy, 
                                             this.transform.position, 
                                             Quaternion.identity);
            spawner.transform.RotateAround(spawner.transform.position,
                                           Vector3.up,
                                           UnityEngine.Random.Range(0.0f, 360.0f));

            RaycastHit hit;
            //ensures that the physics cast only works if the tree is hitting the terrain_layer
            int layer_mask = 1 << terrain_layer;

            //sets the height of the enemy spawner
            if((Physics.Raycast(pos, Vector3.down, out hit, layer_mask)) ||
               (Physics.Raycast(pos, Vector3.up, out hit, layer_mask)))
            {
                spawner.transform.position = hit.point;
            }
            else
            {
                pos.z += 15.0f;
                if ((Physics.Raycast(pos, Vector3.down, out hit, layer_mask)) ||
                    (Physics.Raycast(pos, Vector3.up, out hit, layer_mask)))
                {
                    spawner.transform.position = hit.point;
                }
            }

            //sets the id of the spawner
            spawner.GetComponent<EnemySpawner_Script>().SetID(id);
        }
    }

}