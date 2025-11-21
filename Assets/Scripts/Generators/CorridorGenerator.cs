// -------------------- //
// Scripts/Generators/CorridorGenerator.cs
// -------------------- //

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CorridorGenerator
{
    public List<CorridorModel> GenerateTotalCorridors(List<PartitionModel> partitions, System.Random random)
    {
        var totalCorrs = new List<CorridorModel>();
        var totalFloors = GetFloorTiles(partitions);
        var roomPairs = new HashSet<(int, int)>();
        
        foreach (var part in partitions)
        {
            if (part.Neighbors == null) continue;
            
            foreach (var neighbor in part.Neighbors)
            {
                // NOTE: If part.Neighbors wasn't null then for sure each neighbor in Neighbors isn't null.
                // Keep it just in case, but remove in the future once sure.
                if (neighbor == null) continue;
                if (part.Room == null || neighbor.Room == null) continue;
                
                var roomA = part.Room;
                var roomB = neighbor.Room;
                var roomPairKey = (Mathf.Min(roomA.ID, roomB.ID), Mathf.Max(roomA.ID, roomB.ID));
                
                if (roomPairs.Contains(roomPairKey)) continue;
                
                var corridor = GenerateCorridor(roomA, roomB, totalFloors, random);
                if (corridor != null)
                {
                    totalCorrs.Add(corridor);
                    roomPairs.Add(roomPairKey);
                }
            }
        }
        
        Debug.Log($"CorridorGenerator.GenerateTotalCorridors(): Generated all total corridors.");
        return totalCorrs;
    }

    private HashSet<Vector2Int> GetFloorTiles(List<PartitionModel> partitions)
    {
        var totalFloors = new HashSet<Vector2Int>();

        foreach (var part in partitions)
            if (part.Room != null)
                foreach (var floorPos in GetFloorTile(part.Room))
                    totalFloors.Add(floorPos);
        
        Debug.Log($"CorridorGenerator.GetFloorTiles(): Collected all total floors.");
        return totalFloors;
    }

    private IEnumerable<Vector2Int> GetFloorTile(RoomModel room)
    {
        for (int x = room.Bounds.xMin + 1; x < room.Bounds.xMax - 1; x++)
            for (int y = room.Bounds.yMin + 1; y < room.Bounds.yMax - 1; y++)
                yield return new Vector2Int(x, y);
    }

    private CorridorModel GenerateCorridor(RoomModel roomA, RoomModel roomB, HashSet<Vector2Int> totalFloors, System.Random random)
    {
        if (roomA == null || roomB == null) return null;
        
        var (doorA, doorB) = FindDoorAlignment(roomA, roomB, random);
        if (doorA == null || doorB == null)
        {
            doorA = FindClosestWall(roomA, roomB.Center);
            doorB = FindClosestWall(roomB, roomA.Center);
        }
        
        if (doorA == null) doorA = FindAnyValidWall(roomA);
        if (doorB == null) doorB = FindAnyValidWall(roomB);
        
        if (doorA == null || doorB == null)
        {
            Debug.LogWarning($"Failed to find door positions between rooms {roomA.ID} and {roomB.ID}");
            return null;
        }

        var corrTiles = CreateLShapedCorridor(doorA.Value, doorB.Value, totalFloors);
        if (corrTiles.Count == 0)
        {
            Debug.LogWarning($"Failed to create corridor between rooms {roomA.ID} and {roomB.ID}");
            return null;
        }

        return new CorridorModel(corrTiles, roomA, roomB, new DoorModel(doorA.Value), new DoorModel(doorB.Value));
    }

    private (Vector2Int?, Vector2Int?) FindDoorAlignment(RoomModel roomA, RoomModel roomB, System.Random random)
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

    private Vector2Int? FindClosestWall(RoomModel room, Vector2Int target)
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

    private Vector2Int? FindAnyValidWall(RoomModel room)
    {
        if (room?.Bounds == null) return null;
        
        var bounds = room.Bounds;
        if (bounds.width >= 3 && bounds.height >= 3)
            return new Vector2Int(bounds.xMin + 1, bounds.yMin + 1);
        
        return null;
    }

    private List<Vector2Int> CreateLShapedCorridor(Vector2Int start, Vector2Int end, HashSet<Vector2Int> totalFloors)
    {
        var tiles = new List<Vector2Int>();
        int dx = Mathf.Clamp(end.x - start.x, -1, 1);
        
        for (int x = start.x; x != end.x; x += dx)
        {
            var pos = new Vector2Int(x, start.y);
            if (!totalFloors.Contains(pos))
                tiles.Add(pos);
        }

        int dy = Mathf.Clamp(end.y - start.y, -1, 1);
        for (int y = start.y; y != end.y; y += dy)
        {
            var pos = new Vector2Int(end.x, y);
            if (!totalFloors.Contains(pos))
                tiles.Add(pos);
        }

        tiles.Add(end);
        return tiles;
    }
}