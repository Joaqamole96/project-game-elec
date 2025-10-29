// ==================================================
// Partition Model
// ---------------
// A data model representing a Partition.
// ==================================================

using UnityEngine;
using URandom = UnityEngine.Random;
using static FGA.Configs.PartitionConfig;
using FGA.Configs;
using static Helpers.LogHelper;

namespace FGA.Models
{
    public class PartitionModel
    {
        // Initialized Variables
        public RectInt Bounds { get; private set; }
        public int Depth { get; private set; }

        // Outfitted Variables
        public RoomModel Room { get; private set; }

        // Configured Variables
        public NodeLevel NodeLevel { get; private set; }
        public NodeExposure NodeExposure { get; private set; }

        // Cached Variables
        public (PartitionModel LeftChild, PartitionModel RightChild) Children { get; private set; }

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
        public PartitionModel LeftChild => Children.LeftChild;
        public PartitionModel RightChild => Children.RightChild;

        //----------------------------------------------------------------------------------

        public PartitionModel(RectInt bounds, int depth)
        {
            ValidateInitialize(bounds, depth);

            Bounds = bounds;
            Declare(this, Bounds);
            Depth = depth;
            Declare(this, Depth);

            IsInitialized = true; 
            Success(this, "This Partition has been initialized.");
        }

        public PartitionModel(Vector2Int position, Vector2Int size, int depth)
            : this(new RectInt(position, size), depth) { }

        public PartitionModel(int x, int y, int width, int height, int depth)
            : this(new RectInt(x, y, width, height), depth) { }

        private void ValidateInitialize(RectInt bounds, int depth)
        {
            if (IsInitialized)
            {
                throw Failure(this, "This Partition is already initialized.");
            }
            if (bounds.width < MIN_PARTITION_SIZE)
            {
                throw Failure(this, $"This Partition's Width must be atleast {MIN_PARTITION_SIZE}.");
            }
            if (bounds.height < MIN_PARTITION_SIZE)
            {
                throw Failure(this, $"This Partition's Height must be atleast {MIN_PARTITION_SIZE}.");
            }
            if (depth < 0)
            {
                throw Failure(this, "This Partition's Depth must be atleast 0.");
            }
        }

        //----------------------------------------------------------------------------------

        public void Outfit(RoomModel room)
        {
            ValidateOutfit(room);

            Room = room;
            Declare(this, Room);

            IsOutfitted = true;
            Success(this, "This Partition has been outfitted.");
        }

        private void ValidateOutfit(RoomModel room)
        {
            if (IsOutfitted)
            {
                throw Failure(this, "This Partition is already outfitted.");
            }
            if (room == null)
            {
                throw Failure(this, "This Partition's Room cannot be null.");
            }
            if (!Bounds.Contains(room.Position))
            {
                throw Failure(this, "This Partition's Room cannot be outside of the bounds.");
            }
        }

        //----------------------------------------------------------------------------------

        public void Configure(NodeLevel nodeLevel, NodeExposure nodeExposure)
        {
            NodeLevel = nodeLevel;
            Declare(this, NodeLevel);
            NodeExposure = nodeExposure;
            Declare(this, NodeExposure);

            Success(this, "This Partition has been configured.");
        }

        public void ConfigureRoot() => Configure(NodeLevel.Root, NodeExposure.Root);

        //----------------------------------------------------------------------------------

        public (PartitionModel LeftChild, PartitionModel RightChild) Split()
        {
            // Make depth of children 1 more than the parent.
            int childDepth = Depth + 1;
            // Determine split orientation based on size.
            SplitOrientation splitOrientation = DetermineSplitOrientation();
            // Get split axis.
            (int minAxis, int maxAxis) = GetSplitAxisRange(splitOrientation);
            int splitAxis = URandom.Range(minAxis, maxAxis);
            // Set bounds of children based on split orientation and axis.
            (RectInt leftChildBounds, RectInt rightChildBounds) = CalculateChildrenBounds(splitOrientation, splitAxis);
            
            // Initialize children.
            PartitionModel leftChild = new PartitionModel(leftChildBounds, childDepth);
            PartitionModel rightChild = new PartitionModel(rightChildBounds, childDepth);

            // Cache children.
            Children = (leftChild, rightChild);

            Success(this, "This Partition has been split into two children Partitions.");
            return (leftChild, rightChild);
        }

        private SplitOrientation DetermineSplitOrientation()
        {
            if (Width > Height)
            {
                return SplitOrientation.Vertical;
            }
            else if (Width < Height)
            {
                return SplitOrientation.Horizontal;
            }
            else
            {
                return (URandom.value < 0.5f) ?
                    SplitOrientation.Vertical : 
                    SplitOrientation.Horizontal;
            }
        }

        private (int, int) GetSplitAxisRange(SplitOrientation splitOrientation)
        {
            if (splitOrientation == SplitOrientation.Vertical)
            {
                return (
                    (X + MIN_PARTITION_SIZE),
                    (X + Width - MIN_PARTITION_SIZE)
                );
            }
            else if (splitOrientation == SplitOrientation.Horizontal)
            {
                return (
                    (Y + MIN_PARTITION_SIZE),
                    (Y + Height - MIN_PARTITION_SIZE)
                );
            }
            else
            {
                return URandom.value < 0.5f ? (
                        (X + MIN_PARTITION_SIZE),
                        (X + Width - MIN_PARTITION_SIZE)
                    ) : (
                        (Y + MIN_PARTITION_SIZE),
                        (Y + Height - MIN_PARTITION_SIZE)
                    );
            }
        }

        private (RectInt, RectInt) CalculateChildrenBounds(SplitOrientation splitOrientation, int splitAxis)
        {
            if (splitOrientation == SplitOrientation.Vertical)
            {
                return (
                    new RectInt(X, Y, (splitAxis - X), Height),
                    new RectInt(splitAxis, Y, (X + Width - splitAxis), Height)
                );
            }
            else if (splitOrientation == SplitOrientation.Horizontal)
            {
                return (
                    new RectInt(X, Y, Width, (splitAxis - Y)),
                    new RectInt(X, splitAxis, Width, (Y + Height - splitAxis))
                );
            }
            else
            {
                if (URandom.value < 0.5f)
                {
                    return (
                        new RectInt(X, Y, (splitAxis - X), Height),
                        new RectInt(splitAxis, Y, (X + Width - splitAxis), Height)
                    );
                }
                else
                {
                    return (
                        new RectInt(X, Y, Width, (splitAxis - Y)),
                        new RectInt(X, splitAxis, Width, (Y + Height - splitAxis))
                    );
                }
            }
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