using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>A data model of a room.</summary>
public class RoomModel
{
    public RectInt Bounds;
    public int ID;
    public List<RoomModel> ConnectedRooms;
    public RoomType Type;
    public RoomAccess State;
    public int DistanceFromEntrance;
    public bool IsRevealed;
    public bool IsCleared;
    public List<Vector2Int> SpawnPositions;
    public DoorModel EntranceDoor, ExitDoor;

    public Vector2Int Center => new(
        (Bounds.xMin + Bounds.xMax) / 2,
        (Bounds.yMin + Bounds.yMax) / 2
    );

    public RoomModel(RectInt bounds, int id, RoomType type)
    {
        Bounds = bounds;
        ID = id;
        ConnectedRooms = new List<RoomModel>();
        Type = type;
        State = GetDefaultStateForType(type);
        IsRevealed = type == RoomType.Entrance;
        IsCleared = type != RoomType.Combat && type != RoomType.Boss;
        SpawnPositions = new List<Vector2Int>();
    }

    /// <summary>Enumerates the position of each floor tile in the room.</summary>
    public IEnumerable<Vector2Int> GetFloorTiles()
    {
        for (int x = Bounds.xMin + 1; x < Bounds.xMax - 1; x++)
            for (int y = Bounds.yMin + 1; y < Bounds.yMax - 1; y++)
                yield return new Vector2Int(x, y);
    }

    /// <summary>Enumerates the position of each wall tile on each face of the room.</summary>
    public IEnumerable<Vector2Int> GetWallPerimeter()
    {
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            yield return new Vector2Int(x, Bounds.yMax - 1);
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            yield return new Vector2Int(x, Bounds.yMin);
        for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            yield return new Vector2Int(Bounds.xMax - 1, y);
        for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            yield return new Vector2Int(Bounds.xMin, y);
    }

    /// <summary>Checks if this position is within the bounds of the room.</summary>
    /// <param name="position">The position to check within the bounds of the room.</param>
    public bool ContainsPosition(Vector2Int position)
    {
        return
        position.x >= Bounds.xMin &&
        position.x < Bounds.xMax &&
        position.y >= Bounds.yMin &&
        position.y < Bounds.yMax;
    }

    public IEnumerable<Vector2Int> GetInnerTiles(int padding = 1)
    {
        for (int x = Bounds.xMin + padding; x < Bounds.xMax - padding; x++)
            for (int y = Bounds.yMin + padding; y < Bounds.yMax - padding; y++)
                yield return new Vector2Int(x, y);
    }
    
    private RoomAccess GetDefaultStateForType(RoomType type)
    {
        return type switch 
        { 
            RoomType.Combat or RoomType.Boss => RoomAccess.Closed, 
            _ => RoomAccess.Open 
        };
    }
    
    public void GenerateSpawnPositions(int count, int padding = 2)
    {
        var innerTiles = GetInnerTiles(padding).ToList();
        if (innerTiles.Count == 0) return;
        
        SpawnPositions.Clear();
        // Use HashSet to avoid duplicate positions
        var usedPositions = new HashSet<Vector2Int>();
        
        for (int i = 0; i < Mathf.Min(count, innerTiles.Count); i++)
        {
            Vector2Int spawnPos;
            int attempts = 0;
            do {
                spawnPos = innerTiles[Random.Range(0, innerTiles.Count)];
                attempts++;
            } while (usedPositions.Contains(spawnPos) && attempts < 10);
            
            SpawnPositions.Add(spawnPos);
            usedPositions.Add(spawnPos);
        }
    }
}

/// <summary>The state of a room that determines its accessibility.</summary>
public enum RoomAccess
{
    Open, Closed,
}

/// <summary>The type of a room that defines its purpose and functionality.</summary>
public enum RoomType
{
    // Endpoints
    Entrance, Exit,

    // Standard
    Empty, Combat, Shop, Treasure,

    // Special
    Boss, Survival, Puzzle, Pursuit,
}

/// <summary>The saved data of a room.</summary>
[System.Serializable]
public class RoomSaveData
{
    public int RoomID;
    public RoomAccess Access;
    public bool IsRevealed;
    public bool IsCleared;
}