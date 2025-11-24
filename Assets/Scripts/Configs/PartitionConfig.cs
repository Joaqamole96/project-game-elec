// -------------------------------------------------- //
// Scripts/Configs/PartitionConfig.cs
// -------------------------------------------------- //

using UnityEngine;

[System.Serializable]
public class PartitionConfig : ScriptableObject
{
    [Range(10, 50)] public int MinPartitionSize = 25;

    [Range(20, 100)] public int MaxPartitionSize = 35;

    [Range(0, 10)] public int ExtraConnections = 3;

    [Range(0.3f, 0.7f)] public float MinSplitRatio = 0.35f;

    [Range(0.3f, 0.7f)] public float MaxSplitRatio = 0.65f;
    
    // public float GetRandomSplitRatio(System.Random random) => (float)(random.NextDouble() * (MaxSplitRatio - MinSplitRatio) + MinSplitRatio);

    // ------------------------- //
    
    // public PartitionConfig Clone() => this;

    public void Validate()
    {
        MinPartitionSize = Mathf.Clamp(MinPartitionSize, 10, 50);
        MaxPartitionSize = Mathf.Clamp(MaxPartitionSize, 20, 100);
        ExtraConnections = Mathf.Clamp(ExtraConnections, 0, 10);
        MinSplitRatio = Mathf.Clamp(MinSplitRatio, 0.3f, 0.7f);
        MaxSplitRatio = Mathf.Clamp(MaxSplitRatio, 0.3f, 0.7f);

        // Ensure min is less than max
        if (MinPartitionSize >= MaxPartitionSize) MinPartitionSize = MaxPartitionSize - 5;

        if (MinSplitRatio >= MaxSplitRatio) MinSplitRatio = MaxSplitRatio - 0.1f;
    }

    // public bool ShouldSplitPartition(int partitionSize, bool isVertical)
    // {
    //     int threshold = isVertical ? MaxPartitionSize : MaxPartitionSize;
    //     return partitionSize > threshold;
    // }

    // public bool IsPartitionTooSmall(int size) => size <= MinPartitionSize;
}