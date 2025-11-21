using System.Collections.Generic;
using UnityEngine;

public class RoomModel
{
    // System constants
    public static readonly int MIN_SIZE = 15;
    public static readonly int MAX_SIZE = 25;
    public static readonly int MIN_INSET = 2;
    public static readonly int MAX_INSET = 4;
    public static readonly int SPAWN_PADDING = 2;
    
    // Instance data
    public RectInt Bounds;
    public int ID;
    public List<RoomModel> ConnectedRooms = new();
    public RoomType Type;
    public RoomAccess State;
    public int DistanceFromEntrance;
    public bool IsRevealed;
    public bool IsCleared;
    public List<Vector2Int> SpawnPositions = new();

    public Vector2Int Center => new(
        (Bounds.xMin + Bounds.xMax) / 2,
        (Bounds.yMin + Bounds.yMax) / 2
    );

    public RoomModel(RectInt bounds, int id, RoomType type)
    {
        Bounds = bounds;
        ID = id;
        Type = type;
        State = GetDefaultStateForType(type);
        IsRevealed = type == RoomType.Entrance;
        IsCleared = type != RoomType.Combat && type != RoomType.Boss;
    }

    public bool ContainsPosition(Vector2Int position)
    {
        return position.x >= Bounds.xMin && position.x < Bounds.xMax &&
               position.y >= Bounds.yMin && position.y < Bounds.yMax;
    }

    private RoomAccess GetDefaultStateForType(RoomType type)
    {
        return type switch 
        { 
            RoomType.Combat or RoomType.Boss => RoomAccess.Closed, 
            _ => RoomAccess.Open 
        };
    }
}