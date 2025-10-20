// ==================================================
// Floor Model
// -----------
// A data model representing a single Floor object.
// Serves as a functional representation of a grid,
// complete with its area, maximum depth, and cell
// partitions.
// ==================================================

using UnityEngine;
using System;
using System.Collections.Generic;

namespace Models.FGA
{
    public class FloorModel
    {
        #region Properties

        // Initialized Variables
        public RectInt Bounds { get; }
        public int MaximumDepth { get; }
        public int Seed { get; }

        // Accessor Variables
        public Vector2Int Position => Bounds.position;
        public int X => Bounds.x;
        public int Y => Bounds.y;
        public Vector2Int Size => Bounds.size;
        public int Width => Bounds.width;
        public int Height => Bounds.height;
        public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);

        // Outfitted Variables
        public List<PartitionModel> Partitions { get; private set; }

        // State Flags
        public bool IsInitialized { get; private set; } = false;
        public bool IsOutfitted { get; private set; } = false;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public FloorModel(RectInt bounds, int maximumDepth, int seed = 0)
        {
            ValidateInitialize(bounds, maximumDepth, seed);

            Bounds = bounds;
            MaximumDepth = maximumDepth;
            Seed = seed;

            IsInitialized = true;
            Debug.Log($"FloorModel: Initialized (Bounds: {Bounds}, Maximum Depth: {MaximumDepth}, Seed: {Seed}).");
        }

        public FloorModel(Vector2Int position, Vector2Int size, int maximumDepth, int seed = 0)
            : this(new RectInt(position, size), maximumDepth, seed) { }

        public FloorModel(int x, int y, int width, int height, int maximumDepth, int seed = 0)
            : this(new RectInt(x, y, width, height), maximumDepth, seed) { }

        private void ValidateInitialize(RectInt bounds, int maximumDepth, int seed)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("FloorModel: Cannot be re-initialized.");
            }
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds), $"FloorModel: Declared area ({bounds.width}x{bounds.height}) is too small.");
            }
            if (maximumDepth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumDepth), $"FloorModel: Declared maximum depth ({maximumDepth}) is invalid.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Outfitting

        public void Outfit(List<PartitionModel> partitions)
        {
            ValidateOutfit(partitions);

            Partitions = partitions;
            IsOutfitted = true;
            Debug.Log($"FloorModel: Outfitted (Partitions: {Partitions.Count}).");
        }

        private void ValidateOutfit(List<PartitionModel> partitions)
        {
            if (IsOutfitted)
            {
                throw new InvalidOperationException("FloorModel: Already outfitted.");
            }
            if (partitions == null)
            {
                throw new ArgumentNullException(nameof(partitions), "FloorModel: Cannot outfit with a null partitions list.");
            }
            if (partitions.Count == 0)
            {
                throw new ArgumentException("FloorModel: Cannot outfit with an empty partitions list.", nameof(partitions));
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Debug

        public string Describe()
        {
            return "FloorModel: Describing this instance:\n" +
                $"Bounds: {Bounds}\n" +
                $"Maximum Depth: {MaximumDepth}\n" +
                $"Seed: {Seed}\n" +
                $"Partitions: {(Partitions != null ? Partitions.Count.ToString() : "None")}";
        }

        #endregion
    }
}