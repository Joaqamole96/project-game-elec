// ============================================================================
// Controllers for BSP Dungeon Generation
// Each section (FloorController, PartitionController, etc.) should be in its own file under FGA/Controllers
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using FGA.Models;

namespace FGA.Controllers
{
    // ============================================================================
    // FloorController.cs
    // Manages high-level floor generation and coordination of BSP logic
    // ============================================================================
    public class FloorController
    {
        public FloorModel Floor { get; private set; }
        private PartitionController partitionController;

        public FloorController(int width, int height)
        {
            Floor = new FloorModel(width, height);
            partitionController = new PartitionController();
        }

        public void GenerateFloor()
        {
            // Start recursive BSP split
            Floor.RootPartition = partitionController.SplitPartition(new RectInt(0, 0, Floor.Width, Floor.Height), 0);

            // Assign rooms to leaf partitions
            Floor.Rooms = partitionController.GenerateRooms(Floor.RootPartition);

            // Connect rooms with corridors
            Floor.Corridors = partitionController.GenerateCorridors(Floor.Rooms);

            Floor.PrintToConsole(); // Debugging only
        }
    }

    // ============================================================================
    // PartitionController.cs
    // Handles recursive BSP splitting, room creation, and corridor linking
    // ============================================================================
    public class PartitionController
    {
        private const int MinPartitionSize = 1; // Minimum allowed partition size in cells
        private const int MaxDepth = 5; // Max recursive depth

        public PartitionModel SplitPartition(RectInt bounds, int depth)
        {
            if (depth >= MaxDepth || bounds.width <= MinPartitionSize || bounds.height <= MinPartitionSize)
                return new PartitionModel(bounds, depth, SplitOrientation.Horizontal);

            SplitOrientation orientation = bounds.width > bounds.height ? SplitOrientation.Vertical : SplitOrientation.Horizontal;
            int splitLine = orientation == SplitOrientation.Vertical ?
                Random.Range(1, bounds.width - 1) : Random.Range(1, bounds.height - 1);

            PartitionModel parent = new PartitionModel(bounds, depth, orientation);

            if (orientation == SplitOrientation.Vertical)
            {
                var leftRect = new RectInt(bounds.x, bounds.y, splitLine, bounds.height);
                var rightRect = new RectInt(bounds.x + splitLine, bounds.y, bounds.width - splitLine, bounds.height);
                parent.LeftChild = SplitPartition(leftRect, depth + 1);
                parent.RightChild = SplitPartition(rightRect, depth + 1);
            }
            else
            {
                var bottomRect = new RectInt(bounds.x, bounds.y, bounds.width, splitLine);
                var topRect = new RectInt(bounds.x, bounds.y + splitLine, bounds.width, bounds.height - splitLine);
                parent.LeftChild = SplitPartition(bottomRect, depth + 1);
                parent.RightChild = SplitPartition(topRect, depth + 1);
            }

            return parent;
        }

        public List<RoomModel> GenerateRooms(PartitionModel root)
        {
            List<RoomModel> rooms = new();
            Traverse(root, partition =>
            {
                if (partition.IsLeaf)
                {
                    int roomWidth = Random.Range(partition.Bounds.width / 2, partition.Bounds.width);
                    int roomHeight = Random.Range(partition.Bounds.height / 2, partition.Bounds.height);
                    int x = partition.Bounds.x + Random.Range(0, partition.Bounds.width - roomWidth);
                    int y = partition.Bounds.y + Random.Range(0, partition.Bounds.height - roomHeight);

                    RoomModel room = new(new RectInt(x, y, roomWidth, roomHeight));
                    partition.Room = room;
                    rooms.Add(room);
                }
            });
            return rooms;
        }

        public List<CorridorModel> GenerateCorridors(List<RoomModel> rooms)
        {
            List<CorridorModel> corridors = new();
            for (int i = 0; i < rooms.Count - 1; i++)
            {
                Vector2Int start = Vector2Int.RoundToInt(rooms[i].Bounds.center);
                Vector2Int end = Vector2Int.RoundToInt(rooms[i + 1].Bounds.center);
                List<Vector2Int> path = GenerateCorridorPath(start, end);
                corridors.Add(new CorridorModel(start, end, path));
            }
            return corridors;
        }

        private List<Vector2Int> GenerateCorridorPath(Vector2Int start, Vector2Int end)
        {
            List<Vector2Int> path = new();
            Vector2Int current = start;
            while (current.x != end.x)
            {
                current.x += (end.x > current.x) ? 1 : -1;
                path.Add(current);
            }
            while (current.y != end.y)
            {
                current.y += (end.y > current.y) ? 1 : -1;
                path.Add(current);
            }
            return path;
        }

        private void Traverse(PartitionModel partition, System.Action<PartitionModel> action)
        {
            if (partition == null) return;
            action(partition);
            Traverse(partition.LeftChild, action);
            Traverse(partition.RightChild, action);
        }
    }
}