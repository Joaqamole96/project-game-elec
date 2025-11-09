using UnityEngine;
using System.Collections.Generic;

public class BiomeManager
{
    public List<BiomeTheme> Themes = new List<BiomeTheme>();
    private System.Random _random;
    private Dictionary<string, GameObject> _prefabCache;

    public BiomeManager()
    {
        _random = new System.Random();
        _prefabCache = new Dictionary<string, GameObject>();
        InitializeDefaultThemes();
    }

    private void InitializeDefaultThemes()
    {
        // Default Theme (for testing)
        Themes.Add(new BiomeTheme("Default", 1, 100, 1.0f) // Set to cover all floors for testing
        {
            // KEEP "Prefab" suffix since that's what you named them
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

    public BiomeTheme GetThemeForFloor(int floorLevel, int seed)
    {
        _random = new System.Random(seed + floorLevel);
        
        // Get all themes valid for this floor level
        var validThemes = Themes.FindAll(theme => floorLevel >= theme.StartLevel && floorLevel <= theme.EndLevel);
        
        if (validThemes.Count == 0)
        {
            Debug.LogWarning($"No theme found for floor {floorLevel}, using default");
            return Themes[0];
        }

        // If only one theme, use it
        if (validThemes.Count == 1)
            return validThemes[0];

        // Weighted random selection between multiple themes
        float totalWeight = 0f;
        foreach (var theme in validThemes)
            totalWeight += theme.Weight;

        float randomValue = (float)_random.NextDouble() * totalWeight;
        float currentWeight = 0f;

        foreach (var theme in validThemes)
        {
            currentWeight += theme.Weight;
            if (randomValue <= currentWeight)
            {
                Debug.Log($"Selected theme '{theme.Name}' for floor {floorLevel} (weight: {theme.Weight})");
                return theme;
            }
        }

        return validThemes[0]; // Fallback
    }

    public GameObject GetPrefab(string prefabPath)
    {
        if (string.IsNullOrEmpty(prefabPath))
        {
            Debug.LogWarning("Empty prefab path provided!");
            return null;
        }

        // Check cache first
        if (_prefabCache.ContainsKey(prefabPath))
        {
            Debug.Log($"Found cached prefab: {prefabPath}");
            return _prefabCache[prefabPath];
        }

        // Load from Resources
        Debug.Log($"Loading prefab from Resources: {prefabPath}");
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        
        if (prefab != null)
        {
            _prefabCache[prefabPath] = prefab;
            Debug.Log($"Successfully loaded prefab: {prefabPath}");
        }
        else
        {
            Debug.LogError($"Prefab not found at path: {prefabPath}");
            // List available resources for debugging
            var availableFloors = Resources.LoadAll<GameObject>("Themes/Default");
            Debug.Log($"Available resources in Themes/Default: {availableFloors.Length}");
            foreach (var resource in availableFloors)
            {
                Debug.Log($" - {resource.name}");
            }
        }

        return prefab;
    }

    public GameObject GetSpecialRoomPrefab(RoomType roomType, BiomeTheme theme)
{
    if (theme == null) return null;
    
    string path = roomType switch
    {
        RoomType.Entrance => theme.EntrancePrefabPath,
        RoomType.Exit => theme.ExitPrefabPath,
        RoomType.Shop => theme.ShopPrefabPath,
        RoomType.Treasure => theme.TreasurePrefabPath,
        RoomType.Boss => theme.BossPrefabPath,
        _ => null
    };
    
    return GetPrefab(path);
}

    // Helper method to get specific prefabs with fallback
    public GameObject GetFloorPrefab(BiomeTheme theme)
    {
        return GetPrefab(theme?.FloorPrefabPath) ?? GetPrefab("Themes/Default/FloorPrefab");
    }

    public GameObject GetWallPrefab(BiomeTheme theme)
    {
        return GetPrefab(theme?.WallPrefabPath) ?? GetPrefab("Themes/Default/WallPrefab");
    }

    public GameObject GetDoorPrefab(BiomeTheme theme)
    {
        return GetPrefab(theme?.DoorPrefabPath) ?? GetPrefab("Themes/Default/DoorPrefab");
    }

    public GameObject GetDoorTopPrefab(BiomeTheme theme)
    {
        return GetPrefab(theme?.DoorTopPrefabPath) ?? GetPrefab("Themes/Default/DoorTopPrefab");
    }
}