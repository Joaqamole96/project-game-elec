using System;
using UnityEngine;

[Serializable]
public class RoomConfig
{
    [Header("Room Configuration")]
    public int MinInset = 4;
    public int MaxInset = 8;
    public int MinRoomSize = 20;
    public int MaxRoomSize = 30;
    public int MaxRooms = 20;
}