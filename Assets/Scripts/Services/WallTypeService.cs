// -------------------------------------------------- //
// Scripts/Services/WallTypeService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class WallTypeService
{
    public static WallType DetermineWallType(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        try
        {
            if (IsCorridorWall(pos, rooms, allFloorTiles)) return WallType.Corridor;
            else return DetermineRoomWallType(pos, rooms);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WallTypeService: Error determining wall type at position {pos}: {ex.Message}");
            return WallType.Interior;
        }
    }

    private static bool IsCorridorWall(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        bool adjacentToCorridor = IsAdjacentToCorridor(pos, allFloorTiles, rooms);
        bool isRoomPerimeter = IsPartOfRoomPerimeter(pos, rooms);

        return adjacentToCorridor && !isRoomPerimeter;
    }

    private static bool IsAdjacentToCorridor(Vector2Int pos, HashSet<Vector2Int> allFloorTiles, List<RoomModel> rooms)
    {
        return GetCardinalNeighbors(pos)
            .Any(neighbor => allFloorTiles.Contains(neighbor) && !IsInAnyRoom(neighbor, rooms));
    }

    private static bool IsPartOfRoomPerimeter(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms)
        {
            if (room?.Bounds == null) 
            {
                Debug.LogWarning("WallTypeService: Encountered room with null Bounds");
                continue;
            }
            if (IsOnRoomEdge(pos, room) && room.ContainsPosition(pos)) return true;
        }
        return false;
    }

    private static WallType DetermineRoomWallType(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms)
        {
            if (room?.Bounds == null)
            {
                Debug.LogWarning("WallTypeService: Encountered room with null Bounds");
                continue;
            }
            
            if (IsOnRoomEdge(pos, room) && room.ContainsPosition(pos)) return GetSpecificWallType(pos, room);
        }
        return WallType.Interior;
    }

    private static bool IsOnRoomEdge(Vector2Int pos, RoomModel room)
    {
        return 
            pos.y == room.Bounds.yMax - 1 || 
            pos.y == room.Bounds.yMin || 
            pos.x == room.Bounds.xMax - 1 || 
            pos.x == room.Bounds.xMin;
    }

    private static WallType GetSpecificWallType(Vector2Int pos, RoomModel room)
    {
        var bounds = room.Bounds;

        bool isNorth = pos.y == bounds.yMax - 1;
        bool isSouth = pos.y == bounds.yMin;
        bool isEast = pos.x == bounds.xMax - 1;
        bool isWest = pos.x == bounds.xMin;
        
        if (isNorth && isWest) return WallType.NorthWestCorner;
        if (isNorth && isEast) return WallType.NorthEastCorner;
        if (isSouth && isWest) return WallType.SouthWestCorner;
        if (isSouth && isEast) return WallType.SouthEastCorner;
        
        if (isNorth) return WallType.North;
        if (isSouth) return WallType.South;
        if (isEast) return WallType.East;
        if (isWest) return WallType.West;
        
        Debug.LogWarning($"WallTypeService: Position {pos} is on room edge but no specific wall type matched");
        return WallType.Interior;
    }

    private static bool IsInAnyRoom(Vector2Int position, List<RoomModel> rooms)
        => rooms.Any(room => room?.ContainsPosition(position) == true);

    private static List<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
    {
        return new()
        {
            new Vector2Int(pos.x + 1, pos.y),
            new Vector2Int(pos.x - 1, pos.y),
            new Vector2Int(pos.x, pos.y + 1),
            new Vector2Int(pos.x, pos.y - 1)
        };
    }
}