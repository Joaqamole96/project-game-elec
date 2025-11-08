using System.Collections.Generic;
using UnityEngine;

public class CorridorModel
{
    public List<Vector2Int> Tiles;
    public RoomModel StartRoom, EndRoom;
    
    // For algorithm compatibility - KEEP these as Vector2Int
    public Vector2Int StartDoorPosition { get; private set; }
    public Vector2Int EndDoorPosition { get; private set; }
    
    // For gameplay - created on demand
    public DoorModel StartDoor { get; private set; }
    public DoorModel EndDoor { get; private set; }

    public float Length => Tiles?.Count ?? 0f;

    public CorridorModel(List<Vector2Int> tiles, RoomModel startRoom, RoomModel endRoom, 
                        Vector2Int startDoorPos, Vector2Int endDoorPos)
    {
        Tiles = tiles ?? new List<Vector2Int>();
        StartRoom = startRoom;
        EndRoom = endRoom;
        StartDoorPosition = startDoorPos;
        EndDoorPosition = endDoorPos;

        // Safe connection
        if (startRoom != null && endRoom != null && startRoom != endRoom)
        {
            if (!startRoom.ConnectedRooms.Contains(endRoom))
                startRoom.ConnectedRooms.Add(endRoom);
            if (!endRoom.ConnectedRooms.Contains(startRoom))
                endRoom.ConnectedRooms.Add(startRoom);
        }
    }
    
    // Create DoorModels only when needed (lazy initialization)
    public DoorModel GetStartDoor()
    {
        StartDoor ??= new DoorModel(StartDoorPosition, DoorType.Wood);
        return StartDoor;
    }
    
    public DoorModel GetEndDoor()
    {
        EndDoor ??= new DoorModel(EndDoorPosition, DoorType.Wood);
        return EndDoor;
    }

    public bool ContainsPosition(Vector2Int position) => Tiles.Contains(position);

    public Vector2Int GetCenter() => Tiles?.Count > 0 ? Tiles[Tiles.Count / 2] : Vector2Int.zero;
}