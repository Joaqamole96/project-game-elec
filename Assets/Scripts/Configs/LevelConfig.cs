// LevelConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for floor generation, size, and progression.
/// </summary>
[System.Serializable]
public class LevelConfig
{
    [Header("Randomization")]
    [Tooltip("Seed for deterministic dungeon generation (0 for random)")]
    public int Seed = 0;

    [Header("Floor Progression")]
    [Range(1, 100)]
    [Tooltip("Current floor level (affects difficulty and theme)")]
    public int FloorLevel = 1;

    [Header("Floor Dimensions")]
    [Range(50, 1000)]
    [Tooltip("Width of the dungeon in tiles")]
    public int Width = 150;

    [Range(50, 1000)]
    [Tooltip("Height of the dungeon in tiles")]
    public int Height = 150;

    [Header("Progression Scaling")]
    [Range(5, 50)]
    [Tooltip("How much the floor grows each level")]
    public int FloorGrowth = 20;

    [Range(50, 200)]
    [Tooltip("Minimum floor size for generation")]
    public int MinFloorSize = 100;

    [Range(200, 2000)]
    [Tooltip("Maximum floor size to prevent performance issues")]
    public int MaxFloorSize = 1000;

    /// <summary>
    /// Gets the floor area in tiles (width × height).
    /// </summary>
    public int Area => Width * Height;

    /// <summary>
    /// Gets the aspect ratio of the floor (width / height).
    /// </summary>
    public float AspectRatio => (float)Width / Height;

    /// <summary>
    /// Creates a deep copy of this LevelConfig instance.
    /// </summary>
    public LevelConfig Clone()
    {
        return new LevelConfig
        {
            Seed = Seed,
            FloorLevel = FloorLevel,
            Width = Width,
            Height = Height,
            FloorGrowth = FloorGrowth,
            MinFloorSize = MinFloorSize,
            MaxFloorSize = MaxFloorSize
        };
    }

    /// <summary>
    /// Validates all configuration values to ensure they are within reasonable ranges.
    /// </summary>
    public void Validate()
    {
        Seed = Mathf.Max(0, Seed);
        FloorLevel = Mathf.Clamp(FloorLevel, 1, 100);
        Width = Mathf.Clamp(Width, 50, 1000);
        Height = Mathf.Clamp(Height, 50, 1000);
        FloorGrowth = Mathf.Clamp(FloorGrowth, 5, 50);
        MinFloorSize = Mathf.Clamp(MinFloorSize, 50, 200);
        MaxFloorSize = Mathf.Clamp(MaxFloorSize, 200, 2000);

        // Ensure min is less than max
        if (MinFloorSize >= MaxFloorSize)
        {
            MinFloorSize = MaxFloorSize - 50;
        }
    }

    /// <summary>
    /// Grows the floor size based on progression settings.
    /// </summary>
    /// <param name="growWidth">If true, grow width; otherwise grow height.</param>
    public void GrowFloor(bool growWidth)
    {
        if (growWidth)
        {
            Width = Mathf.Min(Width + FloorGrowth, MaxFloorSize);
        }
        else
        {
            Height = Mathf.Min(Height + FloorGrowth, MaxFloorSize);
        }
    }

    /// <summary>
    /// Checks if the floor has reached maximum size.
    /// </summary>
    public bool IsAtMaxSize => Width >= MaxFloorSize && Height >= MaxFloorSize;
}