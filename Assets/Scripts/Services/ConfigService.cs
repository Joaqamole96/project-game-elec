// ConfigService.cs
using UnityEngine;

/// <summary>
/// Container for runtime copies of configuration objects to allow modification without affecting original assets.
/// </summary>
public class ConfigService
{
    public GameConfig GameConfig { get; private set; }
    public LevelConfig LevelConfig { get; private set; }
    public PartitionConfig PartitionConfig { get; private set; }
    public RoomConfig RoomConfig { get; private set; }

    /// <summary>
    /// Creates deep copies of all configuration objects for runtime use.
    /// </summary>
    public ConfigService(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig, RoomConfig roomConfig)
    {
        GameConfig = CreateConfigCopy(gameConfig);
        LevelConfig = CreateConfigCopy(levelConfig);
        PartitionConfig = CreateConfigCopy(partitionConfig);
        RoomConfig = CreateConfigCopy(roomConfig);
        
        ValidateAllConfigs();
        
        Debug.Log($"Runtime configs created - Level: {LevelConfig.Width}x{LevelConfig.Height}, Floor: {LevelConfig.FloorLevel}");
    }

    private T CreateConfigCopy<T>(T original) where T : class, new()
    {
        if (original == null)
        {
            Debug.LogWarning($"Null config provided, creating default instance for {typeof(T).Name}");
            return new T();
        }

        // Use clone method if available, otherwise create new instance
        if (original is IConfigCloneable cloneable)
        {
            return (T)cloneable.Clone();
        }

        // Fallback to manual copying for specific types
        return ManualConfigCopy(original);
    }

    private T ManualConfigCopy<T>(T original) where T : class, new()
    {
        var copy = new T();
        
        if (original is GameConfig originalGame && copy is GameConfig copyGame)
        {
            copyGame.SimplifyGeometry = originalGame.SimplifyGeometry;
            copyGame.EnemiesPerCombatRoom = originalGame.EnemiesPerCombatRoom;
            copyGame.TreasureRoomsPerFloor = originalGame.TreasureRoomsPerFloor;
            copyGame.ShopRoomsPerFloor = originalGame.ShopRoomsPerFloor;
        }
        else if (original is LevelConfig originalLevel && copy is LevelConfig copyLevel)
        {
            copyLevel.Seed = originalLevel.Seed;
            copyLevel.FloorLevel = originalLevel.FloorLevel;
            copyLevel.Width = originalLevel.Width;
            copyLevel.Height = originalLevel.Height;
            copyLevel.FloorGrowth = originalLevel.FloorGrowth;
            copyLevel.MinFloorSize = originalLevel.MinFloorSize;
            copyLevel.MaxFloorSize = originalLevel.MaxFloorSize;
        }
        else if (original is PartitionConfig originalPartition && copy is PartitionConfig copyPartition)
        {
            copyPartition.MinPartitionSize = originalPartition.MinPartitionSize;
            copyPartition.MaxPartitionSize = originalPartition.MaxPartitionSize;
            copyPartition.ExtraConnections = originalPartition.ExtraConnections;
            copyPartition.MinSplitRatio = originalPartition.MinSplitRatio;
            copyPartition.MaxSplitRatio = originalPartition.MaxSplitRatio;
        }
        else if (original is RoomConfig originalRoom && copy is RoomConfig copyRoom)
        {
            copyRoom.MinInset = originalRoom.MinInset;
            copyRoom.MaxInset = originalRoom.MaxInset;
            copyRoom.MinRoomSize = originalRoom.MinRoomSize;
            copyRoom.MaxRoomSize = originalRoom.MaxRoomSize;
            copyRoom.MaxRooms = originalRoom.MaxRooms;
            copyRoom.SpawnPadding = originalRoom.SpawnPadding;
        }
        else
        {
            Debug.LogWarning($"No manual copy implementation for {typeof(T).Name}, using default values");
        }
        
        return copy;
    }

    private void ValidateAllConfigs()
    {
        GameConfig?.Validate();
        LevelConfig?.Validate();
        PartitionConfig?.Validate();
        RoomConfig?.Validate();
    }

    /// <summary>
    /// Updates the level configuration for progression to the next floor.
    /// </summary>
    public void ProgressToNextFloor(System.Random random)
    {
        LevelConfig.FloorLevel++;
        LevelConfig.GrowFloor(random.NextDouble() > 0.5);
        LevelConfig.Validate();
        
        Debug.Log($"Progressed to floor {LevelConfig.FloorLevel}, new size: {LevelConfig.Width}x{LevelConfig.Height}");
    }

    /// <summary>
    /// Resets all configurations to their original state.
    /// </summary>
    public void Reset(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig, RoomConfig roomConfig)
    {
        GameConfig = CreateConfigCopy(gameConfig);
        LevelConfig = CreateConfigCopy(levelConfig);
        PartitionConfig = CreateConfigCopy(partitionConfig);
        RoomConfig = CreateConfigCopy(roomConfig);
        
        ValidateAllConfigs();
    }
}

/// <summary>
/// Interface for configuration objects that support cloning.
/// </summary>
public interface IConfigCloneable
{
    object Clone();
}