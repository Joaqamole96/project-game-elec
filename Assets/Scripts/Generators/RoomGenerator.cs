// -------------------------------------------------- //
// Scripts/Generators/PartitionGenerator.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;

public class RoomGenerator
{
    public readonly System.Random _random;
    
    public RoomGenerator(int seed) => _random = new(seed);

    // ------------------------- //

    public List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves, RoomConfig roomConfig)
    {
        int roomIdCounter = 0;

        List<RoomModel> rooms = new();
        
        foreach (var leaf in leaves)
        {
            if (leaf == null) continue;

            var room = CreateRoomInPartition(leaf, roomConfig, roomIdCounter);
            if (room != null)
            {
                rooms.Add(room);
                leaf.Room = room;
                roomIdCounter++;
            }
        }
        
        Debug.Log($"Created {rooms.Count} rooms from {leaves.Count} parts");
        return rooms;
    }

    private RoomModel CreateRoomInPartition(PartitionModel leaf, RoomConfig roomConfig, int roomId)
    {
        // Calculate maximum possible insets
        int maxHorizontalInset = (leaf.Bounds.width - roomConfig.MinRoomSize) / 2;
        int maxVerticalInset = (leaf.Bounds.height - roomConfig.MinRoomSize) / 2;
        
        // Clamp insets to safe values
        int leftInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxHorizontalInset);
        int rightInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxHorizontalInset);
        int bottomInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxVerticalInset);
        int topInset = Mathf.Clamp(_random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxVerticalInset);

        // Ensure we have at least a minimum room size
        int roomWidth = leaf.Bounds.width - (leftInset + rightInset);
        int roomHeight = leaf.Bounds.height - (bottomInset + topInset);
        
        // Force minimum room size and ensure we have at least 5x5 for floors
        if (roomWidth < 5 || roomHeight < 5)
        {
            Debug.LogWarning($"Partition too small for room: {leaf.Bounds}. Adjusting insets...");
            
            // Use minimal insets to create smallest possible room
            leftInset = 1;
            rightInset = 1;
            bottomInset = 1;
            topInset = 1;
            
            roomWidth = Mathf.Max(5, leaf.Bounds.width - 2);
            roomHeight = Mathf.Max(5, leaf.Bounds.height - 2);
        }

        // Ensure room meets minimum size requirements
        if (roomWidth < roomConfig.MinRoomSize || roomHeight < roomConfig.MinRoomSize)
        {
            // Adjust insets to meet minimum size
            int neededWidth = roomConfig.MinRoomSize - roomWidth;
            int neededHeight = roomConfig.MinRoomSize - roomHeight;
            
            leftInset = Mathf.Max(1, leftInset - neededWidth / 2);
            rightInset = Mathf.Max(1, rightInset - neededWidth / 2);
            bottomInset = Mathf.Max(1, bottomInset - neededHeight / 2);
            topInset = Mathf.Max(1, topInset - neededHeight / 2);
            
            roomWidth = leaf.Bounds.width - (leftInset + rightInset);
            roomHeight = leaf.Bounds.height - (bottomInset + topInset);
        }

        RectInt roomBounds = new(
            leaf.Bounds.x + leftInset,
            leaf.Bounds.y + bottomInset,
            roomWidth,
            roomHeight
        );

        if (roomBounds.width >= 5 && roomBounds.height >= 5)
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

    public void FindAndAssignNeighbors(List<PartitionModel> parts)
    {
        if (parts == null) return;

        foreach (var part in parts)
            part.Neighbors.Clear();

        Dictionary<int, List<PartitionModel>> rightEdgeMap = new(), bottomEdgeMap = new();

        // Build edge maps for efficient neighbor finding
        foreach (var part in parts)
        {
            if (part == null) continue;

            // Right edge mapping
            if (!rightEdgeMap.ContainsKey(part.Bounds.xMax)) rightEdgeMap[part.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[part.Bounds.xMax].Add(part);
            
            // Bottom edge mapping
            if (!bottomEdgeMap.ContainsKey(part.Bounds.yMax)) bottomEdgeMap[part.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[part.Bounds.yMax].Add(part);
        }

        // Find horizontal neighbors
        foreach (var part in parts)
        {
            if (part == null) continue;

            if (rightEdgeMap.TryGetValue(part.Bounds.xMin, out var horizontalCandidates)) FindValidNeighbors(part, horizontalCandidates);

            if (bottomEdgeMap.TryGetValue(part.Bounds.yMin, out var verticalCandidates)) FindValidNeighbors(part, verticalCandidates);
        }
    }

    private void FindValidNeighbors(PartitionModel part, List<PartitionModel> candidates)
    {
        foreach (var candidate in candidates)
        {
            if (candidate == null || candidate == part) continue;

            if (ArePartitionsNeighbors(part.Bounds, candidate.Bounds))
            {
                if (!part.Neighbors.Contains(candidate))
                    part.Neighbors.Add(candidate);
                if (!candidate.Neighbors.Contains(part))
                    candidate.Neighbors.Add(part);
            }
        }
    }

    private bool ArePartitionsNeighbors(RectInt boundsA, RectInt boundsB)
    {
        bool touchHorz = boundsA.xMax == boundsB.xMin || boundsB.xMax == boundsA.xMin;
        bool touchVert = boundsA.yMax == boundsB.yMin || boundsB.yMax == boundsA.yMin;

        bool overlapX = boundsA.xMin < boundsB.xMax && boundsB.xMin < boundsA.xMax;
        bool overlapY = boundsA.yMin < boundsB.yMax && boundsB.yMin < boundsA.yMax;

        return (touchHorz && overlapY) || (touchVert && overlapX);
    }
}