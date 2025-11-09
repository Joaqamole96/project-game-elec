// PartitionConfig.cs
using UnityEngine;

/// <summary>
/// Configuration settings for Binary Space Partitioning (BSP) algorithm.
/// Controls how the dungeon space is divided into partitions.
/// </summary>
[System.Serializable]
public class PartitionConfig
{
    [Header("Partition Size Limits")]
    [Range(10, 50)]
    [Tooltip("Minimum size for partitions (smaller values create more rooms)")]
    public int MinPartitionSize = 25;

    [Range(20, 100)]
    [Tooltip("Maximum size for partitions (larger values create bigger rooms)")]
    public int MaxPartitionSize = 35;

    [Header("Connectivity")]
    [Range(0, 10)]
    [Tooltip("Extra corridor connections beyond minimum spanning tree")]
    public int ExtraConnections = 3;

    [Header("Split Behavior")]
    [Range(0.3f, 0.7f)]
    [Tooltip("Minimum split ratio for balanced partitions (0.3 = 30/70 split)")]
    public float MinSplitRatio = 0.35f;

    [Range(0.3f, 0.7f)]
    [Tooltip("Maximum split ratio for balanced partitions (0.7 = 70/30 split)")]
    public float MaxSplitRatio = 0.65f;

    /// <summary>
    /// Gets a random split ratio within configured bounds.
    /// </summary>
    public float GetRandomSplitRatio(System.Random random)
    {
        return (float)(random.NextDouble() * (MaxSplitRatio - MinSplitRatio) + MinSplitRatio);
    }

    /// <summary>
    /// Creates a deep copy of this PartitionConfig instance.
    /// </summary>
    public PartitionConfig Clone()
    {
        return new PartitionConfig
        {
            MinPartitionSize = MinPartitionSize,
            MaxPartitionSize = MaxPartitionSize,
            ExtraConnections = ExtraConnections,
            MinSplitRatio = MinSplitRatio,
            MaxSplitRatio = MaxSplitRatio
        };
    }

    /// <summary>
    /// Validates all configuration values to ensure they are within reasonable ranges.
    /// </summary>
    public void Validate()
    {
        MinPartitionSize = Mathf.Clamp(MinPartitionSize, 10, 50);
        MaxPartitionSize = Mathf.Clamp(MaxPartitionSize, 20, 100);
        ExtraConnections = Mathf.Clamp(ExtraConnections, 0, 10);
        MinSplitRatio = Mathf.Clamp(MinSplitRatio, 0.3f, 0.7f);
        MaxSplitRatio = Mathf.Clamp(MaxSplitRatio, 0.3f, 0.7f);

        // Ensure min is less than max
        if (MinPartitionSize >= MaxPartitionSize)
        {
            MinPartitionSize = MaxPartitionSize - 5;
        }

        if (MinSplitRatio >= MaxSplitRatio)
        {
            MinSplitRatio = MaxSplitRatio - 0.1f;
        }
    }

    /// <summary>
    /// Checks if a partition of given size should be split.
    /// </summary>
    public bool ShouldSplitPartition(int partitionSize, bool isVertical)
    {
        int threshold = isVertical ? MaxPartitionSize : MaxPartitionSize;
        return partitionSize > threshold;
    }

    /// <summary>
    /// Checks if a partition is too small to split further.
    /// </summary>
    public bool IsPartitionTooSmall(int size)
    {
        return size <= MinPartitionSize;
    }
}