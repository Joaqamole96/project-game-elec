// PartitionModel.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a partition in the Binary Space Partitioning (BSP) tree.
/// Used during dungeon generation to divide space into rooms.
/// </summary>
public class PartitionModel
{
    /// <summary>Bounds of this partition in grid coordinates.</summary>
    public RectInt Bounds { get; private set; }
    
    /// <summary>Left child partition (null if leaf node).</summary>
    public PartitionModel LeftChild { get; set; }
    
    /// <summary>Right child partition (null if leaf node).</summary>
    public PartitionModel RightChild { get; set; }
    
    /// <summary>Room generated within this partition (null if not a leaf).</summary>
    public RoomModel Room { get; set; }
    
    /// <summary>Adjacent partitions for corridor generation.</summary>
    public List<PartitionModel> Neighbors { get; private set; }
    
    /// <summary>Whether this is a leaf node (no children).</summary>
    public bool IsLeaf => LeftChild == null && RightChild == null;
    
    /// <summary>Center point of the partition.</summary>
    public Vector2Int Center => new(Bounds.x + Bounds.width / 2, Bounds.y + Bounds.height / 2);

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
        Neighbors = new List<PartitionModel>();
    }

    /// <summary>
    /// Gets all leaf partitions in this subtree.
    /// </summary>
    public List<PartitionModel> GetLeafPartitions()
    {
        var leaves = new List<PartitionModel>();
        CollectLeaves(this, leaves);
        return leaves;
    }

    private void CollectLeaves(PartitionModel partition, List<PartitionModel> leaves)
    {
        if (partition == null) return;
        
        if (partition.IsLeaf)
        {
            leaves.Add(partition);
        }
        else
        {
            CollectLeaves(partition.LeftChild, leaves);
            CollectLeaves(partition.RightChild, leaves);
        }
    }

    /// <summary>
    /// Checks if this partition can contain a room of the specified minimum size.
    /// </summary>
    public bool CanContainRoom(int minRoomSize)
    {
        return Bounds.width >= minRoomSize && Bounds.height >= minRoomSize;
    }

    /// <summary>
    /// Gets the area of this partition in tiles.
    /// </summary>
    public int Area => Bounds.width * Bounds.height;

    /// <summary>
    /// Checks if this partition overlaps with another partition.
    /// </summary>
    public bool Overlaps(PartitionModel other)
    {
        return Bounds.Overlaps(other.Bounds);
    }

    /// <summary>
    /// Checks if this partition touches another partition on any edge.
    /// </summary>
    public bool Touches(PartitionModel other)
    {
        return Bounds.xMax == other.Bounds.xMin || Bounds.xMin == other.Bounds.xMax ||
               Bounds.yMax == other.Bounds.yMin || Bounds.yMin == other.Bounds.yMax;
    }
}