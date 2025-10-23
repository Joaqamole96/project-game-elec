// ==================================================
// Partition Model
// ---------------
// A data model representing a Partition.
// ==================================================

using System;
using UnityEngine;
using URandom = UnityEngine.Random;
using static FGA.Configs.FloorConfig;
using static FGA.Configs.PartitionConfig;
using FGA.Configs;

namespace FGA.Models
{
    public class PartitionModel
    {
        // Initialized Variables
        public RectInt Bounds { get; private set; }
        public int Depth { get; private set; }
        public FloorModel Floor { get; private set; }

        // Outfitted Variables
        public RoomModel Room { get; private set; }

        // Configured Variables
        public SplitOrientation SplitOrientation { get; private set; }
        public int SplitAxis { get; private set; }
        public NodeLevel NodeLevel { get; private set; }
        public NodeExposure NodeExposure { get; private set; }

        // Cached Variables
        public PartitionModel ChildA { get; private set; }
        public PartitionModel ChildB { get; private set; }

        // State Flags
        public bool IsInitialized { get; private set; } = false;
        public bool IsOutfitted { get; private set; } = false;
        public bool CanBeSplit => Width >= MIN_PARTITION_SPLIT_SIZE && Height >= MIN_PARTITION_SPLIT_SIZE;

        // Accessor Variables
        public Vector2Int Position => Bounds.position;
        public int X => Bounds.x;
        public int Y => Bounds.y;
        public Vector2Int Size => Bounds.size;
        public int Width => Bounds.width;
        public int Height => Bounds.height;
        public Vector2Int Center => Vector2Int.RoundToInt(Bounds.center);

        //----------------------------------------------------------------------------------

