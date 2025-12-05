// -------------------------------------------------- //
// Scripts/Models/LevelModel.cs
// -------------------------------------------------- //

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelModel
{
    public List<RoomModel> Rooms { get; set; } = new List<RoomModel>();
    public List<CorridorModel> Corridors { get; set; } = new List<CorridorModel>();
    public HashSet<Vector2Int> AllFloorTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> AllWallTiles { get; set; } = new HashSet<Vector2Int>();
    public HashSet<Vector2Int> AllDoorTiles { get; set; } = new HashSet<Vector2Int>();
    public Dictionary<Vector2Int, WallType> WallTypes { get; set; } = new Dictionary<Vector2Int, WallType>();
    public Dictionary<Vector2Int, DoorModel> GameplayDoors { get; private set; } = new Dictionary<Vector2Int, DoorModel>();
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
        foreach (var doorPos in AllDoorTiles) GameplayDoors[doorPos] = new(doorPos);
        
        _isInitialized = true;
        
        Debug.Log($"Level spatial data initialized: {Rooms.Count} rooms, {Corridors.Count} corridors, {AllFloorTiles.Count} floor tiles");
    }

    private void CalculateOverallBounds()
    {
        if (Rooms.Count == 0) 
        {
            OverallBounds = new(0, 0, 0, 0, 0, 0);
            return;
        }
        
        int minX = Rooms.Min(r => r.Bounds.xMin);
        int maxX = Rooms.Max(r => r.Bounds.xMax);
        int minY = Rooms.Min(r => r.Bounds.yMin);
        int maxY = Rooms.Max(r => r.Bounds.yMax);
        
        OverallBounds = new(minX, minY, 0, maxX - minX, maxY - minY, 1);
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
            foreach (var tile in room.GetFloorTiles()) _tileToRoomMap[tile] = room;
    }

    private void BuildCorridorTileMap()
    {
        foreach (var corridor in Corridors.Where(c => c?.Tiles != null))
            foreach (var tile in corridor.Tiles)
            {
                _tileToCorridorMap[tile] = corridor;
                if (!_tileToRoomMap.ContainsKey(tile)) _tileToRoomMap[tile] = null;
            }
    }
    
    private void BuildRoomGraph()
    {
        _roomGraph = new Dictionary<RoomModel, List<RoomModel>>();
        
        foreach (var room in Rooms) _roomGraph[room] = new List<RoomModel>();
        
        foreach (var corridor in Corridors.Where(corridor => corridor.StartRoom != null && corridor.EndRoom != null))
        {
            AddRoomConnection(corridor.StartRoom, corridor.EndRoom);
            AddRoomConnection(corridor.EndRoom, corridor.StartRoom);
        }
    }

    private void AddRoomConnection(RoomModel roomA, RoomModel roomB)
    {
        if (_roomGraph.ContainsKey(roomA) && !_roomGraph[roomA].Contains(roomB)) _roomGraph[roomA].Add(roomB);
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
}