// ================================================== //
// Scripts/Registries/ItemRegistry.cs - Item Database
// ================================================== //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central registry for all items in the game
/// Manages item prefabs, drop tables, and shop inventories
/// </summary>
public static class ItemRegistry
{
    // ==========================================
    // ITEM DEFINITIONS
    // ==========================================
    
    [System.Serializable]
    public class ItemDefinition
    {
        public string itemID;
        public string itemName;
        public ItemType itemType;
        public GameObject prefab;
        public int baseValue;
        public float dropWeight; // Higher = more common
        public string description;
    }
    
    // ==========================================
    // ITEM DATABASE
    // ==========================================
    
    private static Dictionary<string, ItemDefinition> _itemDatabase;
    
    static ItemRegistry()
    {
        InitializeDatabase();
    }
    
    private static void InitializeDatabase()
    {
        _itemDatabase = new Dictionary<string, ItemDefinition>
        {
            // HEALTH POTIONS
            {
                "small_health", new ItemDefinition
                {
                    itemID = "small_health",
                    itemName = "Small Health Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateHealthPotionPrefab(30, new Color(1f, 0.3f, 0.3f)),
                    baseValue = 20,
                    dropWeight = 10f,
                    description = "Restores 30 HP"
                }
            },
            {
                "medium_health", new ItemDefinition
                {
                    itemID = "medium_health",
                    itemName = "Medium Health Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateHealthPotionPrefab(50, new Color(1f, 0.2f, 0.2f)),
                    baseValue = 50,
                    dropWeight = 5f,
                    description = "Restores 50 HP"
                }
            },
            {
                "large_health", new ItemDefinition
                {
                    itemID = "large_health",
                    itemName = "Large Health Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateHealthPotionPrefab(100, new Color(1f, 0.1f, 0.1f)),
                    baseValue = 100,
                    dropWeight = 2f,
                    description = "Restores 100 HP"
                }
            },
            {
                "max_health", new ItemDefinition
                {
                    itemID = "max_health",
                    itemName = "Max Health Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateMaxHealthPotionPrefab(),
                    baseValue = 200,
                    dropWeight = 0.5f,
                    description = "Fully restores HP"
                }
            },
            
            // BUFF POTIONS
            {
                "speed_boost", new ItemDefinition
                {
                    itemID = "speed_boost",
                    itemName = "Speed Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateSpeedPotionPrefab(),
                    baseValue = 75,
                    dropWeight = 3f,
                    description = "+50% speed for 10s"
                }
            },
            {
                "damage_boost", new ItemDefinition
                {
                    itemID = "damage_boost",
                    itemName = "Strength Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateDamagePotionPrefab(),
                    baseValue = 100,
                    dropWeight = 2f,
                    description = "+50% damage for 15s"
                }
            },
            {
                "invincibility", new ItemDefinition
                {
                    itemID = "invincibility",
                    itemName = "Invincibility Potion",
                    itemType = ItemType.Consumable,
                    prefab = CreateInvincibilityPotionPrefab(),
                    baseValue = 300,
                    dropWeight = 0.2f,
                    description = "Invulnerable for 5s"
                }
            },
            
            // CURRENCY
            {
                "small_gold", new ItemDefinition
                {
                    itemID = "small_gold",
                    itemName = "Pile of Gold",
                    itemType = ItemType.Currency,
                    prefab = CreateSmallGoldPrefab(10),
                    baseValue = 10,
                    dropWeight = 15f,
                    description = "10 Gold"
                }
            },
            {
                "gold_pile", new ItemDefinition
                {
                    itemID = "gold_pile",
                    itemName = "Gold Pile",
                    itemType = ItemType.Currency,
                    prefab = CreateLargeGoldPrefab(50),
                    baseValue = 50,
                    dropWeight = 3f,
                    description = "50 Gold"
                }
            }
        };
        
        Debug.Log($"ItemRegistry: Initialized with {_itemDatabase.Count} items");
    }
    
    // ==========================================
    // PREFAB CREATION HELPERS
    // ==========================================

    private static GameObject CreateHealthPotionPrefab(int healAmount, Color color)
    {
        // Try to load from ResourceService first
        GameObject prefab = null;
        
        if (healAmount <= 30)
            prefab = ResourceService.LoadSmallHealthPotionPrefab();
        else if (healAmount <= 50)
            prefab = ResourceService.LoadMediumHealthPotionPrefab();
        else if (healAmount <= 100)
            prefab = ResourceService.LoadLargeHealthPotionPrefab();
        
        // Fallback: Create simple prefab
        if (prefab == null)
        {
            prefab = ResourceService.CreateHealthPotionPrefab(healAmount, color);
        }
        
        // Add component based on heal amount
        if (healAmount <= 30 && prefab.GetComponent<SmallHealthPotion>() == null)
            prefab.AddComponent<SmallHealthPotion>().healAmount = healAmount;
        else if (healAmount <= 50 && prefab.GetComponent<MediumHealthPotion>() == null)
            prefab.AddComponent<MediumHealthPotion>().healAmount = healAmount;
        else if (healAmount <= 100 && prefab.GetComponent<LargeHealthPotion>() == null)
            prefab.AddComponent<LargeHealthPotion>().healAmount = healAmount;
        
        return prefab;
    }
    
