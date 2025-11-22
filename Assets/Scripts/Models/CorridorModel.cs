// CorridorModel.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a corridor connecting two rooms in the dungeon.
/// Contains pathfinding data and door positions.
/// </summary>
public class CorridorModel
{
    /// <summary>List of tile positions that make up the corridor path.</summary>
    public List<Vector2Int> Tiles { get; private set; }
    
    /// <summary>Starting room connected by this corridor.</summary>
    public RoomModel StartRoom { get; private set; }
    
    /// <summary>Ending room connected by this corridor.</summary>
    public RoomModel EndRoom { get; private set; }
    
    /// <summary>Door position at the start room connection.</summary>
    public Vector2Int StartDoorPosition { get; private set; }
    
    /// <summary>Door position at the end room connection.</summary>
    public Vector2Int EndDoorPosition { get; private set; }
    
    /// <summary>Door model for the start room connection (created on demand).</summary>
    public DoorModel StartDoor { get; private set; }
    
    /// <summary>Door model for the end room connection (created on demand).</summary>
    public DoorModel EndDoor { get; private set; }
    
    /// <summary>Length of the corridor in tiles.</summary>
    public float Length => Tiles?.Count ?? 0f;
    
    /// <summary>Center position of the corridor.</summary>
    public Vector2Int Center => Tiles?.Count > 0 ? Tiles[Tiles.Count / 2] : Vector2Int.zero;

    public CorridorModel(List<Vector2Int> tiles, RoomModel startRoom, RoomModel endRoom, 
                        Vector2Int startDoorPos, Vector2Int endDoorPos)
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
    
    /// <summary>
    /// Gets or creates the door model for the start room connection.
    /// </summary>
    public DoorModel GetStartDoor()
    {
        StartDoor ??= new DoorModel(StartDoorPosition, DoorType.Wood);
        return StartDoor;
    }
    
    /// <summary>
    /// Gets or creates the door model for the end room connection.
    /// </summary>
    public DoorModel GetEndDoor()
    {
        EndDoor ??= new DoorModel(EndDoorPosition, DoorType.Wood);
        return EndDoor;
    }

    /// <summary>
    /// Checks if this corridor contains the specified position.
    /// </summary>
    public bool ContainsPosition(Vector2Int position) => Tiles.Contains(position);

    /// <summary>
    /// Checks if this corridor connects the specified room.
    /// </summary>
    public bool ConnectsRoom(RoomModel room)
    {
        return StartRoom == room || EndRoom == room;
    }

    /// <summary>
    /// Gets the other room connected by this corridor.
    /// </summary>
    public RoomModel GetOtherRoom(RoomModel room)
    {
        if (room == StartRoom) return EndRoom;
        if (room == EndRoom) return StartRoom;
        return null;
    }
}