// -------------------------------------------------- //
// Scripts/Services/ConfigRegistry.cs
// -------------------------------------------------- //

using UnityEngine;

public class ConfigRegistry
{
    public GameConfig GameConfig;
    public LevelConfig LevelConfig;
    public PartitionConfig PartitionConfig;
    public RoomConfig RoomConfig;

    public ConfigRegistry(GameConfig gameConfig, LevelConfig levelConfig, PartitionConfig partitionConfig, RoomConfig roomConfig)
    {
        try
        {
            Debug.Log("ConfigRegistry: Initializing configuration service...");
            // Create runtime copies to prevent modifying original ScriptableObjects
            GameConfig = CreateConfigCopy(gameConfig);
            LevelConfig = CreateConfigCopy(levelConfig);
            PartitionConfig = CreateConfigCopy(partitionConfig);
            RoomConfig = CreateConfigCopy(roomConfig);
            // Validate all configurations for consistency
            ValidateAllConfigs();
            Debug.Log($"ConfigRegistry: Runtime configs created successfully");
            Debug.Log($"ConfigRegistry: Level - {LevelConfig.Width}x{LevelConfig.Height}, Floor {LevelConfig.FloorLevel}");
            Debug.Log($"ConfigRegistry: Game - Seed: {LevelConfig.Seed}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigRegistry: Error during initialization: {ex.Message}");
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
                Debug.LogWarning($"ConfigRegistry: Null config provided for {typeof(T).Name}, creating default instance");
                return ScriptableObject.CreateInstance<T>();
            }
            T copy = Object.Instantiate(original);
            copy.name = $"{original.name}_RuntimeCopy";
            Debug.Log($"ConfigRegistry: Created runtime copy of {typeof(T).Name} - {copy.name}");
            return copy;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigRegistry: Error creating config copy for {typeof(T).Name}: {ex.Message}");
            return ScriptableObject.CreateInstance<T>(); // Return default instance as fallback
        }
    }

    private void ValidateAllConfigs()
    {
        try
        {
            Debug.Log("ConfigRegistry: Validating all configurations...");
            GameConfig?.Validate();
            LevelConfig?.Validate();
            PartitionConfig?.Validate();
            RoomConfig?.Validate();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigRegistry: Error during config validation: {ex.Message}");
        }
    }

    private void CreateFallbackConfigs()
    {
        try
        {
            Debug.LogWarning("ConfigRegistry: Creating fallback configurations due to initialization errors");
            if (GameConfig == null)
            {
                GameConfig = ScriptableObject.CreateInstance<GameConfig>();
                Debug.LogWarning("ConfigRegistry: Created fallback GameConfig");
            }
            if (LevelConfig == null)
            {
                LevelConfig = ScriptableObject.CreateInstance<LevelConfig>();
                Debug.LogWarning("ConfigRegistry: Created fallback LevelConfig");
            }
            if (PartitionConfig == null)
            {
                PartitionConfig = ScriptableObject.CreateInstance<PartitionConfig>();
                Debug.LogWarning("ConfigRegistry: Created fallback PartitionConfig");
            }
            if (RoomConfig == null)
            {
                RoomConfig = ScriptableObject.CreateInstance<RoomConfig>();
                Debug.LogWarning("ConfigRegistry: Created fallback RoomConfig");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ConfigRegistry: Error creating fallback configs: {ex.Message}");
        }
    }
}