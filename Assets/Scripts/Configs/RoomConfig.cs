// -------------------------------------------------- //
// Scripts/Configs/RoomConfig.cs
// -------------------------------------------------- //

using UnityEngine;

[System.Serializable]
public class RoomConfig
{
    [Range(1, 10)] public int MinInset = 4;

    [Range(5, 15)] public int MaxInset = 8;

    [Range(5, 30)] public int MinRoomSize = 20;

    [Range(15, 50)] public int MaxRoomSize = 30;

    [Range(5, 50)] public int MaxRooms = 20;

    [Range(1, 5)] public int SpawnPadding = 2;

    // public int MinRoomDimension => MinRoomSize + (MinInset * 2);

    // ------------------------- //

    // public RoomConfig Clone() => this;

    public void Validate()
    {
        MinInset = Mathf.Clamp(MinInset, 1, 10);
        MaxInset = Mathf.Clamp(MaxInset, 5, 15);
        MinRoomSize = Mathf.Max(3, MinRoomSize); // Ensure at least 3x3 rooms
        MinRoomSize = Mathf.Clamp(MinRoomSize, 5, 30);
        MaxRoomSize = Mathf.Clamp(MaxRoomSize, 15, 50);
        MaxRooms = Mathf.Clamp(MaxRooms, 5, 50);
        SpawnPadding = Mathf.Clamp(SpawnPadding, 1, 5);

        if (MinInset >= MaxInset) MinInset = MaxInset - 2;

        if (MinRoomSize >= MaxRoomSize) MinRoomSize = MaxRoomSize - 5;

        // Ensure rooms can actually be created with these settings
        if (MinRoomSize < 5) MinRoomSize = 5;
    }

    // public int GetRandomInset(System.Random random) => random.Next(MinInset, MaxInset + 1);

    // public int CalculateRoomSize(int partitionSize, int leftInset, int rightInset) => partitionSize - (leftInset + rightInset);

    // public bool IsValidRoomSize(int width, int height)
    //     => width >= MinRoomSize && 
    //         height >= MinRoomSize &&
    //         width <= MaxRoomSize && 
    //         height <= MaxRoomSize;

    // public bool CanPartitionContainRoom(int partitionWidth, int partitionHeight)
    // {
    //     int minRequiredWidth = MinRoomSize + (MinInset * 2);
    //     int minRequiredHeight = MinRoomSize + (MinInset * 2);
        
    //     return partitionWidth >= minRequiredWidth && partitionHeight >= minRequiredHeight;
    // }
}