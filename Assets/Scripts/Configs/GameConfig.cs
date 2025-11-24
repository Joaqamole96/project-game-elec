// -------------------------------------------------- //
// Scripts/Configs/GameConfig.cs
// -------------------------------------------------- //

using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "Configs/Game Config")]
[System.Serializable]
public class GameConfig : ScriptableObject
{
    public bool SimplifyGeometry = true;

    [Range(1, 10)] public int EnemiesPerCombatRoom = 3;

    [Range(0, 5)] public int TreasureRoomsPerFloor = 1;

    [Range(0, 3)] public int ShopRoomsPerFloor = 1;

    // ------------------------- //

    // public GameConfig Clone() => this;

    public void Validate()
    {
        EnemiesPerCombatRoom = Mathf.Clamp(EnemiesPerCombatRoom, 1, 10);
        TreasureRoomsPerFloor = Mathf.Clamp(TreasureRoomsPerFloor, 0, 5);
        ShopRoomsPerFloor = Mathf.Clamp(ShopRoomsPerFloor, 0, 3);
    }
}