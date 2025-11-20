// PartitionModel.cs
using System.Collections.Generic;
using UnityEngine;

public class PartitionModel
{
    public RectInt Bounds;
    public PartitionModel LeftChild;
    public PartitionModel RightChild;
    public RoomModel Room;
    public List<PartitionModel> Neighbors;
    public bool IsLeaf => LeftChild == null && RightChild == null;

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
        Neighbors = new List<PartitionModel>();
    }

    private void CollectLeaves(PartitionModel partition, List<PartitionModel> leaves)
    {
        if (partition == null) 
            return;
        else if (partition.IsLeaf)
            leaves.Add(partition);
        else
        {
            CollectLeaves(partition.LeftChild, leaves);
            CollectLeaves(partition.RightChild, leaves);
        }
    }
}