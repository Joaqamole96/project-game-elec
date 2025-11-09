// RoomConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for room generation, sizing, and placement.
/// </summary>
[System.Serializable]
public class RoomConfig
{
    [Header("Room Inset Settings")]
    [Range(1, 10)]
    [Tooltip("Minimum inset from partition bounds for room creation")]
    public int MinInset = 4;

    [Range(5, 15)]
    [Tooltip("Maximum inset from partition bounds for room creation")]
    public int MaxInset = 8;

    [Header("Room Size Limits")]
    [Range(5, 30)]
    [Tooltip("Minimum room size in tiles (must be at least 3x3 for valid rooms)")]
    public int MinRoomSize = 20;

    [Range(15, 50)]
    [Tooltip("Maximum room size in tiles")]
    public int MaxRoomSize = 30;

    [Header("Room Count")]
    [Range(5, 50)]
    [Tooltip("Maximum number of rooms to generate per floor")]
    public int MaxRooms = 20;

    [Header("Room Padding")]
    [Range(1, 5)]
    [Tooltip("Padding around room edges for spawn positions and gameplay")]
    public int SpawnPadding = 2;

    /// <summary>
    /// Gets the minimum room dimension including insets.
    /// </summary>
    public int MinRoomDimension => MinRoomSize + (MinInset * 2);

    /// <summary>
    /// Creates a deep copy of this RoomConfig instance.
    /// </summary>
    public RoomConfig Clone()
    {
        return new RoomConfig
        {
            MinInset = MinInset,
            MaxInset = MaxInset,
            MinRoomSize = MinRoomSize,
            MaxRoomSize = MaxRoomSize,
            MaxRooms = MaxRooms,
            SpawnPadding = SpawnPadding
        };
    }

    /// <summary>
    /// Validates all configuration values to ensure they are within reasonable ranges.
    /// </summary>
    public void Validate()
    {
        MinInset = Mathf.Clamp(MinInset, 1, 10);
        MaxInset = Mathf.Clamp(MaxInset, 5, 15);
        MinRoomSize = Mathf.Max(3, MinRoomSize); // Ensure at least 3x3 rooms
        MinRoomSize = Mathf.Clamp(MinRoomSize, 5, 30);
        MaxRoomSize = Mathf.Clamp(MaxRoomSize, 15, 50);
        MaxRooms = Mathf.Clamp(MaxRooms, 5, 50);
        SpawnPadding = Mathf.Clamp(SpawnPadding, 1, 5);

        // Ensure min is less than max
        if (MinInset >= MaxInset)
        {
            MinInset = MaxInset - 2;
        }

        if (MinRoomSize >= MaxRoomSize)
        {
            MinRoomSize = MaxRoomSize - 5;
        }

        // Ensure rooms can actually be created with these settings
        if (MinRoomSize < 3)
        {
            MinRoomSize = 3;
            Debug.LogWarning("RoomConfig: MinRoomSize increased to 3 to ensure valid rooms");
        }
    }

    /// <summary>
    /// Gets a random inset value within configured bounds.
    /// </summary>
    public int GetRandomInset(System.Random random)
    {
        return random.Next(MinInset, MaxInset + 1);
    }

    /// <summary>
    /// Calculates the actual room size after applying insets to partition bounds.
    /// </summary>
    public int CalculateRoomSize(int partitionSize, int leftInset, int rightInset)
    {
        return partitionSize - (leftInset + rightInset);
    }

    /// <summary>
    /// Checks if a room of given size is valid according to configuration.
    /// </summary>
    public bool IsValidRoomSize(int width, int height)
    {
        return width >= MinRoomSize && height >= MinRoomSize &&
               width <= MaxRoomSize && height <= MaxRoomSize;
    }

    /// <summary>
    /// Checks if a partition can contain a valid room with current settings.
    /// </summary>
    public bool CanPartitionContainRoom(int partitionWidth, int partitionHeight)
    {
        int minRequiredWidth = MinRoomSize + (MinInset * 2);
        int minRequiredHeight = MinRoomSize + (MinInset * 2);
        
        return partitionWidth >= minRequiredWidth && partitionHeight >= minRequiredHeight;
    }
}