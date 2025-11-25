// -------------------------------------------------- //
// Scripts/Services/ResourceService.cs (UPDATED STRUCTURE)
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Centralized resource loading system using Unity's Resources API.
/// 
/// DIRECTORY STRUCTURE:
/// Resources/
/// ├── Layout/              (Room components - biome-specific)
/// │   ├── _Default/
/// │   ├── Grasslands/
/// │   ├── Dungeon/
/// │   └── Caves/
/// ├── Props/               (Environmental props - biome-specific)
/// │   ├── _Default/
/// │   ├── Grasslands/
/// │   ├── Dungeon/
/// │   └── Caves/
/// ├── Enemies/             (Enemies - biome-specific)
/// │   ├── _Default/
/// │   ├── Grasslands/
/// │   ├── Dungeon/
/// │   └── Caves/
/// ├── Landmarks/           (Special room markers - component-based)
/// ├── Players/             (Player prefabs - component-based)
/// ├── Items/               (Items - component-based)
/// ├── Weapons/             (Weapons - component-based)
/// └── UI/                  (UI elements - component-based)
/// </summary>
public static class ResourceService
{
    // Cache to avoid repeated Resources.Load calls
    private static Dictionary<string, GameObject> _prefabCache = new();
    
    // Biome names
    public const string BIOME_DEFAULT = "_Default";
    public const string BIOME_GRASSLANDS = "Grasslands";
    public const string BIOME_DUNGEON = "Dungeon";
    public const string BIOME_CAVES = "Caves";
    
    // Component categories (biome-specific)
    private const string CATEGORY_LAYOUT = "Layout";
    private const string CATEGORY_PROPS = "Props";
    private const string CATEGORY_ENEMIES = "Enemies";
    
    // Component categories (biome-independent)
    private const string CATEGORY_LANDMARKS = "Landmarks";
    private const string CATEGORY_PLAYERS = "Players";
    private const string CATEGORY_ITEMS = "Items";
    private const string CATEGORY_WEAPONS = "Weapons";
    private const string CATEGORY_UI = "UI";
    
    // ------------------------- //
    // CORE LOADING
    // ------------------------- //
    
    /// <summary>
    /// Loads a prefab from Resources/{category}/{biome}/{prefabName} (biome-specific)
    /// </summary>
    private static GameObject LoadBiomeSpecificPrefab(string category, string biome, string prefabName)
    {
        string path = $"{category}/{biome}/{prefabName}";
        
        // Check cache first
        if (_prefabCache.TryGetValue(path, out GameObject cached))
        {
            return cached;
        }
        
        // Load from Resources
        GameObject prefab = Resources.Load<GameObject>(path);
        
        if (prefab != null)
        {
            _prefabCache[path] = prefab;
            Debug.Log($"ResourceService: Loaded {path}");
        }
        else
        {
            Debug.LogWarning($"ResourceService: Failed to load {path}");
        }
        
        return prefab;
    }
    
    /// <summary>
    /// Loads a prefab from Resources/{category}/{prefabName} (biome-independent)
    /// </summary>
    private static GameObject LoadCategoryPrefab(string category, string prefabName)
    {
        string path = $"{category}/{prefabName}";
        
        // Check cache first
        if (_prefabCache.TryGetValue(path, out GameObject cached))
        {
            return cached;
        }
        
        // Load from Resources
        GameObject prefab = Resources.Load<GameObject>(path);
        
        if (prefab != null)
        {
            _prefabCache[path] = prefab;
            Debug.Log($"ResourceService: Loaded {path}");
        }
        else
        {
            Debug.LogWarning($"ResourceService: Failed to load {path}");
        }
        
        return prefab;
    }
    
    /// <summary>
    /// Loads with fallback to _Default biome if not found
    /// </summary>
    private static GameObject LoadWithFallback(string category, string biome, string prefabName)
    {
        GameObject prefab = LoadBiomeSpecificPrefab(category, biome, prefabName);
        
        if (prefab == null && biome != BIOME_DEFAULT)
        {
            Debug.Log($"ResourceService: Falling back to _Default biome for {prefabName}");
            prefab = LoadBiomeSpecificPrefab(category, BIOME_DEFAULT, prefabName);
        }
        
        return prefab;
    }
    
    // ------------------------- //
    // LAYOUT PREFABS (Biome-Specific)
    // ------------------------- //
    
    public static GameObject LoadFloorPrefab(string biome)
        => LoadWithFallback(CATEGORY_LAYOUT, biome, "FloorPrefab");
    
    public static GameObject LoadWallPrefab(string biome)
        => LoadWithFallback(CATEGORY_LAYOUT, biome, "WallPrefab");
    
