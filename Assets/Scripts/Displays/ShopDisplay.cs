// ================================================== //
// Scripts/Display/ShopDisplay.cs
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI display for shop interface
/// Shows available items and handles purchase UI
/// </summary>
public class ShopDisplay : MonoBehaviour
{
    [Header("UI References")]
    public GameObject shopPanel;
    public Transform itemsContainer;
    public GameObject itemCardPrefab;
    public TextMeshProUGUI playerGoldText;
    public TextMeshProUGUI shopTitleText;
    public Button closeButton;
    
    [Header("Item Card Template")]
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemPriceText;
    public TextMeshProUGUI itemDescriptionText;
    public Image itemIconImage;
    public Button purchaseButton;
    
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
        
        // Load item card prefab if not assigned
        if (itemCardPrefab == null)
        {
            itemCardPrefab = Resources.Load<GameObject>("UI/comp_ShopItem");
        }
    }
    
    void Update()
    {
        // Close shop with ESC
        if (shopPanel != null && shopPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            CloseShop();
        }
        
        // Update gold display in real-time
        if (shopPanel != null && shopPanel.activeSelf)
        {
            UpdateGoldDisplay();
        }
    }
    
    // ==========================================
    // SHOP DISPLAY
    // ==========================================
    
    public void OpenShop(ShopController shop)
    {
        if (shop == null || player == null)
        {
            Debug.LogWarning("Cannot open shop - invalid shop or player");
            return;
        }
        
        currentShop = shop;
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Update shop title
        if (shopTitleText != null)
        {
            shopTitleText.text = "SHOP";
        }
        
        UpdateGoldDisplay();
        PopulateShopItems();
        
        Debug.Log("Shop opened");
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
        
        currentShop?.CloseShop();
        currentShop = null;
        
        Debug.Log("Shop closed");
    }
    
    // ==========================================
    // ITEM DISPLAY
    // ==========================================
    
    private void PopulateShopItems()
    {
        if (itemsContainer == null || currentShop == null)
        {
            Debug.LogWarning("Cannot populate shop items - missing container or shop");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get shop inventory
        List<string> inventory = currentShop.GetShopInventory();
        
        // Create item cards
        foreach (string itemID in inventory)
        {
            CreateItemCard(itemID);
        }
        
        Debug.Log($"Populated shop with {inventory.Count} items");
    }
    
    private void CreateItemCard(string itemID)
    {
        // Get item data
        var itemDef = ItemRegistry.GetItem(itemID);
        if (itemDef == null)
        {
            Debug.LogWarning($"Item definition not found for '{itemID}'");
            return;
        }
        
        // Create card
        GameObject card;
        if (itemCardPrefab != null)
        {
            card = Instantiate(itemCardPrefab, itemsContainer);
        }
        else
        {
            // Fallback: create simple card
            card = CreateFallbackItemCard();
        }
        
        card.name = $"ItemCard_{itemID}";
        
        // Setup card
        SetupItemCard(card, itemID, itemDef);
    }
    
    private void SetupItemCard(GameObject card, string itemID, ItemRegistry.ItemDefinition itemDef)
    {
        // Get price
        int price = currentShop.GetItemPrice(itemID);
        bool canAfford = currentShop.CanAfford(itemID);
        
        // Find components in card
        var nameText = card.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        var priceText = card.transform.Find("Price")?.GetComponent<TextMeshProUGUI>();
        var descText = card.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        var icon = card.transform.Find("Icon")?.GetComponent<Image>();
        var buyButton = card.transform.Find("BuyButton")?.GetComponent<Button>();
        
        // If components not found, try getting them directly from card
        if (buyButton == null)
        {
            buyButton = card.GetComponent<Button>();
        }
        
        // Set text
        if (nameText != null) nameText.text = itemDef.itemName;
        if (priceText != null)
        {
            priceText.text = $"{price} Gold";
            priceText.color = canAfford ? Color.white : Color.red;
        }
        if (descText != null) descText.text = itemDef.description;
        
        // Set icon color
        if (icon != null)
        {
            icon.color = GetItemTypeColor(itemDef.itemType);
        }
        
        // Setup purchase button
        if (buyButton != null)
        {
            buyButton.interactable = canAfford;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => OnPurchaseClicked(itemID));
        }
        else
        {
            Debug.LogWarning($"No button found on item card for {itemID}");
        }
    }
    
    private GameObject CreateFallbackItemCard()
    {
        GameObject card = new GameObject("ItemCard_Fallback");
        
        // Add image background
        Image bg = card.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(200, 100);
        
        // Add button
        Button button = card.AddComponent<Button>();
        
        return card;
    }
    
    private Color GetItemTypeColor(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Consumable => new Color(1f, 0.3f, 0.3f),
            ItemType.Currency => new Color(1f, 0.84f, 0f),
            ItemType.Equipment => new Color(0.2f, 0.8f, 1f),
            _ => Color.white
        };
    }
    
    // ==========================================
    // PURCHASE HANDLING
    // ==========================================
    
    private void OnPurchaseClicked(string itemID)
    {
        if (currentShop == null)
        {
            Debug.LogWarning("Cannot purchase - no active shop");
            return;
        }
        
        bool success = currentShop.PurchaseItem(itemID);
        
        if (success)
        {
            Debug.Log($"Purchased item: {itemID}");
            
            // Refresh display
            UpdateGoldDisplay();
            PopulateShopItems(); // Refresh to update affordability
            
            // Play purchase sound
            PlayPurchaseSound();
        }
        else
        {
            Debug.Log("Purchase failed");
            PlayErrorSound();
        }
    }
    
    private void UpdateGoldDisplay()
    {
        if (playerGoldText != null && player != null && player.inventory != null)
        {
            playerGoldText.text = $"Gold: {player.inventory.gold}";
        }
    }
    
    private void PlayPurchaseSound()
    {
        // TODO: Play purchase success sound
        Debug.Log("*Purchase sound*");
    }
    
    private void PlayErrorSound()
    {
        // TODO: Play error sound
        Debug.Log("*Error sound*");
    }
}