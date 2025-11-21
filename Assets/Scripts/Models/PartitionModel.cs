using System.Collections.Generic;
using UnityEngine;

public class PartitionModel
{
    public static readonly int MIN_SIZE = 20;
    public static readonly int MAX_SIZE = 35;
    
    public RectInt Bounds;
    public PartitionModel LeftChild;
    public PartitionModel RightChild;
    public RoomModel Room;
    public List<PartitionModel> Neighbors = new();
    public int Width => Bounds.width;
    public int Height => Bounds.height;
    public int X => Bounds.x;
    public int Y => Bounds.y;
    public bool IsLeaf => LeftChild == null && RightChild == null;

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
    }
}