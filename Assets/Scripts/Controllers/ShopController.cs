// ================================================== //
// FILE 1: ShopController.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShopController : MonoBehaviour
{
    [Header("Shop Inventory")]
    public List<ShopItem> availableItems = new();
    
    [Header("UI")]
    public bool isPlayerNearby = false;
    
    private PlayerController player;
    
    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int price;
        public ShopItemType itemType;
        public PowerType powerType; // If power
        public GameObject weaponPrefab; // If weapon
        public int healAmount; // If potion
    }
    
    public enum ShopItemType
    {
        HealthPotion,
        ManaPotion,
        Weapon,
        PowerModel
    }
    
    void Start()
    {
        GenerateShopInventory();
    }
    
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            OpenShop();
        }
    }
    
    private void GenerateShopInventory()
    {
        availableItems.Clear();
        
        // Always stock health potions
        availableItems.Add(new ShopItem
        {
            itemName = "Health Potion",
            price = 50,
            itemType = ShopItemType.HealthPotion,
            healAmount = 50
        });
        
        availableItems.Add(new ShopItem
        {
            itemName = "Large Health Potion",
            price = 100,
            itemType = ShopItemType.HealthPotion,
            healAmount = 100
        });
        
        // Random power
        PowerType randomPower = (PowerType)Random.Range(0, System.Enum.GetValues(typeof(PowerType)).Length);
        PowerModel powerPreview = new(randomPower);
        availableItems.Add(new ShopItem
        {
            itemName = powerPreview.powerName,
            price = 200,
            itemType = ShopItemType.PowerModel,
            powerType = randomPower
        });
        
        Debug.Log($"Shop generated with {availableItems.Count} items");
    }
    
    private void OpenShop()
    {
        Debug.Log("=== SHOP MENU ===");
        for (int i = 0; i < availableItems.Count; i++)
        {
            ShopItem item = availableItems[i];
            Debug.Log($"{i + 1}. {item.itemName} - {item.price} Gold");
        }
        Debug.Log("=================");
        
        // TODO: Show UI menu
        // For now, auto-buy first item if player has gold
        if (player != null && player.inventory != null)
        {
            Debug.Log($"Player gold: {player.inventory.gold}");
        }
    }
    
    public bool BuyItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= availableItems.Count) return false;
        if (player == null || player.inventory == null) return false;
        
        ShopItem item = availableItems[itemIndex];
        
        // Check if player has enough gold
        if (player.inventory.gold < item.price)
        {
            Debug.Log("Not enough gold!");
            return false;
        }
        
        // Process purchase
        player.inventory.SpendGold(item.price);
        
        switch (item.itemType)
        {
            case ShopItemType.HealthPotion:
                player.Heal(item.healAmount);
                Debug.Log($"Bought and used {item.itemName}");
                break;
                
            case ShopItemType.PowerModel:
                if (player.powerManager != null)
                {
                    player.powerManager.AddPower(item.powerType);
                    Debug.Log($"Bought power: {item.itemName}");
                }
                break;
                
            case ShopItemType.Weapon:
                // TODO: Add weapon to player
                Debug.Log($"Bought weapon: {item.itemName}");
                break;
        }
        
        return true;
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = other.GetComponent<PlayerController>();
            Debug.Log("Press E to open shop");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;
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
                    bossPrefab = biomeManager.GetBossEnemyPrefab();
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
        FloorProgressionManager progressionManager = FindObjectOfType<FloorProgressionManager>();
        if (progressionManager != null)
        {
            // Enable exit portal
            Debug.Log("Exit portal unlocked!");
        }
    }
}

// ================================================== //
// FILE 4: PowerPickup.cs
// ================================================== //

// using UnityEngine;

public class PowerPickup : MonoBehaviour
{
    public PowerType powerType;
    
    void Start()
    {
        // Ensure has trigger collider
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Rotate for visual effect
        StartCoroutine(RotatePickup());
    }
    
    private System.Collections.IEnumerator RotatePickup()
    {
        while (true)
        {
            transform.Rotate(Vector3.up, 90f * Time.deltaTime);
            yield return null;
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.powerManager != null)
            {
                if (player.powerManager.AddPower(powerType))
                {
                    Debug.Log($"Picked up power: {powerType}");
                    Destroy(gameObject);
                }
            }
        }
    }
}