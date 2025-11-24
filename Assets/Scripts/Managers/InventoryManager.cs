// -------------------------------------------------- //
// Scripts/Managers/InventoryManager.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player inventory (gold, keys, items, etc.)
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("Currency")]
    public int gold = 0;
    
    [Header("Keys")]
    public List<KeyType> keys = new();
    
    [Header("Items")]
    public List<ItemModel> items = new();
    public int maxInventorySize = 20;
    
    // ------------------------- //
    // GOLD
    // ------------------------- //
    
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gold: +{amount} (Total: {gold})");
        
        // Notify UI
        UIManager uiManager = GameDirector.Instance?.uiManager;
        if (uiManager != null)
        {
            // uiManager.UpdateGoldDisplay(gold);
        }
    }
    
    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"Gold: -{amount} (Total: {gold})");
            return true;
        }
        
        Debug.Log("Not enough gold!");
        return false;
    }
    
    // ------------------------- //
    // KEYS
    // ------------------------- //
    
    public void AddKey(KeyType keyType)
    {
        if (!keys.Contains(keyType))
        {
            keys.Add(keyType);
            Debug.Log($"Acquired {keyType} key");
        }
    }
    
    public bool HasKey(KeyType keyType)
    {
        return keys.Contains(keyType);
    }
    
    public void UseKey(KeyType keyType)
    {
        if (keys.Contains(keyType))
        {
            keys.Remove(keyType);
            Debug.Log($"Used {keyType} key");
        }
    }
    
    // ------------------------- //
    // ITEMS
    // ------------------------- //
    
    public bool AddItem(ItemModel item)
    {
        if (items.Count >= maxInventorySize)
        {
            Debug.Log("Inventory full!");
            return false;
        }
        
        items.Add(item);
        Debug.Log($"Added to inventory: {item.itemName}");
        return true;
    }
    
    public void RemoveItem(ItemModel item)
    {
        items.Remove(item);
    }
    
    public bool HasItem(string itemName)
    {
        return items.Exists(i => i.itemName == itemName);
    }
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    public void ClearInventory()
    {
        gold = 0;
        keys.Clear();
        items.Clear();
        Debug.Log("Inventory cleared");
    }
    
    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        Debug.Log("=== INVENTORY ===");
        Debug.Log($"Gold: {gold}");
        Debug.Log($"Keys: {string.Join(", ", keys)}");
        Debug.Log($"Items: {items.Count}/{maxInventorySize}");
        foreach (var item in items)
        {
            Debug.Log($"  - {item.itemName}");
        }
        Debug.Log("================");
    }
}