// ================================================== //
// Scripts/Display/ShopDisplay.cs (COMPLETE REWRITE)
// ================================================== //

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI display for shop interface - FIXED VERSION
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
    
    private ShopController currentShop;
    private PlayerController player;
    
    void Start()
    {
        player = PlayerController.Instance;
        
        // Setup close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseShop);
            Debug.Log("ShopDisplay: Close button listener added");
        }
        else
        {
            Debug.LogWarning("ShopDisplay: Close button not assigned!");
        }
        
        // Start hidden
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        // Load item card prefab if not assigned
        if (itemCardPrefab == null)
        {
            itemCardPrefab = Resources.Load<GameObject>("UI/comp_ShopItem");
            if (itemCardPrefab == null)
            {
                Debug.LogWarning("ShopDisplay: Item card prefab not found, will create fallback");
            }
        }
    }
    
    void Update()
    {
        // ESC to close
        if (shopPanel != null && shopPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
            
            // Update gold display continuously
            UpdateGoldDisplay();
        }
    }
    
    // ==========================================
    // SHOP DISPLAY
    // ==========================================
    
    public void OpenShop(ShopController shop)
    {
        if (shop == null)
        {
            Debug.LogError("ShopDisplay: Cannot open - shop is null");
            return;
        }
        
        if (player == null)
        {
            player = PlayerController.Instance;
            if (player == null)
            {
                Debug.LogError("ShopDisplay: Cannot open - player not found");
                return;
            }
        }
        
        currentShop = shop;
        
        Debug.Log("ShopDisplay: Opening shop");
        
        // Show panel
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("ShopDisplay: shopPanel is null!");
            return;
        }
        
        // Pause game
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // Set title
        if (shopTitleText != null)
        {
            shopTitleText.text = "SHOP";
        }
        
        // Update UI
        UpdateGoldDisplay();
        PopulateShopItems();
        
        Debug.Log("ShopDisplay: Shop opened successfully");
    }
    
    public void CloseShop()
    {
        Debug.Log("ShopDisplay: Closing shop");
        
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        
        // Resume game
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        if (currentShop != null)
        {
            currentShop.CloseShop();
        }
        currentShop = null;
    }
    
    // ==========================================
    // ITEM DISPLAY
    // ==========================================
    
    private void PopulateShopItems()
    {
        if (itemsContainer == null)
        {
            Debug.LogError("ShopDisplay: itemsContainer is null!");
            return;
        }
        
        if (currentShop == null)
        {
            Debug.LogError("ShopDisplay: currentShop is null!");
            return;
        }
        
        // Clear existing items
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        // Get inventory
        List<string> inventory = currentShop.GetShopInventory();
        
        Debug.Log($"ShopDisplay: Populating {inventory.Count} items");
        
        // Create item cards
        int successCount = 0;
        foreach (string itemID in inventory)
        {
            if (CreateItemCard(itemID))
            {
                successCount++;
            }
        }
        
        Debug.Log($"ShopDisplay: Created {successCount}/{inventory.Count} item cards");
    }
    
    private bool CreateItemCard(string itemID)
    {
        // Get item data
        var itemDef = ItemRegistry.GetItem(itemID);
        if (itemDef == null)
        {
            Debug.LogWarning($"ShopDisplay: Item '{itemID}' not found in registry");
            return false;
        }
        
        // Create card
        GameObject card = CreateCard();
        if (card == null)
        {
            Debug.LogError("ShopDisplay: Failed to create card");
            return false;
        }
        
        card.name = $"ItemCard_{itemID}";
        
        // Setup card with item data
        return SetupItemCard(card, itemID, itemDef);
    }
    
    private GameObject CreateCard()
    {
        if (itemCardPrefab != null)
        {
            return Instantiate(itemCardPrefab, itemsContainer);
        }
        else
        {
            // Fallback: Create simple card
            return CreateFallbackItemCard();
        }
    }
    
    private bool SetupItemCard(GameObject card, string itemID, ItemRegistry.ItemDefinition itemDef)
    {
        // Get price
        int price = currentShop.GetItemPrice(itemID);
        bool canAfford = currentShop.CanAfford(itemID);
        
        // Find UI components
        TextMeshProUGUI nameText = FindTextInChildren(card, "ItemName", "Name");
        TextMeshProUGUI priceText = FindTextInChildren(card, "Price", "PriceText");
        TextMeshProUGUI descText = FindTextInChildren(card, "Description", "DescText");
        Image icon = FindImageInChildren(card, "Icon");
        Button buyButton = FindButtonInChildren(card, "BuyButton", "Button");
        
        // Set text values
        if (nameText != null)
        {
            nameText.text = itemDef.itemName;
            Debug.Log($"Set name: {itemDef.itemName}");
        }
        else
        {
            Debug.LogWarning($"Name text not found for {itemID}");
        }
        
        if (priceText != null)
        {
            priceText.text = $"{price}G";
            priceText.color = canAfford ? Color.white : Color.red;
            Debug.Log($"Set price: {price}G");
        }
        else
        {
            Debug.LogWarning($"Price text not found for {itemID}");
        }
        
        if (descText != null)
        {
            descText.text = itemDef.description;
        }
        
        // Set icon color
        if (icon != null)
        {
            icon.color = GetItemTypeColor(itemDef.itemType);
        }
        
        // Setup button
        if (buyButton != null)
        {
            buyButton.interactable = canAfford;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => OnPurchaseClicked(itemID));
            Debug.Log($"Button setup for {itemID}");
        }
        else
        {
            Debug.LogWarning($"Buy button not found for {itemID}");
        }
        
        return true;
    }
    
    private GameObject CreateFallbackItemCard()
    {
        GameObject card = new GameObject("ItemCard_Fallback");
        card.transform.SetParent(itemsContainer, false);
        
        // Add layout
        RectTransform rect = card.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(300, 100);
        
        // Background
        Image bg = card.AddComponent<Image>();
        bg.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        // Name text
        GameObject nameObj = new GameObject("ItemName");
        nameObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
        nameText.fontSize = 24;
        nameText.alignment = TextAlignmentOptions.Center;
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 0.6f);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.offsetMin = Vector2.zero;
        nameRect.offsetMax = Vector2.zero;
        
        // Price text
        GameObject priceObj = new GameObject("Price");
        priceObj.transform.SetParent(card.transform, false);
        TextMeshProUGUI priceText = priceObj.AddComponent<TextMeshProUGUI>();
        priceText.fontSize = 20;
        priceText.alignment = TextAlignmentOptions.Center;
        RectTransform priceRect = priceObj.GetComponent<RectTransform>();
        priceRect.anchorMin = new Vector2(0, 0.3f);
        priceRect.anchorMax = new Vector2(1, 0.6f);
        priceRect.offsetMin = Vector2.zero;
        priceRect.offsetMax = Vector2.zero;
        
        // Button
        GameObject btnObj = new GameObject("BuyButton");
        btnObj.transform.SetParent(card.transform, false);
        Button btn = btnObj.AddComponent<Button>();
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.3f, 0.6f, 0.3f);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.2f, 0);
        btnRect.anchorMax = new Vector2(0.8f, 0.3f);
        btnRect.offsetMin = Vector2.zero;
        btnRect.offsetMax = Vector2.zero;
        
        // Button text
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "BUY";
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.fontSize = 18;
        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        return card;
    }
    
    // ==========================================
    // HELPER METHODS
    // ==========================================
    
    private TextMeshProUGUI FindTextInChildren(GameObject parent, params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                TextMeshProUGUI text = child.GetComponent<TextMeshProUGUI>();
                if (text != null) return text;
            }
        }
        
        // Deep search
        TextMeshProUGUI[] allText = parent.GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in allText)
        {
            foreach (string name in possibleNames)
            {
                if (text.gameObject.name.Contains(name))
                {
                    return text;
                }
            }
        }
        
        return null;
    }
    
    private Image FindImageInChildren(GameObject parent, string name)
    {
        Transform child = parent.transform.Find(name);
        if (child != null)
        {
            return child.GetComponent<Image>();
        }
        return null;
    }
    
    private Button FindButtonInChildren(GameObject parent, params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            Transform child = parent.transform.Find(name);
            if (child != null)
            {
                Button btn = child.GetComponent<Button>();
                if (btn != null) return btn;
            }
        }
        
        // Check parent itself
        Button parentBtn = parent.GetComponent<Button>();
        if (parentBtn != null) return parentBtn;
        
        return null;
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
            Debug.LogWarning("ShopDisplay: Cannot purchase - no active shop");
            return;
        }
        
        Debug.Log($"ShopDisplay: Attempting to purchase {itemID}");
        
        bool success = currentShop.PurchaseItem(itemID);
        
        if (success)
        {
            Debug.Log($"ShopDisplay: Successfully purchased {itemID}");
            
            // Refresh display
            UpdateGoldDisplay();
            PopulateShopItems();
            
            // Play sound
            PlayPurchaseSound();
        }
        else
        {
            Debug.Log("ShopDisplay: Purchase failed");
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
        Debug.Log("*Purchase sound*");
    }
    
    private void PlayErrorSound()
    {
        Debug.Log("*Error sound*");
    }
}