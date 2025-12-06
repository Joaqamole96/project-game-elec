/// <summary>
/// Gold coin - adds gold to player inventory
/// </summary>
public class GoldItemModel : ItemModel
{
    public int goldAmount = 10;
    
    protected override void ApplyEffect(PlayerController player)
    {
        if (player.inventory != null)
        {
            // Apply gold multiplier from powers
            int finalAmount = goldAmount;
            if (player.powerManager != null)
            {
                finalAmount = player.powerManager.ModifyGoldGained(goldAmount);
            }
            
            player.inventory.AddGold(finalAmount);
        }
    }
    
    public override string GetDescription()
    {
        return $"{goldAmount} Gold";
    }
}