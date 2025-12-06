// ================================================== //
// Scripts/Models/ItemModel.cs - Refactored Item System
// ================================================== //

using UnityEngine;

/// <summary>
/// Base class for all pickupable items in the game world
/// Items are physical GameObjects that can be dropped by enemies or purchased from shops
/// </summary>
[RequireComponent(typeof(Collider))]
public abstract class ItemModel : MonoBehaviour
{
    [Header("Item Identity")]
    public string itemID;
    public string itemName = "Item";
    public ItemType itemType = ItemType.Consumable;
    
    [Header("Visual")]
    public Sprite icon;
    public Color glowColor = Color.white;
    
    [Header("Gameplay")]
    public int value = 10; // Sell/buy price
    public bool autoPickup = true;
    
    [Header("Effects")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    
    private Renderer itemRenderer;
    private Material glowMaterial;
    private bool isBeingPickedUp = false;
    
    protected virtual void Start()
    {
        SetupVisuals();
        SetupCollider();
    }
    
    protected virtual void Update()
    {
        // Gentle rotation for visual appeal
        transform.Rotate(Vector3.up, 45f * Time.deltaTime);
        
        // Gentle bobbing motion
        float bob = Mathf.Sin(Time.time * 2f) * 0.1f;
        Vector3 pos = transform.position;
        pos.y = transform.position.y + bob * Time.deltaTime;
        transform.position = pos;
    }
    
    private void SetupVisuals()
    {
        itemRenderer = GetComponentInChildren<Renderer>();
        if (itemRenderer != null)
        {
            // Create glowing material
            glowMaterial = new Material(Shader.Find("Standard"));
            glowMaterial.color = glowColor;
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.SetColor("_EmissionColor", glowColor * 1.5f);
            itemRenderer.material = glowMaterial;
        }
    }
    
    private void SetupCollider()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 0.5f;
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isBeingPickedUp) return;
        
        if (other.CompareTag("Player") && autoPickup)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                OnPickup(player);
            }
        }
    }
    
    protected virtual void OnPickup(PlayerController player)
    {
        if (isBeingPickedUp) return;
        isBeingPickedUp = true;
        
        Debug.Log($"Picked up: {itemName}");
        
        // Apply item effect
        ApplyEffect(player);
        
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
        
        // Destroy item
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Override this to implement specific item effects
    /// </summary>
    protected abstract void ApplyEffect(PlayerController player);
    
    /// <summary>
    /// Get item description for UI tooltips
    /// </summary>
    public abstract string GetDescription();
}

// ================================================== //
// CONSUMABLE ITEMS
// ================================================== //

/// <summary>
/// Small health potion - restores 30 HP
/// </summary>
public class SmallHealthPotion : ItemModel
{
    public int healAmount = 30;
    
    protected override void ApplyEffect(PlayerController player)
    {
        player.Heal(healAmount);
        Debug.Log($"Restored {healAmount} HP");
    }
    
    public override string GetDescription()
    {
        return $"Restores {healAmount} HP";
    }
}

/// <summary>
/// Medium health potion - restores 50 HP
/// </summary>
public class MediumHealthPotion : ItemModel
{
    public int healAmount = 50;
    
    protected override void ApplyEffect(PlayerController player)
    {
        player.Heal(healAmount);
        Debug.Log($"Restored {healAmount} HP");
    }
    
    public override string GetDescription()
    {
        return $"Restores {healAmount} HP";
    }
}

/// <summary>
/// Large health potion - restores 100 HP
/// </summary>
public class LargeHealthPotion : ItemModel
{
    public int healAmount = 100;
    
    protected override void ApplyEffect(PlayerController player)
    {
        player.Heal(healAmount);
        Debug.Log($"Restored {healAmount} HP");
    }
    
    public override string GetDescription()
    {
        return $"Restores {healAmount} HP";
    }
}

