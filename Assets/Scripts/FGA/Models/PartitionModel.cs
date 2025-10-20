// ==================================================
// Partition Model
// ---------------
// A data model representing a single partition.
// Represents a cell in a grid, as well as a node in
// a Binary Space Partitioning tree.
// ==================================================

using UnityEngine;
using System;

namespace Models.FGA
{
    public enum SplitOrientation { None, Horizontal, Vertical }
    public enum Level { Root, Internal, Leaf }
    public enum Exposure { Root, Core, Edge, Corner } 

    public class PartitionModel
    {
        #region Properties

        // Initialized Variables
        public RectInt Bounds { get; }
        public int CurrentDepth { get; }

        // Accessor Variables
        public Vector2Int Position => Bounds.position;
        public int X => Bounds.x;
        public int Y => Bounds.y;
        public Vector2Int Size => Bounds.size;
        public int Width => Bounds.width;
        public int Height => Bounds.height;
        public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);

        // Configured Variables
        public SplitOrientation SplitOrientation { get; private set; } = SplitOrientation.None;
        public int SplitAxis { get; private set; } = 0;
        public Level Level { get; private set; }
        public Exposure Exposure { get; private set; }

        // Outfitted Variables
        public RoomModel Room { get; private set; }

        // Cached Variables
        public PartitionModel ChildA { get; private set; }
        public PartitionModel ChildB { get; private set; }

        // State Flags
        public bool IsInitialized { get; private set; } = false;
        public bool IsOutfitted { get; private set; } = false;
        public bool CanBeSplit => Width > 1 && Height > 1;

        #endregion

        //----------------------------------------------------------------------------------

        #region Initialization

        public PartitionModel(RectInt bounds, int depth)
        {
            ValidateInitialize(bounds, depth);

            Bounds = bounds;
            CurrentDepth = depth;

            IsInitialized = true;
            Debug.Log($"PartitionModel: Initialized (Bounds: {Bounds}, Depth: {CurrentDepth}).");
        }

        public PartitionModel(Vector2Int position, Vector2Int size, int depth)
            : this(new RectInt(position, size), depth) { }

        public PartitionModel(int x, int y, int width, int height, int depth)
            : this(new RectInt(x, y, width, height), depth) { }

