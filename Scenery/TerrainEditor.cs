//Written by Eric Hollas
//
//This script corresponds to the Terrain_Script. Through the Inspector GUI for Unity
//   this script initializes the variables and calls the functions to set the values
//   of the terrain object associated with the corresponding Terrain_Script.
//


using UnityEngine;
using UnityEditor;
using EditorGUITable;

[CustomEditor(typeof(Terrain_Script))]
[CanEditMultipleObjects]

public class TerrainEditor : Editor
{
    private Vector2 scroll_pos;

    //for the foldouts in the GUI
    private bool show_hills         = false;
    private bool show_mountains     = false;
    private bool show_splatmaps     = false;
    private bool show_forest        = false;
    private bool show_candy_patches = false;
    private bool show_water         = false;
    private bool show_enemy         = false;

    //the serialized properties correspond to variables in the Terrain_Script and
    //   and will be initialized in OnEnable()

    SerializedProperty hills_scale_length;
    SerializedProperty hills_scale_width;
    SerializedProperty hills_scale_height;
    SerializedProperty hills_depth;
    SerializedProperty hills_roughness;

    SerializedProperty mnts_height_min;
    SerializedProperty mnts_height_max;
    SerializedProperty mnts_width_min;
    SerializedProperty mnts_width_max;

    GUITableState texture_table;
    SerializedProperty texture_table_props;

    GUITableState forest_table;
    SerializedProperty forest_table_props;
    SerializedProperty forest_height_min;
    SerializedProperty forest_height_max;
    SerializedProperty forest_height_scale;
    SerializedProperty forest_first_level;

    SerializedProperty water_coast_range;

    SerializedProperty candy_object;
    SerializedProperty candy_num_patches;

    SerializedProperty enemy;
    SerializedProperty enemy_num;


    public void OnEnable()
    {
        //initializes the local variables to this script and match them to 
        //   their counterparts in Terrain_Script via serializedObject.FindProperty()

        hills_scale_length  = serializedObject.FindProperty("hills_scale_length");
        hills_scale_width   = serializedObject.FindProperty("hills_scale_width");
        hills_scale_height  = serializedObject.FindProperty("hills_scale_height");
        hills_depth         = serializedObject.FindProperty("hills_depth");
        hills_roughness     = serializedObject.FindProperty("hills_roughness");

        mnts_height_min     = serializedObject.FindProperty("mnts_height_min");
        mnts_height_max     = serializedObject.FindProperty("mnts_height_max");
        mnts_width_min      = serializedObject.FindProperty("mnts_width_min");
        mnts_width_max      = serializedObject.FindProperty("mnts_width_max");

        texture_table       = new GUITableState("texture_table");
        texture_table_props = serializedObject.FindProperty("texture_maps");

        forest_table        = new GUITableState("forest_table");
        forest_table_props  = serializedObject.FindProperty("forest");
        forest_height_min   = serializedObject.FindProperty("forest_height_min");
        forest_height_max   = serializedObject.FindProperty("forest_height_max");
        forest_height_scale = serializedObject.FindProperty("forest_height_scale");
        forest_first_level  = serializedObject.FindProperty("candy_tree_height");

        water_coast_range   = serializedObject.FindProperty("coast_inland_range");

        candy_object        = serializedObject.FindProperty("candycane_patch");
        candy_num_patches   = serializedObject.FindProperty("candy_num_patches");

        enemy               = serializedObject.FindProperty("enemy");
        enemy_num           = serializedObject.FindProperty("num_enemies");
    }
    public override void OnInspectorGUI()
    {
        //the functions called within the foldout if’s correspond to the Terrain_Script


        //initializes the InspectorGUI

        serializedObject.Update();
        Terrain_Script terrain_script = (Terrain_Script)target;
        Rect rt = EditorGUILayout.BeginVertical();
        scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos,
                                                     GUILayout.Width(rt.width),
                                                     GUILayout.Height(rt.height));
        EditorGUI.indentLevel++;

        //the hills settings and function
        show_hills = EditorGUILayout.Foldout(show_hills, "Hills");
        if(show_hills)
        {
            GUILayout.Label("Hills", EditorStyles.boldLabel);
            EditorGUILayout.Slider(hills_scale_length, 0.0f, 1.0f, new GUIContent("Hill Lengths"));
            EditorGUILayout.Slider(hills_scale_width, 0.0f, 1.0f, new GUIContent("Hill Widths"));
            EditorGUILayout.Slider(hills_scale_height, 0.0f, 1.0f, new GUIContent("Hill Heights"));
            EditorGUILayout.Slider(hills_depth, 0.0f, 1.0f, new GUIContent("Hill Depths"));
            EditorGUILayout.IntSlider(hills_roughness, 0, 10, new GUIContent("Hill Roughness"));

            if(GUILayout.Button("Apply Hills"))
            {
                terrain_script.ApplyHills();
            }
        }

