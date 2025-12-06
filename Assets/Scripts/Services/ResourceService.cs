// -------------------------------------------------- //
// Scripts/Services/ResourceService.cs (UPDATED)
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
    // LANDMARK PREFABS (UPDATED)
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

    public static GameObject LoadSmallHealthPotionPrefab()
        => LoadPrefab("pf_SmallHealthPotion", CATEGORY_ITEMS);

    public static GameObject LoadMediumHealthPotionPrefab()
        => LoadPrefab("pf_MediumHealthPotion", CATEGORY_ITEMS);

    public static GameObject LoadLargeHealthPotionPrefab()
        => LoadPrefab("pf_LargeHealthPotion", CATEGORY_ITEMS);

    public static GameObject LoadMaxHealthPotionPrefab()
        => LoadPrefab("pf_MaxHealthPotion", CATEGORY_ITEMS);

    public static GameObject LoadSpeedPotionPrefab()
        => LoadPrefab("pf_SpeedPotion", CATEGORY_ITEMS);

    public static GameObject LoadDamagePotionPrefab()
        => LoadPrefab("pf_DamagePotion", CATEGORY_ITEMS);

    public static GameObject LoadInvincibilityPotionPrefab()
        => LoadPrefab("pf_InvincibilityPotion", CATEGORY_ITEMS);

    public static GameObject LoadSmallGoldPrefab()
        => LoadPrefab("pf_SmallGold", CATEGORY_ITEMS);

    public static GameObject LoadLargeGoldPrefab()
        => LoadPrefab("pf_LargeGold", CATEGORY_ITEMS);

    public static GameObject LoadPowerPickupPrefab()
        => LoadPrefab("pf_PowerPickup", CATEGORY_ITEMS);

    // ------------------------- //
    // ITEM PREFAB CREATION HELPERS
    // ------------------------- //

    /// <summary>
    /// Creates a fallback health potion prefab with physics
    /// </summary>
    public static GameObject CreateHealthPotionPrefab(int healAmount, Color color)
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        prefab.name = $"HealthPotion_{healAmount}";
        prefab.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
        
        // Setup physics
        Rigidbody rb = prefab.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.drag = 2f; // Prevent rolling
        rb.angularDrag = 5f; // Prevent rolling
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // Setup collider
        CapsuleCollider col = prefab.GetComponent<CapsuleCollider>();
        col.isTrigger = true; // Trigger for pickup, but has physics
        
        // Visual setup
        SetupItemVisuals(prefab, color);
        
        return prefab;
    }

    /// <summary>
    /// Creates a fallback gold coin prefab with physics
    /// </summary>
    public static GameObject CreateSmallGoldPrefab(int amount, Color color)
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        prefab.name = $"SmallGold_{amount}";
        prefab.transform.localScale = Vector3.one * 0.3f;
        
        // Setup physics
        Rigidbody rb = prefab.AddComponent<Rigidbody>();
        rb.mass = 0.2f;
        rb.drag = 2f;
        rb.angularDrag = 5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // Setup collider
        SphereCollider col = prefab.GetComponent<SphereCollider>();
        col.isTrigger = true;
        
        // Visual setup
        SetupItemVisuals(prefab, color);
        
        return prefab;
    }

    /// <summary>
    /// Creates a fallback gold pile prefab with physics
    /// </summary>
    public static GameObject CreateLargeGoldPrefab(int amount, Color color)
    {
        GameObject prefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        prefab.name = $"LargeGold_{amount}";
        prefab.transform.localScale = new Vector3(0.5f, 0.3f, 0.5f);
        
        // Setup physics
        Rigidbody rb = prefab.AddComponent<Rigidbody>();
        rb.mass = 0.8f;
        rb.drag = 3f;
        rb.angularDrag = 10f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // Setup collider
        BoxCollider col = prefab.GetComponent<BoxCollider>();
        col.isTrigger = true;
        
        // Visual setup
        SetupItemVisuals(prefab, color);
        
        return prefab;
    }

    /// <summary>
    /// Sets up visual properties for item prefabs
    /// </summary>
    private static void SetupItemVisuals(GameObject prefab, Color color)
    {
        Renderer renderer = prefab.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 1.5f);
            renderer.material = mat;
        }
    }
    
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
    // UI PREFABS (UPDATED)
    // ------------------------- //
    
    public static GameObject LoadHUDUI()
        => LoadPrefab("ui_HUD", CATEGORY_UI);
    
    public static GameObject LoadMobileControlsUI()
        => LoadPrefab("ui_MobileControls", CATEGORY_UI);
    
    public static GameObject LoadShopUI()
        => LoadPrefab("ui_Shop", CATEGORY_UI);
    
    public static GameObject LoadCrosshairUI()
        => LoadPrefab("ui_Crosshair", CATEGORY_UI);
    
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