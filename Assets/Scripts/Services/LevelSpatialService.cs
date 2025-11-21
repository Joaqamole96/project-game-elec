using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelSpatialService
{
    public void InitializeSpatialData(LevelModel layout)
    {
        if (layout.IsInitialized) return;

        CalculateOverallBounds(layout);
        BuildTileMaps(layout);
        BuildRoomGraph(layout);
        InitializeGameplayObjects(layout);
        
        layout.IsInitialized = true;
        Debug.Log($"Level spatial data initialized: {layout.Rooms.Count} rooms, {layout.Corridors.Count} corridors, {layout.AllFloorTiles.Count} floor tiles");
    }

    private void CalculateOverallBounds(LevelModel layout)
    {
        if (layout.Rooms.Count == 0) 
        {
            layout.OverallBounds = new BoundsInt(0, 0, 0, 0, 0, 0);
            return;
        }
        
        int minX = layout.Rooms.Min(r => r.Bounds.xMin);
        int maxX = layout.Rooms.Max(r => r.Bounds.xMax);
        int minY = layout.Rooms.Min(r => r.Bounds.yMin);
        int maxY = layout.Rooms.Max(r => r.Bounds.yMax);
        
        layout.OverallBounds = new BoundsInt(minX, minY, 0, maxX - minX, maxY - minY, 1);
    }
    
    private void BuildTileMaps(LevelModel layout)
    {
        layout.TileToRoomMap = new Dictionary<Vector2Int, RoomModel>();
        layout.TileToCorridorMap = new Dictionary<Vector2Int, CorridorModel>();
        
        // Build room tile map
        foreach (var room in layout.Rooms)
        {
            var floorTiles = GetRoomFloorTiles(room);
            foreach (var tile in floorTiles)
                layout.TileToRoomMap[tile] = room;
        }

        // Build corridor tile map
        foreach (var corridor in layout.Corridors.Where(c => c?.Tiles != null))
        {
            foreach (var tile in corridor.Tiles)
            {
                layout.TileToCorridorMap[tile] = corridor;
                if (!layout.TileToRoomMap.ContainsKey(tile))
                    layout.TileToRoomMap[tile] = null;
            }
        }
    }

    private IEnumerable<Vector2Int> GetRoomFloorTiles(RoomModel room)
    {
        for (int x = room.Bounds.xMin + 1; x < room.Bounds.xMax - 1; x++)
            for (int y = room.Bounds.yMin + 1; y < room.Bounds.yMax - 1; y++)
                yield return new Vector2Int(x, y);
    }
    
    private void BuildRoomGraph(LevelModel layout)
    {
        layout.RoomGraph = new Dictionary<RoomModel, List<RoomModel>>();
        
        // Initialize graph with all rooms
        foreach (var room in layout.Rooms)
            layout.RoomGraph[room] = new List<RoomModel>();
        
        // Add connections from corridors
        foreach (var corridor in layout.Corridors.Where(c => c?.StartRoom != null && c?.EndRoom != null))
        {
            AddRoomConnection(layout.RoomGraph, corridor.StartRoom, corridor.EndRoom);
            AddRoomConnection(layout.RoomGraph, corridor.EndRoom, corridor.StartRoom);
        }
    }

    private void AddRoomConnection(Dictionary<RoomModel, List<RoomModel>> roomGraph, RoomModel roomA, RoomModel roomB)
    {
        if (roomGraph.ContainsKey(roomA) && !roomGraph[roomA].Contains(roomB))
            roomGraph[roomA].Add(roomB);
    }
    
    private void InitializeGameplayObjects(LevelModel layout)
    {
        foreach (var doorPos in layout.AllDoorTiles)
            layout.GameplayDoors[doorPos] = new DoorModel(doorPos);
    }

    public RoomModel GetRoomAtPosition(LevelModel layout, Vector2Int position)
    {
        if (!layout.IsInitialized) InitializeSpatialData(layout);
        layout.TileToRoomMap.TryGetValue(position, out var room);
        return room;
    }
}