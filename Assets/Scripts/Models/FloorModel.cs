// ================================= //
// FloorModel.cs
// 
// A data model representing a floor in the game.
// Contains properties relevant to floor data management.
// ================================= //

// ---------- Imports ---------- //

using UnityEngine;
using System.Collections.Generic;

// ---------- Data Structures ---------- //

[System.Serializable]
public enum SplitType { Horizontal, Vertical }

// ------- 1. Partition ------- //

// Represents an area of the grid.
[System.Serializable]
public class Partition
{
    // ----- Attributes ----- //
    
    // The rectangular bounds of the partition in grid coordinates.
    public RectInt Area { get; private set; } 

    // The depth of the partition in the BSP tree.
    public int Depth { get; private set; }

    // Child partitions resulting from a split.
    public Partition LeftChild { get; set; }
    public Partition RightChild { get; set; }

    // Indicates if the partition is a leaf node (ready for a room).
    public bool IsLeaf { get; set; } = false;

    // The rectangular bounds of the room contained within this partition (only for leaf nodes).
    public RectInt? Room { get; set; } // Nullable RectInt

    // List of edge points within the partition.
    public List<Vector2Int> EdgePoints { get; private set; } = new List<Vector2Int>();

    // ----- Methods ----- //

    // --- Constructor -- //
    // Initializes a new partition with the specified area and depth.
    public Partition(RectInt area, int depth)
    {
        Area = area;
        Depth = depth;
    }
}

// ------- 2. Path ------- //

// Functionally represents a Corridor, or an edge between nodes.
[System.Serializable]
public struct Path
{
    public Partition PartitionA { get; private set; }
    public Partition PartitionB { get; private set; }

    // NEW: List of discrete tile coordinates that make up the corridor.
    public List<Vector2Int> PathTiles { get; private set; }

    public Path(Partition a, Partition b, List<Vector2Int> pathTiles)
    {
        PartitionA = a;
        PartitionB = b;
        PathTiles = pathTiles;
    }
}

// ------- 3. Floor Model ------- //

// Represents the data model for a floor in the game.
public class FloorModel
{
    public Partition RootPartition;
    // The root partition of the BSP tree.
    public List<Partition> Partitions { get; private set; } = new List<Partition>();
    // List of all leaf partitions in the BSP tree.
    public List<Path> Paths { get; private set; } = new List<Path>();
    // List of paths (corridors) connecting partitions.
}
