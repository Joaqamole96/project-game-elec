using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WallTypeCalculator
{
    public static WallType DetermineWallType(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        // First, check if this is a corridor wall (not adjacent to any room)
        if (IsCorridorWall(pos, rooms, allFloorTiles))
        {
            return WallType.Corridor;
        }

        // Then check if this is a corner or edge wall for rooms
        foreach (var room in rooms)
        {
            if (room?.Bounds == null) continue;

            var bounds = room.Bounds;
            bool isNorth = pos.y == bounds.yMax - 1;
            bool isSouth = pos.y == bounds.yMin;
            bool isEast = pos.x == bounds.xMax - 1;
            bool isWest = pos.x == bounds.xMin;

            // Check if this position is actually on the room's perimeter
            bool isRoomPerimeter = (isNorth || isSouth || isEast || isWest) && 
                                  room.ContainsPosition(new Vector2Int(pos.x, pos.y));

            if (isRoomPerimeter)
            {
                if (isNorth && isWest) return WallType.NorthWestCorner;
                if (isNorth && isEast) return WallType.NorthEastCorner;
                if (isSouth && isWest) return WallType.SouthWestCorner;
                if (isSouth && isEast) return WallType.SouthEastCorner;
                
                if (isNorth) return WallType.North;
                if (isSouth) return WallType.South;
                if (isEast) return WallType.East;
                if (isWest) return WallType.West;
            }
        }

        // If it's not a corridor wall or room wall, it's interior
        return WallType.Interior;
    }

    private static bool IsCorridorWall(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        // A wall is a corridor wall if:
        // 1. It's adjacent to a corridor floor tile
        // 2. It's NOT part of any room's perimeter
        
        // Check if this position is adjacent to any corridor floor tile
        var neighbors = GetNeighbors(pos);
        bool adjacentToCorridor = neighbors.Any(neighbor => 
            allFloorTiles.Contains(neighbor) && !IsInAnyRoom(neighbor, rooms));

        // Check if this position is part of any room's perimeter
        bool isRoomPerimeter = false;
        foreach (var room in rooms)
        {
            if (room?.Bounds == null) continue;
            
            var bounds = room.Bounds;
            bool isNorth = pos.y == bounds.yMax - 1;
            bool isSouth = pos.y == bounds.yMin;
            bool isEast = pos.x == bounds.xMax - 1;
            bool isWest = pos.x == bounds.xMin;
            
            bool onRoomEdge = (isNorth || isSouth || isEast || isWest);
            bool inRoomBounds = room.ContainsPosition(new Vector2Int(pos.x, pos.y));
            
            if (onRoomEdge && inRoomBounds)
            {
                isRoomPerimeter = true;
                break;
            }
        }

        return adjacentToCorridor && !isRoomPerimeter;
    }

    private static bool IsInAnyRoom(Vector2Int position, List<RoomModel> rooms)
    {
        return rooms.Any(room => room?.ContainsPosition(position) == true);
    }

    private static List<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),     // Right
            new Vector2Int(pos.x - 1, pos.y),     // Left
            new Vector2Int(pos.x, pos.y + 1),     // Up
            new Vector2Int(pos.x, pos.y - 1)      // Down
        };
    }
}