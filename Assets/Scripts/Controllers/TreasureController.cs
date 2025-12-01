// ================================================== //
// FILE 2: TreasureChestController.cs
// ================================================== //

using UnityEngine;

public class TreasureChestController : MonoBehaviour
{
    [Header("Treasure Settings")]
    public bool isOpened = false;
    public TreasureType treasureType = TreasureType.Random;
    
    [Header("Specific Rewards (if not random)")]
    public PowerType specificPower;
    public GameObject specificWeapon;
    public int goldAmount = 100;
    
    public enum TreasureType
    {
        Random,
        PowerModel,
        Weapon,
        Gold
    }
    
    void Start()
    {
        // Ensure has trigger collider
        if (!TryGetComponent<Collider>(out var collider))
        {
            BoxCollider box = gameObject.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(2f, 2f, 2f);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            OpenChest(other.GetComponent<PlayerController>());
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
        Debug.Log("Chest opened!");
        // TODO: Play animation, particle effect, sound
        
        // Destroy or hide chest
        Destroy(gameObject, 1f);
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
            player.powerManager.AddPower(powerToGive);
            Debug.Log($"Treasure: Gained power {powerToGive}!");
        }
    }
    
    private void GiveWeaponReward(PlayerController player)
    {
        Debug.Log("Treasure: Found weapon!");
    }
    
    private void GiveGoldReward(PlayerController player)
    {
        if (player.inventory != null)
        {
            player.inventory.AddGold(goldAmount);
            Debug.Log($"Treasure: Found {goldAmount} gold!");
        }
    }
}