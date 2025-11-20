// GeometryGenerator.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeometryGenerator
{
    public void BuildFinalGeometry(LevelModel layout)
    {
        if (layout == null)
        {
            Debug.LogError("Cannot build geometry - layout is null!");
            return;
        }

        ClearLayoutData(layout);
        BuildFloorGeometry(layout);
        BuildWallGeometry(layout);
        
        Debug.Log($"Geometry built: {layout.Rooms.Count} rooms, {layout.AllFloorTiles.Count} floor tiles");
    }
    
    private void ClearLayoutData(LevelModel layout)
    {
        layout.AllFloorTiles.Clear();
        layout.AllWallTiles.Clear();
        layout.AllDoorTiles.Clear();
        layout.WallTypes.Clear();
    }

    private void BuildFloorGeometry(LevelModel layout)
    {
        var allFloorTiles = new HashSet<Vector2Int>();
        int roomsProcessed = 0;
        int corridorTilesProcessed = 0;

        foreach (var room in layout.Rooms.Where(room => room != null))
        {
            try 
            {
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
        corridorTilesProcessed = AddCorridorFloorsToSet(layout.Corridors, allFloorTiles);
        layout.AllFloorTiles = allFloorTiles;
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
            foreach (var tile in corridor.Tiles)
            {
                floorSet.Add(tile);
                count++;
            }
        return count;
    }
    
    private void BuildWallGeometry(LevelModel layout)
    {
        BuildWallsWithTypes(layout);
    }

    private void BuildWallsWithTypes(LevelModel layout)
    {
        var allWalls = new HashSet<Vector2Int>();
        var wallTypes = new Dictionary<Vector2Int, WallType>();

        var roomWallPerimeters = BuildRoomWalls(layout, allWalls);
        BuildCorridorWalls(layout, allWalls, roomWallPerimeters);
        ProcessDoorsAndWallTypes(layout, allWalls, wallTypes, roomWallPerimeters);

        layout.AllWallTiles = allWalls;
        layout.WallTypes = wallTypes;
    }

    private Dictionary<RoomModel, HashSet<Vector2Int>> BuildRoomWalls(LevelModel layout, HashSet<Vector2Int> allWalls)
    {
        var roomWallPerimeters = new Dictionary<RoomModel, HashSet<Vector2Int>>();
        
        foreach (var room in layout.Rooms.Where(room => room != null))
        {
            var wallPerimeter = new HashSet<Vector2Int>(room.GetWallPerimeter());
            roomWallPerimeters[room] = wallPerimeter;
            allWalls.UnionWith(wallPerimeter);
        }

        return roomWallPerimeters;
    }

    private void BuildCorridorWalls(LevelModel layout, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        foreach (var corridor in layout.Corridors.Where(c => c?.Tiles != null))
            foreach (var corridorTile in corridor.Tiles)
                AddWallsAroundCorridorTile(corridorTile, layout.AllFloorTiles, allWalls, roomWallPerimeters, layout.Rooms);
    }

    private void AddWallsAroundCorridorTile(Vector2Int corridorTile, HashSet<Vector2Int> floorTiles, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters, List<RoomModel> rooms)
    {
        foreach (var neighbor in GetCardinalNeighbors(corridorTile))
            if (!floorTiles.Contains(neighbor) && !allWalls.Contains(neighbor))
            {
                allWalls.Add(neighbor);
                AddToRoomWallPerimeter(neighbor, rooms, roomWallPerimeters);
            }
    }

    private void AddToRoomWallPerimeter(Vector2Int wallPosition, List<RoomModel> rooms, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        var room = GetRoomAtPosition(wallPosition, rooms);
        if (room != null && roomWallPerimeters.ContainsKey(room))
            roomWallPerimeters[room].Add(wallPosition);
    }

    private void ProcessDoorsAndWallTypes(LevelModel layout, HashSet<Vector2Int> allWalls, Dictionary<Vector2Int, WallType> wallTypes, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        RemoveDoorPositionsFromWalls(layout, allWalls, roomWallPerimeters);
        DetermineWallTypes(layout, allWalls, wallTypes);
    }

    private void RemoveDoorPositionsFromWalls(LevelModel layout, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters)
    {
        foreach (var corridor in layout.Corridors.Where(IsValidCorridor))
        {
            RemoveDoorPosition(corridor.StartDoorPosition, corridor.StartRoom, allWalls, roomWallPerimeters, layout);
            RemoveDoorPosition(corridor.EndDoorPosition, corridor.EndRoom, allWalls, roomWallPerimeters, layout);
        }
    }

    private bool IsValidCorridor(CorridorModel corridor)
    {
        return corridor?.StartRoom != null && corridor?.EndRoom != null;
    }

    private void RemoveDoorPosition(Vector2Int doorPosition, RoomModel room, HashSet<Vector2Int> allWalls, Dictionary<RoomModel, HashSet<Vector2Int>> roomWallPerimeters, LevelModel layout)
    {
        if (roomWallPerimeters.ContainsKey(room))
            roomWallPerimeters[room].Remove(doorPosition);
        allWalls.Remove(doorPosition);
        layout.AllDoorTiles.Add(doorPosition);
    }

    private void DetermineWallTypes(LevelModel layout, HashSet<Vector2Int> allWalls, Dictionary<Vector2Int, WallType> wallTypes)
    {
        foreach (var wallPos in allWalls)
            wallTypes[wallPos] = WallTypeCalculator.DetermineWallType(wallPos, layout.Rooms, layout.AllFloorTiles);
    }

    private List<Vector2Int> GetCardinalNeighbors(Vector2Int pos)
    {
        return new List<Vector2Int>
        {
            new(pos.x + 1, pos.y),
            new(pos.x - 1, pos.y),
            new(pos.x, pos.y + 1),
            new(pos.x, pos.y - 1)
        };
    }

    private RoomModel GetRoomAtPosition(Vector2Int position, List<RoomModel> rooms)
    {
        return rooms.FirstOrDefault(room => room.ContainsPosition(position));
    }
}