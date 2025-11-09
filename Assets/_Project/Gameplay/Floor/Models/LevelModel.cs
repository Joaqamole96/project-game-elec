// LevelModel.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the complete dungeon level with rooms, corridors, and spatial data.
/// Optimized for both rendering and gameplay access patterns.
/// </summary>
public class LevelModel
{
    /// <summary>All rooms in the level.</summary>
    public List<RoomModel> Rooms { get; set; } = new List<RoomModel>();
    
    /// <summary>All corridors connecting rooms.</summary>
    public List<CorridorModel> Corridors { get; set; } = new List<CorridorModel>();
    
    // Rendering data
    /// <summary>All floor tile positions for rendering.</summary>
    public HashSet<Vector2Int> AllFloorTiles { get; set; } = new HashSet<Vector2Int>();
    
    /// <summary>All wall tile positions for rendering.</summary>
    public HashSet<Vector2Int> AllWallTiles { get; set; } = new HashSet<Vector2Int>();
    
    /// <summary>All door tile positions for rendering.</summary>
    public HashSet<Vector2Int> AllDoorTiles { get; set; } = new HashSet<Vector2Int>();
    
    /// <summary>Wall types for each wall position.</summary>
    public Dictionary<Vector2Int, WallType> WallTypes { get; set; } = new Dictionary<Vector2Int, WallType>();
    
    // Gameplay objects (created on demand)
    /// <summary>Gameplay floor models for special tiles only.</summary>
    public Dictionary<Vector2Int, FloorModel> GameplayFloors { get; private set; } = new Dictionary<Vector2Int, FloorModel>();
    
    /// <summary>Gameplay wall models for interactive walls only.</summary>
    public Dictionary<Vector2Int, WallModel> GameplayWalls { get; private set; } = new Dictionary<Vector2Int, WallModel>();
    
    /// <summary>All door models for gameplay interactions.</summary>
    public Dictionary<Vector2Int, DoorModel> GameplayDoors { get; private set; } = new Dictionary<Vector2Int, DoorModel>();
    
    // Spatial data for efficient queries
    /// <summary>Overall bounds of the entire level.</summary>
    public BoundsInt OverallBounds { get; private set; }
    
    private Dictionary<Vector2Int, RoomModel> _tileToRoomMap;
    private Dictionary<Vector2Int, CorridorModel> _tileToCorridorMap;
    private Dictionary<RoomModel, List<RoomModel>> _roomGraph;
    private bool _isInitialized = false;

    /// <summary>
    /// Initializes spatial data structures for efficient querying.
    /// </summary>
    public void InitializeSpatialData()
    {
        if (_isInitialized) return;

        CalculateOverallBounds();
        BuildTileMaps();
        BuildRoomGraph();
        InitializeGameplayObjects();
        
        _isInitialized = true;
        
        Debug.Log($"Level spatial data initialized: {Rooms.Count} rooms, {Corridors.Count} corridors, {AllFloorTiles.Count} floor tiles");
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
        
        BuildRoomTileMap();
        BuildCorridorTileMap();
    }

    private void BuildRoomTileMap()
    {
        foreach (var room in Rooms)
        {
            foreach (var tile in room.GetFloorTiles())
            {
                _tileToRoomMap[tile] = room;
            }
        }
    }

    private void BuildCorridorTileMap()
    {
        foreach (var corridor in Corridors.Where(c => c?.Tiles != null))
        {
            foreach (var tile in corridor.Tiles)
            {
                _tileToCorridorMap[tile] = corridor;
                // Mark corridor tiles as not belonging to any room
                if (!_tileToRoomMap.ContainsKey(tile))
                {
                    _tileToRoomMap[tile] = null;
                }
            }
        }
    }
    
    private void BuildRoomGraph()
    {
        _roomGraph = new Dictionary<RoomModel, List<RoomModel>>();
        
        // Initialize graph with all rooms
        foreach (var room in Rooms)
        {
            _roomGraph[room] = new List<RoomModel>();
        }
        
        // Add connections from corridors
        foreach (var corridor in Corridors.Where(IsValidCorridor))
        {
            AddRoomConnection(corridor.StartRoom, corridor.EndRoom);
            AddRoomConnection(corridor.EndRoom, corridor.StartRoom);
        }
    }

