// -------------------- //
// Scripts/Models/LevelModel.cs
// -------------------- //

using System.Collections.Generic;
using UnityEngine;

public class LevelModel
{
    public const int BASE_FLOOR_SIZE = 100;
    public const int MAX_FLOOR_SIZE = 500;
    
    public int Width;
    public int Height;
    public BoundsInt OverallBounds;
    public List<RoomModel> Rooms = new();
    public List<CorridorModel> Corridors = new();
    public HashSet<Vector2Int> AllFloorTiles = new();
    public HashSet<Vector2Int> AllWallTiles = new();
    public HashSet<Vector2Int> AllDoorTiles = new();
    public Dictionary<Vector2Int, DoorModel> GameplayDoors = new();
    public Dictionary<Vector2Int, RoomModel> TileToRoomMap = new();
    public Dictionary<Vector2Int, CorridorModel> TileToCorridorMap = new();
    public Dictionary<RoomModel, List<RoomModel>> RoomGraph = new();
    public bool IsInitialized = false;
    
    // For PropRenderer compatibility
    public List<PropData> AdditionalProps = new();
    
    private LevelSpatialService _spatialService;

    public LevelModel(int width, int height)
    {
        Width = width;
        Height = height;
        _spatialService = new LevelSpatialService();
    }

    /// <summary>
    /// Initializes spatial data structures for efficient queries
    /// </summary>
    public void InitializeSpatialData()
    {
        if (_spatialService == null)
            _spatialService = new LevelSpatialService();
            
        _spatialService.InitializeSpatialData(this);
    }

    /// <summary>
    /// Gets the room at a specific world position
    /// </summary>
    public RoomModel GetRoomAtPosition(Vector2Int position)
    {
        if (!IsInitialized)
        {
            if (_spatialService == null)
                _spatialService = new LevelSpatialService();
            _spatialService.InitializeSpatialData(this);
        }
        
        return _spatialService.GetRoomAtPosition(this, position);
    }

    /// <summary>
    /// Checks if a position contains a floor tile
    /// </summary>
    public bool IsFloorTile(Vector2Int position)
    {
        return AllFloorTiles.Contains(position);
    }

    /// <summary>
    /// Checks if a position contains a wall tile
    /// </summary>
    public bool IsWallTile(Vector2Int position)
    {
        return AllWallTiles.Contains(position);
    }

    /// <summary>
    /// Checks if a position contains a door tile
    /// </summary>
    public bool IsDoorTile(Vector2Int position)
    {
        return AllDoorTiles.Contains(position);
    }
}