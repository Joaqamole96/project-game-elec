// ================================================== //
// FILE 1: ShopController.cs
// ================================================== //

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ShopController : MonoBehaviour
{
    [Header("Shop Inventory")]
    public List<Commodity> availableItems = new();
    
    [Header("UI")]
    public ShopDisplay shopDisplay;
    public bool isPlayerNearby = false;
    
    private PlayerController player;
    
    [System.Serializable]
    public class Commodity
    {
        public string itemName;
        public int price;
        public CommodityType itemType;
        public PowerType powerType; // If power
        public GameObject weaponPrefab; // If weapon
        public int healAmount; // If potion
    }
    
    public enum CommodityType
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
        availableItems.Add(new Commodity
        {
            itemName = "Health Potion",
            price = 50,
            itemType = CommodityType.HealthPotion,
            healAmount = 50
        });
        
        availableItems.Add(new Commodity
        {
            itemName = "Large Health Potion",
            price = 100,
            itemType = CommodityType.HealthPotion,
            healAmount = 100
        });
        
        // Random power
        PowerType randomPower = (PowerType)Random.Range(0, System.Enum.GetValues(typeof(PowerType)).Length);
        PowerModel powerPreview = new(randomPower);
        availableItems.Add(new Commodity
        {
            itemName = powerPreview.powerName,
            price = 200,
            itemType = CommodityType.PowerModel,
            powerType = randomPower
        });
        
        Debug.Log($"Shop generated with {availableItems.Count} items");
    }
    
    private void OpenShop()
    {
        if (shopDisplay != null)
        {
            shopDisplay.OpenShop(this);
        }
        else
        {
            // Fallback to debug menu
            Debug.Log("=== SHOP MENU ===");
            for (int i = 0; i < availableItems.Count; i++)
            {
                Commodity item = availableItems[i];
                Debug.Log($"{i + 1}. {item.itemName} - {item.price} Gold");
            }
            Debug.Log("=================");
        }
    }
    
    public bool BuyItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= availableItems.Count) return false;
        if (player == null || player.inventory == null) return false;
        
        Commodity item = availableItems[itemIndex];
        
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
            case CommodityType.HealthPotion:
                player.Heal(item.healAmount);
                Debug.Log($"Bought and used {item.itemName}");
                break;
                
            case CommodityType.PowerModel:
                if (player.powerManager != null)
                {
                    player.powerManager.AddPower(item.powerType);
                    Debug.Log($"Bought power: {item.itemName}");
                }
                break;
                
            case CommodityType.Weapon:
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
            UIManager.Instance?.ShowShopDisplay(this);
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