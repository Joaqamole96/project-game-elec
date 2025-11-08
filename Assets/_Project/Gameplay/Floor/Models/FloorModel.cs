using UnityEngine;

/// <summary>Represents an individual walkable floor tile - use sparingly for gameplay tiles only</summary>
public class FloorModel
{
    public Vector2Int Position;
    public RoomType RoomType; // If part of a room
    public MaterialType Material;
    
    public FloorModel(Vector2Int position, RoomType roomType = RoomType.Combat)
    {
        Position = position;
        RoomType = roomType;
        Material = MaterialType.Default;
    }
}

public enum MaterialType
{
    Default, Cracked, Mossy, Bloody
}