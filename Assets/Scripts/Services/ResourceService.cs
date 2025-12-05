// -------------------------------------------------- //
// Scripts/Services/ResourceService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public static class ResourceService
{
    private static readonly Dictionary<string, GameObject> _prefabCache = new();
    private static readonly Dictionary<string, Material> _materialCache = new();
    
    public const string BIOME_GRASSLANDS = "Grasslands";
    public const string BIOME_DUNGEON = "Dungeon";
    public const string BIOME_CAVES = "Caves";
    
    private const string RESOURCE_LAYOUT = "Layout";
    private const string RESOURCE_PROPS = "Props";
    private const string RESOURCE_ENEMIES = "Enemies";
    
    private const string RESOURCE_LANDMARKS = "Landmarks";
    private const string RESOURCE_PLAYERS = "Players";
    private const string RESOURCE_ITEMS = "Items";
    private const string RESOURCE_WEAPONS = "Weapons";
    
    private static GameObject LoadPrefab(string prefabName, string resource, string biome = null)
    {
        try
        {
            string path;
            if (biome != null) path = $"{resource}/{biome}/{prefabName}";
            else path = $"{resource}/{prefabName}";

            if (_prefabCache.TryGetValue(path, out GameObject cached)) return cached;

            GameObject prefab = Resources.Load<GameObject>(path);

            if (prefab != null) return _prefabCache[path];
            else
            {
                Debug.LogWarning($"Resource not found at path \"{path}\".");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading prefab {prefabName}: {ex.Message}");
            return null;
        }
    }

    private static Material LoadMaterial(string materialName, string resource, string biome = null)
    {
        try
        {
            string path;
            if (biome != null) path = $"{resource}/{biome}/{materialName}";
            else path = $"{resource}/{materialName}";

            if (_materialCache.TryGetValue(path, out Material cached)) return cached;

            Material material = Resources.Load<Material>(path);

            if (material != null) return _materialCache[path];
            else
            {
                Debug.LogWarning($"Resource not found at path \"{path}\".");
                return null;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading material {materialName}: {ex.Message}");
            return null;
        }
    }
    
    // ------------------------- //
    // LAYOUT PREFABS
    // ------------------------- //
    
    public static GameObject LoadFloorPrefab()
        => LoadPrefab(RESOURCE_LAYOUT, "pf_Floor");
    
    public static GameObject LoadWallPrefab()
        => LoadPrefab(RESOURCE_LAYOUT, "pf_Wall");
    
    public static GameObject LoadDoorwayPrefab()
        => LoadPrefab(RESOURCE_LAYOUT, "pf_Doorway");
    
    public static GameObject LoadCornerPrefab()
        => LoadPrefab(RESOURCE_LAYOUT, "pf_Corner");
    
    public static GameObject LoadCeilingPrefab()
        => LoadPrefab(RESOURCE_LAYOUT, "pf_Ceiling");
    
    // ------------------------- //
    // BIOME LAYOUT PREFABS
    // ------------------------- //
    
    public static Material LoadFloorMaterial(string biome)
        => LoadMaterial(RESOURCE_LAYOUT, biome, "mat_Floor");
    
    public static Material LoadWallMaterial(string biome)
        => LoadMaterial(RESOURCE_LAYOUT, biome, "mat_Wall");
    
    public static Material LoadDoorMaterial(string biome)
        => LoadMaterial(RESOURCE_LAYOUT, biome, "mat_Door");
    
    public static Material LoadCeilingMaterial(string biome)
        => LoadMaterial(RESOURCE_LAYOUT, biome, "mat_Ceiling");
    
    // ------------------------- //
    // PROPS PREFABS
    // ------------------------- //
    
    // Small props are 1x1.
    public static GameObject LoadSmallPropPrefab(string biome)
        => LoadPrefab(RESOURCE_PROPS, biome, "pf_SmallProp");

    // Medium props are 1x2 or 2x1.
    public static GameObject LoadMediumPropPrefab(string biome)
        => LoadPrefab(RESOURCE_PROPS, biome, "pf_MediumProp");

    // Large props are 2x2.
    public static GameObject LoadLargePropPrefab(string biome)
        => LoadPrefab(RESOURCE_PROPS, biome, "pf_LargeProp");
    
    // ------------------------- //
    // ENEMY PREFABS
    // ------------------------- //
    
    public static GameObject LoadMeleeEnemyPrefab(string biome)
        => LoadPrefab(RESOURCE_ENEMIES, biome, "pf_MeleeEnemy");
    
    public static GameObject LoadRangedEnemyPrefab(string biome)
        => LoadPrefab(RESOURCE_ENEMIES, biome, "pf_RangedEnemy");
    
    public static GameObject LoadTankEnemyPrefab(string biome)
        => LoadPrefab(RESOURCE_ENEMIES, biome, "pf_TankEnemy");
    
    public static GameObject LoadBossEnemyPrefab(string biome)
        => LoadPrefab(RESOURCE_ENEMIES, biome, "pf_BossEnemy");
    
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
            if (prefabName != null) 
                return LoadPrefab(RESOURCE_LANDMARKS, prefabName);

            Debug.LogWarning($"No prefab mapping for room type {roomType}");
            return null;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error loading landmark for {roomType}: {ex.Message}");
            return null;
        }
    }
    
    public static GameObject LoadEntrancePrefab()
        => LoadPrefab(RESOURCE_LANDMARKS, "pf_Entrance");
    
    public static GameObject LoadExitPrefab()
        => LoadPrefab(RESOURCE_LANDMARKS, "pf_Exit");
    
    public static GameObject LoadShopPrefab()
        => LoadPrefab(RESOURCE_LANDMARKS, "pf_Shop");
    
    public static GameObject LoadTreasurePrefab()
        => LoadPrefab(RESOURCE_LANDMARKS, "pf_Treasure");
    
    // ------------------------- //
    // PLAYER PREFABS
    // ------------------------- //
    
    public static GameObject LoadPlayerPrefab()
        => LoadPrefab(RESOURCE_PLAYERS, "pf_Player");
    
    public static GameObject LoadCameraPrefab()
        => LoadPrefab(RESOURCE_PLAYERS, "pf_Camera");
    
    // ------------------------- //
    // ITEM PREFABS
    // ------------------------- //
    
    public static GameObject LoadHealthPotionPrefab()
        => LoadPrefab(RESOURCE_ITEMS, "pf_HealthPotion");
    
    public static GameObject LoadChestPrefab()
        => LoadPrefab(RESOURCE_ITEMS, "pf_ManaPotion");
    
    // ------------------------- //
    // WEAPON PREFABS
    // ------------------------- //
    
    public static GameObject LoadSwordPrefab()
        => LoadPrefab(RESOURCE_WEAPONS, "pf_Sword");
    
    public static GameObject LoadBowPrefab()
        => LoadPrefab(RESOURCE_WEAPONS, "pf_Bow");
    
    public static GameObject LoadStaffPrefab()
        => LoadPrefab(RESOURCE_WEAPONS, "pf_Staff");
    
    public static GameObject LoadAxePrefab()
        => LoadPrefab(RESOURCE_WEAPONS, "pf_Axe");
    
    // ------------------------- //
    // BATCH LOADING METHODS
    // ------------------------- //

    public static void PreloadBiome(string biome)
    {
        try
        {
            Debug.Log($"ResourceService: Preloading biome '{biome}'...");
            // Layout prefabs
            LoadFloorMaterial(biome);
            LoadWallMaterial(biome);
            LoadDoorMaterial(biome);
            LoadCeilingMaterial(biome);
            // Prop prefabs
            LoadSmallPropPrefab(biome);
            LoadMediumPropPrefab(biome);
            LoadLargePropPrefab(biome);
            // Enemy prefabs
            LoadMeleeEnemyPrefab(biome);
            LoadRangedEnemyPrefab(biome);
            LoadTankEnemyPrefab(biome);
            LoadBossEnemyPrefab(biome);
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

            Debug.Log($"Cleared {keysToRemove.Count} cached prefabs for biome '{biome}'");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error clearing biome cache for '{biome}': {ex.Message}");
        }
    }
}