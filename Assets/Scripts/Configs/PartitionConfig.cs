// PartitionConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for Binary Space Partitioning (BSP) algorithm.
/// Controls how the dungeon space is divided into partitions.
/// </summary>
[System.Serializable]
public class PartitionConfig
{
    [Tooltip("Minimum size for partitions (smaller values create more rooms)")]
    [Range(10, 50)] public int MinSize = 25;

    [Tooltip("Maximum size for partitions (larger values create bigger rooms)")]
    [Range(20, 100)] public int MaxSize = 35;

    [Tooltip("Extra corridor connections beyond minimum spanning tree")]
    [Range(0, 10)] public int ExtraConnections = 3;

    [Tooltip("Minimum split ratio for balanced partitions (0.3 = 30/70 split)")]
    [Range(0.3f, 0.7f)] public float MinSplitRatio = 0.35f;
    
    [Tooltip("Maximum split ratio for balanced partitions (0.7 = 70/30 split)")]
    [Range(0.3f, 0.7f)] public float MaxSplitRatio = 0.65f;
    
    /// <summary> Validates all configuration values to ensure they are within reasonable ranges. </summary>
    public void Validate()
    {
        // Clamp values to their defined ranges
        MinSize = Mathf.Clamp(MinSize, 20, 30);
        MaxSize = Mathf.Clamp(MaxSize, 30, 50);
        ExtraConnections = Mathf.Clamp(ExtraConnections, 0, 10);
        MinSplitRatio = Mathf.Clamp(MinSplitRatio, 0.3f, 0.7f);
        MaxSplitRatio = Mathf.Clamp(MaxSplitRatio, 0.3f, 0.7f);
        // The minimum split ratio must be at least 0.1 less than the maximum split ratio
        if (MinSplitRatio >= MaxSplitRatio)
            MinSplitRatio = MaxSplitRatio - 0.1f;
    }
}