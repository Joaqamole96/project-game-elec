// FloorModel.cs
using System.Collections.Generic;
using UnityEngine;

namespace FGA.Models
{
    [System.Serializable]
    public class RoomModel
    {
        public RectInt rect;
        public RoomType type = RoomType.Side;
        public bool onMainPath = false;
        public int depth = -1; // BFS distance
        public bool hasKey = false;

        public RoomModel() {}
        public RoomModel(RectInt r) { rect = r; }
        public Vector2Int Center => new Vector2Int((int)rect.center.x, (int)rect.center.y);
    }

    public enum RoomType { Start, MainPath, Side, Boss, Shop, Treasure }

    [System.Serializable]
    public class CorridorModel
    {
        public List<Vector2Int> tiles = new List<Vector2Int>();
        public int roomA = -1, roomB = -1;
        public CorridorModel() {}
        public CorridorModel(IEnumerable<Vector2Int> path, int a = -1, int b = -1) { tiles.AddRange(path); roomA = a; roomB = b; }
    }

    [System.Serializable]
    public class FloorModel
    {
        public int width;
        public int height;
        public int seed;

        public List<RectInt> partitions = new List<RectInt>(); // raw BSP leaves
        public List<RoomModel> rooms = new List<RoomModel>();
        public List<CorridorModel> corridors = new List<CorridorModel>();

        public List<Vector2Int> doorTiles = new List<Vector2Int>();
        public List<Vector2Int> lockedDoorTiles = new List<Vector2Int>();
        public List<Vector2Int> keyTiles = new List<Vector2Int>();

        TileType[,] tiles;

        public FloorModel(int w = 80, int h = 60, int seed = 0)
        {
            width = w; height = h; this.seed = seed;
            tiles = new TileType[width, height];
            ClearContent();
        }

        public void ClearContent()
        {
            partitions.Clear(); rooms.Clear(); corridors.Clear();
            doorTiles.Clear(); lockedDoorTiles.Clear(); keyTiles.Clear();
            tiles = new TileType[width, height];
            for (int x = 0; x < width; x++) for (int y = 0; y < height; y++) tiles[x, y] = TileType.Void;
        }

        public TileType GetTile(int x, int y) { if (InBounds(x, y)) return tiles[x, y]; return TileType.Void; }
        public void SetTile(int x, int y, TileType t) { if (InBounds(x, y)) tiles[x, y] = t; }
        public bool InBounds(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;
    }
}