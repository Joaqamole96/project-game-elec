// GameConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for core gameplay mechanics and balancing.
/// </summary>
[System.Serializable]
public class GameConfig
{
    [Header("Geometry Settings")]
    [Tooltip("Simplify geometry for better performance on mobile devices")]
    public bool SimplifyGeometry = true;

    [Header("Gameplay Balance")]
    [Range(1, 10)]
    [Tooltip("Number of enemies spawned in each combat room")]
    public int EnemiesPerCombatRoom = 3;

    [Range(0, 5)]
    [Tooltip("Maximum number of treasure rooms per floor")]
    public int TreasureRoomsPerFloor = 1;

    [Range(0, 3)]
    [Tooltip("Maximum number of shop rooms per floor")]
    public int ShopRoomsPerFloor = 1;

    [Header("Player Progression")]
    [Range(1, 100)]
    [Tooltip("Base player health at start of game")]
    public int PlayerBaseHealth = 100;

    [Range(1f, 10f)]
    [Tooltip("Player movement speed in units per second")]
    public float PlayerMovementSpeed = 5f;

    /// <summary>
    /// Creates a deep copy of this GameConfig instance.
    /// </summary>
    public GameConfig Clone()
    {
        return new GameConfig
        {
            SimplifyGeometry = SimplifyGeometry,
            EnemiesPerCombatRoom = EnemiesPerCombatRoom,
            TreasureRoomsPerFloor = TreasureRoomsPerFloor,
            ShopRoomsPerFloor = ShopRoomsPerFloor,
            PlayerBaseHealth = PlayerBaseHealth,
            PlayerMovementSpeed = PlayerMovementSpeed
        };
    }

    /// <summary>
    /// Validates all configuration values to ensure they are within reasonable ranges.
    /// </summary>
    public void Validate()
    {
        EnemiesPerCombatRoom = Mathf.Clamp(EnemiesPerCombatRoom, 1, 10);
        TreasureRoomsPerFloor = Mathf.Clamp(TreasureRoomsPerFloor, 0, 5);
        ShopRoomsPerFloor = Mathf.Clamp(ShopRoomsPerFloor, 0, 3);
        PlayerBaseHealth = Mathf.Clamp(PlayerBaseHealth, 1, 100);
        PlayerMovementSpeed = Mathf.Clamp(PlayerMovementSpeed, 1f, 10f);
    }
}