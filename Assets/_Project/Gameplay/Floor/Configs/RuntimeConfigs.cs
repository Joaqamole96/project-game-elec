using UnityEngine;

// Simple container for runtime config copies
public class RuntimeConfigs
{
    public GameConfig GameConfig { get; private set; }
    public LevelConfig LevelConfig { get; private set; }
    public PartitionConfig PartitionConfig { get; private set; }
    public RoomConfig RoomConfig { get; private set; }

    public RuntimeConfigs(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig, RoomConfig roomConfig)
    {
        // Create deep copies of the configs for runtime use
        GameConfig = CreateCopy(gameConfig);
        LevelConfig = CreateCopy(levelConfig);
        PartitionConfig = CreateCopy(partitionConfig);
        RoomConfig = CreateCopy(roomConfig);
        
        Debug.Log($"Runtime configs created - Level: {LevelConfig.Width}x{LevelConfig.Height}, Floor: {LevelConfig.FloorLevel}");
    }

    private T CreateCopy<T>(T original) where T : class, new()
    {
        if (original == null) return new T();
        
        // Simple manual copy - you could use reflection or other methods for more complex objects
        var copy = new T();
        
        // For now, we'll handle LevelConfig specifically since it's the main issue
        if (original is LevelConfig originalLevel && copy is LevelConfig copyLevel)
        {
            copyLevel.Seed = originalLevel.Seed;
            copyLevel.FloorLevel = originalLevel.FloorLevel;
            copyLevel.Width = originalLevel.Width;
            copyLevel.Height = originalLevel.Height;
            copyLevel.FloorGrowth = originalLevel.FloorGrowth;
            copyLevel.MinFloorSize = originalLevel.MinFloorSize;
            copyLevel.MaxFloorSize = originalLevel.MaxFloorSize;
        }
        // Add similar copying for other config types if needed
        
        return copy;
    }
}