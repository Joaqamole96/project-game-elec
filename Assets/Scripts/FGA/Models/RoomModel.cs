// ==================================================
// Room Model
// -----------
// A data model representing a Room.
// ==================================================

using UnityEngine;
using System.Collections.Generic;
using static FGA.Configs.RoomConfig;
using static Helpers.LogHelper;

namespace FGA.Models
{
    public class RoomModel
    {
        // Initialized Variables
        public RectInt Bounds { get; }
        public RoomType Type { get; private set; }

        // Configured Variables
        public static Dictionary<RoomFace, RectInt> Openings { get; private set; }
        
        // State Flags
        public bool IsInitialized { get; private set; } = false;
        public bool IsOutfitted { get; private set; } = false;

        // Accessor Variables
        public Vector2Int Position => Bounds.position;
        public int X => Bounds.x;
        public int Y => Bounds.y;
        public Vector2Int Size => Bounds.size;
        public int Width => Bounds.width;
        public int Height => Bounds.height;
        public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);

        //----------------------------------------------------------------------------------

        public RoomModel(RectInt bounds, RoomType type)
        {
            ValidateInitialize(bounds, type);
            
            Bounds = bounds;
            Declare(this, Bounds);
            Type = type;
            Declare(this, Type);

            IsInitialized = true; 
            Success(this, "This Room has been initialized.");
        }

        public RoomModel(Vector2Int position, Vector2Int size, RoomType type)
            : this(new RectInt(position, size), type) { }

        public RoomModel(int x, int y, int width, int height, RoomType type)
            : this(new RectInt(x, y, width, height), type) { }

        private void ValidateInitialize(RectInt bounds, RoomType type)
        {
            if (IsInitialized)
            {
                throw Failure(this, "This Room is already initialized.");
            }
            if (bounds.width < MIN_ROOM_SIZE)
            {
                throw Failure(this, $"This Room's Width must be at least {MIN_ROOM_SIZE}.");
            }
            if (bounds.height < MIN_ROOM_SIZE)
            {
                throw Failure(this, $"This Room's Height must be at least {MIN_ROOM_SIZE}.");
            }
        }

        //----------------------------------------------------------------------------------

        public void Configure(Dictionary<RoomFace, RectInt> openings)
        {
            Openings = openings;
            // Use SetOpenings() and change params in the future
            Declare(this, Openings);

            Success(this, "This Room has been configured.");
        }

        //----------------------------------------------------------------------------------

        public void SetOpenings(List<RoomFace> roomFaces)
        {
            // foreach (RoomFace roomFace in roomFaces)
            // {
            //     if (roomFace == RoomFace.North)
            //     {
            //         Vector2Int northOpeningPosition = new Vector2Int(X + Mathf.FloorToInt(Width / 2), Y + Height);
            //         Vector2Int northOpeningSize = new Vector2Int(Width % 2 + 1);
            //     }
            // }

            // Process:
            // Foreach room face, create a 2x1 or 1x2 RectInt Opening.
            // - For north/south, place Opening anywhere between x+1 to x+width-1
            // - For west/east, place Opening anywhere between y+1 to y+height-1
        }

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