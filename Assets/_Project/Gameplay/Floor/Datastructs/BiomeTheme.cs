// BiomeTheme.cs
using UnityEngine;

/// <summary>
/// Defines a biome theme with prefab paths and visual properties for dungeon generation.
/// </summary>
[System.Serializable]
public class BiomeTheme
{
    [Header("Basic Settings")]
    public string Name;
    public int StartLevel;
    public int EndLevel;
    
    [Tooltip("Weight for random selection between multiple themes in same level range")]
    public float Weight = 1.0f;
    
    [Header("Prefab Paths")]
    public string FloorPrefabPath;
    public string WallPrefabPath;
    public string DoorPrefabPath;
    public string DoorTopPrefabPath;
    
    [Header("Special Room Prefabs")]
    public string EntrancePrefabPath;
    public string ExitPrefabPath;
    public string ShopPrefabPath;
    public string TreasurePrefabPath;
    public string BossPrefabPath;
    
    [Header("Materials (Optimization)")]
    public Material FloorMaterial;
    public Material WallMaterial;

    /// <summary>
    /// Creates a new biome theme with the specified level range and weight.
    /// </summary>
    public BiomeTheme(string name, int startLevel, int endLevel, float weight = 1.0f)
    {
        Name = name;
        StartLevel = startLevel;
        EndLevel = endLevel;
        Weight = weight;
    }

    /// <summary>
    /// Checks if this theme is valid for the specified floor level.
    /// </summary>
    public bool IsValidForFloor(int floorLevel)
    {
        return floorLevel >= StartLevel && floorLevel <= EndLevel;
    }
}