using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages player's collected powers and applies their effects
/// </summary>
public class PowerManager : MonoBehaviour
{
    [Header("Active Powers")]
    public List<PowerModel> activePowers = new();
    public int maxPowers = 10;
    
    [Header("Dash Settings (if Dash power active)")]
    public float dashCooldown = 2f;
    private float lastDashTime = 0f;
    
    [Header("Regen Settings")]
    private float regenTimer = 0f;
    private const float REGEN_INTERVAL = 5f;
    
    private PlayerController player;
    
    void Start()
    {
        player = GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PowerManager: No PlayerController found!");
        }
    }
    
    void Update()
    {
        HandleDashInput();
        HandleHealthRegen();
    }
    
    // ------------------------- //
    // POWER ACQUISITION
    // ------------------------- //
    
    public bool AddPower(PowerType powerType)
    {
        if (activePowers.Count >= maxPowers)
        {
            Debug.Log("PowerManager: Max powers reached!");
            return false;
        }
        
        // Check if already have this power
        if (HasPower(powerType))
        {
            Debug.Log($"PowerManager: Already have {powerType}");
            return false;
        }

        PowerModel newPower = new(powerType)
        {
            isActive = true
        };
        activePowers.Add(newPower);
        
        ApplyPowerEffect(newPower);
        
        Debug.Log($"PowerManager: Acquired {newPower.powerName}!");
        return true;
    }
    
    public bool HasPower(PowerType powerType)
    {
        return activePowers.Exists(p => p.type == powerType);
    }
    
    private void ApplyPowerEffect(PowerModel power)
    {
        if (player == null) return;
        
        switch (power.type)
        {
            case PowerType.SpeedBoost:
                player.moveSpeed *= (1f + power.value);
                Debug.Log($"Speed increased to {player.moveSpeed}");
                break;
                
            case PowerType.MaxHealth:
                player.maxHealth += (int)power.value;
                player.Heal((int)power.value); // Also heal by that amount
                Debug.Log($"Max health increased to {player.maxHealth}");
                break;
                
            case PowerType.AttackSpeed:
                player.attackCooldown *= (1f - power.value);
                Debug.Log($"Attack cooldown reduced to {player.attackCooldown}");
                break;
                
            case PowerType.Damage:
                player.playerDamage = Mathf.RoundToInt(player.playerDamage * (1f + power.value));
                Debug.Log($"Damage increased to {player.playerDamage}");
                break;
                
            // Other powers are handled dynamically when needed
        }
    }
    
    // ------------------------- //
    // ACTIVE ABILITIES
    // ------------------------- //
    
    private void HandleDashInput()
    {
        if (!HasPower(PowerType.Dash)) return;
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            PerformDash();
        }
    }
    
    private void PerformDash()
    {
        lastDashTime = Time.time;
        
        PowerModel dashPower = activePowers.Find(p => p.type == PowerType.Dash);
        if (dashPower == null) return;
        
        // Dash in movement direction (or forward if not moving)
        Vector3 dashDirection = player.transform.forward;
        
        // Apply dash force
        if (player.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.velocity = dashDirection * dashPower.value;
            Debug.Log("Dash!");
        }
    }
    
    private void HandleHealthRegen()
    {
        if (!HasPower(PowerType.HealthRegen)) return;
        
        regenTimer += Time.deltaTime;
        
        if (regenTimer >= REGEN_INTERVAL)
        {
            regenTimer = 0f;
            PowerModel regenPower = activePowers.Find(p => p.type == PowerType.HealthRegen);
            if (regenPower != null && player != null)
            {
                player.Heal((int)regenPower.value);
            }
        }
    }
    
    // ------------------------- //
    // COMBAT MODIFIERS
    // ------------------------- //
    
    public int ModifyDamageDealt(int baseDamage)
    {
        float finalDamage = baseDamage;
        
        // Check for critical hit
        if (HasPower(PowerType.CriticalHit))
        {
            PowerModel critPower = activePowers.Find(p => p.type == PowerType.CriticalHit);
            if (Random.value < critPower.value)
            {
                finalDamage *= 2f;
                Debug.Log("CRITICAL HIT!");
            }
        }
        
        return Mathf.RoundToInt(finalDamage);
    }
    
    public int ModifyDamageTaken(int incomingDamage)
    {
        float finalDamage = incomingDamage;
        
        // Damage reduction
        if (HasPower(PowerType.DamageReduction))
        {
            PowerModel defensePower = activePowers.Find(p => p.type == PowerType.DamageReduction);
            finalDamage *= (1f - defensePower.value);
        }
        
        return Mathf.Max(1, Mathf.RoundToInt(finalDamage));
    }
    
    public void OnDamageDealt(int damageDealt)
    {
        // Vampire healing
        if (HasPower(PowerType.Vampire) && player != null)
        {
            PowerModel vampirePower = activePowers.Find(p => p.type == PowerType.Vampire);
            int healAmount = Mathf.RoundToInt(damageDealt * vampirePower.value);
            if (healAmount > 0)
            {
                player.Heal(healAmount);
                Debug.Log($"Vampire heal: {healAmount} HP");
            }
        }
    }
    
    public int ModifyGoldGained(int baseGold)
    {
        if (HasPower(PowerType.ExtraGold))
        {
            PowerModel goldPower = activePowers.Find(p => p.type == PowerType.ExtraGold);
            return Mathf.RoundToInt(baseGold * (1f + goldPower.value));
        }
        return baseGold;
    }
    
    // ------------------------- //
    // UTILITY
    // ------------------------- //
    
    public void ClearAllPowers()
    {
        activePowers.Clear();
        Debug.Log("PowerManager: All powers cleared");
    }
    
    [ContextMenu("Print Active Powers")]
    public void PrintActivePowers()
    {
        Debug.Log("=== ACTIVE POWERS ===");
        foreach (var power in activePowers)
        {
            Debug.Log($"- {power.powerName}: {power.description}");
        }
        Debug.Log($"Total: {activePowers.Count}/{maxPowers}");
        Debug.Log("====================");
    }
    
    // Helper for UI
    public string GetPowerListString()
    {
        if (activePowers.Count == 0) return "No powers";
        
        string result = "";
        foreach (var power in activePowers)
        {
            result += $"• {power.powerName}\n";
        }
        return result;
    }
}