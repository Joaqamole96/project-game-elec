// -------------------------------------------------- //
// Scripts/Models/BiomeModel.cs
// -------------------------------------------------- //

// using UnityEngine;

[System.Serializable]
public class BiomeModel
{
    public string Name;
    public int StartLevel;
    public int EndLevel;
    public float Weight = 1.0f;
    
    public string FloorPrefabPath;
    public string WallPrefabPath;
    public string DoorPrefabPath;
    public string DoorTopPrefabPath;
    
    // NOTE: Remove these in future implementation; these are Landmarks which are global.
    public string EntrancePrefabPath;
    public string ExitPrefabPath;
    public string ShopPrefabPath;
    public string TreasurePrefabPath;
    public string BossPrefabPath;
    
    // public Material FloorMaterial;
    // public Material WallMaterial;

    public BiomeModel(string name, int startLevel, int endLevel, float weight = 1.0f)
    {
        Name = name;
        StartLevel = startLevel;
        EndLevel = endLevel;
        Weight = weight;
    }
    
    // public bool IsValidForFloor(int floorLevel) => floorLevel >= StartLevel && floorLevel <= EndLevel;
}