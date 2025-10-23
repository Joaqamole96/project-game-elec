// ==================================================
// Floor Model
// -----------
// A data model representing a Floor.
// ==================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using static FGA.Configs.FloorConfig;
using static FGA.Configs.PartitionConfig;

namespace FGA.Models
{
    public class FloorModel
    {
        // Initialized Variables
        public int Level { get; private set; }
        public int Seed { get; private set; }
        public RectInt Bounds { get; private set; }
        public int Depth { get; private set; }

        // Outfitted Variables
        public List<PartitionModel> Partitions { get; private set; }
        public TilemapModel Tilemap { get; private set; }

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

        private bool ValidateInitialize(int level)
        {
            if (IsInitialized)
            {
                throw new InvalidOperationException($"This object is already initialized.");
            }
            else if (level < MIN_FLOOR_LEVEL)
            {
                throw new ArgumentOutOfRangeException($"`level` ({level}) is below {MIN_FLOOR_LEVEL}.");
            }
            else if (level > MAX_FLOOR_LEVEL)
            {
                throw new ArgumentOutOfRangeException($"`level` ({level}) is above {MAX_FLOOR_LEVEL}.");
            }
            else
            {
                return true;
            }
        }

        private RectInt SetBounds(int level)
        {
            int growthIncrement = Mathf.FloorToInt((level - 1) / 3f);
            Debug.Log($"FloorModel(SetBounds): Growth increment is calculated to be {growthIncrement}.");
            int dimension = BASE_PARTITION_SIZE + PARTITION_GROWTH * growthIncrement;
            Debug.Log($"FloorModel(SetBounds): Dimension is calculated to be {dimension}.");

            int width, height;
            width = height = Mathf.Clamp(dimension, MIN_FLOOR_SIZE, MAX_FLOOR_SIZE);
            Debug.Log($"FloorModel(SetBounds): Width and height are calculated to be {width} x {height}.");
            
            return new RectInt(FLOOR_POSITION, new Vector2Int(width, height));
        }

        private int SetDepth(int width, int height)
        {
            int depthX = Mathf.FloorToInt(Mathf.Log(width / 20f, 2));
            Debug.Log($"FloorModel(SetDepth): Depth X is calculated to be {depthX}.");
            int depthY = Mathf.FloorToInt(Mathf.Log(height / 20f, 2));
            Debug.Log($"FloorModel(SetDepth): Depth Y is calculated to be {depthY}.");

            int geometricDepth = Mathf.Min(depthX, depthY);
            Debug.Log($"FloorModel(SetDepth): Geometric is calculated to be {geometricDepth}.");

            float scale = Mathf.Clamp01(width * height / 40000f);
            Debug.Log($"FloorModel(SetDepth): Scale is calculated to be {scale}.");
            int heuristicDepth = Mathf.RoundToInt(Mathf.Lerp(2f, geometricDepth, scale));
            Debug.Log($"FloorModel(SetDepth): Heuristic depth is calculated to be {heuristicDepth}.");

            return Mathf.Clamp(heuristicDepth, 1, 6);
        }

        public FloorModel(int level, int seed)
        {
            if (ValidateInitialize(level))
            {
                Level = level;
                Debug.Log($"FloorModel(FloorModel): `Level` is set to {Level}.");
                Seed = seed;
                Debug.Log($"FloorModel(FloorModel): `Seed` is set to {Seed}.");
                Bounds = SetBounds(level);
                Debug.Log($"FloorModel(FloorModel): `Bounds` is set to {Bounds}.");
                Depth = SetDepth(Width, Height);
                Debug.Log($"FloorModel(FloorModel): `Depth` is set to {Depth}.");

                IsInitialized = true;
                Debug.Log($"FloorModel(FloorModel): Initialization success!");
            }
            else
            {
                Debug.Log($"FloorModel(FloorModel): Initialization failure!");
            }
        }

        //----------------------------------------------------------------------------------

        private bool ValidateOutfit(List<PartitionModel> partitions)
        {
            if (IsOutfitted)
            {
                throw new InvalidOperationException($"This object is already outfitted.");
            }
            else if (partitions == null)
            {
                throw new ArgumentNullException($"`partitions` {partitions} is null.");
            }
            else if (partitions.Count == 0)
            {
                throw new ArgumentException($"`partitions` {partitions} is empty.");
            }
            else
            {
                return true;
            }
        }

        public void Outfit(List<PartitionModel> partitions)
        {
            if (ValidateOutfit(partitions))
            {
                Partitions = partitions;
                Debug.Log($"FloorModel(Outfit): `Partitions` is set to {partitions}.");

                IsOutfitted = true;
                Debug.Log($"FloorModel(Outfit): Outfitting success!");
            }
            else
            {
                Debug.Log($"FloorModel(Outfit): Outfitting failure!");
            }
        }

        //----------------------------------------------------------------------------------

        // public void Configure() {}

        //----------------------------------------------------------------------------------

        public void Describe()
        {
            // Code to describe this object's attributes in the console
        }

        public void Illustrate()
        {
            // Code to illustrate this object's visual appearance in the console
        }
    }
}