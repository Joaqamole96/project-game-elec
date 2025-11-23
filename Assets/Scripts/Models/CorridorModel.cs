// -------------------------------------------------- //
// Scripts/Models/CorridorModel.cs
// -------------------------------------------------- //

using System.Collections.Generic;
using UnityEngine;

public class CorridorModel
{
    public List<Vector2Int> Tiles;
    public RoomModel StartRoom;
    public RoomModel EndRoom;
    public Vector2Int StartDoorPosition;
    public Vector2Int EndDoorPosition;

    public CorridorModel(List<Vector2Int> tiles, RoomModel startRoom, RoomModel endRoom, Vector2Int startDoorPos, Vector2Int endDoorPos)
    {
        Tiles = tiles ?? new List<Vector2Int>();
        StartRoom = startRoom;
        EndRoom = endRoom;
        StartDoorPosition = startDoorPos;
        EndDoorPosition = endDoorPos;

        ConnectRooms();
    }
    
    private void ConnectRooms()
    {
        if (StartRoom != null && EndRoom != null && StartRoom != EndRoom)
        {
            StartRoom.AddConnectedRoom(EndRoom);
            EndRoom.AddConnectedRoom(StartRoom);
        }
    }
}