using UnityEngine;

/// <summary>
/// Small health potion - restores 30 HP
/// </summary>
public class HealthPotionModel : ItemModel
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