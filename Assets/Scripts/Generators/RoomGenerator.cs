using UnityEngine;
using System.Collections.Generic;

// WARNING!: This code was derived from legacy code. There are several errors and outdated processes in this code.
public class RoomGenerator
{
    public List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves, RoomModel roomModel, System.Random random)
    {
        var rooms = new List<RoomModel>();
        int roomIdCounter = 0;
        
        foreach (var leaf in leaves)
        {
            if (leaf == null) continue;

            var room = CreateRoomInPartition(leaf, roomModel, random, roomIdCounter);
            if (room != null)
            {
                rooms.Add(room);
                leaf.Room = room;
                roomIdCounter++;
            }
        }
        
        Debug.Log($"Created {rooms.Count} rooms from {leaves.Count} partitions");
        return rooms;
    }

    private RoomModel CreateRoomInPartition(PartitionModel leaf, RoomModel room, System.Random random, int roomId)
    {
        // Calculate maximum possible insets
        int maxHorizontalInset = (leaf.Bounds.width - room.MIN_SIZE) / 2;
        int maxVerticalInset = (leaf.Bounds.height - room.MIN_SIZE) / 2;
        
        // Clamp insets to safe values
        int leftInset = Mathf.Clamp(random.Next(room.MIN_INSET, room.MAX_INSET + 1), 1, maxHorizontalInset);
        int rightInset = Mathf.Clamp(random.Next(room.MIN_INSET, room.MAX_INSET + 1), 1, maxHorizontalInset);
        int bottomInset = Mathf.Clamp(random.Next(room.MIN_INSET, room.MAX_INSET + 1), 1, maxVerticalInset);
        int topInset = Mathf.Clamp(random.Next(room.MIN_INSET, room.MAX_INSET + 1), 1, maxVerticalInset);

        // Ensure we have at least a minimum room size
        int roomWidth = leaf.Bounds.width - (leftInset + rightInset);
        int roomHeight = leaf.Bounds.height - (bottomInset + topInset);
        
        // FIX: Force minimum room size and ensure we have at least 3x3 for floors
        if (roomWidth < 3 || roomHeight < 3)
        {
            Debug.LogWarning($"Partition too small for room: {leaf.Bounds}. Adjusting insets...");
            
            // Use minimal insets to create smallest possible room
            leftInset = 1;
            rightInset = 1;
            bottomInset = 1;
            topInset = 1;
            
            roomWidth = Mathf.Max(3, leaf.Bounds.width - 2);
            roomHeight = Mathf.Max(3, leaf.Bounds.height - 2);
        }

        // Ensure room meets minimum size requirements
        if (roomWidth < room.MIN_SIZE || roomHeight < room.MIN_SIZE)
        {
            // Adjust insets to meet minimum size
            int neededWidth = room.MIN_SIZE - roomWidth;
            int neededHeight = room.MIN_SIZE - roomHeight;
            
            leftInset = Mathf.Max(1, leftInset - neededWidth / 2);
            rightInset = Mathf.Max(1, rightInset - neededWidth / 2);
            bottomInset = Mathf.Max(1, bottomInset - neededHeight / 2);
            topInset = Mathf.Max(1, topInset - neededHeight / 2);
            
            roomWidth = leaf.Bounds.width - (leftInset + rightInset);
            roomHeight = leaf.Bounds.height - (bottomInset + topInset);
        }

        RectInt roomBounds = new RectInt(
            leaf.Bounds.x + leftInset,
            leaf.Bounds.y + bottomInset,
            roomWidth,
            roomHeight
        );

        // Final safety check
        if (roomBounds.width >= 3 && roomBounds.height >= 3)
        {
            var room = new RoomModel(roomBounds, roomId, RoomType.Combat);
            Debug.Log($"Created room {room.ID}: {roomBounds} (Size: {roomBounds.width}x{roomBounds.height})");
            return room;
        }
        else
        {
            Debug.LogWarning($"Skipped room creation - bounds too small: {roomBounds}");
            return null;
        }
    }

    /// <summary>
    /// Finds and assigns neighboring partitions for corridor generation.
    /// </summary>
    public void FindAndAssignNeighbors(List<PartitionModel> partitions)
    {
        if (partitions == null) return;

        foreach (var partition in partitions)
            partition.Neighbors.Clear();

        var rightEdgeMap = new Dictionary<int, List<PartitionModel>>();
        var bottomEdgeMap = new Dictionary<int, List<PartitionModel>>();

        // Build edge maps for efficient neighbor finding
        foreach (var partition in partitions)
        {
            if (partition == null) continue;

            // Right edge mapping
            if (!rightEdgeMap.ContainsKey(partition.Bounds.xMax))
                rightEdgeMap[partition.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[partition.Bounds.xMax].Add(partition);
            
            // Bottom edge mapping
            if (!bottomEdgeMap.ContainsKey(partition.Bounds.yMax))
                bottomEdgeMap[partition.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[partition.Bounds.yMax].Add(partition);
        }

        // Find horizontal neighbors
        foreach (var partition in partitions)
        {
            if (partition == null) continue;

            if (rightEdgeMap.TryGetValue(partition.Bounds.xMin, out var horizontalCandidates))
            {
                FindValidNeighbors(partition, horizontalCandidates);
            }

            if (bottomEdgeMap.TryGetValue(partition.Bounds.yMin, out var verticalCandidates))
            {
                FindValidNeighbors(partition, verticalCandidates);
            }
        }
    }

    private void FindValidNeighbors(PartitionModel partition, List<PartitionModel> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate == null || candidate == partition) continue;

            if (ArePartitionsNeighbors(partition.Bounds, candidate.Bounds))
            {
                if (!partition.Neighbors.Contains(candidate))
                    partition.Neighbors.Add(candidate);
                if (!candidate.Neighbors.Contains(partition))
                    candidate.Neighbors.Add(partition);
            }
        }
    }

    private bool ArePartitionsNeighbors(RectInt boundsA, RectInt boundsB)
    {
        bool touchHorizontally = boundsA.xMax == boundsB.xMin || boundsB.xMax == boundsA.xMin;
        bool touchVertically = boundsA.yMax == boundsB.yMin || boundsB.yMax == boundsA.yMin;

        bool overlapX = boundsA.xMin < boundsB.xMax && boundsB.xMin < boundsA.xMax;
        bool overlapY = boundsA.yMin < boundsB.yMax && boundsB.yMin < boundsA.yMax;

        return (touchHorizontally && overlapY) || (touchVertically && overlapX);
    }
}