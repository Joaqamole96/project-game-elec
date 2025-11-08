using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GeometryBuilder
{
    public void BuildFinalGeometry(LevelModel layout)
    {
        if (layout == null)
        {
            Debug.LogError("Cannot build geometry - layout is null!");
            return;
        }

        layout.AllFloorTiles.Clear();
        layout.AllWallTiles.Clear();
        layout.AllDoorTiles.Clear();
        layout.WallTypes.Clear();

        var allFloorTiles = new HashSet<Vector2Int>();
        int roomsProcessed = 0;
        int corridorTilesProcessed = 0;

        // Collect all floor tiles (rooms + corridors) with better error handling
        foreach (var room in layout.Rooms)
        {
            if (room != null)
            {
                try 
                {
                    var roomFloors = room.GetFloorTiles();
                    int roomFloorCount = 0;
                    
                    foreach (var floorPos in roomFloors)
                    {
                        allFloorTiles.Add(floorPos);
                        roomFloorCount++;
                    }
                    
                    roomsProcessed++;
                    Debug.Log($"Room {room.ID} ({room.Type}): added {roomFloorCount} floor tiles");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error processing room {room?.ID}: {ex.Message}");
                }
            }
        }

        foreach (var corridor in layout.Corridors)
        {
            if (corridor?.Tiles != null)
            {
                foreach (var tile in corridor.Tiles)
                {
                    allFloorTiles.Add(tile);
                    corridorTilesProcessed++;
                }
            }
        }

        layout.AllFloorTiles = allFloorTiles;
        
        Debug.Log($"Geometry built: {roomsProcessed} rooms, {corridorTilesProcessed} corridor tiles, {allFloorTiles.Count} total floor tiles");
        
        BuildWallsWithTypes(layout);
    }
    
    private void BuildWallsWithTypes(LevelModel layout)
    {
        var allWalls = new HashSet<Vector2Int>();
        var wallTypes = new Dictionary<Vector2Int, WallType>();

        // Step 1: Build walls around rooms (existing logic)
        var roomWallPerimeters = BuildRoomWalls(layout, allWalls);
        
        // Step 2: Build walls around corridors (NEW)
        BuildCorridorWalls(layout, allWalls, roomWallPerimeters);
        
        // Step 3: Remove door positions and determine wall types
        ProcessDoorsAndWallTypes(layout, allWalls, wallTypes, roomWallPerimeters);

        layout.AllWallTiles = allWalls;
        layout.WallTypes = wallTypes;
    }

    private Dictionary<RoomModel, HashSet<Vector2Int>> BuildRoomWalls(LevelModel layout, HashSet<Vector2Int> allWalls)
    {
        var roomWallPerimeters = new Dictionary<RoomModel, HashSet<Vector2Int>>();
        
        foreach (var room in layout.Rooms)
        {
            if (room != null)
            {
                var wallPerimeter = new HashSet<Vector2Int>(room.GetWallPerimeter());
                roomWallPerimeters[room] = wallPerimeter;
                allWalls.UnionWith(wallPerimeter);
            }
        }

        return roomWallPerimeters;
    }

    private void BuildCorridorWalls(LevelModel layout, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        foreach (var corridor in layout.Corridors)
        {
            if (corridor?.Tiles == null) continue;

            // For each corridor tile, add walls to its sides if they're not floor tiles
            foreach (var corridorTile in corridor.Tiles)
            {
                var neighbors = GetNeighbors(corridorTile);
                foreach (var neighbor in neighbors)
                {
                    // If this neighbor position is not a floor tile and not already a wall, add it as a wall
                    if (!layout.AllFloorTiles.Contains(neighbor) && !allWalls.Contains(neighbor))
                    {
                        allWalls.Add(neighbor);
                        
                        // Check if this wall belongs to a room and add it to room wall perimeter
                        var room = GetRoomAtPosition(neighbor, layout.Rooms);
                        if (room != null && roomWallPerimeters.ContainsKey(room))
                        {
                            roomWallPerimeters[room].Add(neighbor);
                        }
                    }
                }
            }
        }
    }

    private void ProcessDoorsAndWallTypes(LevelModel layout, HashSet<Vector2Int> allWalls, Dictionary<Vector2Int, WallType> wallTypes, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        // Remove door positions from walls
        foreach (var corridor in layout.Corridors)
        {
            if (corridor?.StartRoom != null && corridor?.EndRoom != null && 
                roomWallPerimeters.ContainsKey(corridor.StartRoom) && 
                roomWallPerimeters.ContainsKey(corridor.EndRoom))
            {
                roomWallPerimeters[corridor.StartRoom].Remove(corridor.StartDoorPosition);
                roomWallPerimeters[corridor.EndRoom].Remove(corridor.EndDoorPosition);
                
                allWalls.Remove(corridor.StartDoorPosition);
                allWalls.Remove(corridor.EndDoorPosition);
                
                layout.AllDoorTiles.Add(corridor.StartDoorPosition);
                layout.AllDoorTiles.Add(corridor.EndDoorPosition);
            }
        }

        // Determine wall types for all remaining walls
        foreach (var wallPos in allWalls)
        {
            wallTypes[wallPos] = WallTypeCalculator.DetermineWallType(wallPos, layout.Rooms, layout.AllFloorTiles);
        }
    }

    private List<Vector2Int> GetNeighbors(Vector2Int pos)
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