// BiomeManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages biome themes and provides efficient prefab loading with caching.
/// Supports weighted random theme selection for different floor levels.
/// </summary>
public class BiomeManager
{
    private readonly Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
    private System.Random _random = new System.Random();
    
    /// <summary>Available biome themes for dungeon generation.</summary>
    public List<BiomeTheme> Themes { get; private set; } = new List<BiomeTheme>();

    public BiomeManager()
    {
        InitializeDefaultThemes();
    }

    private void InitializeDefaultThemes()
    {
        Themes.Add(new BiomeTheme("Default", 1, 100, 1.0f)
        {
            FloorPrefabPath = "Themes/Default/FloorPrefab",
            WallPrefabPath = "Themes/Default/WallPrefab",
            DoorPrefabPath = "Themes/Default/DoorPrefab",
            DoorTopPrefabPath = "Themes/Default/DoorTopPrefab",
            EntrancePrefabPath = "Themes/Default/EntrancePrefab",
            ExitPrefabPath = "Themes/Default/ExitPrefab",
            ShopPrefabPath = "Themes/Default/ShopPrefab",
            TreasurePrefabPath = "Themes/Default/TreasurePrefab",
            BossPrefabPath = "Themes/Default/BossPrefab"
        });
    }

    /// <summary>
    /// Selects an appropriate biome theme for the given floor level using weighted random selection.
    /// </summary>
    /// <param name="floorLevel">Current floor level for theme selection.</param>
    /// <param name="seed">Random seed for deterministic selection.</param>
    /// <returns>Selected biome theme, or default if no valid themes found.</returns>
    public BiomeTheme GetThemeForFloor(int floorLevel, int seed)
    {
        _random = new System.Random(seed + floorLevel);
        
        var validThemes = Themes.FindAll(theme => floorLevel >= theme.StartLevel && floorLevel <= theme.EndLevel);
        
        if (validThemes.Count == 0)
        {
            Debug.LogWarning($"No theme found for floor {floorLevel}, using default");
            return Themes[0];
        }

        if (validThemes.Count == 1)
            return validThemes[0];

        // Weighted random selection
        float totalWeight = CalculateTotalWeight(validThemes);
        float randomValue = (float)_random.NextDouble() * totalWeight;
        
        return SelectThemeByWeight(validThemes, randomValue);
    }

    private float CalculateTotalWeight(List<BiomeTheme> themes)
    {
        float total = 0f;
        foreach (var theme in themes)
            total += theme.Weight;
        return total;
    }

    private BiomeTheme SelectThemeByWeight(List<BiomeTheme> themes, float randomValue)
    {
        float currentWeight = 0f;
        foreach (var theme in themes)
        {
            currentWeight += theme.Weight;
            if (randomValue <= currentWeight)
                return theme;
        }
        return themes[0];
    }

    /// <summary>
    /// Gets a prefab from the specified path, using cache when possible.
    /// </summary>
    /// <param name="prefabPath">Resources path to the prefab.</param>
    /// <returns>Loaded prefab or null if not found.</returns>
    public GameObject GetPrefab(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogWarning("Empty prefab path provided!");
            return null;
        }

        if (_prefabCache.TryGetValue(prefabPath, out GameObject cachedPrefab))
            return cachedPrefab;

        return LoadAndCachePrefab(prefabPath);
    }

    private GameObject LoadAndCachePrefab(string prefabPath)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            _prefabCache[prefabPath] = prefab;
        }
        else
        {
            Debug.LogError($"Prefab not found at path: {prefabPath}");
            LogAvailableResources(prefabPath);
        }

        return prefab;
    }

    private void LogAvailableResources(string failedPath)
    {
        var availableFloors = Resources.LoadAll<GameObject>("Themes/Default");
        Debug.Log($"Available resources in Themes/Default: {availableFloors.Length}");
        foreach (var resource in availableFloors)
        {
            Debug.Log($" - {resource.name}");
        }
    }

    /// <summary>
    /// Gets a special room prefab based on room type and current theme.
    /// </summary>
    public GameObject GetSpecialRoomPrefab(RoomType roomType, BiomeTheme theme)
    {
        if (theme == null) return null;
        
        string path = GetSpecialRoomPath(roomType, theme);
        return GetPrefab(path);
    }

    private string GetSpecialRoomPath(RoomType roomType, BiomeTheme theme)
    {
        return roomType switch
        {
            RoomType.Entrance => theme.EntrancePrefabPath,
            RoomType.Exit => theme.ExitPrefabPath,
            RoomType.Shop => theme.ShopPrefabPath,
            RoomType.Treasure => theme.TreasurePrefabPath,
            RoomType.Boss => theme.BossPrefabPath,
            _ => null
        };
    }

    // Helper methods with fallback support
    public GameObject GetFloorPrefab(BiomeTheme theme) => 
        GetPrefab(theme?.FloorPrefabPath) ?? GetPrefab("Themes/Default/FloorPrefab");

    public GameObject GetWallPrefab(BiomeTheme theme) => 
        GetPrefab(theme?.WallPrefabPath) ?? GetPrefab("Themes/Default/WallPrefab");

    public GameObject GetDoorPrefab(BiomeTheme theme) => 
        GetPrefab(theme?.DoorPrefabPath) ?? GetPrefab("Themes/Default/DoorPrefab");

    public GameObject GetDoorTopPrefab(BiomeTheme theme) => 
        GetPrefab(theme?.DoorTopPrefabPath) ?? GetPrefab("Themes/Default/DoorTopPrefab");
}