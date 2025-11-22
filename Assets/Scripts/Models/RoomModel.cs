// -------------------- //
// Scripts/Models/RoomModel.cs
// -------------------- //

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

    /// <summary>
    /// Returns all floor tile positions within the room (excluding walls)
    /// </summary>
    public IEnumerable<Vector2Int> GetFloorTiles()
    {
        for (int x = Bounds.xMin + 1; x < Bounds.xMax - 1; x++)
        {
            for (int y = Bounds.yMin + 1; y < Bounds.yMax - 1; y++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    /// <summary>
    /// Returns all wall perimeter positions (outer border of room)
    /// </summary>
    public IEnumerable<Vector2Int> GetWallPerimeter()
    {
        // Top and bottom walls
        for (int x = Bounds.xMin; x < Bounds.xMax; x++)
        {
            yield return new Vector2Int(x, Bounds.yMin);
            yield return new Vector2Int(x, Bounds.yMax - 1);
        }
        
        // Left and right walls (excluding corners already counted)
        for (int y = Bounds.yMin + 1; y < Bounds.yMax - 1; y++)
        {
            yield return new Vector2Int(Bounds.xMin, y);
            yield return new Vector2Int(Bounds.xMax - 1, y);
        }
    }

    /// <summary>
    /// Generates enemy spawn positions within the room
    /// </summary>
    public void GenerateSpawnPositions(int spawnCount)
    {
        SpawnPositions.Clear();
        
        if (Bounds.width <= SPAWN_PADDING * 2 || Bounds.height <= SPAWN_PADDING * 2)
        {
            Debug.LogWarning($"Room {ID} too small for spawn positions");
            return;
        }

        int minX = Bounds.xMin + SPAWN_PADDING;
        int maxX = Bounds.xMax - SPAWN_PADDING;
        int minY = Bounds.yMin + SPAWN_PADDING;
        int maxY = Bounds.yMax - SPAWN_PADDING;

        // Generate grid-based spawn positions to avoid clustering
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(spawnCount));
        float cellWidth = (maxX - minX) / (float)gridSize;
        float cellHeight = (maxY - minY) / (float)gridSize;

        for (int i = 0; i < spawnCount; i++)
        {
            int gridX = i % gridSize;
            int gridY = i / gridSize;
            
            int posX = minX + Mathf.RoundToInt(gridX * cellWidth + cellWidth / 2);
            int posY = minY + Mathf.RoundToInt(gridY * cellHeight + cellHeight / 2);
            
            // Add small random offset
            posX += Random.Range(-1, 2);
            posY += Random.Range(-1, 2);
            
            posX = Mathf.Clamp(posX, minX, maxX - 1);
            posY = Mathf.Clamp(posY, minY, maxY - 1);
            
            SpawnPositions.Add(new Vector2Int(posX, posY));
        }

        Debug.Log($"Room {ID}: Generated {SpawnPositions.Count} spawn positions");
    }

    /// <summary>
    /// Returns a random spawn position or Vector2Int.zero if none available
    /// </summary>
    public Vector2Int GetRandomSpawnPosition()
    {
        if (SpawnPositions.Count == 0)
            return Vector2Int.zero;
            
        return SpawnPositions[Random.Range(0, SpawnPositions.Count)];
    }

    /// <summary>
    /// Marks this room as cleared (no more enemies)
    /// </summary>
    public void MarkAsCleared()
    {
        IsCleared = true;
        
        if (State == RoomAccess.Closed)
        {
            State = RoomAccess.Open;
        }
        
        Debug.Log($"Room {ID} ({Type}) cleared!");
    }
}