// -------------------------------------------------- //
// Scripts/Services/ResourceService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static service for managing resource loading and caching of game prefabs
/// Provides biome-specific and biome-independent prefab loading with caching
/// </summary>
public static class ResourceService
{
    // Cache to avoid repeated Resources.Load calls
    private static Dictionary<string, GameObject> _prefabCache = new();
    
    // Biome constants
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
    // CORE LOADING METHODS
    // ------------------------- //
    
    /// <summary>
    /// Loads a prefab from Resources/{category}/{biome}/{prefabName} (biome-specific)
    /// </summary>
    /// <param name="category">Resource category (Layout, Props, Enemies)</param>
    /// <param name="biome">Biome name for biome-specific assets</param>
    /// <param name="prefabName">Name of the prefab to load</param>
    /// <returns>Loaded GameObject or null if not found</returns>
    private static GameObject LoadBiomeSpecificPrefab(string category, string biome, string prefabName)
    {
        try
        {
            string path = $"{category}/{biome}/{prefabName}";
            
            // Check cache first for performance
            if (_prefabCache.TryGetValue(path, out GameObject cached))
            {
                return cached;
            }
            
            // Load from Resources
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                _prefabCache[path] = prefab;
                Debug.Log($"ResourceService: Successfully loaded {path}");
                return prefab;
            }
            else
            {
                Debug.LogWarning($"ResourceService: Failed to load {path} - resource not found");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error loading biome-specific prefab {prefabName}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads a prefab from Resources/{category}/{prefabName} (biome-independent)
    /// </summary>
    /// <param name="category">Resource category</param>
    /// <param name="prefabName">Name of the prefab to load</param>
    /// <returns>Loaded GameObject or null if not found</returns>
    private static GameObject LoadCategoryPrefab(string category, string prefabName)
    {
        try
        {
            string path = $"{category}/{prefabName}";
            
            // Check cache first for performance
            if (_prefabCache.TryGetValue(path, out GameObject cached))
            {
                return cached;
            }
            
            // Load from Resources
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                _prefabCache[path] = prefab;
                Debug.Log($"ResourceService: Successfully loaded {path}");
                return prefab;
            }
            else
            {
                Debug.LogWarning($"ResourceService: Failed to load {path} - resource not found");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error loading category prefab {prefabName}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Loads with fallback to _Default biome if not found in specified biome
    /// </summary>
    /// <param name="category">Resource category</param>
    /// <param name="biome">Preferred biome</param>
    /// <param name="prefabName">Name of prefab to load</param>
    /// <returns>Loaded GameObject or null if neither biome has the prefab</returns>
    private static GameObject LoadWithFallback(string category, string biome, string prefabName)
    {
        try
        {
            GameObject prefab = LoadBiomeSpecificPrefab(category, biome, prefabName);
            if (prefab == null && biome != BIOME_DEFAULT)
            {
                Debug.Log($"ResourceService: Falling back to {BIOME_DEFAULT} biome for {prefabName}");
                prefab = LoadBiomeSpecificPrefab(category, BIOME_DEFAULT, prefabName);
            }
            return prefab;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error in fallback loading for {prefabName}: {ex.Message}");
            return null;
        }
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
    
    /// <summary>
    /// Loads landmark prefab based on room type
    /// </summary>
    /// <param name="roomType">Type of room landmark to load</param>
    /// <returns>Loaded landmark prefab or null if not found</returns>
    public static GameObject LoadLandmarkPrefab(RoomType roomType)
    {
        try
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
            
            Debug.LogWarning($"ResourceService: No prefab mapping for room type {roomType}");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error loading landmark for {roomType}: {ex.Message}");
            return null;
        }
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
    
    /// <summary>
    /// Loads key prefab based on key type
    /// </summary>
    /// <param name="keyType">Type of key to load</param>
    /// <returns>Loaded key prefab or null if not found</returns>
    public static GameObject LoadKeyPrefab(KeyType keyType)
    {
        try
        {
            string keyName = keyType switch
            {
                KeyType.Key => "Key",
                _ => "Key" // Default fallback
            };
            return LoadCategoryPrefab(CATEGORY_ITEMS, keyName);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error loading key prefab for {keyType}: {ex.Message}");
            return null;
        }
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
    // BATCH LOADING METHODS
    // ------------------------- //
    
    /// <summary>
    /// Preloads all essential prefabs for a specific biome to populate cache
    /// </summary>
    /// <param name="biome">Biome to preload</param>
    public static void PreloadBiome(string biome)
    {
        try
        {
            Debug.Log($"ResourceService: Preloading biome '{biome}'...");
            
            // Layout prefabs
            LoadFloorPrefab(biome);
            LoadWallPrefab(biome);
            LoadDoorPrefab(biome);
            LoadDoorTopPrefab(biome);
            LoadCeilingPrefab(biome);
            
            // Prop prefabs
            LoadTorchPrefab(biome);
            LoadPillarPrefab(biome);
            LoadBarrelPrefab(biome);
            LoadCratePrefab(biome);
            
            // Enemy prefabs
            LoadBasicEnemyPrefab(biome);
            LoadEliteEnemyPrefab(biome);
            LoadBossEnemyPrefab(biome);
            
            Debug.Log($"ResourceService: Biome '{biome}' preloaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error preloading biome '{biome}': {ex.Message}");
        }
    }
    
    /// <summary>
    /// Preloads all common (biome-independent) prefabs to populate cache
    /// </summary>
    public static void PreloadCommonPrefabs()
    {
        try
        {
            Debug.Log("ResourceService: Preloading common prefabs...");
            
            // Landmark prefabs
            LoadEntrancePrefab();
            LoadExitPrefab();
            LoadShopPrefab();
            LoadTreasurePrefab();
            LoadBossPrefab();
            
            // Player prefabs
            LoadPlayerPrefab();
            LoadCameraPrefab();
            
            // Item prefabs
            LoadHealthPotionPrefab();
            LoadCoinPrefab();
            LoadChestPrefab();
            
            // Weapon prefabs
            LoadSwordPrefab();
            LoadBowPrefab();
            
            Debug.Log("ResourceService: Common prefabs preloaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error preloading common prefabs: {ex.Message}");
        }
    }
    
    // ------------------------- //
    // CACHE MANAGEMENT METHODS
    // ------------------------- //
    
    /// <summary>
    /// Clears the entire prefab cache
    /// </summary>
    public static void ClearCache()
    {
        try
        {
            int cacheSize = _prefabCache.Count;
            _prefabCache.Clear();
            Debug.Log($"ResourceService: Cache cleared ({cacheSize} prefabs removed)");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error clearing cache: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Clears cached prefabs for a specific biome
    /// </summary>
    /// <param name="biome">Biome whose cached prefabs should be cleared</param>
    public static void ClearBiomeCache(string biome)
    {
        try
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
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error clearing biome cache for '{biome}': {ex.Message}");
        }
    }
    
    // ------------------------- //
    // UTILITY METHODS
    // ------------------------- //
    
    /// <summary>
    /// Checks if a biome-specific prefab exists without loading it
    /// </summary>
    /// <param name="category">Resource category</param>
    /// <param name="biome">Biome name</param>
    /// <param name="prefabName">Prefab name to check</param>
    /// <returns>True if the prefab exists</returns>
    public static bool PrefabExists(string category, string biome, string prefabName)
    {
        try
        {
            string path = $"{category}/{biome}/{prefabName}";
            GameObject prefab = Resources.Load<GameObject>(path);
            return prefab != null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error checking prefab existence {category}/{biome}/{prefabName}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Checks if a category prefab exists without loading it
    /// </summary>
    /// <param name="category">Resource category</param>
    /// <param name="prefabName">Prefab name to check</param>
    /// <returns>True if the prefab exists</returns>
    public static bool PrefabExists(string category, string prefabName)
    {
        try
        {
            string path = $"{category}/{prefabName}";
            GameObject prefab = Resources.Load<GameObject>(path);
            return prefab != null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error checking prefab existence {category}/{prefabName}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Logs all currently cached prefabs for debugging
    /// </summary>
    public static void LogLoadedPrefabs()
    {
        try
        {
            Debug.Log("=== ResourceService Cache ===");
            foreach (var kvp in _prefabCache)
            {
                Debug.Log($"  {kvp.Key} → {kvp.Value.name}");
            }
            Debug.Log($"Total: {_prefabCache.Count} prefabs cached");
            Debug.Log("===========================");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error logging loaded prefabs: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the current cache size
    /// </summary>
    /// <returns>Number of cached prefabs</returns>
    public static int GetCacheSize() => _prefabCache.Count;
    
    /// <summary>
    /// Gets list of available biomes by checking directory existence
    /// </summary>
    /// <returns>List of available biome names</returns>
    public static List<string> GetAvailableBiomes()
    {
        try
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
            
            Debug.Log($"ResourceService: Found {existingBiomes.Count} available biomes");
            return existingBiomes;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error getting available biomes: {ex.Message}");
            return new List<string> { BIOME_DEFAULT };
        }
    }
}