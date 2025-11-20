// CorridorModel.cs
using System.Collections.Generic;
using UnityEngine;

public class CorridorModel
{
    public List<Vector2Int> Tiles;
    public RoomModel StartRoom;
    public RoomModel EndRoom;
    public DoorModel StartDoor;
    public DoorModel EndDoor;

    public CorridorModel(List<Vector2Int> tiles, RoomModel startRoom, RoomModel endRoom, DoorModel startDoor, DoorModel endDoor)
    {
        Tiles = tiles;
        StartRoom = startRoom;
        EndRoom = endRoom;
        StartDoor = startDoor;
        EndDoor = endDoor;

        if ((StartRoom != null) && (EndRoom != null) && (StartRoom != EndRoom))
        {
            StartRoom.AddConnectedRoom(EndRoom);
            EndRoom.AddConnectedRoom(StartRoom);
        }
    }
}