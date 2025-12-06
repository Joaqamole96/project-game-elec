// ================================================== //
// Scripts/Models/ItemModel.cs (PHYSICS FIX)
// ================================================== //
// Replace the base ItemModel class with this version:

using UnityEngine;

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
    public int value = 10;
    public bool autoPickup = true;
    
    [Header("Effects")]
    public GameObject pickupEffectPrefab;
    public AudioClip pickupSound;
    
    [Header("Physics")]
    private Rigidbody rb;
    private Collider itemCollider;
    private bool isBeingPickedUp = false;
    private bool hasLanded = false;
    
    private Renderer itemRenderer;
    private Material glowMaterial;
    private float bobTimer = 0f;
    
    protected virtual void Start()
    {
        SetupPhysics();
        SetupVisuals();
        SetupCollider();
    }
    
    protected virtual void Update()
    {
        // Only bob after landing
        if (hasLanded)
        {
            bobTimer += Time.deltaTime;
            
            // Gentle rotation
            transform.Rotate(Vector3.up, 45f * Time.deltaTime);
            
            // Gentle bobbing
            if (rb != null && rb.IsSleeping())
            {
                float bob = Mathf.Sin(bobTimer * 2f) * 0.05f;
                Vector3 pos = transform.position;
                pos.y += bob * Time.deltaTime;
                transform.position = pos;
            }
        }
    }
    
    private void SetupPhysics()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // CRITICAL: Proper physics settings to prevent phasing
        rb.mass = 0.5f;
        rb.drag = 1f;
        rb.angularDrag = 3f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Freeze rotation to prevent rolling
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        Debug.Log($"ItemModel: Physics setup complete for {itemName}");
    }
    
    private void SetupVisuals()
    {
        itemRenderer = GetComponentInChildren<Renderer>();
        if (itemRenderer != null)
        {
            glowMaterial = new Material(Shader.Find("Standard"));
            glowMaterial.color = glowColor;
            glowMaterial.EnableKeyword("_EMISSION");
            glowMaterial.SetColor("_EmissionColor", glowColor * 1.5f);
            itemRenderer.material = glowMaterial;
        }
    }
    
    private void SetupCollider()
    {
        itemCollider = GetComponent<Collider>();
        if (itemCollider == null)
        {
            // Create appropriate collider based on primitive type
            if (GetComponent<MeshFilter>() != null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                itemCollider = box;
            }
            else
            {
                SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
                sphere.radius = 0.5f;
                itemCollider = sphere;
            }
        }
        
        // CRITICAL: Collider is NOT trigger while falling
        // It becomes trigger only after landing
        itemCollider.isTrigger = false;
        
        Debug.Log($"ItemModel: Collider setup - Type: {itemCollider.GetType().Name}, IsTrigger: {itemCollider.isTrigger}");
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (!hasLanded)
        {
            Debug.Log($"ItemModel: {itemName} landed on {collision.gameObject.name}");
            hasLanded = true;
            
            // Wait a moment, then convert to trigger for pickup
            Invoke(nameof(ConvertToTrigger), 0.5f);
        }
    }
    
    private void ConvertToTrigger()
    {
        if (itemCollider != null)
        {
            itemCollider.isTrigger = true;
            Debug.Log($"ItemModel: {itemName} converted to trigger, ready for pickup");
        }
    }
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isBeingPickedUp || !hasLanded) return;
        
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
        
        ApplyEffect(player);
        
        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        Destroy(gameObject);
    }
    
    protected abstract void ApplyEffect(PlayerController player);
    public abstract string GetDescription();
    
    void OnDrawGizmos()
    {
        Gizmos.color = hasLanded ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}

// ================================================== //
// CONSUMABLE ITEMS
// ================================================== //

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