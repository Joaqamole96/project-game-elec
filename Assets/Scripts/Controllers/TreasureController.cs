// ================================================== //
// Scripts/Controllers/TreasureController.cs (UPDATED)
// ================================================== //

using UnityEngine;

/// <summary>
/// Treasure chest controller - spawns from prefab, grants rewards on open
/// </summary>
public class TreasureController : MonoBehaviour
{
    [Header("Treasure Settings")]
    public bool isOpened = false;
    public TreasureType treasureType = TreasureType.Random;
    
    [Header("Specific Rewards (if not random)")]
    public PowerType specificPower;
    public GameObject specificWeapon;
    public int goldAmount = 100;
    
    [Header("Visual")]
    public Animator chestAnimator;
    public GameObject openChestModel;
    public GameObject closedChestModel;
    
    [Header("Effects")]
    public GameObject openEffectPrefab;
    public AudioClip openSound;
    
    public enum TreasureType
    {
        Random,
        PowerModel,
        Weapon,
        Gold
    }
    
    void Start()
    {
        // Get animator if available
        if (chestAnimator == null)
        {
            chestAnimator = GetComponentInChildren<Animator>();
        }
        
        // Ensure has trigger collider
        if (!TryGetComponent<Collider>(out var collider))
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2f, 2f);
        }
        else
        {
            collider.isTrigger = true;
        }
        
        // Set initial visual state
        UpdateVisualState();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                OpenChest(player);
            }
        }
    }
    
    private void OpenChest(PlayerController player)
    {
        if (isOpened || player == null) return;
        
        isOpened = true;
        
        // Determine reward
        TreasureType reward = treasureType;
        if (reward == TreasureType.Random)
        {
            reward = (TreasureType)Random.Range(1, 4); // 1-3 (skip Random enum value)
        }
        
        // Grant reward
        switch (reward)
        {
            case TreasureType.PowerModel:
                GivePowerReward(player);
                break;
                
            case TreasureType.Weapon:
                GiveWeaponReward(player);
                break;
                
            case TreasureType.Gold:
                GiveGoldReward(player);
                break;
        }
        
        // Visual feedback
        PlayOpenAnimation();
        PlayOpenEffect();
        UpdateVisualState();
        
        Debug.Log("Chest opened!");
        
        // Optional: Destroy chest after some time
        Destroy(gameObject, 3f);
    }
    
    private void GivePowerReward(PlayerController player)
    {
        PowerType powerToGive = specificPower;
        
        // Random power if not specified
        if (treasureType == TreasureType.Random)
        {
            powerToGive = (PowerType)Random.Range(0, System.Enum.GetValues(typeof(PowerType)).Length);
        }
        
        if (player.powerManager != null)
        {
            bool success = player.powerManager.AddPower(powerToGive);
            if (success)
            {
                Debug.Log($"Treasure: Gained power {powerToGive}!");
                
                // Save progress
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.OnPowerAcquired();
                }
            }
            else
            {
                Debug.Log("Treasure: Already have this power or max powers reached");
                // Give gold as compensation
                GiveGoldReward(player);
            }
        }
    }
    
    private void GiveWeaponReward(PlayerController player)
    {
        Debug.Log("Treasure: Found weapon!");
        
        // Spawn weapon pickup near chest
        Vector3 weaponSpawnPos = transform.position + Vector3.up + Random.insideUnitSphere * 1f;
        
        if (WeaponConfig.Instance != null)
        {
            // Get random weapon
            WeaponModel randomWeapon = WeaponConfig.Instance.GetRandomWeapon();
            if (randomWeapon != null && randomWeapon.prefab != null)
            {
                GameObject weaponPickup = Instantiate(randomWeapon.prefab, weaponSpawnPos, Quaternion.identity);
                Debug.Log($"Spawned weapon: {randomWeapon.weaponName}");
            }
        }
    }
    
    private void GiveGoldReward(PlayerController player)
    {
        if (player.inventory != null)
        {
            int finalGold = goldAmount;
            
            // Apply gold multiplier from powers
            if (player.powerManager != null)
            {
                finalGold = player.powerManager.ModifyGoldGained(goldAmount);
            }
            
            player.inventory.AddGold(finalGold);
            Debug.Log($"Treasure: Found {finalGold} gold!");
        }
    }
    
    private void PlayOpenAnimation()
    {
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger("Open");
        }
    }
    
    private void PlayOpenEffect()
    {
        // Spawn particle effect
        if (openEffectPrefab != null)
        {
            GameObject effect = Instantiate(openEffectPrefab, transform.position + Vector3.up, Quaternion.identity);
            Destroy(effect, 2f);
        }
        else
        {
            // Fallback effect
            CreateFallbackOpenEffect();
        }
        
        // Play sound
        if (openSound != null)
        {
            AudioSource.PlayClipAtPoint(openSound, transform.position);
        }
    }
    
    private void CreateFallbackOpenEffect()
    {
        // Create simple particle burst
        for (int i = 0; i < 10; i++)
        {
            GameObject particle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            particle.transform.position = transform.position + Vector3.up;
            particle.transform.localScale = Vector3.one * 0.2f;
            
            Renderer renderer = particle.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.yellow;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", Color.yellow * 2f);
            renderer.material = mat;
            
            Rigidbody rb = particle.AddComponent<Rigidbody>();
            rb.useGravity = true;
            Vector3 randomDir = Random.insideUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y); // Always go up
            rb.velocity = randomDir * 3f;
            
            Destroy(particle.GetComponent<Collider>());
            Destroy(particle, 1f);
        }
    }
    
    private void UpdateVisualState()
    {
        // Swap models if available
        if (closedChestModel != null && openChestModel != null)
        {
            closedChestModel.SetActive(!isOpened);
            openChestModel.SetActive(isOpened);
        }
    }
}