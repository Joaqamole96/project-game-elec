// ============================================================================
// Models for BSP Dungeon Generation
// Each section (FloorModel, GridModel, etc.) should be in its own file under FGA/Models
// ============================================================================

using System.Collections.Generic;
using UnityEngine;

namespace FGA.Models
{
    // ============================================================================
    // FloorModel.cs
    // Represents the entire floor layout, composed of partitions, rooms, corridors, and cells
    // ============================================================================
    public class FloorModel
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public GridModel Grid { get; private set; }
        public PartitionModel RootPartition { get; set; }
        public List<RoomModel> Rooms { get; set; } = new();
        public List<CorridorModel> Corridors { get; set; } = new();

        public FloorModel(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new GridModel(width, height);
        }

        public void PrintToConsole() // Debugging only
        {
            Debug.Log($"Floor ({Width}x{Height}) contains {Rooms.Count} rooms and {Corridors.Count} corridors.");
        }
    }

    // ============================================================================
    // GridModel.cs
    // Represents a grid of cells used for occupancy and connectivity tracking
    // ============================================================================
    public class GridModel
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public CellModel[,] Cells { get; private set; }

        public GridModel(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new CellModel[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Cells[x, y] = new CellModel(x, y);
                }
            }
        }
    }

    // ============================================================================
    // CellModel.cs
    // Represents a single tile or unit of space in the grid
    // ============================================================================
    public class CellModel
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public bool IsOccupied { get; set; }

        public CellModel(int x, int y)
        {
            X = x;
            Y = y;
            IsOccupied = false;
        }
    }

    // ============================================================================
    // PartitionModel.cs
    // Represents a rectangular partition in the BSP tree structure
    // ============================================================================
    public class PartitionModel
    {
        public RectInt Bounds { get; private set; }
        public int Depth { get; private set; }
        public bool IsLeaf => LeftChild == null && RightChild == null;
        public PartitionModel LeftChild { get; set; }
        public PartitionModel RightChild { get; set; }
        public SplitOrientation Orientation { get; private set; }
        public RoomModel Room { get; set; }

        public PartitionModel(RectInt bounds, int depth, SplitOrientation orientation)
        {
            Bounds = bounds;
            Depth = depth;
            Orientation = orientation;
        }
    }

    // ============================================================================
    // SplitOrientation.cs
    // Enum for indicating BSP split direction
    // ============================================================================
    public enum SplitOrientation { Horizontal, Vertical }

    // ============================================================================
    // RoomModel.cs
    // Represents a rectangular room assigned to a leaf partition
    // ============================================================================
    public class RoomModel
    {
        public RectInt Bounds { get; private set; }

        public RoomModel(RectInt bounds)
        {
            Bounds = bounds;
        }
    }

    // ============================================================================
    // CorridorModel.cs
    // Represents a corridor connecting two rooms
    // ============================================================================
    public class CorridorModel
    {
        public Vector2Int Start { get; private set; }
        public Vector2Int End { get; private set; }
        public List<Vector2Int> Path { get; private set; }

        public CorridorModel(Vector2Int start, Vector2Int end, List<Vector2Int> path)
        {
            Start = start;
            End = end;
            Path = path;
        }
    }
}