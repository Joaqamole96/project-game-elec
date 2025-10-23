// ==================================================
// Room Model
// -----------
// A data model representing a single room.
// Represents a node within a floor or partition.
// ==================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace FGA.Models
{
    public enum RoomType
    {
        // Endpoints
        Entrance, Exit,
        // Throughpoints
        Boss, Survival, Pursuit, Puzzle,
        // Standard
        Combat, Treasure, Shop,
        // Base
        Empty
    }

    public class RoomModel
    {
        #region Properties

        // Initialized Variables
        public RectInt Bounds { get; }
        public Vector2Int Position => Bounds.position;
        public int X => Bounds.x;
        public int Y => Bounds.y;
        public Vector2Int Size => Bounds.size;
        public int Width => Bounds.width;
        public int Height => Bounds.height;
        public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);

        // Configured Variables
        public RoomType Type { get; private set; } = RoomType.Empty;

        // Outfitted Variables
        public List<Vector2Int> Openings { get; private set; }
        // State Flags
        public bool IsInitialized { get; private set; } = false;
        public bool IsOutfitted { get; private set; } = false;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public RoomModel(RectInt bounds, RoomType type)
        {
            ValidateInitialize(bounds, type);
            Bounds = bounds;
            Type = type;

            IsInitialized = true;
            Debug.Log($"RoomModel: Initialized (Bounds: {Bounds}, Type: {Type}).");
        }

        public RoomModel(Vector2Int position, Vector2Int size, RoomType type)
            : this(new RectInt(position, size), type) { }

        public RoomModel(int x, int y, int width, int height, RoomType type)
            : this(new RectInt(x, y, width, height), type) { }

        private void ValidateInitialize(RectInt bounds, RoomType type)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("RoomModel: Cannot be re-initialized.");
            }
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds), $"RoomModel: Declared area ({bounds.width}x{bounds.height}) is too small.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Outfitting

        public void AddOpening(Vector2Int point)
        {
            if (Openings == null)
            {
                Openings = new List<Vector2Int>();
            }
            Openings.Add(point);
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Debug

        public void Describe()
        {
            Debug.Log("RoomModel: Describing this instance:\n" +
                $"Bounds: {Bounds}\n" +
                $"Type: {Type}"
            );
        }

        public void Illustrate()
        {
            // Code to illustrate in the console
        }

        #endregion
    }
}