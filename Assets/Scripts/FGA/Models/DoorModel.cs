// ==================================================
// Door Model
// -----------
// A data model of one single Door and its data. A
// functional representation of a node-edge
// connection.
// ==================================================

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Models.FGA
{
    public class DoorModel
    {
        #region Properties

        // Initialized Variables
        public RectInt Bounds { get; private set; }
        public RoomModel Room { get; private set; }

        // Accessor Variables
        public Vector2Int Position => Bounds.position;
        public int X => Bounds.x;
        public int Y => Bounds.y;
        public Vector2Int Size => Bounds.size;
        public int Width => Bounds.width;
        public int Height => Bounds.height;

        // State Flags
        public bool IsInitialized { get; private set; } = false;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public DoorModel(RectInt bounds, RoomModel room)
        {
            ValidateInitialize(bounds, room);

            Bounds = bounds;
            Room = room;

            IsInitialized = true;
            Debug.Log($"DoorModel: Initialized (Bounds: {Bounds}), Room: {Room}");
        }

        public DoorModel(Vector2Int position, Vector2Int size, RoomModel room)
            : this(new RectInt(position, size), room) { }

        public DoorModel(int x, int y, int width, int height, RoomModel room)
            : this(new RectInt(x, y, width, height), room) { }

        private void ValidateInitialize(RectInt bounds, RoomModel room)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("DoorModel: Cannot be re-initialized.");
            }
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds), $"DoorModel: Declared area ({bounds.width}x{bounds.height}) is too small.");
            }
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), $"CorridorModel: Declared room is invalid.");
            }
            if (!room.Bounds.Contains(bounds.position))
            {
                throw new ArgumentException(nameof(bounds), $"DoorModel: Declared position ({bounds.x}, {bounds.y}) is outside the parent room.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Debug

        public void Describe()
        {
            Debug.Log("DoorModel: Describing this instance:\n" +
                $"Bounds: {Bounds}\n" +
                $"Room: {Room}"
            );
        }

        #endregion
    }
}