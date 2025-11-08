using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelModel
{
    public List<RoomModel> Rooms = new();
    public List<CorridorModel> Corridors = new();
    public HashSet<Vector2Int> AllFloorTiles = new();      // For rendering
    public HashSet<Vector2Int> AllWallTiles = new();       // For rendering  
    public HashSet<Vector2Int> AllDoorTiles = new();       // For rendering
    public Dictionary<Vector2Int, WallType> WallTypes = new(); // For rendering
    
    // NEW: Gameplay objects - create ONLY when needed
    public Dictionary<Vector2Int, FloorModel> GameplayFloors = new();   // Only special floors
    public Dictionary<Vector2Int, WallModel> GameplayWalls = new();     // Only interactive walls
    public Dictionary<Vector2Int, DoorModel> GameplayDoors = new();     // All doors as models
    
    // Spatial data (unchanged for performance)
    public BoundsInt OverallBounds { get; private set; }
    private Dictionary<Vector2Int, RoomModel> _tileToRoomMap;
    private Dictionary<Vector2Int, CorridorModel> _tileToCorridorMap;
    private Dictionary<RoomModel, List<RoomModel>> _roomGraph;
    private bool _isInitialized = false;

    public void InitializeSpatialData()
    {
        if (_isInitialized) return;

        CalculateOverallBounds();
        BuildTileMaps();
        BuildRoomGraph();
        InitializeGameplayObjects(); // NEW: Create gameplay models on demand
        _isInitialized = true;
    }

    private void InitializeGameplayObjects()
    {
        // Convert door positions to DoorModels
        foreach (var doorPos in AllDoorTiles)
        {
            GameplayDoors[doorPos] = new DoorModel(doorPos);
        }

        // Note: FloorModel and WallModel are NOT created for every tile by default
        // Only create them when you need gameplay functionality for specific tiles
    }

    // OPTIMIZED: Only create FloorModel for tiles that need special gameplay
    public FloorModel GetOrCreateGameplayFloor(Vector2Int position)
    {
        if (!GameplayFloors.TryGetValue(position, out var floor))
        {
            var room = GetRoomAtPosition(position);
            floor = new FloorModel(position, room?.Type ?? RoomType.Combat);
            GameplayFloors[position] = floor;
        }
        return floor;
    }
    
    // OPTIMIZED: Only create WallModel for walls that need interaction
    public WallModel GetOrCreateGameplayWall(Vector2Int position)
    {
        if (!GameplayWalls.TryGetValue(position, out var wall))
        {
            var wallType = WallTypes.ContainsKey(position) ? WallTypes[position] : WallType.North;
            wall = new WallModel(position, wallType, isDestructible: false);
            GameplayWalls[position] = wall;
        }
        return wall;
    }
    
    private void CalculateOverallBounds()
    {
        if (Rooms.Count == 0) 
        {
            OverallBounds = new BoundsInt(0, 0, 0, 0, 0, 0);
            return;
        }
        
        int minX = Rooms.Min(r => r.Bounds.xMin);
        int maxX = Rooms.Max(r => r.Bounds.xMax);
        int minY = Rooms.Min(r => r.Bounds.yMin);
        int maxY = Rooms.Max(r => r.Bounds.yMax);
        
        OverallBounds = new BoundsInt(minX, minY, 0, maxX - minX, maxY - minY, 1);
    }
    
    private void BuildTileMaps()
    {
        _tileToRoomMap = new Dictionary<Vector2Int, RoomModel>();
        _tileToCorridorMap = new Dictionary<Vector2Int, CorridorModel>();
        
        // Map ALL floor tiles (not just inner tiles) for accurate player detection
        foreach (var room in Rooms)
        {
            foreach (var tile in room.GetFloorTiles()) // Use GetFloorTiles, not GetInnerTiles
            {
                _tileToRoomMap[tile] = room;
            }
        }
        
        foreach (var corridor in Corridors)
        {
            if (corridor?.Tiles != null)
            {
                foreach (var tile in corridor.Tiles)
                {
                    _tileToCorridorMap[tile] = corridor;
                    // Also add corridor tiles to room mapping if not already mapped
                    if (!_tileToRoomMap.ContainsKey(tile))
                    {
                        _tileToRoomMap[tile] = null; // Corridor tiles don't belong to rooms
                    }
                }
            }
        }
    }
    
    private void BuildRoomGraph()
    {
        _roomGraph = new Dictionary<RoomModel, List<RoomModel>>();
        
        foreach (var room in Rooms)
        {
            _roomGraph[room] = new List<RoomModel>();
        }
        
        foreach (var corridor in Corridors)
        {
            if (corridor?.StartRoom != null && corridor?.EndRoom != null && 
                _roomGraph.ContainsKey(corridor.StartRoom) && _roomGraph.ContainsKey(corridor.EndRoom))
            {
                if (!_roomGraph[corridor.StartRoom].Contains(corridor.EndRoom))
                    _roomGraph[corridor.StartRoom].Add(corridor.EndRoom);
                
                if (!_roomGraph[corridor.EndRoom].Contains(corridor.StartRoom))
                    _roomGraph[corridor.EndRoom].Add(corridor.StartRoom);
            }
        }
    }
    
    public RoomModel GetRoomAtPosition(Vector2Int position)
    {
        if (!_isInitialized) InitializeSpatialData();
        _tileToRoomMap.TryGetValue(position, out var room);
        return room;
    }
    
    public Dictionary<RoomModel, List<RoomModel>> RoomGraph 
    { 
        get 
        {
            if (!_isInitialized) InitializeSpatialData();
            return _roomGraph;
        } 
    }
    
    public bool IsFloorTile(Vector2Int position) => AllFloorTiles.Contains(position);
    public bool IsWallTile(Vector2Int position) => AllWallTiles.Contains(position);
    public bool IsDoorTile(Vector2Int position) => AllDoorTiles.Contains(position);
}

[Serializable]
public class FloorSaveData
{
    public int Seed;
    public int FloorLevel;
    public List<RoomSaveData> Rooms;
    public Vector2Int PlayerPosition;
    
    // Move these to a PlayerSaveData file.
    // public int PlayerHealth;
    // public int Gold;
}