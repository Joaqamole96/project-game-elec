// ==================================================
// Tilemap Model
// ---------------
// A data model representing a single tilemap.
// Represents a collection of pixels in a cell.
// ==================================================

using UnityEngine;
using System;

namespace FGA.Models
{
    public enum TileType
    {
        None,
        Floor, Wall, Corridor, Door,
        // Future Implementations
        Hazard, Boost, Pillar
    }
    
    public class TilemapModel
    {
        #region Properties

        // Initialized Variables
        public TileType[,] TileMap { get; private set; }
        public Vector2Int Size { get; }

        // Accessor Variables
        public int Width => Size.x;
        public int Height => Size.y;

        // State Flags
        public bool IsInitialized { get; private set; } = false;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public TilemapModel(Vector2Int size)
        {
            ValidateInitialize(size);

            Size = size;
            TileMap = new TileType[Width, Height];
            InitializeTileMap();

            IsInitialized = true;
            Debug.Log($"TilemapModel: Initialized ({Width}x{Height}).");
        }

        private void ValidateInitialize(Vector2Int size)
        {
            if (size.x <= 0 || size.y <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), $"TilemapModel: Size ({size}) must be positive.");
            }
        }

        private void InitializeTileMap()
        {
            // By default, initialize the entire map to Wall or None
            // The Controller will then stamp the rooms/corridors over this.
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    TileMap[x, y] = TileType.Wall;
                }
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Configuration

        // The Controller will use this to "stamp" the final tile map.
        public void SetTile(TileType type, int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                // This is generally a design error, throw or warn based on preference
                Debug.LogError($"TilemapModel: Attempted to set tile outside of bounds ({x}, {y}).");
                return;
            }
            TileMap[x, y] = type;
        }

        public void SetTile(TileType type, Vector2Int position)
            => SetTile(type, position.x, position.y);

        #endregion

        //----------------------------------------------------------------------------------

        #region Accessor

        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return TileType.Wall; // Treat areas outside the map as walls
            }
            return TileMap[x, y];
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Debug

        public void Describe()
        {
            Debug.Log("TilemapModel: Describing this instance:\n" +
                $"Size: {Size}"
            );
        }

        public void Illustrate()
        {
            // Code to illustrate in the console
        }

        #endregion
    }
}