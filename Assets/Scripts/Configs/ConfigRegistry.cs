// -------------------- //
// Scripts/Configs/ConfigRegistry.cs
// -------------------- //

using UnityEngine;

// NOTE: Starting to doubt the necessity of a ConfigRegistry. 
// It was initially made so that the Configs assigned to 
// DungeonManager would not reset whenever Generate() is run. 
// I am unsure if it still fulfills that function.
public class ConfigRegistry
{
    [Header("Configuration List")]
    public GameConfig GameConfig = new();
    public LevelConfig LevelConfig = new();
    public PartitionConfig PartitionConfig = new();

    // -------------------------------------------------- //

    public ConfigRegistry(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig)
    {
        SaveConfigs(gameConfig, levelConfig, partitionConfig);
        Debug.Log("ConfigRegistry(): Constructed.");
    }

    // -------------------------------------------------- //

    public void SaveConfigs(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig)
    {
        GameConfig = CloneConfig(gameConfig);
        LevelConfig = CloneConfig(levelConfig);
        PartitionConfig = CloneConfig(partitionConfig);

        ValidateConfigs();

        Debug.Log("ConfigRegistry.SaveConfigs(): Saved configs.");
    }

    private T CloneConfig<T>(T original) where T : class, new()
    {
        if (original is ICloneable<T> cloneable)
        {
            Debug.Log($"ConfigRegistry.CloneConfig(): Cloned existing config.");
            return cloneable.Clone();
        }
        else
        {
            Debug.Log($"ConfigRegistry.CloneConfig(): Cloned new config.");
            return new T();
        }
    }

    private void ValidateConfigs()
    {
        GameConfig.Validate();
        LevelConfig.Validate();
        PartitionConfig.Validate();

        Debug.Log($"ConfigRegistry.ValidateConfigs(): Validated configs.");
    }
}