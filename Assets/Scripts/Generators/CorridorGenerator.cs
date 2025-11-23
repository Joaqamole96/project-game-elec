// -------------------------------------------------- //
// Scripts/Generators/CorridorGenerator.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CorridorGenerator
{
    private readonly System.Random _random;

    public CorridorGenerator(int seed) => _random = new(seed);

    // ------------------------- //

    public List<CorridorModel> GenerateAllPossibleCorridors(List<PartitionModel> parts)
    {
        List<CorridorModel> allCorrs = new();
        HashSet<(int, int)> roomPairs = new();
        HashSet<Vector2Int> roomFloors = CollectRoomFloors(parts);
        
        foreach (var part in parts)
        {
            if (part.Neighbors == null) continue;
            foreach (var neighbor in part.Neighbors)
            {
                if (neighbor == null) continue;
                if (part.Room == null || neighbor.Room == null) continue;
                
                var roomA = part.Room;
                var roomB = neighbor.Room;
                
                var roomPairKey = (Mathf.Min(roomA.ID, roomB.ID), Mathf.Max(roomA.ID, roomB.ID));
                if (roomPairs.Contains(roomPairKey)) continue;
                
                var corr = CreateCorridor(roomA, roomB, roomFloors);
                if (corr != null)
                {
                    allCorrs.Add(corr);
                    roomPairs.Add(roomPairKey);
                }
            }
        }
        
        Debug.Log($"Generated {allCorrs.Count} possible corridors.");
        return allCorrs;
    }

    private HashSet<Vector2Int> CollectRoomFloors(List<PartitionModel> parts)
    {
        HashSet<Vector2Int> floorTiles = new();
        
        foreach (var part in parts)
            if (part.Room != null)
                foreach (var floorPos in part.Room.GetFloorTiles()) 
                    floorTiles.Add(floorPos);
        
        return floorTiles;
    }

    private CorridorModel CreateCorridor(RoomModel roomA, RoomModel roomB, HashSet<Vector2Int> roomFloors)
    {
        var (doorPosA, doorPosB) = FindAlignedDoorPositions(roomA, roomB);
        
        if (doorPosA == null || doorPosB == null)
        {
            doorPosA = FindClosestDoorPosition(roomA, roomB.Center);
            doorPosB = FindClosestDoorPosition(roomB, roomA.Center);
        }

        // if (doorPosA == null) doorPosA = FindAnyValidDoorPosition(roomA);
        // if (doorPosB == null) doorPosB = FindAnyValidDoorPosition(roomB);

        if (doorPosA == null || doorPosB == null) throw new($"Failed to find door positions between rooms {roomA.ID} and {roomB.ID}.");

        var corrTiles = CreateLShapedCorridor(doorPosA.Value, doorPosB.Value, roomFloors);
        
        if (corrTiles.Count == 0) throw new($"Failed to create corr between rooms {roomA.ID} and {roomB.ID}");

        return new(corrTiles, roomA, roomB, doorPosA.Value, doorPosB.Value);
    }

    private (Vector2Int?, Vector2Int?) FindAlignedDoorPositions(RoomModel roomA, RoomModel roomB)
    {
        if (roomA?.Bounds == null || roomB?.Bounds == null) return (null, null);

        var boundsA = roomA.Bounds;
        var boundsB = roomB.Bounds;

        if (boundsA.yMax <= boundsB.yMin || boundsB.yMax <= boundsA.yMin)
        {
            int overlapStart = Mathf.Max(boundsA.xMin, boundsB.xMin);
            int overlapEnd = Mathf.Min(boundsA.xMax, boundsB.xMax);

            if (overlapStart < overlapEnd - 2)
            {
                int doorX = _random.Next(overlapStart + 1, overlapEnd - 1);
                bool roomAIsAbove = boundsA.yMax <= boundsB.yMin;

                Vector2Int doorPosA = roomAIsAbove ?
                    new Vector2Int(doorX, boundsA.yMax - 1) :
                    new Vector2Int(doorX, boundsA.yMin);

                Vector2Int doorPosB = roomAIsAbove ?
                    new Vector2Int(doorX, boundsB.yMin) :
                    new Vector2Int(doorX, boundsB.yMax - 1);

                return (doorPosA, doorPosB);
            }
        }

        if (boundsA.xMax <= boundsB.xMin || boundsB.xMax <= boundsA.xMin)
        {
            int overlapStart = Mathf.Max(boundsA.yMin, boundsB.yMin);
            int overlapEnd = Mathf.Min(boundsA.yMax, boundsB.yMax);

            if (overlapStart < overlapEnd - 2)
            {
                int doorY = _random.Next(overlapStart + 1, overlapEnd - 1);
                bool roomAIsLeft = boundsA.xMax <= boundsB.xMin;

                Vector2Int doorPosA = roomAIsLeft ?
                    new Vector2Int(boundsA.xMax - 1, doorY) :
                    new Vector2Int(boundsA.xMin, doorY);

                Vector2Int doorPosB = roomAIsLeft ?
                    new Vector2Int(boundsB.xMin, doorY) :
                    new Vector2Int(boundsB.xMax - 1, doorY);

                return (doorPosA, doorPosB);
            }
        }

        return (null, null);
    }

    private Vector2Int? FindClosestDoorPosition(RoomModel room, Vector2Int target)
    {
        if (room?.Bounds == null) return null;

        var candidates = new List<Vector2Int>();
        var bounds = room.Bounds;

        for (int x = bounds.xMin + 1; x < bounds.xMax - 1; x++)
        {
            candidates.Add(new Vector2Int(x, bounds.yMax - 1));
            candidates.Add(new Vector2Int(x, bounds.yMin));
        }

        for (int y = bounds.yMin + 1; y < bounds.yMax - 1; y++)
        {
            candidates.Add(new Vector2Int(bounds.xMax - 1, y));
            candidates.Add(new Vector2Int(bounds.xMin, y));
        }

        return candidates
            .OrderBy(pos => Vector2Int.Distance(pos, target))
            .FirstOrDefault();
    }

    // private Vector2Int? FindAnyValidDoorPosition(RoomModel room)
    // {
    //     if (room?.Bounds == null) return null;

    //     var bounds = room.Bounds;
    //     if (bounds.width >= 3 && bounds.height >= 3)
    //     {
    //         return new Vector2Int(bounds.xMin + 1, bounds.yMin + 1);
    //     }
    //     return null;
    // }

    private List<Vector2Int> CreateLShapedCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> roomFloors)
    {
        var tiles = new List<Vector2Int>();
        
        // Horizontal segment first
        int dx = Mathf.Clamp((end.x - start.x), -1, 1);
        for (int x = start.x; x != end.x; x += dx)
        {
            Vector2Int pos = new(x, start.y);
            if (!roomFloors.Contains(pos))
                tiles.Add(pos);
        }
        
        // Vertical segment
        int dy = Mathf.Clamp((end.y - start.y), -1, 1);
        for (int y = start.y; y != end.y; y += dy)
        {
            var pos = new Vector2Int(end.x, y);
            if (!roomFloors.Contains(pos))
                tiles.Add(pos);
        }
        
        tiles.Add(end);
        
        return tiles;
    }
}