    public static GameObject LoadDoorPrefab(string biome)
        => LoadWithFallback(CATEGORY_LAYOUT, biome, "DoorPrefab");
    
    public static GameObject LoadDoorTopPrefab(string biome)
        => LoadWithFallback(CATEGORY_LAYOUT, biome, "DoorTopPrefab");
    
    public static GameObject LoadCeilingPrefab(string biome)
        => LoadWithFallback(CATEGORY_LAYOUT, biome, "CeilingPrefab");
    
    // ------------------------- //
    // PROPS PREFABS (Biome-Specific)
    // ------------------------- //
    
    public static GameObject LoadProp(string biome, string propName)
        => LoadWithFallback(CATEGORY_PROPS, biome, propName);
    
    public static GameObject LoadTorchPrefab(string biome)
        => LoadWithFallback(CATEGORY_PROPS, biome, "TorchPrefab");
    
    public static GameObject LoadPillarPrefab(string biome)
        => LoadWithFallback(CATEGORY_PROPS, biome, "PillarPrefab");
    
    public static GameObject LoadBarrelPrefab(string biome)
        => LoadWithFallback(CATEGORY_PROPS, biome, "BarrelPrefab");
    
    public static GameObject LoadCratePrefab(string biome)
        => LoadWithFallback(CATEGORY_PROPS, biome, "CratePrefab");
    
    // ------------------------- //
    // ENEMY PREFABS (Biome-Specific)
    // ------------------------- //
    
