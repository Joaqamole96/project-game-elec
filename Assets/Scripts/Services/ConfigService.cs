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
}