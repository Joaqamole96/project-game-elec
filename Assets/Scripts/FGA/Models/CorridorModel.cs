// ==================================================
// Corridor Model
// -----------
// A data model of one single Corridor and its data.
// A functional representation of an edge. A Corridor
// connects two points on the map. Usually, the
// points are on the Doors of a Room, but the points
// can also be on other Corridors.
// ==================================================

using UnityEngine;
using System.Collections.Generic;
using System;

namespace Models.FGA
{
    public class CorridorModel
    {
        #region Properties

        // Initialized Variables
        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }
        public List<Vector2Int> Path { get; private set; }

        // Accessor Variables
        public int StartX => Start.x;
        public int StartY => Start.x;
        public int EndX => End.x;
        public int EndY => End.y;

        // State Flags
        public bool IsInitialized { get; private set; } = false;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public CorridorModel(Vector2Int start, Vector2Int end, List<Vector2Int> path)
        {
            ValidateInitialize(path);

            Start = start;
            End = end;
            Path = path;

            IsInitialized = true;
            Debug.Log($"CorridorModel: Initialized (Start: ({StartX}, {StartY}), End: ({EndX}, {EndY}), Path ({Path.Count} tiles)).");
        }

        public CorridorModel(int startX, int startY, int endX, int endY, List<Vector2Int> path)
            : this(new Vector2Int(startX, startY), new Vector2Int(endX, endY), path) { }

        private void ValidateInitialize(List<Vector2Int> path)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("CorridorModel: Cannot be re-initialized.");
            }
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), $"CorridorModel: Declared path is invalid.");
            }
            if (path.Count == 0)
            {
                throw new ArgumentException(nameof(path), $"CorridorModel: Declared path is empty.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Debug

        public void Describe()
        {
            Debug.Log("CorridorModel: Describing this instance:\n" +
                $"Start: ({StartX}, {StartY})\n" +
                $"End: ({EndX}, {EndY})\n" +
                $"Path ({Path.Count} tiles\n"
            );
        }

        #endregion
    }
}