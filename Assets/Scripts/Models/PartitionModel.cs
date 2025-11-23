// -------------------------------------------------- //
// Scripts/Models/PartitionModel.cs
// -------------------------------------------------- //

using System.Collections.Generic;
using UnityEngine;

public class PartitionModel
{
    public RectInt Bounds;
    public PartitionModel LeftChild;
    public PartitionModel RightChild;
    public RoomModel Room;
    public List<PartitionModel> Neighbors;

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
        Neighbors = new();
    }
}