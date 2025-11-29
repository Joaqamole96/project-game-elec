// -------------------------------------------------- //
// Scripts/Services/ConfigService.cs
// -------------------------------------------------- //

using UnityEngine;

/// <summary>
/// Service for managing and validating game configuration data
/// Provides runtime access to game, level, partition, and room configurations
/// Handles config validation, copying, and progression between floors
/// </summary>
public class ConfigService
{
    public GameConfig GameConfig { get; private set; }
    public LevelConfig LevelConfig { get; private set; }
    public PartitionConfig PartitionConfig { get; private set; }
    public RoomConfig RoomConfig { get; private set; }

    /// <summary>
    /// Initializes a new instance of ConfigService with provided configuration assets
    /// Creates runtime copies of configs to prevent editing original assets
    /// </summary>
    /// <param name="gameConfig">Game-wide configuration settings</param>
    /// <param name="levelConfig">Level generation and layout configuration</param>
    /// <param name="partitionConfig">Space partitioning and room splitting configuration</param>
    /// <param name="roomConfig">Room-specific generation parameters</param>
    public ConfigService(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig, RoomConfig roomConfig)
    {
        try
        {
            Debug.Log("ConfigService: Initializing configuration service...");
            // Create runtime copies to prevent modifying original ScriptableObjects
            GameConfig = CreateConfigCopy(gameConfig);
            LevelConfig = CreateConfigCopy(levelConfig);
            PartitionConfig = CreateConfigCopy(partitionConfig);
            RoomConfig = CreateConfigCopy(roomConfig);
            // Validate all configurations for consistency
            ValidateAllConfigs();
            Debug.Log($"ConfigService: Runtime configs created successfully");
            Debug.Log($"ConfigService: Level - {LevelConfig.Width}x{LevelConfig.Height}, Floor {LevelConfig.FloorLevel}");
            Debug.Log($"ConfigService: Game - Seed: {LevelConfig.Seed}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigService: Error during initialization: {ex.Message}");
            // Ensure we have valid config instances even if initialization partially fails
            CreateFallbackConfigs();
        }
    }

    private T CreateConfigCopy<T>(T original) where T : ScriptableObject
    {
        try
        {
            if (original == null)
            {
                Debug.LogWarning($"ConfigService: Null config provided for {typeof(T).Name}, creating default instance");
                return ScriptableObject.CreateInstance<T>();
            }
            T copy = Object.Instantiate(original);
            copy.name = $"{original.name}_RuntimeCopy";
            Debug.Log($"ConfigService: Created runtime copy of {typeof(T).Name} - {copy.name}");
            return copy;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigService: Error creating config copy for {typeof(T).Name}: {ex.Message}");
            return ScriptableObject.CreateInstance<T>(); // Return default instance as fallback
        }
    }

    private void ValidateAllConfigs()
    {
        try
        {
            Debug.Log("ConfigService: Validating all configurations...");
            GameConfig?.Validate();
            LevelConfig?.Validate();
            PartitionConfig?.Validate();
            RoomConfig?.Validate();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigService: Error during config validation: {ex.Message}");
        }
    }

