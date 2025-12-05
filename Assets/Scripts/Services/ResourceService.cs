// -------------------------------------------------- //
// Scripts/Services/ResourceService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public static class ResourceService
{
    // Cache to avoid repeated Resources.Load calls
    private static readonly Dictionary<string, GameObject> _prefabCache = new();
    private static readonly Dictionary<string, Material> _materialCache = new();
    // Biome constants
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

    private static GameObject LoadPrefab(string prefabName, string resource, string biome = null)
    {
        try
        {
            string path;
            if (biome != null) path = $"{resource}/{biome}/{prefabName}";
            else path = $"{resource}/{prefabName}";
            
            // Check cache first for performance
            if (_prefabCache.TryGetValue(path, out GameObject cached)) return cached;
            
            // Load from Resources
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab != null)
            {
                _prefabCache[path] = prefab;
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
            Debug.LogError(ex.Message);
            return null;
        }
    }

    private static Material LoadMaterial(string materialName, string resource, string biome)
    {
        try
        {
            string path;
            if (biome != null) path = $"{resource}/{biome}/{materialName}";
            else path = $"{resource}/{materialName}";
            
            // Check cache first for performance
            if (_materialCache.TryGetValue(path, out Material cached)) return cached;
            
            // Load from Resources
            Material material = Resources.Load<Material>(path);
            if (material != null)
            {
                _materialCache[path] = material;
                return material;
            }
            else
            {
                Debug.LogWarning($"ResourceService: Failed to load {path} - resource not found");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
            return null;
        }
    }
    
    // ------------------------- //
    // LAYOUT PREFABS
    // ------------------------- //
    
    public static GameObject LoadFloorPrefab()
        => LoadPrefab("pf_Floor", CATEGORY_LAYOUT);
    
    public static GameObject LoadCornerPrefab()
        => LoadPrefab("pf_Corner", CATEGORY_LAYOUT);
    
    public static GameObject LoadWallPrefab()
        => LoadPrefab("pf_Wall", CATEGORY_LAYOUT);
    
    public static GameObject LoadDoorwayPrefab()
        => LoadPrefab("pf_Doorway", CATEGORY_LAYOUT);
    
    public static GameObject LoadCeilingPrefab()
        => LoadPrefab("pf_Ceiling", CATEGORY_LAYOUT);
    
    // ------------------------- //
    // LAYOUT MATERIALS
    // ------------------------- //
    
    
    public static Material LoadFloorMaterial(string biome)
        => LoadMaterial("mat_Floor", CATEGORY_LAYOUT, biome);
    
    public static Material LoadWallMaterial(string biome)
        => LoadMaterial("mat_Wall", CATEGORY_LAYOUT, biome);
    
    public static Material LoadDoorMaterial(string biome)
        => LoadMaterial("mat_Door", CATEGORY_LAYOUT, biome);
    
    public static Material LoadCeilingMaterial(string biome)
        => LoadMaterial("mat_Ceiling", CATEGORY_LAYOUT, biome);
    
    // ------------------------- //
    // PROPS PREFABS
    // ------------------------- //
    
    public static GameObject LoadSmallProp(string biome)
        => LoadPrefab("pf_SmallProp", CATEGORY_PROPS, biome);
    
    public static GameObject LoadMediumProp(string biome)
        => LoadPrefab("pf_MediumProp", CATEGORY_PROPS, biome);
    
    public static GameObject LoadLargeProp(string biome)
        => LoadPrefab("pf_LargeProp", CATEGORY_PROPS, biome);
    
    // ------------------------- //
    // ENEMY PREFABS
    // ------------------------- //
    
    public static GameObject LoadBossEnemyPrefab(string biome)
        => LoadPrefab("pf_BossEnemy", CATEGORY_ENEMIES, biome);
    
    public static GameObject LoadMeleeEnemyPrefab(string biome)
        => LoadPrefab("pf_MeleeEnemy", CATEGORY_ENEMIES, biome);
    
    public static GameObject LoadRangedEnemyPrefab(string biome)
        => LoadPrefab("pf_RangedEnemy", CATEGORY_ENEMIES, biome);
    
    public static GameObject LoadTankEnemyPrefab(string biome)
        => LoadPrefab("pf_TankEnemy", CATEGORY_ENEMIES, biome);
    
    // ------------------------- //
    // LANDMARK PREFABS
    // ------------------------- //

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
                _ => null
            };
            if (prefabName != null) return LoadPrefab(prefabName, CATEGORY_LANDMARKS);
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
        => LoadPrefab("pf_Entrance", CATEGORY_LANDMARKS);
    
    public static GameObject LoadExitPrefab()
        => LoadPrefab("pf_Exit", CATEGORY_LANDMARKS);
    
    public static GameObject LoadShopPrefab()
        => LoadPrefab("pf_Shop", CATEGORY_LANDMARKS);

    public static GameObject LoadTreasurePrefab()
        => LoadPrefab("pf_Treasure", CATEGORY_LANDMARKS);
    
    // ------------------------- //
    // PLAYER PREFABS
    // ------------------------- //
    
    public static GameObject LoadPlayerPrefab()
        => LoadPrefab("pf_Player", CATEGORY_PLAYERS);
    
    public static GameObject LoadCameraPrefab()
        => LoadPrefab("pf_Camera", CATEGORY_PLAYERS);
    
    // ------------------------- //
    // ITEM PREFABS
    // ------------------------- //
    
    public static GameObject LoadHealthPotionPrefab()
        => LoadPrefab("pf_HealthPotion", CATEGORY_ITEMS);
    
    // ------------------------- //
    // WEAPON PREFABS
    // ------------------------- //
    
        public static GameObject LoadSwordPrefab()
        => LoadPrefab("pf_Sword", CATEGORY_WEAPONS);
    
    public static GameObject LoadBowPrefab()
        => LoadPrefab("pf_Bow", CATEGORY_WEAPONS);
    
    public static GameObject LoadStaffPrefab()
        => LoadPrefab("pf_Staff", CATEGORY_WEAPONS);
    
    public static GameObject LoadAxePrefab()
        => LoadPrefab("pf_Axe", CATEGORY_WEAPONS);
    
    public static GameObject LoadProjectilePrefab()
        => LoadPrefab("pf_Projectile", CATEGORY_WEAPONS);
    
    // ------------------------- //
    // UI PREFABS
    // ------------------------- //
    
    public static GameObject LoadHUDPrefab()
        => LoadPrefab("ui_HUD", CATEGORY_UI);
    
    public static GameObject LoadMinimapPrefab()
        => LoadPrefab(CATEGORY_UI, "MinimapPrefab");
    
    public static GameObject LoadMainMenuPrefab()
        => LoadPrefab(CATEGORY_UI, "MainMenuPrefab");
    
    public static GameObject LoadPauseMenuPrefab()
        => LoadPrefab(CATEGORY_UI, "PauseMenuPrefab");
    
    public static GameObject LoadGameOverScreenPrefab()
        => LoadPrefab(CATEGORY_UI, "GameOverScreenPrefab");
    
    // ------------------------- //
    // BATCH LOADING METHODS
    // ------------------------- //

    public static void PreloadBiome(string biome)
    {
        try
        {
            Debug.Log($"ResourceService: Preloading biome '{biome}'...");
            // Prop prefabs
            LoadSmallProp(biome);
            LoadMediumProp(biome);
            LoadLargeProp(biome);
            // Enemy prefabs
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