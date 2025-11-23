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
        if (IsCorridorWall(pos, rooms, allFloorTiles)) return WallType.Corridor;

        var roomWallType = DetermineRoomWallType(pos, rooms);
        if (roomWallType != WallType.Interior) return roomWallType;

        return WallType.Interior;
    }

    private static bool IsCorridorWall(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        bool adjacentToCorridor = IsAdjacentToCorridor(pos, allFloorTiles, rooms);
        bool isRoomPerimeter = IsPartOfRoomPerimeter(pos, rooms);
        
        return adjacentToCorridor && !isRoomPerimeter;
    }

    private static bool IsAdjacentToCorridor(Vector2Int pos, HashSet<Vector2Int> allFloorTiles, List<RoomModel> rooms) 
        => GetCardinalNeighbors(pos).Any(neighbor => allFloorTiles.Contains(neighbor) && !IsInAnyRoom(neighbor, rooms));

    private static bool IsPartOfRoomPerimeter(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms.Where(room => room?.Bounds != null))
            if (IsOnRoomEdge(pos, room) && room.ContainsPosition(new Vector2Int(pos.x, pos.y)))
                return true;

        return false;
    }

    private static WallType DetermineRoomWallType(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms.Where(room => room?.Bounds != null))
        {
            if (!IsOnRoomEdge(pos, room) || !room.ContainsPosition(new Vector2Int(pos.x, pos.y))) continue;

            return GetSpecificWallType(pos, room);
        }

        return WallType.Interior;
    }

    private static bool IsOnRoomEdge(Vector2Int pos, RoomModel room)
    {
        var bounds = room.Bounds;
        return pos.y == bounds.yMax - 1 || 
            pos.y == bounds.yMin || 
            pos.x == bounds.xMax - 1 || 
            pos.x == bounds.xMin;
    }

    private static WallType GetSpecificWallType(Vector2Int pos, RoomModel room)
    {
        var bounds = room.Bounds;
        bool isNorth = pos.y == bounds.yMax - 1;
        bool isSouth = pos.y == bounds.yMin;
        bool isEast = pos.x == bounds.xMax - 1;
        bool isWest = pos.x == bounds.xMin;

        // Check corners first
        if (isNorth && isWest) return WallType.NorthWestCorner;
        if (isNorth && isEast) return WallType.NorthEastCorner;
        if (isSouth && isWest) return WallType.SouthWestCorner;
        if (isSouth && isEast) return WallType.SouthEastCorner;
        
        // Then edges
        if (isNorth) return WallType.North;
        if (isSouth) return WallType.South;
        if (isEast) return WallType.East;
        if (isWest) return WallType.West;

        return WallType.Interior;
    }

    private static bool IsInAnyRoom(Vector2Int position, List<RoomModel> rooms) 
        => rooms.Any(room => room?.ContainsPosition(position) == true);

    private static List<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
        => new()
        {
            new(pos.x + 1, pos.y),     // Right
            new(pos.x - 1, pos.y),     // Left
            new(pos.x, pos.y + 1),     // Up
            new(pos.x, pos.y - 1)      // Down
        };
}