    private bool IsValidCorridor(CorridorModel corridor)
    {
        return corridor?.StartRoom != null && corridor?.EndRoom != null;
    }

    private void AddRoomConnection(RoomModel roomA, RoomModel roomB)
    {
        if (_roomGraph.ContainsKey(roomA) && !_roomGraph[roomA].Contains(roomB))
        {
            _roomGraph[roomA].Add(roomB);
        }
    }
    
    private void InitializeGameplayObjects()
    {
        InitializeDoors();
        // FloorModel and WallModel are created on demand to save memory
    }

    private void InitializeDoors()
    {
        foreach (var doorPos in AllDoorTiles)
        {
            GameplayDoors[doorPos] = new DoorModel(doorPos);
        }
    }

    // Public API for gameplay systems
    
    /// <summary>
    /// Gets the room at the specified position, or null if not in a room.
    /// </summary>
    public RoomModel GetRoomAtPosition(Vector2Int position)
    {
        if (!_isInitialized) InitializeSpatialData();
        _tileToRoomMap.TryGetValue(position, out var room);
        return room;
    }
    
    /// <summary>
    /// Gets the corridor at the specified position, or null if not in a corridor.
    /// </summary>
    public CorridorModel GetCorridorAtPosition(Vector2Int position)
    {
        if (!_isInitialized) InitializeSpatialData();
        _tileToCorridorMap.TryGetValue(position, out var corridor);
        return corridor;
    }

    /// <summary>
    /// Gets the room graph for pathfinding and AI.
    /// </summary>
    public Dictionary<RoomModel, List<RoomModel>> RoomGraph 
    { 
        get 
        {
            if (!_isInitialized) InitializeSpatialData();
            return _roomGraph;
        } 
    }

    // Tile type queries
    
    /// <summary>
    /// Checks if the position is a walkable floor tile.
    /// </summary>
    public bool IsFloorTile(Vector2Int position) => AllFloorTiles.Contains(position);
    
    /// <summary>
    /// Checks if the position is a wall tile.
    /// </summary>
    public bool IsWallTile(Vector2Int position) => AllWallTiles.Contains(position);
    
    /// <summary>
    /// Checks if the position is a door tile.
    /// </summary>
    public bool IsDoorTile(Vector2Int position) => AllDoorTiles.Contains(position);
    
    /// <summary>
    /// Checks if the position is traversable by the player.
    /// </summary>
    public bool IsTraversable(Vector2Int position) => IsFloorTile(position) || IsDoorTile(position);

    // Gameplay object management (lazy initialization)
    
    /// <summary>
    /// Gets or creates a FloorModel for tiles that need special gameplay.
    /// </summary>
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
    
    /// <summary>
    /// Gets or creates a WallModel for walls that need interaction.
    /// </summary>
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
    
    /// <summary>
    /// Gets the door model at the specified position.
    /// </summary>
    public DoorModel GetDoorAtPosition(Vector2Int position)
    {
        GameplayDoors.TryGetValue(position, out var door);
        return door;
    }

    /// <summary>
    /// Gets all rooms of the specified type.
    /// </summary>
    public List<RoomModel> GetRoomsOfType(RoomType roomType)
    {
        return Rooms.Where(room => room.Type == roomType).ToList();
    }

    /// <summary>
    /// Gets the entrance room for player spawning.
    /// </summary>
    public RoomModel GetEntranceRoom()
    {
        return Rooms.FirstOrDefault(room => room.Type == RoomType.Entrance);
    }

    /// <summary>
    /// Gets the exit room for level progression.
    /// </summary>
    public RoomModel GetExitRoom()
    {
        return Rooms.FirstOrDefault(room => room.Type == RoomType.Exit);
    }
}

/// <summary>
/// Serialized data for saving/loading floor state.
/// </summary>
[Serializable]
public class FloorSaveData
{
    public int Seed;
    public int FloorLevel;
    public List<RoomSaveData> Rooms;
    public Vector2Int PlayerPosition;
    
    // Player data would be in a separate PlayerSaveData class
    // public int PlayerHealth;
    // public int Gold;
}