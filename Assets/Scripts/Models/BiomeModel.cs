// BiomeModel.cs

[System.Serializable]
public class BiomeModel
{
    public string Name;
    public int MinLevel;
    public int MaxLevel;
    public float Weight;

    public BiomeModel(string name, int minLevel, int maxLevel, float weight = 1.0f)
    {
        Name = name;
        MinLevel = minLevel;
        MaxLevel = maxLevel;
        Weight = weight;
    }
}