    private void CreateFallbackConfigs()
    {
        try
        {
            Debug.LogWarning("ConfigService: Creating fallback configurations due to initialization errors");
            if (GameConfig == null)
            {
                GameConfig = ScriptableObject.CreateInstance<GameConfig>();
                Debug.LogWarning("ConfigService: Created fallback GameConfig");
            }
            if (LevelConfig == null)
            {
                LevelConfig = ScriptableObject.CreateInstance<LevelConfig>();
                Debug.LogWarning("ConfigService: Created fallback LevelConfig");
            }
            if (PartitionConfig == null)
            {
                PartitionConfig = ScriptableObject.CreateInstance<PartitionConfig>();
                Debug.LogWarning("ConfigService: Created fallback PartitionConfig");
            }
            if (RoomConfig == null)
            {
                RoomConfig = ScriptableObject.CreateInstance<RoomConfig>();
                Debug.LogWarning("ConfigService: Created fallback RoomConfig");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigService: Error creating fallback configs: {ex.Message}");
        }
    }

    /// <summary>
    /// Progresses to the next floor level and adjusts level configuration accordingly
    /// Typically called when player completes a floor
    /// </summary>
    /// <param name="random">Random number generator for procedural growth decisions</param>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    // public void ProgressToNextFloor(System.Random random)
    // {
    //     try
    //     {
    //         if (LevelConfig == null)
    //         {
    //             Debug.LogError("ConfigService: Cannot progress to next floor - LevelConfig is null");
    //             return;
    //         }

    //         if (random == null)
    //         {
    //             Debug.LogError("ConfigService: Cannot progress to next floor - Random generator is null");
    //             return;
    //         }

    //         int previousFloor = LevelConfig.FloorLevel;
    //         int previousWidth = LevelConfig.Width;
    //         int previousHeight = LevelConfig.Height;

    //         // Increment floor level
    //         LevelConfig.FloorLevel++;
            
    //         // Randomly decide whether to grow the floor size
    //         bool shouldGrow = random.NextDouble() > 0.5;
    //         LevelConfig.GrowFloor(shouldGrow);
            
    //         // Re-validate the updated configuration
    //         LevelConfig.Validate();

    //         Debug.Log($"ConfigService: Progressed from floor {previousFloor} to {LevelConfig.FloorLevel}");
    //         Debug.Log($"ConfigService: Floor size changed from {previousWidth}x{previousHeight} to {LevelConfig.Width}x{LevelConfig.Height}");
    //         Debug.Log($"ConfigService: Floor growth decision: {shouldGrow}");

    //         // Notify other systems about floor progression
    //         OnFloorProgressed(previousFloor, LevelConfig.FloorLevel);
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"ConfigService: Error progressing to next floor: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// Handles notifications and events when floor progression occurs
    /// </summary>
    /// <param name="previousFloor">The floor number before progression</param>
    /// <param name="newFloor">The new current floor number</param>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    // private void OnFloorProgressed(int previousFloor, int newFloor)
    // {
    //     try
    //     {
    //         // This method can be extended to notify other systems about floor changes
    //         Debug.Log($"ConfigService: Floor progression event - {previousFloor} -> {newFloor}");

    //         // Example: Adjust difficulty based on floor level
    //         if (newFloor > previousFloor)
    //         {
    //             AdjustDifficultyForFloor(newFloor);
    //         }
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"ConfigService: Error in floor progression handler: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// Adjusts game difficulty parameters based on the current floor level
    /// </summary>
    /// <param name="currentFloor">The current floor level</param>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    // private void AdjustDifficultyForFloor(int currentFloor)
    // {
    //     try
    //     {
    //         // Example difficulty scaling - adjust based on your game's needs
    //         if (currentFloor % 5 == 0)
    //         {
    //             Debug.Log($"ConfigService: Reached milestone floor {currentFloor} - consider increasing difficulty");
    //             // RoomConfig.IncreaseEnemyDensity();
    //             // GameConfig.IncreaseDifficulty();
    //         }
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"ConfigService: Error adjusting difficulty for floor {currentFloor}: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// Validates configuration compatibility between different config types
    /// </summary>
    /// <returns>True if all configurations are compatible, false otherwise</returns>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    // public bool ValidateConfigCompatibility()
    // {
    //     try
    //     {
    //         Debug.Log("ConfigService: Validating configuration compatibility...");

    //         bool compatible = true;

    //         // Check if level dimensions are reasonable for room and partition configs
    //         if (LevelConfig != null && RoomConfig != null)
    //         {
    //             int minRoomSize = RoomConfig.MinRoomSize;
    //             if (LevelConfig.Width < minRoomSize * 2 || LevelConfig.Height < minRoomSize * 2)
    //             {
    //                 Debug.LogError($"ConfigService: Level size {LevelConfig.Width}x{LevelConfig.Height} too small for minimum room size {minRoomSize}");
    //                 compatible = false;
    //             }
    //         }

    //         if (compatible)
    //         {
    //             Debug.Log("ConfigService: All configurations are compatible");
    //         }
    //         else
    //         {
    //             Debug.LogError("ConfigService: Configuration compatibility check failed");
    //         }

    //         return compatible;
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"ConfigService: Error during compatibility validation: {ex.Message}");
    //         return false;
    //     }
    // }

    /// <summary>
    /// Gets a summary of all current configuration values for debugging
    /// </summary>
    /// <returns>Formatted string with config summary</returns>
    public string GetConfigSummary()
    {
        try
        {
            return $"Config Summary:\n" +
                   $"Floor: {LevelConfig?.FloorLevel ?? -1}\n" +
                   $"Level Size: {LevelConfig?.Width ?? -1}x{LevelConfig?.Height ?? -1}\n" +
                   $"Seed: {LevelConfig?.Seed ?? -1}";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigService: Error generating config summary: {ex.Message}");
            return "Config Summary: Error generating summary";
        }
    }

    /// <summary>
    /// Logs all current configuration values for debugging purposes
    /// </summary>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    // public void LogAllConfigs()
    // {
    //     try
    //     {
    //         Debug.Log("=== ConfigService: Current Configuration State ===");
    //         Debug.Log(GetConfigSummary());
    //         Debug.Log("=================================================");
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"ConfigService: Error logging configurations: {ex.Message}");
    //     }
    // }

    /// <summary>
    /// Cleans up runtime configuration instances
    /// </summary>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    // public void Cleanup()
    // {
    //     try
    //     {
    //         Debug.Log("ConfigService: Cleaning up runtime configurations...");

    //         // Destroy runtime copies to free memory
    //         if (GameConfig != null) Object.DestroyImmediate(GameConfig);
    //         if (LevelConfig != null) Object.DestroyImmediate(LevelConfig);
    //         if (PartitionConfig != null) Object.DestroyImmediate(PartitionConfig);
    //         if (RoomConfig != null) Object.DestroyImmediate(RoomConfig);

    //         Debug.Log("ConfigService: Configuration cleanup completed");
    //     }
    //     catch (System.Exception ex)
    //     {
    //         Debug.LogError($"ConfigService: Error during cleanup: {ex.Message}");
    //     }
    // }
}