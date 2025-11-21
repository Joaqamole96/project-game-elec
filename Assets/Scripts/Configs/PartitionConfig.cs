// -------------------- //
// Scripts/Configs/PartitionConfig.cs
// -------------------- //

using UnityEngine;

[System.Serializable]
public class PartitionConfig
{
    [Range(0.3f, 0.5f)] public float MinSplitRatio = 0.3f;
    [Range(0.5f, 0.7f)] public float MaxSplitRatio = 0.7f;

    // -------------------------------------------------- //
    
    public void Validate()
    {
        MinSplitRatio = Mathf.Clamp(MinSplitRatio, 0.3f, 0.5f);
        MaxSplitRatio = Mathf.Clamp(MaxSplitRatio, 0.5f, 0.7f);

        Debug.Log("PartitionConfig.Validate(): Validated successfully.");
    }

    public PartitionConfig Clone()
    {
        Debug.Log("PartitionConfig.Clone(): Cloning...");
        return new() 
        { 
            MinSplitRatio = MinSplitRatio, 
            MaxSplitRatio = MaxSplitRatio,
        };
    }
}