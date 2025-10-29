// ==================================================
// Tilemap Model
// ---------------
// A data model representing a collection of pixels
// in a cell.
// ==================================================

using System;
using UnityEngine;
using static Configs.FGA.TilemapConfig;
using static Helpers.LogHelper;

namespace FGA.Models
{
    public class TilemapModel
    {
        // Initialized Variables
        public TileType[,] Tilemap { get; private set; }
        public Vector2Int Size { get; }

        // Accessor Variables
        public int Width => Size.x;
        public int Height => Size.y;

        // State Flags
        public bool IsInitialized { get; private set; } = false;

        //----------------------------------------------------------------------------------

        public TilemapModel(Vector2Int size)
        {
            ValidateInitialize(size);
            
            Size = size;
            Declare(this, Size);
            Tilemap = SetTilemap(size.x, size.y);
            Declare(this, Tilemap);

            IsInitialized = true;
            Success(this, "This Tilemap has been initialized.");
        }

        private void ValidateInitialize(Vector2Int size)
        {
            if (IsInitialized)
            {
                throw Failure(this, "This Tilemap is already initialized,");
            }
            if (size.x <= 0)
            {
                throw Failure(this, "This Tilemap's Width must be atleast 1.");
            }
            if (size.y <= 0)
            {
                throw Failure(this, "This Tilemap's Height must be atleast 1.");
            }
        }

        private TileType[,] SetTilemap(int width, int height)
        {
            TileType[,] tileMap = new TileType[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileMap[x, y] = TileType.Wall;
                }
            }

            return tileMap;
        }

        //----------------------------------------------------------------------------------

        public void SetTile(TileType type, int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw Failure(this, "The Tile cannot be placed outside of the bounds.");
            }
            Tilemap[x, y] = type;
        }

        public void SetTile(TileType type, Vector2Int position)
            => SetTile(type, position.x, position.y);

        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return TileType.Wall;
            }
            return Tilemap[x, y];
        }

        public void GetTile(Vector2Int position)
            => GetTile(position.x, position.y);

        //----------------------------------------------------------------------------------

        public void Describe()
        {
            // Describe the class object's attributes in the console.
        }

        public void Illustrate()
        {
            // Illustrate the class object as a visual display in the console.
        }
    }
}