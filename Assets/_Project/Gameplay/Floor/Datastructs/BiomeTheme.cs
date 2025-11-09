using UnityEngine;

[System.Serializable]
public class BiomeTheme
{
    public string Name;
    public int StartLevel;
    public int EndLevel;
    public float Weight = 1.0f; // For random selection between multiple themes in same level range
    
    // Prefab paths
    public string FloorPrefabPath;
    public string WallPrefabPath;
    public string DoorPrefabPath;
    public string DoorTopPrefabPath; // The half-wall above doors
    
    // Special room prefabs
    public string EntrancePrefabPath;
    public string ExitPrefabPath;
    public string ShopPrefabPath;
    public string TreasurePrefabPath;
    public string BossPrefabPath;
    
    // Material for tiling (optimization)
    public Material FloorMaterial;
    public Material WallMaterial;

    // Constructor for easy creation
    public BiomeTheme(string name, int startLevel, int endLevel, float weight = 1.0f)
    {
        Name = name;
        StartLevel = startLevel;
        EndLevel = endLevel;
        Weight = weight;
    }
}