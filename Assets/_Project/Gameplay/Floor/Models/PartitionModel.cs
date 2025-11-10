using System.Collections.Generic;
using UnityEngine;

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
    
    // NEW: Pre-assigned room type for early room assignment
    public RoomType PreAssignedRoomType { get; set; } = RoomType.Combat;
    
    /// <summary>Whether this is a leaf node (no children).</summary>
    public bool IsLeaf => LeftChild == null && RightChild == null;
    
    /// <summary>Center point of the partition.</summary>
    public Vector2Int Center => new Vector2Int(Bounds.x + Bounds.width / 2, Bounds.y + Bounds.height / 2);

    public PartitionModel(RectInt bounds)
    {
        Bounds = bounds;
        Neighbors = new List<PartitionModel>();
    }

    // KEEP all existing methods (GetLeafPartitions, CanContainRoom, Overlaps, Touches, etc.)
    // They remain valid for the new system

    /// <summary>
    /// NEW: Checks if this partition can contain a room of specified size with insets.
    /// </summary>
    public bool CanContainRoom(Vector2Int roomSize, int minInset)
    {
        int requiredWidth = roomSize.x + (minInset * 2);
        int requiredHeight = roomSize.y + (minInset * 2);
        
        return Bounds.width >= requiredWidth && Bounds.height >= requiredHeight;
    }
}