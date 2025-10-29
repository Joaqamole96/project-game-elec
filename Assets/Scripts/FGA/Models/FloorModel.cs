// ==================================================
// Floor Model
// -----------
// A data model representing a Floor.
// ==================================================

using System.Collections.Generic;
using UnityEngine;
using static FGA.Configs.FloorConfig;
using static FGA.Configs.PartitionConfig;
using static Helpers.LogHelper;

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

        public FloorModel(int level, int seed)
        {
            ValidateInitialize(level);

            Level = level;
            Declare(this, Level);
            Seed = seed;
            Declare(this, Seed);
            Bounds = SetBounds(level);
            Declare(this, Bounds);
            Depth = SetDepth(Width, Height);
            Declare(this, Depth);

            IsInitialized = true; 
            Success(this, "This Floor has been initialized.");
        }

        private void ValidateInitialize(int level)
        {
            if (IsInitialized)
            {
                throw Failure(this, "This Floor is already initialized.");
            }
            if (level < MIN_FLOOR_LEVEL)
            {
                throw Failure(this, $"This Floor's Level must be at least {MIN_FLOOR_LEVEL}.");
            }
            if (level > MAX_FLOOR_LEVEL)
            {
                throw Failure(this, $"This Floor's Level must be at most {MAX_FLOOR_LEVEL}.");
            }
        }

        private RectInt SetBounds(int level)
        {
            // Increase Size by 1 for every 3 Levels.
            int growthIncrement = Mathf.FloorToInt((level - 1) / 3f);
            // Set dimension to 60 + 20 * growthIncrement. Is 60 at Level 1, and 160 at Level 15.
            int dimension = BASE_PARTITION_SIZE + PARTITION_GROWTH * growthIncrement;

            int width, height;
            // Width and height is set to 
            width = height = Mathf.Clamp(dimension, MIN_FLOOR_SIZE, MAX_FLOOR_SIZE);

            return new RectInt(FLOOR_POSITION, new Vector2Int(width, height));
        }

        private int SetDepth(int width, int height)
        {
            int depthX = Mathf.FloorToInt(Mathf.Log(width / 20f, 2));
            int depthY = Mathf.FloorToInt(Mathf.Log(height / 20f, 2));

            int geometricDepth = Mathf.Min(depthX, depthY);

            float scale = Mathf.Clamp01(width * height / 40000f);
            int heuristicDepth = Mathf.RoundToInt(Mathf.Lerp(2f, geometricDepth, scale));

            return Mathf.Clamp(heuristicDepth, 1, 6);
        }

        //----------------------------------------------------------------------------------

        public void Outfit(List<PartitionModel> partitions)
        {
            ValidateOutfit(partitions);

            Partitions = partitions;
            Declare(this, Partitions);

            IsOutfitted = true; 
            Success(this, "This Floor has been outfitted.");
        }

        private void ValidateOutfit(List<PartitionModel> partitions)
        {
            if (IsOutfitted)
            {
                throw Failure(this, "This Floor is already outfitted.");
            }
            if (partitions == null)
            {
                throw Failure(this, "This Floor's Partitions cannot be null.");
            }
            if (partitions.Count == 0)
            {
                throw Failure(this, "This Floor's Partitions cannot be empty.");
            }
        }

        //----------------------------------------------------------------------------------

        // public void Configure() {}

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