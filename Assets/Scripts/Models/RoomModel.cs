// RoomModel.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a room in the dungeon with bounds, type, and gameplay state.
/// </summary>
public class RoomModel
{
    /// <summary>Bounds of the room in grid coordinates.</summary>
    public RectInt Bounds { get; private set; }
    
    /// <summary>Unique identifier for this room.</summary>
    public int ID { get; private set; }
    
    /// <summary>Rooms connected to this room via corridors.</summary>
    public List<RoomModel> ConnectedRooms { get; private set; }
    
    /// <summary>Type and purpose of this room.</summary>
    public RoomType Type { get; set; }
    
    /// <summary>Access state of the room.</summary>
    public RoomAccess State { get; set; }
    
    /// <summary>Distance from entrance room for progression.</summary>
    public int DistanceFromEntrance { get; set; }
    
    /// <summary>Whether the room has been revealed to the player.</summary>
    public bool IsRevealed { get; set; }
    
    /// <summary>Whether the room has been cleared of enemies.</summary>
    public bool IsCleared { get; set; }
    
    /// <summary>Positions for spawning enemies or objects.</summary>
    public List<Vector2Int> SpawnPositions { get; private set; }
    
    /// <summary>Door at the room entrance (if any).</summary>
    public DoorModel EntranceDoor { get; set; }
    
    /// <summary>Door at the room exit (if any).</summary>
    public DoorModel ExitDoor { get; set; }
    
    /// <summary>Center point of the room.</summary>
    public Vector2Int Center => new(
        (Bounds.xMin + Bounds.xMax) / 2,
        (Bounds.yMin + Bounds.yMax) / 2
    );
    
    /// <summary>Area of the room in tiles.</summary>
    public int Area => Bounds.width * Bounds.height;

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

    /// <summary>
    /// Enumerates all floor tile positions within the room (excluding walls).
    /// </summary>
    public IEnumerable<Vector2Int> GetFloorTiles()
    {
        // Ensure we have valid bounds that can contain floor tiles
        if (Bounds.width < 3 || Bounds.height < 3)
        {
            Debug.LogWarning($"Room {ID} bounds too small for floor tiles: {Bounds}");
            yield break;
        }

        for (int x = Bounds.xMin + 1; x < Bounds.xMax - 1; x++)
        {
            for (int y = Bounds.yMin + 1; y < Bounds.yMax - 1; y++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    /// <summary>
    /// Enumerates all wall tile positions around the room perimeter.
    /// </summary>
    public IEnumerable<Vector2Int> GetWallPerimeter()
    {
        // Top wall
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            yield return new Vector2Int(x, Bounds.yMax - 1);
        
        // Bottom wall
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
            yield return new Vector2Int(x, Bounds.yMin);
        
        // Right wall
        for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            yield return new Vector2Int(Bounds.xMax - 1, y);
        
        // Left wall
        for (int y = Bounds.yMin; y < Bounds.yMax; y++)
            yield return new Vector2Int(Bounds.xMin, y);
    }

    /// <summary>
    /// Checks if the position is within the room bounds.
    /// </summary>
    public bool ContainsPosition(Vector2Int position)
    {
        return position.x >= Bounds.xMin &&
               position.x < Bounds.xMax &&
               position.y >= Bounds.yMin &&
               position.y < Bounds.yMax;
    }

    /// <summary>
    /// Enumerates inner tiles with optional padding from walls.
    /// </summary>
    public IEnumerable<Vector2Int> GetInnerTiles(int padding = 1)
    {
        for (int x = Bounds.xMin + padding; x < Bounds.xMax - padding; x++)
            for (int y = Bounds.yMin + padding; y < Bounds.yMax - padding; y++)
                yield return new Vector2Int(x, y);
    }
    
    /// <summary>
    /// Adds a connected room if not already present.
    /// </summary>
    public void AddConnectedRoom(RoomModel room)
    {
        if (room != null && room != this && !ConnectedRooms.Contains(room))
        {
            ConnectedRooms.Add(room);
        }
    }

    /// <summary>
    /// Checks if this room is connected to the specified room.
    /// </summary>
    public bool IsConnectedTo(RoomModel room)
    {
        return ConnectedRooms.Contains(room);
    }

    /// <summary>
    /// Generates spawn positions within the room with proper spacing.
    /// </summary>
    public void GenerateSpawnPositions(int count, int padding = 2)
    {
        var innerTiles = GetInnerTiles(padding).ToList();
        if (innerTiles.Count == 0) return;
        
        SpawnPositions.Clear();
        var usedPositions = new HashSet<Vector2Int>();
        
        for (int i = 0; i < Mathf.Min(count, innerTiles.Count); i++)
        {
            Vector2Int spawnPos = FindValidSpawnPosition(innerTiles, usedPositions);
            if (spawnPos != Vector2Int.zero)
            {
                SpawnPositions.Add(spawnPos);
                usedPositions.Add(spawnPos);
            }
        }
    }

    private Vector2Int FindValidSpawnPosition(List<Vector2Int> availableTiles, HashSet<Vector2Int> usedPositions)
    {
        int attempts = 0;
        Vector2Int spawnPos;
        
        do {
            spawnPos = availableTiles[Random.Range(0, availableTiles.Count)];
            attempts++;
        } while (usedPositions.Contains(spawnPos) && attempts < 10);
        
        return attempts < 10 ? spawnPos : Vector2Int.zero;
    }

    /// <summary>
    /// Gets a random spawn position from the generated list.
    /// </summary>
    public Vector2Int GetRandomSpawnPosition()
    {
        if (SpawnPositions.Count == 0) return Center;
        return SpawnPositions[Random.Range(0, SpawnPositions.Count)];
    }

    /// <summary>
    /// Marks the room as cleared and updates its state.
    /// </summary>
    public void MarkAsCleared()
    {
        IsCleared = true;
        if (State == RoomAccess.Closed)
        {
            State = RoomAccess.Open;
        }
    }

    /// <summary>
    /// Marks the room as revealed to the player.
    /// </summary>
    public void Reveal()
    {
        IsRevealed = true;
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

/// <summary>
/// Accessibility state of a room.
/// </summary>
public enum RoomAccess
{
    Open,
    Closed,
    Locked
}

/// <summary>
/// Purpose and functionality of a room.
/// </summary>
public enum RoomType
{
    // Critical path
    Entrance,
    Exit,

    // Standard rooms
    Empty,
    Combat,
    Shop,
    Treasure,

    // Special rooms
    Boss,
    Survival,
    Puzzle,
    Secret
}

/// <summary>
/// Serialized data for saving room state.
/// </summary>
[System.Serializable]
public class RoomSaveData
{
    public int RoomID;
    public RoomAccess Access;
    public bool IsRevealed;
    public bool IsCleared;
}