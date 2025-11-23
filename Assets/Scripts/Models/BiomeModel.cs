// -------------------------------------------------- //
// Scripts/Models/BiomeModel.cs
// -------------------------------------------------- //

[System.Serializable]
public class BiomeModel
{
    public string Name;
    public int StartLevel;
    public int EndLevel;
    
    public string FloorPrefabPath;
    public string WallPrefabPath;
    public string DoorPrefabPath;
    public string DoorTopPrefabPath;

    public BiomeModel(string name, int startLevel, int endLevel)
    {
        Name = name;
        StartLevel = startLevel;
        EndLevel = endLevel;

        FloorPrefabPath = $"Biomes/{name}/FloorPrefab";
        WallPrefabPath = $"Biomes/{name}/WallPrefab";
        DoorPrefabPath = $"Biomes/{name}/DoorPrefab";
        DoorTopPrefabPath = $"Biomes/{name}/DoorTopPrefab";
    }
}