    private static GameObject CreateMaxHealthPotionPrefab()
    {
        GameObject prefab = ResourceService.LoadMaxHealthPotionPrefab();
        
        if (prefab == null)
        {
            prefab = ResourceService.CreateHealthPotionPrefab(999, new Color(1f, 0.8f, 0.2f));
        }
        
        if (prefab.GetComponent<MaxHealthPotion>() == null)
        {
            prefab.AddComponent<MaxHealthPotion>();
        }
        
        return prefab;
    }
    
    private static GameObject CreateSpeedPotionPrefab()
    {
        GameObject prefab = ResourceService.LoadSpeedPotionPrefab();
        
        if (prefab == null)
        {
            prefab = ResourceService.CreateHealthPotionPrefab(0, new Color(0.2f, 1f, 0.2f));
        }
        
        if (prefab.GetComponent<SpeedBoostPotion>() == null)
        {
            prefab.AddComponent<SpeedBoostPotion>();
        }
        
        return prefab;
    }

    private static GameObject CreateDamagePotionPrefab()
    {
        GameObject prefab = ResourceService.LoadDamagePotionPrefab();
        
        if (prefab == null)
        {
            prefab = ResourceService.CreateHealthPotionPrefab(0, new Color(1f, 0.5f, 0.2f));
        }
        
        if (prefab.GetComponent<DamageBoostPotion>() == null)
        {
            prefab.AddComponent<DamageBoostPotion>();
        }
        
        return prefab;
    }

    private static GameObject CreateInvincibilityPotionPrefab()
    {
        GameObject prefab = ResourceService.LoadInvincibilityPotionPrefab();
        
        if (prefab == null)
        {
            prefab = ResourceService.CreateHealthPotionPrefab(0, new Color(1f, 1f, 0.2f));
        }
        
        if (prefab.GetComponent<InvincibilityPotion>() == null)
        {
            prefab.AddComponent<InvincibilityPotion>();
        }
        
        return prefab;
    }

    private static GameObject CreateSmallGoldPrefab(int amount)
    {
        GameObject prefab = ResourceService.LoadSmallGoldPrefab();
        
        if (prefab == null)
        {
            prefab = ResourceService.CreateSmallGoldPrefab(amount, new Color(1f, 0.84f, 0f));
        }
        
        if (prefab.GetComponent<GoldItemModel>() == null)
        {
            prefab.AddComponent<GoldItemModel>().goldAmount = amount;
        }
        
        return prefab;
    }

