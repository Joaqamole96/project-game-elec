// -------------------------------------------------- //
// Scripts/Configs/RoomConfig.cs
// -------------------------------------------------- //

using UnityEngine;

[CreateAssetMenu(fileName = "RoomConfig", menuName = "Configs/RoomConfig")]
[System.Serializable]
public class RoomConfig : ScriptableObject
{
    [Range(1, 10)] public int MinInset = 4;

    [Range(5, 15)] public int MaxInset = 8;

    [Range(5, 30)] public int MinRoomSize = 20;

    [Range(15, 50)] public int MaxRoomSize = 30;

    [Range(5, 50)] public int MaxRooms = 20;

    [Range(1, 5)] public int SpawnPadding = 2;

    // ------------------------- //

    public void Validate()
    {
        MinInset = Mathf.Clamp(MinInset, 1, 10);
        MaxInset = Mathf.Clamp(MaxInset, 5, 15);
        MinRoomSize = Mathf.Max(3, MinRoomSize);
        MinRoomSize = Mathf.Clamp(MinRoomSize, 5, 30);
        MaxRoomSize = Mathf.Clamp(MaxRoomSize, 15, 50);
        MaxRooms = Mathf.Clamp(MaxRooms, 5, 50);
        SpawnPadding = Mathf.Clamp(SpawnPadding, 1, 5);

        if (MinInset >= MaxInset) MinInset = MaxInset - 2;
        if (MinRoomSize >= MaxRoomSize) MinRoomSize = MaxRoomSize - 5;
        if (MinRoomSize < 5) MinRoomSize = 5;
    }
}