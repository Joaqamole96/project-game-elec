// BiomeModel.cs

[System.Serializable]
public class BiomeModel
{
    public string Name;
    public int MinLevel;
    public int MaxLevel;
    public BiomeModel(string name, int minLevel, int maxLevel)
    {
        Name = name;
        MinLevel = minLevel;
        MaxLevel = maxLevel;
    }
}