        private void ValidateInitialize(RectInt bounds, int depth)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("PartitionModel: Cannot be re-initialized.");
            }
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bounds), $"PartitionModel: Declared area ({bounds.width}x{bounds.height}) is too small.");
            }
            if (depth < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(depth), $"PartitionModel: Declared depth ({depth}) is invalid.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Configuration

        public void Configure(SplitOrientation splitOrientation, int splitAxis, Level level, Exposure exposure)
        {
            SplitOrientation = splitOrientation;
            SplitAxis = splitAxis;
            Level = level;
            Exposure = exposure;

            Debug.Log($"PartitionModel: Configured (Split: ({SplitOrientation}; {SplitAxis}), Level: {Level}, Exposure: {Exposure}).");
        }

        public void Configure(SplitOrientation splitOrientation, int splitAxis)
        {
            SplitOrientation = splitOrientation;
            SplitAxis = splitAxis;
            Debug.Log($"PartitionModel: Split has been configured ({SplitOrientation}; {SplitAxis}).");
        }

        public void Configure(Level level, Exposure exposure)
        {
            Level = level;
            Exposure = exposure;
            Debug.Log($"PartitionModel: Environment has been configured (Level: {Level}, Exposure: {Exposure}).");
        }

        public void Configure() => Configure(SplitOrientation.None, 0, Level.Root, Exposure.Root);

        #endregion

        //----------------------------------------------------------------------------------

        #region Outfitting

        public void Outfit(RoomModel room)
        {
            ValidateOutfit(room);

            Room = room;
            IsOutfitted = true;
            Debug.Log($"PartitionModel: Outfitted with (Room: {Room}).");
        }

        private void ValidateOutfit(RoomModel room)
        {
            if (IsOutfitted)
            {
                throw new InvalidOperationException("PartitionModel: Already outfitted.");
            }
            if (room == null)
            {
                throw new ArgumentNullException(nameof(room), $"PartitionModel: Declared room ({room}) is null.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Partitioning

        public (PartitionModel ChildA, PartitionModel ChildB) Split()
        {
            if (!CanBeSplit)
            {
                throw new InvalidOperationException($"PartitionModel: Cannot be split; my dimensions ({Width}x{Height}) are too small.");
            }

            SplitOrientation splitOrientation = DetermineSplitOrientation();

            (int minAxis, int maxAxis) = GetSplitAxisRange(splitOrientation);

            if (minAxis >= maxAxis)
            {
                throw new InvalidOperationException($"PartitionModel: Generated split range is inverted ({minAxis} >= {maxAxis}).");
            }
            
            int splitAxis = UnityEngine.Random.Range(minAxis, maxAxis);
            
            (RectInt childABounds, RectInt childBBounds) = CalculateChildBounds(splitOrientation, splitAxis);

            if (childABounds.width <= 0 || childABounds.height <= 0 ||
                childBBounds.width <= 0 || childBBounds.height <= 0)
            {
                throw new Exception("PartitionModel: Split produced an invalid child rectangle (size <= 0).");
            }

            int childDepth = CurrentDepth + 1;
            PartitionModel childA = new PartitionModel(childABounds, childDepth);
            childA.Configure(splitOrientation, splitAxis);
            PartitionModel childB = new PartitionModel(childBBounds, childDepth);
            childB.Configure(splitOrientation, splitAxis);

            ChildA = childA;
            ChildB = childB;

            Debug.Log($"PartitionModel: Has been split into two children (Orientation: {splitOrientation}, Axis: {splitAxis}).");
            return (childA, childB);
        }

        private SplitOrientation DetermineSplitOrientation()
        {
            SplitOrientation splitOrientation;

            if (Width > Height)
            {
                splitOrientation = SplitOrientation.Vertical;
            }
            else if (Width < Height)
            {
                splitOrientation = SplitOrientation.Horizontal;
            }
            else
            {
                splitOrientation = UnityEngine.Random.value < 0.5f ? SplitOrientation.Vertical : SplitOrientation.Horizontal;
            }

            return splitOrientation;
        }

        private (int minAxis, int maxAxis) GetSplitAxisRange(SplitOrientation splitOrientation)
        {
            switch (splitOrientation)
            {
                case SplitOrientation.Vertical:
                    return (X + 1, X + Width); 
                case SplitOrientation.Horizontal:
                    return (Y + 1, Y + Height);
                case SplitOrientation.None:
                default:
                    throw new InvalidOperationException($"PartitionModel: Cannot get split axis range for Orientation {splitOrientation}.");
            }
        }

        private (RectInt childA, RectInt childB) CalculateChildBounds(SplitOrientation splitOrientation, int splitAxis)
        {
            switch (splitOrientation)
            {
                case SplitOrientation.Vertical:
                    // Child A: left side
                    int childAWidth = splitAxis - X;
                    RectInt childABounds = new RectInt(X, Y, childAWidth, Height);

                    // Child B: right side
                    int childBWidth = X + Width - splitAxis;
                    RectInt childBBounds = new RectInt(splitAxis, Y, childBWidth, Height);
                    
                    return (childABounds, childBBounds);

                case SplitOrientation.Horizontal:
                    // Child A: bottom side
                    int childAHeight = splitAxis - Y;
                    RectInt childABoundsH = new RectInt(X, Y, Width, childAHeight);

                    // Child B: top side
                    int childBHeight = Y + Height - splitAxis;
                    RectInt childBBoundsH = new RectInt(X, splitAxis, Width, childBHeight);
                    
                    return (childABoundsH, childBBoundsH);

                case SplitOrientation.None:
                default:
                    throw new InvalidOperationException($"PartitionModel: Cannot calculate child bounds for Orientation {splitOrientation}.");
            }
        }

        #endregion

        //----------------------------------------------------------------------------------

        #region Debug

        public void Describe()
        {
            Debug.Log("PartitionModel: Describing this instance:\n" +
                $"Bounds: {Bounds}\n" +
                $"Current Depth: {CurrentDepth}\n" +
                $"Split: ({SplitOrientation}; {SplitAxis})\n" +
                $"Level: {Level}\n" +
                $"Exposure: {Exposure}\n" +
                $"Room: {(Room != null ? Room.ToString() : "None")}"
            );
        }

        #endregion
    }
}