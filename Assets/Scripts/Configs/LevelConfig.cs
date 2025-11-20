// LevelConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for floor generation, size, and progression.
/// </summary>
[System.Serializable]
public class LevelConfig
{
    [Tooltip("Seed for deterministic dungeon generation (0 for random)")]
    public int Seed = 0;

    [Tooltip("Current floor level (affects difficulty and biome)")]
    [Range(1, 100)] public int LevelNumber = 1;

    [Tooltip("Width of the dungeon in tiles")]
    [Range(50, 1000)] public int Width = 150;

    [Tooltip("Height of the dungeon in tiles")]
    [Range(50, 1000)] public int Height = 150;

    [Tooltip("How much the floor grows each level")]
    [Range(5, 50)] public int Growth = 20;

    [Tooltip("Minimum floor size for generation")]
    [Range(50, 200)] public int MinSize = 100;
    
    [Tooltip("Maximum floor size to prevent performance issues")]
    [Range(200, 2000)] public int MaxSize = 1000;

    /// <summary> Validates all configuration values to ensure they are within reasonable ranges. </summary>
    public void Validate()
    {
        // Clamp values to their defined ranges
        Seed = Mathf.Max(0, Seed);
        LevelNumber = Mathf.Clamp(LevelNumber, 1, 100);
        Width = Mathf.Clamp(Width, 50, 1000);
        Height = Mathf.Clamp(Height, 50, 1000);
        Growth = Mathf.Clamp(Growth, 5, 50);
        MinSize = Mathf.Clamp(MinSize, 50, 200);
        MaxSize = Mathf.Clamp(MaxSize, 200, 2000);
    }
}