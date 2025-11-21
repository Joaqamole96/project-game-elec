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

    public LevelModel(int width, int height)
    {
        Width = width;
        Height = height;
    }
}