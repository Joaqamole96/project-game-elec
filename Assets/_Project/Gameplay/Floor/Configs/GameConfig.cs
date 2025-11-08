using UnityEngine;

[System.Serializable]
public class GameConfig
{
    [Header("Game Settings")]
    public bool SimplifyGeometry = true;
    public int EnemiesPerCombatRoom = 3;
    public int TreasureRoomsPerFloor = 1;
    public int ShopRoomsPerFloor = 1;
}