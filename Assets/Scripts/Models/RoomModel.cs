// -------------------------------------------------- //
// Scripts/Models/RoomModel.cs
// -------------------------------------------------- //

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    public IEnumerable<Vector2Int> GetFloorTiles()
    {
        if (Bounds.width < 3 || Bounds.height < 3)
        {
            Debug.LogWarning($"Room {ID} bounds too small for floor tiles: {Bounds}");
            yield break;
        }

        for (int x = Bounds.xMin + 1; x < Bounds.xMax - 1; x++)
            for (int y = Bounds.yMin + 1; y < Bounds.yMax - 1; y++) yield return new Vector2Int(x, y);
    }

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

    public bool ContainsPosition(Vector2Int position) 
        => position.x >= Bounds.xMin &&
            position.x < Bounds.xMax &&
            position.y >= Bounds.yMin &&
            position.y < Bounds.yMax;

    public IEnumerable<Vector2Int> GetInnerTiles(int padding = 1)
    {
        for (int x = Bounds.xMin + padding; x < Bounds.xMax - padding; x++)
            for (int y = Bounds.yMin + padding; y < Bounds.yMax - padding; y++)
                yield return new Vector2Int(x, y);
    }
    
    public void AddConnectedRoom(RoomModel room)
    {
        if (room != null && room != this && !ConnectedRooms.Contains(room)) ConnectedRooms.Add(room);
    }

    public void GenerateSpawnPositions(int count, int padding = 3)
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

    public Vector2Int GetRandomSpawnPosition()
    {
        if (SpawnPositions.Count == 0) return Center;
        return SpawnPositions[Random.Range(0, SpawnPositions.Count)];
    }

    public void MarkAsCleared()
    {
        IsCleared = true;
        if (State == RoomAccess.Closed) State = RoomAccess.Open;
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

public enum RoomAccess { Open, Closed, Locked }

public enum RoomType 
{
    // Critical path
    Entrance, Exit,

    // Standard rooms
    Empty, Combat, Shop, Treasure,

    // Special rooms
    Boss, Survival, Puzzle, Secret
}