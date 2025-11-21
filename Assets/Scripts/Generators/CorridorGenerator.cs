using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorGenerator
{
    public List<CorridorModel> GenerateAllPossibleCorridors(List<PartitionModel> partitions, System.Random random)
    {
        if (partitions == null || random == null)
        {
            Debug.LogError("CorridorGenerator: Null parameters provided");
            return new List<CorridorModel>();
        }

        var allCorridors = new List<CorridorModel>();
        var roomFloorTiles = CollectRoomFloorTiles(partitions);
        var connectedPairs = new HashSet<(int, int)>();
        
        foreach (var partition in partitions)
        {
            if (partition?.Neighbors == null) continue;
            
            foreach (var neighbor in partition.Neighbors)
            {
                if (neighbor == null) continue;
                if (partition.Room == null || neighbor.Room == null) continue;
                
                var roomA = partition.Room;
                var roomB = neighbor.Room;
                var pairKey = (Mathf.Min(roomA.ID, roomB.ID), Mathf.Max(roomA.ID, roomB.ID));
                
                if (connectedPairs.Contains(pairKey)) continue;
                
                var corridor = CreateCorridorBetweenRooms(roomA, roomB, roomFloorTiles, random);
                if (corridor != null)
                {
                    allCorridors.Add(corridor);
                    connectedPairs.Add(pairKey);
                }
            }
        }
        
        Debug.Log($"Generated {allCorridors.Count} possible corridors");
        return allCorridors;
    }

    private HashSet<Vector2Int> CollectRoomFloorTiles(List<PartitionModel> partitions)
    {
        var floorTiles = new HashSet<Vector2Int>();
        foreach (var partition in partitions)
            if (partition?.Room != null)
                foreach (var floorPos in GetRoomFloorTiles(partition.Room))
                    floorTiles.Add(floorPos);
        return floorTiles;
    }

    private IEnumerable<Vector2Int> GetRoomFloorTiles(RoomModel room)
    {
        for (int x = room.Bounds.xMin + 1; x < room.Bounds.xMax - 1; x++)
            for (int y = room.Bounds.yMin + 1; y < room.Bounds.yMax - 1; y++)
                yield return new Vector2Int(x, y);
    }

    private CorridorModel CreateCorridorBetweenRooms(RoomModel roomA, RoomModel roomB, HashSet<Vector2Int> roomFloorTiles, System.Random random)
    {
        if (roomA == null || roomB == null) return null;
        
        var (doorA, doorB) = FindAlignedDoorPositions(roomA, roomB, random);
        if (doorA == null || doorB == null)
        {
            doorA = FindClosestWallPosition(roomA, roomB.Center);
            doorB = FindClosestWallPosition(roomB, roomA.Center);
        }
        
        if (doorA == null) doorA = FindAnyValidWallPosition(roomA);
        if (doorB == null) doorB = FindAnyValidWallPosition(roomB);
        
        if (doorA == null || doorB == null)
        {
            Debug.LogWarning($"Failed to find door positions between rooms {roomA.ID} and {roomB.ID}");
            return null;
        }

        var corridorTiles = CreateLShapedCorridor(doorA.Value, doorB.Value, roomFloorTiles);
        if (corridorTiles.Count == 0)
        {
            Debug.LogWarning($"Failed to create corridor between rooms {roomA.ID} and {roomB.ID}");
            return null;
        }

        return new CorridorModel(corridorTiles, roomA, roomB, new DoorModel(doorA.Value), new DoorModel(doorB.Value));
    }

    private (Vector2Int?, Vector2Int?) FindAlignedDoorPositions(RoomModel roomA, RoomModel roomB, System.Random random)
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
                int doorX = random.Next(overlapStart + 1, overlapEnd - 1);
                bool roomAIsAbove = boundsA.yMax <= boundsB.yMin;
                Vector2Int doorA = roomAIsAbove ? new(doorX, boundsA.yMax - 1) : new(doorX, boundsA.yMin);
                Vector2Int doorB = roomAIsAbove ? new(doorX, boundsB.yMin) : new(doorX, boundsB.yMax - 1);
                return (doorA, doorB);
            }
        }

        if (boundsA.xMax <= boundsB.xMin || boundsB.xMax <= boundsA.xMin)
        {
            int overlapStart = Mathf.Max(boundsA.yMin, boundsB.yMin);
            int overlapEnd = Mathf.Min(boundsA.yMax, boundsB.yMax);
            if (overlapStart < overlapEnd - 2)
            {
                int doorY = random.Next(overlapStart + 1, overlapEnd - 1);
                bool roomAIsLeft = boundsA.xMax <= boundsB.xMin;
                Vector2Int doorA = roomAIsLeft ? new(boundsA.xMax - 1, doorY) : new(boundsA.xMin, doorY);
                Vector2Int doorB = roomAIsLeft ? new(boundsB.xMin, doorY) : new(boundsB.xMax - 1, doorY);
                return (doorA, doorB);
            }
        }

        return (null, null);
    }

    private Vector2Int? FindClosestWallPosition(RoomModel room, Vector2Int target)
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

        return candidates.OrderBy(pos => Vector2Int.Distance(pos, target)).FirstOrDefault();
    }

    private Vector2Int? FindAnyValidWallPosition(RoomModel room)
    {
        if (room?.Bounds == null) return null;
        
        var bounds = room.Bounds;
        if (bounds.width >= 3 && bounds.height >= 3)
            return new Vector2Int(bounds.xMin + 1, bounds.yMin + 1);
        
        return null;
    }

    private List<Vector2Int> CreateLShapedCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> roomFloorTiles)
    {
        var tiles = new List<Vector2Int>();
        int dx = Mathf.Clamp(end.x - start.x, -1, 1);
        
        for (int x = start.x; x != end.x; x += dx)
        {
            var pos = new Vector2Int(x, start.y);
            if (!roomFloorTiles.Contains(pos))
                tiles.Add(pos);
        }

        int dy = Mathf.Clamp(end.y - start.y, -1, 1);
        for (int y = start.y; y != end.y; y += dy)
        {
            var pos = new Vector2Int(end.x, y);
            if (!roomFloorTiles.Contains(pos))
                tiles.Add(pos);
        }

        tiles.Add(end);
        return tiles;
    }
}