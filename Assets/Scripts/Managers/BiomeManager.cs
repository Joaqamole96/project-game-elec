using UnityEngine;
using System;
using System.Collections.Generic;

public class BiomeManager
{
    public List<BiomeModel> Biomes = new List<BiomeModel>();

    private System.Random _random;
    private Dictionary<string, GameObject> _prefabCache;

    // -------------------------------------------------- //

    public BiomeManager(LevelConfig levelConfig)
    {
        if (levelConfig == null)
            throw new Exception("BiomeManager(): LevelConfig cannot be null.");

        _random = new System.Random(levelConfig.Seed);
        _prefabCache = new Dictionary<string, GameObject>(); // Initialize the cache
        InitializeBiomes();
        
        Debug.Log("BiomeManager initialized with prefab cache");
    }

    private void InitializeBiomes()
    {
        // Don't initialize a Default Biome; Default is for testing only.
        // Biomes.Add(new BiomeModel("Default", 1, 5));
        // NOTE: Provisionary biomes; may change in the future.
        Biomes.Add(new BiomeModel("Grasslands", 1, 5));
        Biomes.Add(new BiomeModel("Catacombs", 6, 10));
        Biomes.Add(new BiomeModel("Pits", 11, 15));
        
        Debug.Log($"BiomeManager: Initialized {Biomes.Count} biomes");
    }

    // -------------------------------------------------- //

    public BiomeModel GetBiomeForFloor(int floorLevel)
    {
        var candidateBiomes = Biomes.FindAll(biome => (floorLevel >= biome.MinLevel) && (floorLevel <= biome.MaxLevel));

        if (candidateBiomes.Count == 0)
            throw new Exception($"BiomeManager.GetBiomeForFloor(): No candidate biome found for floor {floorLevel}.");

        int selectedBiomeIndex = _random.Next(0, candidateBiomes.Count);
        var selectedBiome = candidateBiomes[selectedBiomeIndex];

        Debug.Log($"BiomeManager.GetBiomeForFloor(): Returning {selectedBiome.Name} for floor {floorLevel}");
        return selectedBiome;
    }

    // -------------------------------------------------- //

    public GameObject GetPrefab(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
            throw new Exception("BiomeManager.GetPrefab(): Empty prefab path provided!");
        
        if (_prefabCache == null)
        {
            Debug.LogWarning("Prefab cache was null, reinitializing...");
            _prefabCache = new Dictionary<string, GameObject>();
        }
        
        if (_prefabCache.TryGetValue(prefabPath, out GameObject cachedPrefab))
        {
            if (cachedPrefab != null)
            {
                Debug.Log($"BiomeManager.GetPrefab(): Returning cached {cachedPrefab.name}");
                return cachedPrefab;
            }
            else
            {
                // Remove null entry from cache
                _prefabCache.Remove(prefabPath);
            }
        }
        
        return GetAndCachePrefab(prefabPath);
    }

    public GameObject GetFloorPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetFloorPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/FloorPrefab");
    }

    public GameObject GetWallPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetWallPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/WallPrefab");
    }

    public GameObject GetDoorPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetDoorPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/DoorPrefab");
    }

    public GameObject GetCeilingPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetCeilingPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/CeilingPrefab");
    }

    // Prop prefab methods for PropRenderer
    public GameObject GetEntrancePropPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetEntrancePropPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/Props/EntranceProp");
    }

    public GameObject GetExitPropPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetExitPropPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/Props/ExitProp");
    }

    public GameObject GetShopPropPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetShopPropPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/Props/ShopProp");
    }

    public GameObject GetTreasurePropPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetTreasurePropPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/Props/TreasureProp");
    }

    public GameObject GetBossPropPrefab(BiomeModel biome)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetBossPropPrefab(): Biome cannot be null!");
            
        return GetPrefab($"Biomes/{biome.Name}/Props/BossProp");
    }

    // Decor prefab methods for DecorRenderer
    public GameObject GetDecorPrefab(BiomeModel biome, string decorType)
    {
        if (biome == null)
            throw new Exception("BiomeManager.GetDecorPrefab(): Biome cannot be null!");
            
        if (string.IsNullOrEmpty(decorType))
            throw new Exception("BiomeManager.GetDecorPrefab(): Decor type cannot be null or empty!");
            
        return GetPrefab($"Biomes/{biome.Name}/Decor/{decorType}");
    }

    public DecorConfig GetRoomDecorConfig(BiomeModel biome, RoomType roomType)
    {
        // Return biome-specific decor configuration
        // This can be expanded to have different decor configs per room type
        return new DecorConfig
        {
            MinDensity = 0.1f,
            MaxDensity = 0.3f,
            UseMeshCombining = true,
            PrefabWeights = new Dictionary<string, float>
            {
                { "Tree", 0.3f },
                { "Rock", 0.4f },
                { "Bush", 0.3f }
            }
        };
    }

    public DecorConfig GetCorridorDecorConfig(BiomeModel biome)
    {
        // Less dense decor in corridors
        return new DecorConfig
        {
            MinDensity = 0.05f,
            MaxDensity = 0.15f,
            UseMeshCombining = true,
            PrefabWeights = new Dictionary<string, float>
            {
                { "Rock", 0.6f },
                { "Bush", 0.4f }
            }
        };
    }

    private GameObject GetAndCachePrefab(string prefabPath)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);

        if (prefab == null)
        {
            // Log available resources for debugging
            LogAvailableResources(prefabPath);
            throw new Exception($"BiomeManager.GetAndCachePrefab(): Prefab not found at path: {prefabPath}");
        }

        if (_prefabCache == null)
        {
            _prefabCache = new Dictionary<string, GameObject>();
        }

        _prefabCache[prefabPath] = prefab;
        Debug.Log($"BiomeManager.GetAndCachePrefab(): Cached {prefab.name} from path: {prefabPath}");

        return prefab;
    }

    // -------------------------------------------------- //

    private void LogAvailableResources(string failedPath)
    {
        // Extract biome name from failed path for more targeted logging
        string biomeName = "Default"; // fallback
        try
        {
            var pathParts = failedPath.Split('/');
            if (pathParts.Length >= 2 && pathParts[0] == "Biomes")
            {
                biomeName = pathParts[1];
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to parse biome name from path: {e.Message}");
        }

        var availableResources = Resources.LoadAll<GameObject>($"Biomes/{biomeName}");
        Debug.Log($"Available resources in Biomes/{biomeName}: {availableResources.Length}");
        foreach (var resource in availableResources)
            Debug.Log($" - {resource.name} (Type: {resource.GetType()})");

        // Also check if the specific prefab exists
        var specificPrefab = Resources.Load<GameObject>(failedPath);
        if (specificPrefab != null)
        {
            Debug.Log($"SUCCESS: Prefab found at {failedPath}: {specificPrefab.name}");
        }
        else
        {
            Debug.LogError($"FAILED: Prefab not found at {failedPath}");
        }
    }

    /// <summary>
    /// Clears the prefab cache to free memory.
    /// </summary>
    public void ClearCache()
    {
        if (_prefabCache != null)
        {
            _prefabCache.Clear();
            Debug.Log("BiomeManager: Prefab cache cleared");
        }
    }

    /// <summary>
    /// Gets the number of prefabs currently cached.
    /// </summary>
    public int GetCacheSize()
    {
        return _prefabCache?.Count ?? 0;
    }
}