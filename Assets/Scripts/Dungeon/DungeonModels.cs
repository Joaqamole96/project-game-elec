using UnityEngine;
using System.Collections.Generic;

public enum WallType
{
    North, South, East, West,
    NorthEastCorner, NorthWestCorner,
    SouthEastCorner, SouthWestCorner,
    Interior, Doorway
}

public enum RoomType
{
    Entrance, Exit, Empty, Combat, Shop, Treasure, Boss
}

public enum RoomState
{
    Open, Closed, Completed
}

[System.Serializable]
public class DungeonConfig
{
    [Header("Dungeon Dimensions")]
    public int Width = 30;
    public int Height = 30;
    public int Seed = 0;
    
    [Header("Partition Settings")]
    public int MinimumPartitionSize = 10;
    
    [Header("Room Settings")]
    public int MinimumInset = 1;
    public int MaximumInset = 2;
    
    [Header("Graph Settings")]
    public int ExtraConnections = 3;
    
    [Header("Rendering Settings")]
    public float FloorHeight = 0f;
    public float WallHeight = 1f;
    public float DoorHeight = 0.8f;
    
    [Header("Floor Progression")]
    public int FloorLevel = 1;
    public int FloorGrowth = 2;
    public int MaxFloorSize = 50;
}

public class PartitionModel
{
    public RectInt Bounds;
    public PartitionModel LeftChild;
    public PartitionModel RightChild;
    public RoomModel Room;
    public List<PartitionModel> Neighbors;

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
        Neighbors = new List<PartitionModel>();
    }
}

public class RoomModel
{
    public RectInt Bounds;
    public int Id;

    public RoomModel(RectInt bounds, int id)
    {
        Bounds = bounds;
        Id = id;
    }

    public IEnumerable<Vector2Int> GetFloorTiles()
    {
        for (int x = Bounds.xMin + 1; x < Bounds.xMax - 1; x++)
        for (int y = Bounds.yMin + 1; y < Bounds.yMax - 1; y++)
        {
            yield return new Vector2Int(x, y);
        }
    }

    public IEnumerable<Vector2Int> GetWallPerimeter()
    {
        // North wall
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            yield return new Vector2Int(x, Bounds.yMax - 1);
        // South wall
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            yield return new Vector2Int(x, Bounds.yMin);
        // East wall
        for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            yield return new Vector2Int(Bounds.xMax - 1, y);
        // West wall
        for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            yield return new Vector2Int(Bounds.xMin, y);
    }
}

public class CorridorModel
{
    public List<Vector2Int> Tiles;
    public RoomModel StartRoom;
    public RoomModel EndRoom;
    public Vector2Int StartDoor;
    public Vector2Int EndDoor;

    public CorridorModel(List<Vector2Int> tiles, RoomModel startRoom, RoomModel endRoom, Vector2Int startDoor, Vector2Int endDoor)
    {
        Tiles = tiles;
        StartRoom = startRoom;
        EndRoom = endRoom;
        StartDoor = startDoor;
        EndDoor = endDoor;
    }
}

public class RoomAssignment
{
    public RoomModel Room;
    public RoomType Type;
    public RoomState State;
    public int DistanceFromEntrance;

    public RoomAssignment(RoomModel room, RoomType type)
    {
        Room = room;
        Type = type;
        State = GetDefaultStateForType(type);
    }
    
    private RoomState GetDefaultStateForType(RoomType type)
    {
        return type switch
        {
            RoomType.Combat or RoomType.Boss => RoomState.Closed,
            _ => RoomState.Open
        };
    }
}

public class DungeonLayout
{
    public List<RoomModel> Rooms = new List<RoomModel>();
    public List<CorridorModel> Corridors = new List<CorridorModel>();
    public HashSet<Vector2Int> AllFloorTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> AllWallTiles = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> AllDoorTiles = new HashSet<Vector2Int>();
    public Dictionary<Vector2Int, WallType> WallTypes = new Dictionary<Vector2Int, WallType>();
}