/// <summary>
/// Max health potion - fully restores HP
/// </summary>
public class MaxHealthPotion : ItemModel
{
    protected override void ApplyEffect(PlayerController player)
    {
        int healAmount = player.maxHealth - player.CurrentHealth;
        player.Heal(healAmount);
        Debug.Log("Fully restored HP!");
    }
    
    public override string GetDescription()
    {
        return "Fully restores HP";
    }
}

/// <summary>
/// Speed boost potion - temporary speed increase
/// </summary>
public class SpeedBoostPotion : ItemModel
{
    public float speedMultiplier = 1.5f;
    public float duration = 10f;
    
    protected override void ApplyEffect(PlayerController player)
    {
        StartCoroutine(ApplyTemporarySpeedBoost(player));
    }
    
    private System.Collections.IEnumerator ApplyTemporarySpeedBoost(PlayerController player)
    {
        float originalSpeed = player.moveSpeed;
        player.moveSpeed *= speedMultiplier;
        
        Debug.Log($"Speed boosted for {duration} seconds!");
        
        yield return new WaitForSeconds(duration);
        
        player.moveSpeed = originalSpeed;
        Debug.Log("Speed boost expired");
    }
    
    public override string GetDescription()
    {
        return $"+{(speedMultiplier - 1) * 100}% speed for {duration}s";
    }
}

/// <summary>
/// Damage boost potion - temporary damage increase
/// </summary>
public class DamageBoostPotion : ItemModel
{
    public float damageMultiplier = 1.5f;
    public float duration = 15f;
    
    protected override void ApplyEffect(PlayerController player)
    {
        StartCoroutine(ApplyTemporaryDamageBoost(player));
    }
    
    private System.Collections.IEnumerator ApplyTemporaryDamageBoost(PlayerController player)
    {
        int originalDamage = player.playerDamage;
        player.playerDamage = Mathf.RoundToInt(player.playerDamage * damageMultiplier);
        
        Debug.Log($"Damage boosted for {duration} seconds!");
        
        yield return new WaitForSeconds(duration);
        
        player.playerDamage = originalDamage;
        Debug.Log("Damage boost expired");
    }
    
    public override string GetDescription()
    {
        return $"+{(damageMultiplier - 1) * 100}% damage for {duration}s";
    }
}

/// <summary>
/// Invincibility potion - temporary invulnerability
/// </summary>
public class InvincibilityPotion : ItemModel
{
    public float duration = 5f;
    
    protected override void ApplyEffect(PlayerController player)
    {
        StartCoroutine(ApplyInvincibility(player));
    }
    
    private System.Collections.IEnumerator ApplyInvincibility(PlayerController player)
    {
        // Note: This requires adding an "isInvincible" flag to PlayerController
        Debug.Log($"Invincible for {duration} seconds!");
        
        // Visual feedback - make player glow
        Renderer renderer = player.GetComponentInChildren<Renderer>();
        Material originalMat = null;
        if (renderer != null)
        {
            originalMat = renderer.material;
            Material glowMat = new Material(Shader.Find("Standard"));
            glowMat.color = Color.yellow;
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", Color.yellow * 2f);
            renderer.material = glowMat;
        }
        
        yield return new WaitForSeconds(duration);
        
        // Restore original material
        if (renderer != null && originalMat != null)
        {
            renderer.material = originalMat;
        }
        
        Debug.Log("Invincibility expired");
    }
    
    public override string GetDescription()
    {
        return $"Invulnerable for {duration}s";
    }
}

// ================================================== //
// POWER ITEMS
// ================================================== //

/// <summary>
/// Power pickup - grants permanent power to player
/// </summary>
public class PowerPickup : ItemModel
{
    public PowerType powerType;
    
    protected override void ApplyEffect(PlayerController player)
    {
        if (player.powerManager != null)
        {
            bool success = player.powerManager.AddPower(powerType);
            if (success)
            {
                Debug.Log($"Acquired power: {powerType}");
                
                // Save progress
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.OnPowerAcquired();
                }
            }
        }
    }
    
    public override string GetDescription()
    {
        PowerModel preview = new PowerModel(powerType);
        return preview.description;
    }
}