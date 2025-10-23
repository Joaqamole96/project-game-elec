// ==================================================
// Corridor Model
// -----------
// A data model of one single Corridor and its data.
// A functional representation of an edge. A Corridor
// connects two points on the map. Usually, the
// points are on the Openings of a Room, but the
// points can also be on other Corridors.
// ==================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FGA.Models
{
    public enum PathOrientation { XFirst, YFirst }
    public enum PathShape { IShape, LShape, ZShape, YShape }
    public enum EndType { Opening, CorridorPoint }
    public class CorridorModel
    {
        #region Properties

        // Initialized Variables
        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }

        // Accessor Variables
        public int StartX => Start.x;
        public int StartY => Start.y;
        public int EndX => End.x;
        public int EndY => End.y;

        // Configured Variables
        public List<Vector2Int> Path { get; private set; }

        // State Flags
        public bool IsInitialized { get; private set; } = false;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public CorridorModel(Vector2Int start, Vector2Int end)
        {
            ValidateInitialize();

            Start = start;
            End = end;

            IsInitialized = true;
            Debug.Log($"CorridorModel: Initialized (Start: ({StartX}, {StartY}), End: ({EndX}, {EndY})).");
        }

        public CorridorModel(int startX, int startY, int endX, int endY)
            : this(new Vector2Int(startX, startY), new Vector2Int(endX, endY)) { }

        private void ValidateInitialize()
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("CorridorModel: Cannot be re-initialized.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Path Generation

        public void ConnectPointsByManhattan()
        {
            
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

        public void Illustrate()
        {
            // Code to illustrate path shape in the console
        }

        #endregion
    }
}