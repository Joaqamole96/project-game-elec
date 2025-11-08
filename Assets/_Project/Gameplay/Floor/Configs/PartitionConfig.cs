using UnityEngine;

[System.Serializable]
public class PartitionConfig
{
    [Header("Partition Configuration")]
    public int MinPartitionSize = 25;
    public int MaxPartitionSize = 35;
    public int ExtraConnections = 3;
}