    public static GameObject LoadEnemy(string biome, string enemyName)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, enemyName);
    
    public static GameObject LoadBasicEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "BasicEnemyPrefab");
    
    public static GameObject LoadEliteEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "EliteEnemyPrefab");
    
    public static GameObject LoadBossEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "BossPrefab");
    
    // ------------------------- //
    // LANDMARK PREFABS (Biome-Independent)
    // ------------------------- //
    
    public static GameObject LoadLandmarkPrefab(RoomType roomType)
    {
        string prefabName = roomType switch
        {
            RoomType.Entrance => "EntrancePrefab",
            RoomType.Exit => "ExitPrefab",
            RoomType.Shop => "ShopPrefab",
            RoomType.Treasure => "TreasurePrefab",
            RoomType.Boss => "BossPrefab",
            _ => null
        };
        
        if (prefabName != null)
        {
            return LoadCategoryPrefab(CATEGORY_LANDMARKS, prefabName);
        }
        
        return null;
    }
    
    public static GameObject LoadEntrancePrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "EntrancePrefab");
    
    public static GameObject LoadExitPrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "ExitPrefab");
    
    public static GameObject LoadShopPrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "ShopPrefab");
    
    public static GameObject LoadTreasurePrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "TreasurePrefab");
    
    public static GameObject LoadBossPrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "BossPrefab");
    
    // ------------------------- //
    // PLAYER PREFABS (Biome-Independent)
    // ------------------------- //
    
    public static GameObject LoadPlayerPrefab()
        => LoadCategoryPrefab(CATEGORY_PLAYERS, "PlayerPrefab");
    
    public static GameObject LoadCameraPrefab()
        => LoadCategoryPrefab(CATEGORY_PLAYERS, "CameraPrefab");
    
    // ------------------------- //
    // ITEM PREFABS (Biome-Independent)
    // ------------------------- //
    
    public static GameObject LoadItemPrefab(string itemName)
        => LoadCategoryPrefab(CATEGORY_ITEMS, itemName);
    
    public static GameObject LoadHealthPotionPrefab()
        => LoadCategoryPrefab(CATEGORY_ITEMS, "HealthPotionPrefab");
    
    public static GameObject LoadCoinPrefab()
        => LoadCategoryPrefab(CATEGORY_ITEMS, "CoinPrefab");
    
    public static GameObject LoadChestPrefab()
        => LoadCategoryPrefab(CATEGORY_ITEMS, "ChestPrefab");
    
    public static GameObject LoadKeyPrefab(KeyType keyType)
    {
        string keyName = keyType switch
        {
            KeyType.Key => "Key",
            _ => "Key"
        };
        return LoadCategoryPrefab(CATEGORY_ITEMS, keyName);
    }
    
    // ------------------------- //
    // WEAPON PREFABS (Biome-Independent)
    // ------------------------- //
    
    public static GameObject LoadWeaponPrefab(string weaponName)
        => LoadCategoryPrefab(CATEGORY_WEAPONS, weaponName);
    
    public static GameObject LoadSwordPrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "SwordPrefab");
    
    public static GameObject LoadBowPrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "BowPrefab");
    
    public static GameObject LoadStaffPrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "StaffPrefab");
    
    public static GameObject LoadAxePrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "AxePrefab");
    
    // ------------------------- //
    // UI PREFABS (Biome-Independent)
    // ------------------------- //
    
    public static GameObject LoadUIPrefab(string uiName)
        => LoadCategoryPrefab(CATEGORY_UI, uiName);
    
    public static GameObject LoadHealthBarPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "HealthBarPrefab");
    
    public static GameObject LoadMinimapPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "MinimapPrefab");
    
    public static GameObject LoadMainMenuPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "MainMenuPrefab");
    
    public static GameObject LoadPauseMenuPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "PauseMenuPrefab");
    
    public static GameObject LoadGameOverScreenPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "GameOverScreenPrefab");
    
    // ------------------------- //
    // BATCH LOADING
    // ------------------------- //
    
    /// <summary>
    /// Preload all prefabs for a specific biome
    /// </summary>
    public static void PreloadBiome(string biome)
    {
        Debug.Log($"ResourceService: Preloading biome '{biome}'...");
        
        // Layout
        LoadFloorPrefab(biome);
        LoadWallPrefab(biome);
        LoadDoorPrefab(biome);
        LoadDoorTopPrefab(biome);
        LoadCeilingPrefab(biome);
        
        // Props
        LoadTorchPrefab(biome);
        LoadPillarPrefab(biome);
        LoadBarrelPrefab(biome);
        LoadCratePrefab(biome);
        
        // Enemies
        LoadBasicEnemyPrefab(biome);
        LoadEliteEnemyPrefab(biome);
        LoadBossEnemyPrefab(biome);
        
        Debug.Log($"ResourceService: Biome '{biome}' preloaded");
    }
    
    /// <summary>
    /// Preload all common (biome-independent) prefabs
    /// </summary>
    public static void PreloadCommonPrefabs()
    {
        Debug.Log("ResourceService: Preloading common prefabs...");
        
        // Landmarks
        LoadEntrancePrefab();
        LoadExitPrefab();
        LoadShopPrefab();
        LoadTreasurePrefab();
        LoadBossPrefab();
        
        // Player
        LoadPlayerPrefab();
        LoadCameraPrefab();
        
        // Items
        LoadHealthPotionPrefab();
        LoadCoinPrefab();
        LoadChestPrefab();
        
        // Weapons
        LoadSwordPrefab();
        LoadBowPrefab();
        
        Debug.Log("ResourceService: Common prefabs preloaded");
    }
    
    // ------------------------- //
    // CACHE MANAGEMENT
    // ------------------------- //
    
    public static void ClearCache()
    {
        _prefabCache.Clear();
        Debug.Log("ResourceService: Cache cleared");
    }
    
    public static void ClearBiomeCache(string biome)
    {
        List<string> keysToRemove = new();
        
        foreach (var key in _prefabCache.Keys)
        {
            if (key.Contains($"/{biome}/"))
            {
                keysToRemove.Add(key);
            }
        }
        
        foreach (var key in keysToRemove)
        {
            _prefabCache.Remove(key);
        }
        
        Debug.Log($"ResourceService: Cleared {keysToRemove.Count} cached prefabs for biome '{biome}'");
    }
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    public static bool PrefabExists(string category, string biome, string prefabName)
    {
        string path = $"{category}/{biome}/{prefabName}";
        GameObject prefab = Resources.Load<GameObject>(path);
        return prefab != null;
    }
    
    public static bool PrefabExists(string category, string prefabName)
    {
        string path = $"{category}/{prefabName}";
        GameObject prefab = Resources.Load<GameObject>(path);
        return prefab != null;
    }
    
    public static void LogLoadedPrefabs()
    {
        Debug.Log("=== ResourceService Cache ===");
        foreach (var kvp in _prefabCache)
        {
            Debug.Log($"  {kvp.Key} → {kvp.Value.name}");
        }
        Debug.Log($"Total: {_prefabCache.Count} prefabs cached");
        Debug.Log("===========================");
    }
    
    public static int GetCacheSize()
    {
        return _prefabCache.Count;
    }
    
    /// <summary>
    /// Get list of available biomes by checking directories
    /// </summary>
    public static List<string> GetAvailableBiomes()
    {
        List<string> biomes = new()
        {
            BIOME_DEFAULT,
            BIOME_GRASSLANDS,
            BIOME_DUNGEON,
            BIOME_CAVES
        };
        
        // Filter to only existing biomes
        List<string> existingBiomes = new();
        
        foreach (string biome in biomes)
        {
            if (PrefabExists(CATEGORY_LAYOUT, biome, "FloorPrefab"))
            {
                existingBiomes.Add(biome);
            }
        }
        
        return existingBiomes;
    }
}