// -------------------------------------------------- //
// Scripts/Models/ItemModel.cs
// -------------------------------------------------- //

using UnityEngine;

public enum ItemType { Consumable, Key, Currency, Equipment }

/// <summary>
/// Base class for all collectible items
/// </summary>
public abstract class ItemModel : MonoBehaviour
{
    [Header("Item Info")]
    public string itemName = "Item";
    public ItemType itemType = ItemType.Consumable;
    public Sprite icon;
    public int stackSize = 1;
    
    [Header("Pickup Settings")]
    public bool autoPickup = true;
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && autoPickup)
        {
            OnPickup(other.gameObject);
        }
    }
    
    protected virtual void OnPickup(GameObject player)
    {
        Debug.Log($"Picked up: {itemName}");
        
        // Spawn pickup effect
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Play sound
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        Destroy(gameObject);
    }
}

// -------------------------------------------------- //
// Scripts/Models/HealthPotion.cs
// -------------------------------------------------- //

/// <summary>
/// Health restoration item
/// </summary>
public class HealthPotion : ItemModel
{
    [Header("Heal Amount")]
    public int healAmount = 50;
    
    protected override void OnPickup(GameObject player)
    {
        if (player.TryGetComponent<PlayerController>(out var playerController))
        {
            playerController.Heal(healAmount);
            Debug.Log($"Healed for {healAmount} HP");
        }
        
        base.OnPickup(player);
    }
}