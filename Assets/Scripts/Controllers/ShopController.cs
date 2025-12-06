// ================================================== //
// FILE 1: ShopController.cs - Refactored
// ================================================== //

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Shop controller that manages shop inventory and item purchases
/// Items are physical world objects that drop near the shop when purchased
/// </summary>
public class ShopController : MonoBehaviour
{
    [Header("Shop Settings")]
    public Transform itemSpawnPoint; // Where items appear when purchased
    public float itemSpawnRadius = 2f;
    
    [Header("Shop Inventory")]
    private List<string> shopInventory = new List<string>();
    private Dictionary<string, int> itemPrices = new Dictionary<string, int>();
    
    [Header("State")]
    public bool isPlayerNearby = false;
    private PlayerController player;
    
    [Header("UI")]
    private ShopDisplay shopDisplay;
    
    void Start()
    {
        GenerateShopInventory();
        
        // Set spawn point if not assigned
        if (itemSpawnPoint == null)
        {
            itemSpawnPoint = transform;
        }
    }
    
    void Update()
    {
        // Open shop with E key
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }
    
    // ==========================================
    // INVENTORY GENERATION
    // ==========================================
    
    private void GenerateShopInventory()
    {
        shopInventory.Clear();
        itemPrices.Clear();
        
        // Get current floor level
        int floorLevel = GetCurrentFloorLevel();
        
        // Generate inventory from ItemRegistry
        shopInventory = ItemRegistry.GenerateShopInventory(floorLevel, 6);
        
        // Calculate prices
        foreach (string itemID in shopInventory)
        {
            int price = ItemRegistry.GetShopPrice(itemID);
            itemPrices[itemID] = price;
        }
        
        Debug.Log($"Shop generated with {shopInventory.Count} items for floor {floorLevel}");
    }
    
    private int GetCurrentFloorLevel()
    {
        LayoutManager layoutManager = GameDirector.Instance?.layoutManager;
        if (layoutManager != null && layoutManager.LevelConfig != null)
        {
            return layoutManager.LevelConfig.FloorLevel;
        }
        return 1;
    }
    
    // ==========================================
    // SHOP INTERACTION
    // ==========================================
    
    private void OpenShop()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowShopDisplay(this);
        }
        else
        {
            Debug.LogWarning("UIManager not found - cannot open shop UI");
        }
    }
    
    public void CloseShop()
    {
        isPlayerNearby = false;
        player = null;
    }
    
    // ==========================================
    // PURCHASE SYSTEM
    // ==========================================
    
    /// <summary>
    /// Attempt to purchase item - spawns item in world if successful
    /// </summary>
    public bool PurchaseItem(string itemID)
    {
        if (player == null || player.inventory == null)
        {
            Debug.LogWarning("Cannot purchase - player not found");
            return false;
        }
        
        // Check if item is in stock
        if (!shopInventory.Contains(itemID))
        {
            Debug.LogWarning($"Item '{itemID}' not in shop inventory");
            return false;
        }
        
        // Get price
        if (!itemPrices.TryGetValue(itemID, out int price))
        {
            Debug.LogWarning($"No price set for item '{itemID}'");
            return false;
        }
        
        // Check if player can afford
        if (player.inventory.gold < price)
        {
            Debug.Log("Not enough gold!");
            return false;
        }
        
        // Process purchase
        player.inventory.SpendGold(price);
        
        // Spawn item in world near shop
        SpawnPurchasedItem(itemID);
        
        Debug.Log($"Purchased {itemID} for {price} gold");
        
        return true;
    }
    
    /// <summary>
    /// Spawn purchased item in world for player to pick up
    /// </summary>
    private void SpawnPurchasedItem(string itemID)
    {
        // Calculate spawn position
        Vector3 spawnPos = GetItemSpawnPosition();
        
        // Spawn item using ItemRegistry
        GameObject item = ItemRegistry.SpawnItem(itemID, spawnPos);
        
        if (item != null)
        {
            Debug.Log($"Item '{itemID}' spawned at {spawnPos}");
            
            // Visual feedback - spawn with a pop effect
            PlayPurchaseEffect(spawnPos);
        }
        else
        {
            Debug.LogError($"Failed to spawn item '{itemID}'");
        }
    }
    
    private Vector3 GetItemSpawnPosition()
    {
        // Random position around spawn point
        Vector2 randomCircle = Random.insideUnitCircle * itemSpawnRadius;
        Vector3 offset = new Vector3(randomCircle.x, 0.5f, randomCircle.y);
        
        return itemSpawnPoint.position + offset;
    }
    
    private void PlayPurchaseEffect(Vector3 position)
    {
        // Create simple particle effect
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = position;
        effect.transform.localScale = Vector3.one * 0.5f;
        
        Renderer renderer = effect.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = Color.yellow;
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", Color.yellow * 2f);
        renderer.material = mat;
        
        Object.Destroy(effect.GetComponent<Collider>());
        Object.Destroy(effect, 0.5f);
    }
    
    // ==========================================
    // PUBLIC API FOR UI
    // ==========================================
    
    public List<string> GetShopInventory()
    {
        return new List<string>(shopInventory);
    }
    
    public int GetItemPrice(string itemID)
    {
        if (itemPrices.TryGetValue(itemID, out int price))
        {
            return price;
        }
        return 0;
    }
    
    public bool CanAfford(string itemID)
    {
        if (player == null || player.inventory == null) return false;
        
        int price = GetItemPrice(itemID);
        return player.inventory.gold >= price;
    }
    
    // ==========================================
    // TRIGGER DETECTION
    // ==========================================
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = other.GetComponent<PlayerController>();
            
            Debug.Log("Press E to open shop");
            
            // Show shop prompt
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowTooltip("Press E to shop", Input.mousePosition);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;
            
            // Hide tooltip
            if (UIManager.Instance != null)
            {
                UIManager.Instance.HideTooltip();
            }
        }
    }
}

