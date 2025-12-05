// -------------------------------------------------- //
// Scripts/Displays/ShopDisplay.cs (DIRECT PURCHASE - NO POPUP)
// -------------------------------------------------- //

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Transform itemsContainer;
    public GameObject itemCardPrefab;
    public TextMeshProUGUI playerGoldText;
    public Button closeButton;
    
    private ShopController currentShop;
    private PlayerController player;
    
    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        player = PlayerController.Instance;
    }
    
    void Update()
    {
        // Close shop with ESC
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
    }
    
    public void OpenShop(ShopController shop)
    {
        if (shop == null || player == null) return;
        
        currentShop = shop;
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        UpdateGoldDisplay();
        PopulateShopItems();
    }
    
    private void PopulateShopItems()
    {
        if (itemsContainer == null || itemCardPrefab == null) return;
        
        // Clear existing items
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Create item cards
        for (int i = 0; i < currentShop.availableItems.Count; i++)
        {
            int itemIndex = i; // Capture for closure
            var item = currentShop.availableItems[i];
            
            GameObject card = Instantiate(itemCardPrefab, itemsContainer);
            SetupItemCard(card, item, itemIndex);
        }
    }
    
    private void SetupItemCard(GameObject card, ShopController.ShopItem item, int itemIndex)
    {
        // Get components
        var nameText = card.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        var priceText = card.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        var descText = card.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        var icon = card.transform.Find("Icon")?.GetComponent<Image>();
        var buyButton = card.transform.Find("BuyButton")?.GetComponent<Button>();
        
        // Set text
        if (nameText != null) nameText.text = item.itemName;
        if (priceText != null) priceText.text = $"{item.price} Gold";
        if (descText != null) descText.text = GetItemDescription(item);
        
        // Set icon color
        if (icon != null)
        {
            icon.color = GetItemColor(item.itemType);
        }
        
        // Setup buy button - DIRECT PURCHASE
        if (buyButton != null)
        {
            bool canAfford = player.inventory.gold >= item.price;
            buyButton.interactable = canAfford;
            
            // Remove any existing listeners
            buyButton.onClick.RemoveAllListeners();
            
            // Add direct purchase listener
            buyButton.onClick.AddListener(() => OnDirectPurchase(itemIndex));
            
            var buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canAfford ? "Buy" : "Too Expensive";
            }
        }
    }
    
    private void OnDirectPurchase(int itemIndex)
    {
        if (currentShop == null) return;
        
        bool success = currentShop.BuyItem(itemIndex);
        
        if (success)
        {
            // Update display
            UpdateGoldDisplay();
            PopulateShopItems(); // Refresh to update affordability
            
            Debug.Log("Purchase successful!");
        }
        else
        {
            Debug.Log("Purchase failed - not enough gold!");
        }
    }
    
    private string GetItemDescription(ShopController.ShopItem item)
    {
        return item.itemType switch
        {
            ShopController.ShopItemType.HealthPotion => $"Restores {item.healAmount} HP",
            ShopController.ShopItemType.ManaPotion => $"Restores {item.healAmount} MP",
            ShopController.ShopItemType.PowerModel => GetPowerDescription(item.powerType),
            ShopController.ShopItemType.Weapon => "Weapon upgrade",
            _ => "Item"
        };
    }
    
    private string GetPowerDescription(PowerType powerType)
    {
        PowerModel preview = new(powerType);
        return preview.description;
    }
    
    private Color GetItemColor(ShopController.ShopItemType itemType)
    {
        return itemType switch
        {
            ShopController.ShopItemType.HealthPotion => Color.red,
            ShopController.ShopItemType.ManaPotion => Color.blue,
            ShopController.ShopItemType.PowerModel => Color.yellow,
            ShopController.ShopItemType.Weapon => Color.green,
            _ => Color.white
        };
    }
    
    private void UpdateGoldDisplay()
    {
        if (playerGoldText != null && player != null && player.inventory != null)
        {
            playerGoldText.text = $"Gold: {player.inventory.gold}";
        }
    }
    
    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentShop = null;
    }
}