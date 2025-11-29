// ================================================== //
// Scripts/Display/ShopDisplay.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Handles shop UI display and purchase interactions
/// </summary>
public class ShopDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Transform itemsContainer;
    public GameObject itemCardPrefab;
    public TextMeshProUGUI playerGoldText;
    public Button closeButton;
    
    [Header("Purchase Popup")]
    public GameObject purchasePopup;
    public TextMeshProUGUI popupText;
    public Button confirmButton;
    public Button cancelButton;
    
    private ShopController currentShop;
    private PlayerController player;
    private ShopController.ShopItem pendingPurchase;
    
    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseShop);
        
        if (confirmButton != null)
            confirmButton.onClick.AddListener(ConfirmPurchase);
        
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelPurchase);
        
        if (shopPanel != null)
            shopPanel.SetActive(false);
        
        if (purchasePopup != null)
            purchasePopup.SetActive(false);
        
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
        
        // Show panel
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Update gold display
        UpdateGoldDisplay();
        
        // Populate items
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
        foreach (var item in currentShop.availableItems)
        {
            GameObject card = Instantiate(itemCardPrefab, itemsContainer);
            SetupItemCard(card, item);
        }
    }
    
    private void SetupItemCard(GameObject card, ShopController.ShopItem item)
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
        
        // Set icon
        if (icon != null)
        {
            icon.color = GetItemColor(item.itemType);
        }
        
        // Setup buy button
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(() => OnBuyButtonClicked(item));
            
            // Disable if can't afford
            bool canAfford = player.inventory.gold >= item.price;
            buyButton.interactable = canAfford;
            
            var buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = canAfford ? "Buy" : "Can't Afford";
            }
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
    
    private void OnBuyButtonClicked(ShopController.ShopItem item)
    {
        pendingPurchase = item;
        
        if (purchasePopup != null)
        {
            purchasePopup.SetActive(true);
        }
        
        if (popupText != null)
        {
            popupText.text = $"Buy {item.itemName} for {item.price} Gold?";
        }
    }
    
    private void ConfirmPurchase()
    {
        if (pendingPurchase == null || currentShop == null) return;
        
        int itemIndex = currentShop.availableItems.IndexOf(pendingPurchase);
        bool success = currentShop.BuyItem(itemIndex);
        
        if (success)
        {
            // Refresh shop display
            UpdateGoldDisplay();
            PopulateShopItems();
            
            // Show success message
            Debug.Log($"Purchased {pendingPurchase.itemName}!");
        }
        
        CancelPurchase();
    }
    
    private void CancelPurchase()
    {
        pendingPurchase = null;
        
        if (purchasePopup != null)
        {
            purchasePopup.SetActive(false);
        }
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