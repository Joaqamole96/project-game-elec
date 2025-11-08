using System.Collections.Generic;
using UnityEngine;

public class PartitionModel
{
    public RectInt Bounds;
    public PartitionModel LeftChild, RightChild;
    public RoomModel Room;
    public List<PartitionModel> Neighbors;

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
        Neighbors = new List<PartitionModel>(); 
    }
}