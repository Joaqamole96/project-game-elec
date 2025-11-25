// ================================================== //
// Scripts/Systems/PowerSystem.cs - COMPLETE
// ================================================== //

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// PowerModel types available in the game
/// </summary>
public enum PowerType
{
    // Movement
    SpeedBoost,      // +20% movement speed
    Dash,            // Quick dash ability
    
    // Combat
    AttackSpeed,     // +30% attack speed
    Damage,          // +25% damage
    CriticalHit,     // 15% chance for 2x damage
    
    // Defense
    MaxHealth,       // +20 max HP
    HealthRegen,     // Regenerate 1 HP per 5 seconds
    DamageReduction, // Take 15% less damage
    
    // Utility
    Vampire,         // Heal 10% of damage dealt
    ExtraGold,       // +50% gold from enemies
    LuckyFind        // Higher chance for rare drops
}

/// <summary>
/// Individual power instance with stats
/// </summary>
[System.Serializable]
public class PowerModel
{
    public PowerType type;
    public string powerName;
    public string description;
    public Sprite icon;
    public float value; // Multiplier or flat bonus
    public bool isActive;
    
    public PowerModel(PowerType powerType)
    {
        type = powerType;
        isActive = false;
        
        switch (powerType)
        {
            case PowerType.SpeedBoost:
                powerName = "Swift Feet";
                description = "Move 20% faster";
                value = 0.2f;
                break;
                
            case PowerType.Dash:
                powerName = "Shadow Step";
                description = "Quick dash ability (Shift key)";
                value = 10f; // Dash distance
                break;
                
            case PowerType.AttackSpeed:
                powerName = "Rapid Strikes";
                description = "Attack 30% faster";
                value = 0.3f;
                break;
                
            case PowerType.Damage:
                powerName = "Ferocious Strike";
                description = "Deal 25% more damage";
                value = 0.25f;
                break;
                
            case PowerType.CriticalHit:
                powerName = "Deadly Precision";
                description = "15% chance for critical hits (2x damage)";
                value = 0.15f;
                break;
                
            case PowerType.MaxHealth:
                powerName = "Vitality";
                description = "+20 maximum health";
                value = 20f;
                break;
                
            case PowerType.HealthRegen:
                powerName = "Regeneration";
                description = "Regenerate 1 HP every 5 seconds";
                value = 1f;
                break;
                
            case PowerType.DamageReduction:
                powerName = "Iron Skin";
                description = "Take 15% less damage";
                value = 0.15f;
                break;
                
            case PowerType.Vampire:
                powerName = "Life Steal";
                description = "Heal for 10% of damage dealt";
                value = 0.1f;
                break;
                
            case PowerType.ExtraGold:
                powerName = "Golden Touch";
                description = "+50% gold from enemies";
                value = 0.5f;
                break;
                
            case PowerType.LuckyFind:
                powerName = "Fortune's Favor";
                description = "Increased rare item drop chance";
                value = 0.2f;
                break;
        }
    }
}

