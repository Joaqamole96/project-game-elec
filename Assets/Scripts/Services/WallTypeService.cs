// -------------------------------------------------- //
// Scripts/Services/WallTypeService.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for determining wall types based on position and room layout
/// Handles classification of walls as corridor walls, room perimeter walls, or interior walls
/// </summary>
public static class WallTypeService
{
    /// <summary>
    /// Determines the type of wall at the specified position
    /// </summary>
    /// <param name="pos">The grid position to evaluate</param>
    /// <param name="rooms">List of all rooms in the layout</param>
    /// <param name="allFloorTiles">HashSet of all floor tile positions</param>
    /// <returns>The determined WallType for the position</returns>
    public static WallType DetermineWallType(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        try
        {
            // Check if this is a corridor wall first (highest priority)
            if (IsCorridorWall(pos, rooms, allFloorTiles))
            {
                return WallType.Corridor;
            }

            // If not a corridor wall, determine room-specific wall type
            var roomWallType = DetermineRoomWallType(pos, rooms);
            return roomWallType;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"WallTypeService: Error determining wall type at position {pos}: {ex.Message}");
            return WallType.Interior; // Fallback to interior on error
        }
    }

    /// <summary>
    /// Checks if a position qualifies as a corridor wall
    /// </summary>
    /// <param name="pos">Position to check</param>
    /// <param name="rooms">List of all rooms</param>
    /// <param name="allFloorTiles">All floor tile positions</param>
    /// <returns>True if this is a corridor wall</returns>
    private static bool IsCorridorWall(Vector2Int pos, List<RoomModel> rooms, HashSet<Vector2Int> allFloorTiles)
    {
        bool adjacentToCorridor = IsAdjacentToCorridor(pos, allFloorTiles, rooms);
        bool isRoomPerimeter = IsPartOfRoomPerimeter(pos, rooms);
        
        // A wall is considered a corridor wall if it's adjacent to a corridor 
        // but not part of a room's perimeter
        return adjacentToCorridor && !isRoomPerimeter;
    }

    /// <summary>
    /// Checks if position is adjacent to corridor tiles
    /// </summary>
    /// <param name="pos">Position to check</param>
    /// <param name="allFloorTiles">All floor tile positions</param>
    /// <param name="rooms">List of all rooms</param>
    /// <returns>True if adjacent to corridor tiles</returns>
    private static bool IsAdjacentToCorridor(Vector2Int pos, HashSet<Vector2Int> allFloorTiles, List<RoomModel> rooms)
    {
        return GetCardinalNeighbors(pos)
            .Any(neighbor => 
                allFloorTiles.Contains(neighbor) && 
                !IsInAnyRoom(neighbor, rooms));
    }

    /// <summary>
    /// Checks if position is part of any room's perimeter
    /// </summary>
    /// <param name="pos">Position to check</param>
    /// <param name="rooms">List of rooms to check against</param>
    /// <returns>True if position is on a room's edge and inside that room</returns>
    private static bool IsPartOfRoomPerimeter(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms)
        {
            if (room?.Bounds == null) 
            {
                Debug.LogWarning("WallTypeService: Encountered room with null Bounds");
                continue;
            }

            if (IsOnRoomEdge(pos, room) && room.ContainsPosition(pos))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Determines the specific wall type for a position within a room context
    /// </summary>
    /// <param name="pos">Position to evaluate</param>
    /// <param name="rooms">List of rooms to check</param>
    /// <returns>The specific WallType for room walls, or Interior if not a room wall</returns>
    private static WallType DetermineRoomWallType(Vector2Int pos, List<RoomModel> rooms)
    {
        foreach (var room in rooms)
        {
            if (room?.Bounds == null)
            {
                Debug.LogWarning("WallTypeService: Encountered room with null Bounds");
                continue;
            }

            // Check if this position is on the edge of the room and inside the room
            if (IsOnRoomEdge(pos, room) && room.ContainsPosition(pos))
            {
                return GetSpecificWallType(pos, room);
            }
        }
        return WallType.Interior;
    }

    /// <summary>
    /// Checks if a position is on the edge of a room's bounds
    /// </summary>
    /// <param name="pos">Position to check</param>
    /// <param name="room">Room to check against</param>
    /// <returns>True if position is on room edge</returns>
    private static bool IsOnRoomEdge(Vector2Int pos, RoomModel room)
    {
        var bounds = room.Bounds;
        return pos.y == bounds.yMax - 1 || 
               pos.y == bounds.yMin || 
               pos.x == bounds.xMax - 1 || 
               pos.x == bounds.xMin;
    }

    /// <summary>
    /// Determines the specific wall type (corner or directional) for a room edge position
    /// </summary>
    /// <param name="pos">Position on room edge</param>
    /// <param name="room">The room containing the position</param>
    /// <returns>Specific WallType for the room edge position</returns>
    private static WallType GetSpecificWallType(Vector2Int pos, RoomModel room)
    {
        var bounds = room.Bounds;
        bool isNorth = pos.y == bounds.yMax - 1;
        bool isSouth = pos.y == bounds.yMin;
        bool isEast = pos.x == bounds.xMax - 1;
        bool isWest = pos.x == bounds.xMin;

        // Check corners first (higher priority than edges)
        if (isNorth && isWest) return WallType.NorthWestCorner;
        if (isNorth && isEast) return WallType.NorthEastCorner;
        if (isSouth && isWest) return WallType.SouthWestCorner;
        if (isSouth && isEast) return WallType.SouthEastCorner;
        
        // Check directional walls
        if (isNorth) return WallType.North;
        if (isSouth) return WallType.South;
        if (isEast) return WallType.East;
        if (isWest) return WallType.West;

        // This shouldn't normally happen for positions on room edges
        Debug.LogWarning($"WallTypeService: Position {pos} is on room edge but no specific wall type matched");
        return WallType.Interior;
    }

    /// <summary>
    /// Checks if a position is inside any room
    /// </summary>
    /// <param name="position">Position to check</param>
    /// <param name="rooms">List of rooms to check</param>
    /// <returns>True if position is inside any room</returns>
    private static bool IsInAnyRoom(Vector2Int position, List<RoomModel> rooms)
    {
        return rooms.Any(room => room?.ContainsPosition(position) == true);
    }

    /// <summary>
    /// Gets the four cardinal neighbors of a position
    /// </summary>
    /// <param name="pos">Center position</param>
    /// <returns>List of four cardinal neighbor positions</returns>
    private static List<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),     // Right/East
            new Vector2Int(pos.x - 1, pos.y),     // Left/West  
            new Vector2Int(pos.x, pos.y + 1),     // Up/North
            new Vector2Int(pos.x, pos.y - 1)      // Down/South
        };
    }
}