        //the mountains settings and function
        show_mountains = EditorGUILayout.Foldout(show_mountains, "Mountains");
        if(show_mountains)
        {
            GUILayout.Label("Mountains", EditorStyles.boldLabel);
            EditorGUILayout.Slider(mnts_height_min, 0.0f, 1.0f, new GUIContent("Mountain Min Height"));
            EditorGUILayout.Slider(mnts_height_max, 0.0f, 1.0f, new GUIContent("Mountain Max Height"));
            EditorGUILayout.Slider(mnts_width_min, 0.0f, 1.0f, new GUIContent("Mountain Min Width"));
            EditorGUILayout.Slider(mnts_width_max, 0.0f, 1.0f, new GUIContent("Mountain Max Width"));


            if(GUILayout.Button("Apply Mountians"))
            {
                terrain_script.ApplyMountains();
            }
        }

        //the textures settings and functions
        show_splatmaps = EditorGUILayout.Foldout(show_splatmaps, "Textures");
        if(show_splatmaps)
        {
            GUILayout.Label("Textures", EditorStyles.boldLabel);
            texture_table = GUITableLayout.DrawTable(texture_table, serializedObject.FindProperty("texture_maps"));
            GUILayout.Space(20.0f);

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add Texture"))
            {
                terrain_script.AddTexture();
            }
            if(GUILayout.Button("Remove Texture(s)"))
            {
                terrain_script.RemoveTextures();
            }
            EditorGUILayout.EndHorizontal();

            if(GUILayout.Button("Apply Textures"))
            {
                terrain_script.ApplyTextures();
            }
        }

        //the trees settings and functions
        show_forest = EditorGUILayout.Foldout(show_forest, "Forest");
        if(show_forest)
        {
            GUILayout.Label("Forest", EditorStyles.boldLabel);
            EditorGUILayout.Slider(forest_height_min, 0.0f, 1.0f, new GUIContent("Min Height On Map"));
            EditorGUILayout.Slider(forest_height_max, 0.0f, 1.0f, new GUIContent("Max Height On Map"));
            EditorGUILayout.Slider(forest_height_scale, 0.5f, 4.0f, new GUIContent("Scale Height of Trees"));
            EditorGUILayout.IntSlider(forest_first_level, 20, 200, new GUIContent("First Level Trees Range"));
            forest_table = GUITableLayout.DrawTable(forest_table, serializedObject.FindProperty("forest"));
            GUILayout.Space(20.0f);

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add Tree Type"))
            {
                terrain_script.AddTree();
            }
            if(GUILayout.Button("Remove Tree Type(s)"))
            {
                terrain_script.RemoveTrees();
            }
            EditorGUILayout.EndHorizontal();

            if(GUILayout.Button("Apply Tree(s)"))
            {
                terrain_script.ApplyTrees();
            }
            if(GUILayout.Button("Remove Forest"))
            {
                terrain_script.DeleteTreeObjects();
            }
        }

        //the coast settings and function
        show_water = EditorGUILayout.Foldout(show_water, "Water");
        if(show_water)
        {
            GUILayout.Label("Water", EditorStyles.boldLabel);
            EditorGUILayout.IntSlider(water_coast_range, 300, 513, new GUIContent("Range Inland to Smooth Coast"));

            if (GUILayout.Button("Smooth Coast"))
            {
                terrain_script.SmoothCoast();
            }
        }

        //the candy patches settings and functions
        show_candy_patches = EditorGUILayout.Foldout(show_candy_patches, "Candy Cane Patches");
        if(show_candy_patches)
        {
            GUILayout.Label("Candy Cane Patches", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(candy_object, new GUIContent("Candy Cane Patch Prefab"));
            EditorGUILayout.IntSlider(candy_num_patches, 1, 100, new GUIContent("Number of Patches"));

            if(GUILayout.Button("Generate Patches"))
            {
                terrain_script.ApplyCandy();
            }
            if(GUILayout.Button("Remove Patches"))
            {
                terrain_script.DeleteCandy();
            }
        }

        //the enemies settings and function
        show_enemy = EditorGUILayout.Foldout(show_enemy, "Enemy GameObjects");
        if(show_enemy)
        {
            GUILayout.Label("Enemy Spawners", EditorStyles.boldLabel);
            EditorGUILayout.ObjectField(enemy, new GUIContent("Enemy Spawner Prefab"));
            EditorGUILayout.IntSlider(enemy_num, 1, 5, new GUIContent("Number of Spawners"));

            if(GUILayout.Button("Spawn Enemy Spawners"))
            {
                terrain_script.SpawnEnemySpawners();
            }
        }


        //the function to reset the terrain’s height map
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        if(GUILayout.Button("Reset Terrain"))
        {
            terrain_script.ResetTerrain();
        }


        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        serializedObject.ApplyModifiedProperties();
    }

}