// ================================================== //
// FILE 3: BossSpawner.cs (Move this to its own file)
// ================================================== //

// using UnityEngine;
// using System.Collections;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Settings")]
    public GameObject bossPrefab;
    public bool hasSpawned = false;
    public bool bossDefeated = false;
    
    [Header("Rewards")]
    public PowerType guaranteedPower;
    public int goldReward = 500;
    
    private GameObject currentBoss;
    
    void Start()
    {
        // Auto-spawn boss after short delay
        if (!hasSpawned)
        {
            StartCoroutine(SpawnBossDelayed());
        }
    }
    
    private IEnumerator SpawnBossDelayed()
    {
        yield return new WaitForSeconds(2f);
        SpawnBoss();
    }
    
    private void SpawnBoss()
    {
        if (hasSpawned) return;
        
        hasSpawned = true;
        
        // Load boss prefab if not assigned
        if (bossPrefab == null)
        {
            LayoutManager layoutManager = FindObjectOfType<LayoutManager>();
            if (layoutManager != null)
            {
                BiomeManager biomeManager = layoutManager.GetComponent<BiomeManager>();
                if (biomeManager != null)
                {
                    bossPrefab = ResourceService.LoadBossEnemyPrefab(biomeManager.CurrentBiome);
                }
            }
        }
        
        if (bossPrefab == null)
        {
            Debug.LogError("BossSpawner: No boss prefab available!");
            return;
        }
        
        // Spawn boss at this position
        Vector3 spawnPosition = transform.position + Vector3.up;
        currentBoss = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        currentBoss.name = "Boss";
        
        // Make boss stronger
        if (currentBoss.TryGetComponent<EnemyController>(out var bossController))
        {
            bossController.maxHealth = 200;
            bossController.damage = 25;
            bossController.moveSpeed = 3f;
            
            // Subscribe to boss death
            StartCoroutine(WaitForBossDeath(bossController));
        }
        
        Debug.Log("Boss spawned!");
    }
    
    private IEnumerator WaitForBossDeath(EnemyController boss)
    {
        // Wait until boss is destroyed
        while (currentBoss != null)
        {
            yield return new WaitForSeconds(0.5f);
        }
        
        OnBossDefeated();
    }
    
    private void OnBossDefeated()
    {
        if (bossDefeated) return;
        
        bossDefeated = true;
        Debug.Log("BOSS DEFEATED!");
        
        // Spawn rewards
        SpawnBossRewards();
        
        // Unlock exit if this is the boss room
        UnlockExit();
    }
    
    private void SpawnBossRewards()
    {
        // Spawn power pickup
        Vector3 rewardPosition = transform.position + Vector3.up;
        
        // Create power pickup object
        GameObject powerPickup = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        powerPickup.transform.position = rewardPosition;
        powerPickup.transform.localScale = Vector3.one * 0.5f;
        powerPickup.name = "BossPowerReward";
        
        // Make it glow
        Renderer renderer = powerPickup.GetComponent<Renderer>();
        Material glowMat = new(Shader.Find("Standard"))
        {
            color = Color.yellow
        };
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", Color.yellow * 2f);
        renderer.material = glowMat;
        
        // Add pickup script
        PowerPickup pickup = powerPickup.AddComponent<PowerPickup>();
        pickup.powerType = guaranteedPower;
        
        Debug.Log($"Boss dropped power: {guaranteedPower}");
    }
    
    private void UnlockExit()
    {
        // Find and unlock the exit portal
        ProgressionManager progressionManager = FindObjectOfType<ProgressionManager>();
        if (progressionManager != null)
        {
            // Enable exit portal
            Debug.Log("Exit portal unlocked!");
        }
    }
}