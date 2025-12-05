// -------------------------------------------------- //
// Scripts/Models/BiomeModel.cs
// -------------------------------------------------- //

[System.Serializable]
public class BiomeModel
{
    public string Name;
    public int StartLevel;
    public int EndLevel;

    public BiomeModel(string name, int startLevel, int endLevel)
    {
        Name = name;
        StartLevel = startLevel;
        EndLevel = endLevel;
    }
}