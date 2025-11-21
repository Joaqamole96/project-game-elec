using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LayoutGenerator
{
    public LevelModel GenerateLayoutGeometry(List<RoomModel> rooms, List<CorridorModel> corridors)
    {
        var levelModel = new LevelModel();
        
        // Combine room and corridor bounds to get overall level bounds
        var allBounds = new List<RectInt>();
        allBounds.AddRange(rooms.Select(r => r.Bounds));
        allBounds.AddRange(corridors.Select(c => c.Bounds));
        levelModel.OverallBounds = CalculateOverallBounds(allBounds);
        
        // Generate floor tiles (rooms + corridors)
        levelModel.AllFloorTiles = GenerateFloorTiles(rooms, corridors);
        
        // Generate wall tiles (around rooms and corridors)
        levelModel.AllWallTiles = GenerateWallTiles(levelModel.AllFloorTiles, levelModel.OverallBounds);
        
        // Generate door tiles (connections between rooms and corridors)
        levelModel.AllDoorTiles = GenerateDoorTiles(rooms, corridors);
        
        // Remove wall types since they're not needed
        // levelModel.WallTypes = new Dictionary<Vector2Int, WallType>(); // Commented out
        
        Debug.Log($"Layout generated: {levelModel.AllFloorTiles.Count} floors, {levelModel.AllWallTiles.Count} walls, {levelModel.AllDoorTiles.Count} doors");
        return levelModel;
    }

    private RectInt CalculateOverallBounds(List<RectInt> allBounds)
    {
        if (allBounds == null || allBounds.Count == 0)
            return new RectInt(0, 0, 1, 1);

        int minX = allBounds.Min(b => b.xMin);
        int maxX = allBounds.Max(b => b.xMax);
        int minY = allBounds.Min(b => b.yMin);
        int maxY = allBounds.Max(b => b.yMax);

        return new RectInt(minX, minY, maxX - minX, maxY - minY);
    }

    private HashSet<Vector2Int> GenerateFloorTiles(List<RoomModel> rooms, List<CorridorModel> corridors)
    {
        var floorTiles = new HashSet<Vector2Int>();

        // Add room floors
        foreach (var room in rooms)
        {
            if (room?.Bounds == null) continue;
            
            for (int x = room.Bounds.xMin; x < room.Bounds.xMax; x++)
            {
                for (int y = room.Bounds.yMin; y < room.Bounds.yMax; y++)
                {
                    floorTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        // Add corridor floors
        foreach (var corridor in corridors)
        {
            if (corridor?.Bounds == null) continue;
            
            for (int x = corridor.Bounds.xMin; x < corridor.Bounds.xMax; x++)
            {
                for (int y = corridor.Bounds.yMin; y < corridor.Bounds.yMax; y++)
                {
                    floorTiles.Add(new Vector2Int(x, y));
                }
            }
        }

        return floorTiles;
    }

    private HashSet<Vector2Int> GenerateWallTiles(HashSet<Vector2Int> floorTiles, RectInt overallBounds)
    {
        var wallTiles = new HashSet<Vector2Int>();
        
        // Expand search area to include buffer around level
        int searchMinX = overallBounds.xMin - 2;
        int searchMaxX = overallBounds.xMax + 2;
        int searchMinY = overallBounds.yMin - 2;
        int searchMaxY = overallBounds.yMax + 2;

        for (int x = searchMinX; x <= searchMaxX; x++)
        {
            for (int y = searchMinY; y <= searchMaxY; y++)
            {
                var pos = new Vector2Int(x, y);
                
                // If this position is not a floor, check if it's adjacent to any floor
                if (!floorTiles.Contains(pos))
                {
                    if (IsAdjacentToFloor(pos, floorTiles))
                    {
                        wallTiles.Add(pos);
                    }
                }
            }
        }

        return wallTiles;
    }

    private bool IsAdjacentToFloor(Vector2Int position, HashSet<Vector2Int> floorTiles)
    {
        // Check all 8 directions (including diagonals for corner walls)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue; // Skip the position itself
                
                var checkPos = new Vector2Int(position.x + dx, position.y + dy);
                if (floorTiles.Contains(checkPos))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private HashSet<Vector2Int> GenerateDoorTiles(List<RoomModel> rooms, List<CorridorModel> corridors)
    {
        var doorTiles = new HashSet<Vector2Int>();

        // Find connections between rooms and corridors
        foreach (var room in rooms)
        {
            if (room?.Bounds == null) continue;

            foreach (var corridor in corridors)
            {
                if (corridor?.Bounds == null) continue;

                var connectionPoints = FindRoomCorridorConnection(room.Bounds, corridor.Bounds);
                foreach (var point in connectionPoints)
                {
                    doorTiles.Add(point);
                }
            }
        }

        // Find connections between adjacent rooms (if any direct room-room connections)
        for (int i = 0; i < rooms.Count; i++)
        {
            for (int j = i + 1; j < rooms.Count; j++)
            {
                var roomA = rooms[i];
                var roomB = rooms[j];
                
                if (roomA?.Bounds == null || roomB?.Bounds == null) continue;

                var connectionPoints = FindAdjacentRoomConnection(roomA.Bounds, roomB.Bounds);
                foreach (var point in connectionPoints)
                {
                    doorTiles.Add(point);
                }
            }
        }

        return doorTiles;
    }

    private List<Vector2Int> FindRoomCorridorConnection(RectInt roomBounds, RectInt corridorBounds)
    {
        var connections = new List<Vector2Int>();

        // Check if room and corridor are adjacent
        if (roomBounds.xMax == corridorBounds.xMin - 1 || roomBounds.xMin == corridorBounds.xMax + 1)
        {
            // Vertical alignment (east/west connection)
            int yOverlapMin = Mathf.Max(roomBounds.yMin, corridorBounds.yMin);
            int yOverlapMax = Mathf.Min(roomBounds.yMax, corridorBounds.yMax);
            
            if (yOverlapMax > yOverlapMin)
            {
                int connectionY = (yOverlapMin + yOverlapMax) / 2; // Middle of overlap
                int doorX = roomBounds.xMax == corridorBounds.xMin - 1 ? roomBounds.xMax : roomBounds.xMin - 1;
                connections.Add(new Vector2Int(doorX, connectionY));
            }
        }
        else if (roomBounds.yMax == corridorBounds.yMin - 1 || roomBounds.yMin == corridorBounds.yMax + 1)
        {
            // Horizontal alignment (north/south connection)
            int xOverlapMin = Mathf.Max(roomBounds.xMin, corridorBounds.xMin);
            int xOverlapMax = Mathf.Min(roomBounds.xMax, corridorBounds.xMax);
            
            if (xOverlapMax > xOverlapMin)
            {
                int connectionX = (xOverlapMin + xOverlapMax) / 2; // Middle of overlap
                int doorY = roomBounds.yMax == corridorBounds.yMin - 1 ? roomBounds.yMax : roomBounds.yMin - 1;
                connections.Add(new Vector2Int(connectionX, doorY));
            }
        }

        return connections;
    }

    private List<Vector2Int> FindAdjacentRoomConnection(RectInt roomABounds, RectInt roomBBounds)
    {
        var connections = new List<Vector2Int>();

        // Check for direct room-to-room adjacency (less common with corridors)
        if (roomABounds.xMax == roomBBounds.xMin - 1 || roomABounds.xMin == roomBBounds.xMax + 1)
        {
            // Vertical alignment
            int yOverlapMin = Mathf.Max(roomABounds.yMin, roomBBounds.yMin);
            int yOverlapMax = Mathf.Min(roomABounds.yMax, roomBBounds.yMax);
            
            if (yOverlapMax > yOverlapMin)
            {
                int connectionY = (yOverlapMin + yOverlapMax) / 2;
                int doorX = roomABounds.xMax == roomBBounds.xMin - 1 ? roomABounds.xMax : roomABounds.xMin - 1;
                connections.Add(new Vector2Int(doorX, connectionY));
            }
        }
        else if (roomABounds.yMax == roomBBounds.yMin - 1 || roomABounds.yMin == roomBBounds.yMax + 1)
        {
            // Horizontal alignment
            int xOverlapMin = Mathf.Max(roomABounds.xMin, roomBBounds.xMin);
            int xOverlapMax = Mathf.Min(roomABounds.xMax, roomBBounds.xMax);
            
            if (xOverlapMax > xOverlapMin)
            {
                int connectionX = (xOverlapMin + xOverlapMax) / 2;
                int doorY = roomABounds.yMax == roomBBounds.yMin - 1 ? roomABounds.yMax : roomABounds.yMin - 1;
                connections.Add(new Vector2Int(connectionX, doorY));
            }
        }

        return connections;
    }

    // Helper method to get room at position (used by other systems)
    // public RoomModel GetRoomAtPosition(Vector2Int position, List<RoomModel> rooms)
    // {
    //     return rooms?.FirstOrDefault(room => room?.Bounds.Contains(position) == true);
    // }
}