        private bool ValidateInitialize(RectInt bounds, int depth, FloorModel floor)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException("This object is already initialized.");
            }
            // ----------------------------------------------
            else if (!floor.Bounds.Contains(bounds.position))
            {
                throw new ArgumentOutOfRangeException($"`bounds.position` ({bounds.position}) is outside {floor.Bounds}.");
            }
            else if (bounds.width < MIN_FLOOR_SIZE || bounds.height < MIN_FLOOR_SIZE)
            {
                throw new ArgumentOutOfRangeException($"`bounds.size` ({bounds.size}) is below {MIN_FLOOR_SIZE}.");
            }
            else if (bounds.width > MAX_FLOOR_SIZE || bounds.height > MAX_FLOOR_SIZE)
            {
                throw new ArgumentOutOfRangeException($"`bounds.size` ({bounds.size}) is above {MAX_FLOOR_SIZE}.");
            }
            else if (depth < 0)
            {
                throw new ArgumentOutOfRangeException($"`depth` ({depth}) cannot be negative.");
            }
            else
            {
                return true;
            }
        }

        public PartitionModel(RectInt bounds, int depth, FloorModel floor)
        {
            if (ValidateInitialize(bounds, depth, floor))
            {
                Bounds = bounds;
                Debug.Log($"PartitionModel(PartitionModel): `Bounds` is set to {Bounds}.");
                Depth = depth;
                Debug.Log($"PartitionModel(PartitionModel): `Depth` is set to {Depth}.");
                Floor = floor;
                Debug.Log($"PartitionModel(PartitionModel): `Floor` is set to {Floor}.");

                IsInitialized = true;
                Debug.Log($"PartitionModel(PartitionModel): Initialization success!");
            }
            else
            {
                Debug.Log($"PartitionModel(PartitionModel): Initialization failure!");
            }
        }

        public PartitionModel(Vector2Int position, Vector2Int size, int depth, FloorModel floor)
            : this(new RectInt(position, size), depth, floor) { }

        public PartitionModel(int x, int y, int width, int height, int depth, FloorModel floor)
            : this(new RectInt(x, y, width, height), depth, floor) { }

        //----------------------------------------------------------------------------------

        private bool ValidateOutfit(RoomModel room)
        {
            if (IsOutfitted)
            {
                throw new InvalidOperationException("This object is already outfitted.");
            }
            else if (room == null)
            {
                throw new ArgumentNullException($"`room` ({room}) is null.");
            }
            else
            {
                return true;
            }
        }

        public void Outfit(RoomModel room)
        {
            if (ValidateOutfit(room))
            {
                Room = room;
                Debug.Log($"PartitionModel(Outfit): `Room` is set to {room}.");

                IsOutfitted = true;
                Debug.Log($"PartitionModel(Outfit): Outfitting success!");
            }
            else
            {
                Debug.Log($"PartitionModel(Outfit): Outfitting failure!");
            }
        }

        //----------------------------------------------------------------------------------

        public void ConfigureSplit(SplitOrientation splitOrientation, int splitAxis)
        {
            SplitOrientation = splitOrientation;
            Debug.Log($"PartitionModel(ConfigureSplit): `SplitOrientation` is set to {splitOrientation}.");
            SplitAxis = splitAxis;
            Debug.Log($"PartitionModel(ConfigureSplit): `SplitAxis` is set to {splitAxis}.");

            Debug.Log($"PartitionModel(ConfigureSplit): Configuration success!");
        }

        public void ConfigureNode(NodeLevel nodeLevel, NodeExposure nodeExposure)
        {
            NodeLevel = nodeLevel;
            Debug.Log($"PartitionModel(ConfigureNode): `NodeLevel` is set to {nodeLevel}.");
            NodeExposure = nodeExposure;
            Debug.Log($"PartitionModel(ConfigureNode): `NodeExposure` is set to {nodeExposure}.");

            Debug.Log($"PartitionModel(ConfigureNode): Configuration success!");
        }

        public void Configure(SplitOrientation splitOrientation, int splitAxis, NodeLevel nodeLevel, NodeExposure nodeExposure)
        {
            ConfigureSplit(splitOrientation, splitAxis);
            ConfigureNode(nodeLevel, nodeExposure);

            Debug.Log($"PartitionModel(Configure): Configuration success!");
        }

        public void ConfigureRoot() => Configure(SplitOrientation.None, 0, NodeLevel.Root, NodeExposure.Root);

        //----------------------------------------------------------------------------------

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
                splitOrientation = URandom.value < 0.5f ? SplitOrientation.Vertical : SplitOrientation.Horizontal;
            }

            return splitOrientation;
        }

        private (int minAxis, int maxAxis) GetSplitAxisRange(SplitOrientation splitOrient)
        {
            int minAxis, maxAxis;

            switch (splitOrient)
            {
                case SplitOrientation.Vertical:
                    minAxis = X + MIN_PARTITION_SIZE;
                    maxAxis = X + Width - MIN_PARTITION_SIZE;
                    break;
                case SplitOrientation.Horizontal:
                    minAxis = Y + MIN_PARTITION_SIZE;
                    maxAxis = Y + Height - MIN_PARTITION_SIZE;
                    break;
                default:
                    if (URandom.value < 0.5f)
                    {
                        minAxis = X + MIN_PARTITION_SIZE;
                        maxAxis = X + Width - MIN_PARTITION_SIZE;
                    }
                    else
                    {
                        minAxis = Y + MIN_PARTITION_SIZE;
                        maxAxis = Y + Height - MIN_PARTITION_SIZE;
                    }
                    break;
            }

            if (minAxis >= maxAxis)
            {
                throw new InvalidOperationException($"PartitionModel(Split): Generated split range is inverted; `minAxis` ({minAxis}) is greater than or equal to `maxAxis` ({maxAxis}).");
            }
            else
            {
                return (minAxis, maxAxis);
            }
        }

        private (RectInt bounds_childA, RectInt bounds_childB) CalculateChildBounds(SplitOrientation splitOrientation, int splitAxis)
        {
            RectInt bounds_childA, bounds_childB;
            switch (splitOrientation)
            {
                case SplitOrientation.Vertical:
                    // Child A: left side
                    int width_childA = splitAxis - X;
                    bounds_childA = new RectInt(X, Y, width_childA, Height);

                    // Child B: right side
                    int width_childB = X + Width - splitAxis;
                    bounds_childB = new RectInt(splitAxis, Y, width_childB, Height);

                    break;
                case SplitOrientation.Horizontal:
                    // Child A: bottom side
                    int height_childA = splitAxis - Y;
                    bounds_childA = new RectInt(X, Y, Width, height_childA);

                    // Child B: top side
                    int height_childB = Y + Height - splitAxis;
                    bounds_childB = new RectInt(X, splitAxis, Width, height_childB);

                    break;
                case SplitOrientation.None:
                default:
                    throw new InvalidOperationException($"PartitionModel(Split): `splitOrient` is set to {splitOrientation}.");
            }

            if (bounds_childA.width <= 0 || bounds_childA.height <= 0 ||
                bounds_childB.width <= 0 || bounds_childB.height <= 0)
            {
                throw new Exception("PartitionModel: Split produced an invalid child rectangle (size <= 0).");
            }
            else
            {
                return (bounds_childA, bounds_childB);
            }
        }

        public (PartitionModel childA, PartitionModel childB) Split()
        {
            // Guard
            if (!CanBeSplit)
            {
                throw new InvalidOperationException($"PartitionModel(Split): Partition cannot be split; `Width` ({Width}) or `Height` ({Height}) is too small.");
            }

            // Local variables
            // Note: Partitions are constructed by bounds, depth, and floor
            // 1. Child A
            PartitionModel childA;
            RectInt bounds_childA;
            // 2. Child B
            PartitionModel childB;
            RectInt bounds_childB;
            // 3. Both children
            int depth;
            SplitOrientation splitOrient;
            int minAxis, maxAxis, splitAxis;

            // 1. Variable assignment
            splitOrient = DetermineSplitOrientation();
            (minAxis, maxAxis) = GetSplitAxisRange(splitOrient);
            splitAxis = URandom.Range(minAxis, maxAxis);
            (bounds_childA, bounds_childB) = CalculateChildBounds(splitOrient, splitAxis);
            depth = Depth + 1;

            // 2. Children assignment
            childA = new PartitionModel(bounds_childA, depth, Floor);
            childB = new PartitionModel(bounds_childB, depth, Floor);

            // 3. Caching for future
            ChildA = childA;
            ChildB = childB;

            Debug.Log($"PartitionModel(Split): This has been split into two children.");
            return (childA, childB);
        }

        //----------------------------------------------------------------------------------

        public void Describe()
        {
            // Code to describe this object's attributes in the console
        }

        public void Illustrate()
        {
            // Code to illustrate in the console
        }
    }
}