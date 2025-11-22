// -------------------- //
// Scripts/Generators/LayoutGenerator.cs
// -------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// WARNING!: This class was derived from legacy code. I tried my best to
// resolve the list of problems, but some require more thorough debugging

public class LayoutGenerator
{
    public void BuildFinalGeometry(LevelModel level)
    {
        ClearLayoutData(level);
        BuildFloorGeometry(level);
        BuildWallGeometry(level);
        
        Debug.Log($"Geometry built: {level.Rooms.Count} rooms, {level.AllFloorTiles.Count} floor tiles");
    }
    
    private void ClearLayoutData(LevelModel level)
    {
        level.AllFloorTiles.Clear();
        level.AllWallTiles.Clear();
        level.AllDoorTiles.Clear();
    }

    private void BuildFloorGeometry(LevelModel level)
    {
        var allFloorTiles = new HashSet<Vector2Int>();
        int roomsProcessed = 0;
        int corridorTilesProcessed = 0;

        // Process room floors
        foreach (var room in level.Rooms.Where(room => room != null))
        {
            try 
            {
                // The RoomModel class, and as a whole all Model classes, no longer has any logic.
                var roomFloors = room.GetFloorTiles();
                int roomFloorCount = AddRoomFloorsToSet(roomFloors, allFloorTiles);
                roomsProcessed++;
                
                Debug.Log($"Room {room.ID} ({room.Type}): added {roomFloorCount} floor tiles");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing room {room?.ID}: {ex.Message}");
            }
        }

        // Process corridor floors
        corridorTilesProcessed = AddCorridorFloorsToSet(level.Corridors, allFloorTiles);

        level.AllFloorTiles = allFloorTiles;
    }

    private int AddRoomFloorsToSet(IEnumerable<Vector2Int> roomFloors, HashSet<Vector2Int> floorSet)
    {
        int count = 0;
        foreach (var floorPos in roomFloors)
        {
            floorSet.Add(floorPos);
            count++;
        }
        return count;
    }

    private int AddCorridorFloorsToSet(List<CorridorModel> corridors, HashSet<Vector2Int> floorSet)
    {
        int count = 0;
        foreach (var corridor in corridors.Where(c => c?.Tiles != null))
        {
            foreach (var tile in corridor.Tiles)
            {
                floorSet.Add(tile);
                count++;
            }
        }
        return count;
    }
    
    private void BuildWallGeometry(LevelModel level)
    {
        BuildWallsWithTypes(level);
    }

    private void BuildWallsWithTypes(LevelModel level)
    {
        var allWalls = new HashSet<Vector2Int>();

        // Build walls in stages
        var roomWallPerimeters = BuildRoomWalls(level, allWalls);
        BuildCorridorWalls(level, allWalls, roomWallPerimeters);
        ProcessDoors(level, allWalls, roomWallPerimeters);

        level.AllWallTiles = allWalls;
    }

    private Dictionary<RoomModel, HashSet<Vector2Int>> BuildRoomWalls(LevelModel level, HashSet<Vector2Int> allWalls)
    {
        var roomWallPerimeters = new Dictionary<RoomModel, HashSet<Vector2Int>>();
        
        foreach (var room in level.Rooms.Where(room => room != null))
        {
            // Again, Model classes have no logic.
            var wallPerimeter = new HashSet<Vector2Int>(room.GetWallPerimeter());
            roomWallPerimeters[room] = wallPerimeter;
            allWalls.UnionWith(wallPerimeter);
        }

        return roomWallPerimeters;
    }

    private void BuildCorridorWalls(LevelModel level, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        foreach (var corridor in level.Corridors.Where(c => c?.Tiles != null))
        {
            foreach (var corridorTile in corridor.Tiles)
            {
                AddWallsAroundCorridorTile(corridorTile, level.AllFloorTiles, allWalls, roomWallPerimeters, level.Rooms);
            }
        }
    }

    private void AddWallsAroundCorridorTile(Vector2Int corridorTile, HashSet<Vector2Int> floorTiles, HashSet<Vector2Int> allWalls, 
                                          Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters, List<RoomModel> rooms)
    {
        foreach (var neighbor in GetCardinalNeighbors(corridorTile))
        {
            // Add wall if position is not a floor tile and not already a wall
            if (!floorTiles.Contains(neighbor) && !allWalls.Contains(neighbor))
            {
                allWalls.Add(neighbor);
                AddToRoomWallPerimeter(neighbor, rooms, roomWallPerimeters);
            }
        }
    }

    private void AddToRoomWallPerimeter(Vector2Int wallPosition, List<RoomModel> rooms, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        var room = GetRoomAtPosition(wallPosition, rooms);
        if (room != null && roomWallPerimeters.ContainsKey(room))
        {
            roomWallPerimeters[room].Add(wallPosition);
        }
    }

    private void ProcessDoors(LevelModel level, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        RemoveDoorPositionsFromWalls(level, allWalls, roomWallPerimeters);
    }

    private void RemoveDoorPositionsFromWalls(LevelModel level, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        foreach (var corridor in level.Corridors.Where(IsValidCorridor))
        {
            RemoveDoorPosition(corridor.StartDoor.Position, corridor.StartRoom, allWalls, roomWallPerimeters, level);
            RemoveDoorPosition(corridor.EndDoor.Position, corridor.EndRoom, allWalls, roomWallPerimeters, level);
        }
    }

    private bool IsValidCorridor(CorridorModel corridor)
    {
        return corridor?.StartRoom != null && corridor?.EndRoom != null;
    }

    private void RemoveDoorPosition(Vector2Int doorPosition, RoomModel room, HashSet<Vector2Int> allWalls, 
                                  Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters, LevelModel level)
    {
        if (roomWallPerimeters.ContainsKey(room))
        {
            roomWallPerimeters[room].Remove(doorPosition);
        }
        
        allWalls.Remove(doorPosition);
        level.AllDoorTiles.Add(doorPosition);
    }

    private List<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            new Vector2Int(pos.x + 1, pos.y),     // Right
            new Vector2Int(pos.x - 1, pos.y),     // Left
            new Vector2Int(pos.x, pos.y + 1),     // Up
            new Vector2Int(pos.x, pos.y - 1)      // Down
        };
    }

    private RoomModel GetRoomAtPosition(Vector2Int position, List<RoomModel> rooms)
    {
        return rooms.FirstOrDefault(room => room.ContainsPosition(position));
    }
}