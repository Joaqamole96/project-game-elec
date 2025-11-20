// RoomConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for room generation, sizing, and placement.
/// </summary>
[System.Serializable]
public class RoomConfig
{
    [Tooltip("Minimum room size in tiles (must be at least 3x3 for valid rooms)")]
    [Range(5, 30)] public int MinRoomSize = 20;

    [Tooltip("Maximum room size in tiles")]
    [Range(15, 50)] public int MaxRoomSize = 30;

    [Tooltip("Minimum inset from partition bounds for room creation")]
    [Range(1, 10)] public int MinInset = 4;

    [Tooltip("Maximum inset from partition bounds for room creation")]
    [Range(5, 15)] public int MaxInset = 8;

    // [Tooltip("Maximum number of rooms to generate per floor")]
    // [Range(5, 50)] public int MaxRooms = 20;
    // NOTE: Room count is implicitly controlled by Floor size and BSP partitioning, so this is deprecated
    
    [Tooltip("Padding around room edges for spawn positions and gameplay")]
    [Range(1, 5)] public int SpawnPadding = 2;

    // [Tooltip("Number of enemies spawned in each combat room")]
    // [Range(1, 10)] public int EnemiesPerCombatRoom = 3;
    // NOTE: Number of enemies in combat room is dependent on room size.

    /// <summary> Validates all configuration values to ensure they are within reasonable ranges. </summary>
    public void Validate()
    {
        MinInset = Mathf.Clamp(MinInset, 1, 5);
        MaxInset = Mathf.Clamp(MaxInset, 5, 10);
        MinRoomSize = Mathf.Clamp(MinRoomSize, 15, 25);
        MaxRoomSize = Mathf.Clamp(MaxRoomSize, 20, 40);
        SpawnPadding = Mathf.Clamp(SpawnPadding, 1, 5);

        if (MinRoomSize >= MaxRoomSize)
            MinRoomSize = MaxRoomSize - 5;
    }
}