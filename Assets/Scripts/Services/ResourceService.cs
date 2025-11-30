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

    private static GameObject LoadBiomeSpecificPrefab(string category, string biome, string prefabName)
    {
        try
        {
            string path = $"{category}/{biome}/{prefabName}";
            // Check cache first for performance
            if (_prefabCache.TryGetValue(path, out GameObject cached)) return cached;
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
    
    private static GameObject LoadCategoryPrefab(string category, string prefabName)
    {
        try
        {
            string path = $"{category}/{prefabName}";
            // Check cache first for performance
            if (_prefabCache.TryGetValue(path, out GameObject cached)) return cached;
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
    
    // public static GameObject LoadBasicEnemyPrefab(string biome)
    //     => LoadWithFallback(CATEGORY_ENEMIES, biome, "BasicEnemyPrefab");
    
    // public static GameObject LoadEliteEnemyPrefab(string biome)
    //     => LoadWithFallback(CATEGORY_ENEMIES, biome, "EliteEnemyPrefab");
    
    public static GameObject LoadBossEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "pf_BossEnemy");
    
    public static GameObject LoadMeleeEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "pf_MeleeEnemy");
    
    public static GameObject LoadRangedEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "pf_RangedEnemy");
    
    public static GameObject LoadTankEnemyPrefab(string biome)
        => LoadWithFallback(CATEGORY_ENEMIES, biome, "pf_TankEnemy");
    
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
                RoomType.Entrance => "pf_Entrance",
                RoomType.Exit => "pf_Exit",
                RoomType.Shop => "pf_Shop",
                RoomType.Treasure => "pf_Treasure",
                RoomType.Boss => "BossPrefab",
                _ => null
            };
            if (prefabName != null) return LoadCategoryPrefab(CATEGORY_LANDMARKS, prefabName);
            Debug.LogWarning($"ResourceService: No prefab mapping for room type {roomType}");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error loading landmark for {roomType}: {ex.Message}");
            return null;
        }
    }
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadEntrancePrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "pf_Entrance");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadExitPrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "pf_Exit");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadShopPrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "pf_Shop");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadTreasurePrefab()
        => LoadCategoryPrefab(CATEGORY_LANDMARKS, "pf_Treasure");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
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
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadItemPrefab(string itemName)
        => LoadCategoryPrefab(CATEGORY_ITEMS, itemName);
    
    public static GameObject LoadHealthPotionPrefab()
        => LoadCategoryPrefab(CATEGORY_ITEMS, "HealthPotionPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadCoinPrefab()
        => LoadCategoryPrefab(CATEGORY_ITEMS, "CoinPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadChestPrefab()
        => LoadCategoryPrefab(CATEGORY_ITEMS, "ChestPrefab");
    
    /// <summary>
    /// Loads key prefab based on key type
    /// </summary>
    /// <param name="keyType">Type of key to load</param>
    /// <returns>Loaded key prefab or null if not found</returns>
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
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
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadWeaponPrefab(string weaponName)
        => LoadCategoryPrefab(CATEGORY_WEAPONS, weaponName);
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
        public static GameObject LoadSwordPrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "pf_Sword");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadBowPrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "BowPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadStaffPrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "StaffPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadAxePrefab()
        => LoadCategoryPrefab(CATEGORY_WEAPONS, "pf_Axe");
    
    // ------------------------- //
    // UI PREFABS (Biome-Independent)
    // ------------------------- //
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadUIPrefab(string uiName)
        => LoadCategoryPrefab(CATEGORY_UI, uiName);
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadHealthBarPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "HealthBarPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadMinimapPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "MinimapPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadMainMenuPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "MainMenuPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
    public static GameObject LoadPauseMenuPrefab()
        => LoadCategoryPrefab(CATEGORY_UI, "PauseMenuPrefab");
    
    // NOTE TO CLAUDE: This currently has 0 references. Let's delete it if it is not planned for use now or in the future.
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
            // LoadBasicEnemyPrefab(biome);
            // LoadEliteEnemyPrefab(biome);
            LoadMeleeEnemyPrefab(biome);
            LoadRangedEnemyPrefab(biome);
            LoadTankEnemyPrefab(biome);
            LoadBossEnemyPrefab(biome);
            Debug.Log($"ResourceService: Biome '{biome}' preloaded successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error preloading biome '{biome}': {ex.Message}");
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
            foreach (var key in _prefabCache.Keys) if (key.Contains($"/{biome}/")) keysToRemove.Add(key);
            foreach (var key in keysToRemove) _prefabCache.Remove(key);
            Debug.Log($"ResourceService: Cleared {keysToRemove.Count} cached prefabs for biome '{biome}'");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"ResourceService: Error clearing biome cache for '{biome}': {ex.Message}");
        }
    }
}