    private static GameObject CreateLargeGoldPrefab(int amount)
    {
        GameObject prefab = ResourceService.LoadLargeGoldPrefab();
        
        if (prefab == null)
        {
            prefab = ResourceService.CreateLargeGoldPrefab(amount, new Color(1f, 0.84f, 0f));
        }
        
        if (prefab.GetComponent<GoldItemModel>() == null)
        {
            prefab.AddComponent<GoldItemModel>().goldAmount = amount;
        }
        
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
    
    // ==========================================
    // PUBLIC API - ITEM ACCESS
    // ==========================================
    
    public static ItemDefinition GetItem(string itemID)
    {
        if (_itemDatabase.TryGetValue(itemID, out ItemDefinition item))
        {
            return item;
        }
        
        Debug.LogWarning($"ItemRegistry: Item '{itemID}' not found");
        return null;
    }
    
    public static GameObject GetItemPrefab(string itemID)
    {
        ItemDefinition item = GetItem(itemID);
        return item?.prefab;
    }
    
    public static List<ItemDefinition> GetAllItems()
    {
        return _itemDatabase.Values.ToList();
    }
    
    public static List<ItemDefinition> GetItemsByType(ItemType type)
    {
        return _itemDatabase.Values
            .Where(item => item.itemType == type)
            .ToList();
    }
    
    // ==========================================
    // DROP TABLES
    // ==========================================
    
    /// <summary>
    /// Get random item drop based on weighted drop table
    /// </summary>
    public static string GetRandomDrop(System.Random random = null)
    {
        if (random == null) random = new System.Random();
        
        // Calculate total weight
        float totalWeight = _itemDatabase.Values.Sum(item => item.dropWeight);
        
        // Random roll
        float roll = (float)random.NextDouble() * totalWeight;
        
        // Find item
        float currentWeight = 0f;
        foreach (var kvp in _itemDatabase)
        {
            currentWeight += kvp.Value.dropWeight;
            if (roll <= currentWeight)
            {
                return kvp.Key;
            }
        }
        
        // Fallback to gold coin
        return "small_gold";
    }
    
    /// <summary>
    /// Get enemy-specific drop table
    /// </summary>
    public static string GetEnemyDrop(string enemyType, int floorLevel, System.Random random = null)
    {
        if (random == null) random = new System.Random();
        
        // Base drop chance (60%)
        if (random.NextDouble() > 0.6f)
        {
            return null; // No drop
        }
        
        // Boss always drops good loot
        if (enemyType.Contains("Boss"))
        {
            return GetBossDrop(random);
        }
        
        // Higher floor = better drops
        float rarityModifier = 1f + (floorLevel * 0.1f);
        
        // Weighted selection with rarity modifier
        var dropTable = _itemDatabase.Values
            .Select(item => new
            {
                itemID = item.itemID,
                weight = item.dropWeight * (item.baseValue < 100 ? rarityModifier : 1f)
            })
            .ToList();
        
        float totalWeight = dropTable.Sum(item => item.weight);
        float roll = (float)random.NextDouble() * totalWeight;
        
        float currentWeight = 0f;
        foreach (var item in dropTable)
        {
            currentWeight += item.weight;
            if (roll <= currentWeight)
            {
                return item.itemID;
            }
        }
        
        return "small_gold";
    }
    
    /// <summary>
    /// Boss-specific high-value drops
    /// </summary>
    private static string GetBossDrop(System.Random random)
    {
        // Bosses drop guaranteed good loot
        string[] bossDrops = new[]
        {
            "large_health",
            "max_health",
            "damage_boost",
            "invincibility",
            "gold_pile"
        };
        
        return bossDrops[random.Next(bossDrops.Length)];
    }
    
    // ==========================================
    // SHOP INVENTORY GENERATION
    // ==========================================
    
    /// <summary>
    /// Generate shop inventory for current floor
    /// </summary>
    public static List<string> GenerateShopInventory(int floorLevel, int itemCount = 6)
    {
        List<string> inventory = new List<string>();
        System.Random random = new System.Random();
        
        // Always include basic health potions
        inventory.Add("small_health");
        inventory.Add("medium_health");
        
        // Add floor-appropriate items
        var availableItems = _itemDatabase.Values
            .Where(item => item.itemType == ItemType.Consumable)
            .Where(item => item.baseValue <= 100 + (floorLevel * 50)) // Price scaling
            .Select(item => item.itemID)
            .ToList();
        
        // Fill remaining slots
        while (inventory.Count < itemCount && availableItems.Count > 0)
        {
            string randomItem = availableItems[random.Next(availableItems.Count)];
            if (!inventory.Contains(randomItem))
            {
                inventory.Add(randomItem);
            }
            availableItems.Remove(randomItem);
        }
        
        return inventory;
    }
    
    /// <summary>
    /// Get shop price for item (includes markup)
    /// </summary>
    public static int GetShopPrice(string itemID, float markupMultiplier = 1.5f)
    {
        ItemDefinition item = GetItem(itemID);
        if (item == null) return 0;
        
        return Mathf.RoundToInt(item.baseValue * markupMultiplier);
    }
    
    // ==========================================
    // ITEM SPAWNING
    // ==========================================
    
    /// <summary>
    /// Spawn item in world at position
    /// </summary>
    public static GameObject SpawnItem(string itemID, Vector3 position)
    {
        ItemDefinition item = GetItem(itemID);
        if (item?.prefab == null)
        {
            Debug.LogWarning($"Cannot spawn item '{itemID}' - prefab not found");
            return null;
        }
        
        // Spawn slightly above ground for physics drop
        Vector3 spawnPos = position + Vector3.up * 0.5f;
        GameObject instance = Object.Instantiate(item.prefab, spawnPos, Quaternion.identity);
        instance.name = item.itemName;
        
        // Add slight random force for variety
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 randomForce = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(1f, 2f),
                Random.Range(-1f, 1f)
            );
            rb.AddForce(randomForce, ForceMode.Impulse);
        }
        
        return instance;
    }
    
    /// <summary>
    /// Spawn random item drop from enemy with physics
    /// </summary>
    public static GameObject SpawnEnemyDrop(Vector3 position, string enemyType, int floorLevel)
    {
        System.Random random = new System.Random();
        string itemID = GetEnemyDrop(enemyType, floorLevel, random);
        
        if (itemID == null) return null;
        
        return SpawnItem(itemID, position);
    }
}