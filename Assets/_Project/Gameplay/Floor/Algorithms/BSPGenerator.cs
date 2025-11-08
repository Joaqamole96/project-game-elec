using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BSPGenerator
{
    public PartitionModel GeneratePartitionTree(LevelConfig levelConfig, PartitionConfig partitionConfig, System.Random random)
    {
        var root = new PartitionModel(new RectInt(0, 0, levelConfig.Width, levelConfig.Height));
        SplitPartition(root, partitionConfig, random);
        return root;
    }

    private void SplitPartition(PartitionModel partition, PartitionConfig config, System.Random random)
    {
        // FIX: Ensure partitions are large enough to split
        bool canSplitVertically = partition.Bounds.width > config.MaxPartitionSize;
        bool canSplitHorizontally = partition.Bounds.height > config.MaxPartitionSize;
        
        // If already within desired range, don't split
        if (partition.Bounds.width <= config.MaxPartitionSize && 
            partition.Bounds.height <= config.MaxPartitionSize)
            return;

        // If too small, don't split
        if (partition.Bounds.width <= config.MinPartitionSize || 
            partition.Bounds.height <= config.MinPartitionSize)
            return;

        bool splitVertically;
        
        // Prefer splitting the larger dimension
        if (canSplitVertically && canSplitHorizontally)
        {
            splitVertically = partition.Bounds.width > partition.Bounds.height;
        }
        else if (canSplitVertically)
        {
            splitVertically = true;
        }
        else if (canSplitHorizontally)
        {
            splitVertically = false;
        }
        else
        {
            return; // Cannot split in either direction
        }

        // FIX: Use safer split ratio and ensure minimum sizes
        float splitRatio = (float)(random.NextDouble() * 0.3f + 0.35f); // 0.35-0.65
        
        if (splitVertically)
        {
            int minSplit = config.MinPartitionSize;
            int maxSplit = partition.Bounds.width - config.MinPartitionSize;
            
            if (maxSplit <= minSplit) return; // Cannot split safely
            
            int splitPoint = Mathf.Clamp(
                Mathf.RoundToInt(partition.Bounds.width * splitRatio),
                minSplit,
                maxSplit
            );

            partition.LeftChild = new PartitionModel(new RectInt(
                partition.Bounds.x, partition.Bounds.y, splitPoint, partition.Bounds.height));
            partition.RightChild = new PartitionModel(new RectInt(
                partition.Bounds.x + splitPoint, partition.Bounds.y, 
                partition.Bounds.width - splitPoint, partition.Bounds.height));
        }
        else
        {
            int minSplit = config.MinPartitionSize;
            int maxSplit = partition.Bounds.height - config.MinPartitionSize;
            
            if (maxSplit <= minSplit) return; // Cannot split safely
            
            int splitPoint = Mathf.Clamp(
                Mathf.RoundToInt(partition.Bounds.height * splitRatio),
                minSplit,
                maxSplit
            );

            partition.LeftChild = new PartitionModel(new RectInt(
                partition.Bounds.x, partition.Bounds.y, partition.Bounds.width, splitPoint));
            partition.RightChild = new PartitionModel(new RectInt(
                partition.Bounds.x, partition.Bounds.y + splitPoint, 
                partition.Bounds.width, partition.Bounds.height - splitPoint));
        }

        SplitPartition(partition.LeftChild, config, random);
        SplitPartition(partition.RightChild, config, random);
    }

    public List<PartitionModel> CollectLeafPartitions(PartitionModel root)
    {
        var leaves = new List<PartitionModel>();
        CollectLeavesRecursive(root, leaves);
        return leaves;
    }

    private void CollectLeavesRecursive(PartitionModel partition, List<PartitionModel> leaves)
    {
        if (partition == null) return;
        
        if (partition.LeftChild == null && partition.RightChild == null)
            leaves.Add(partition);
        else
        {
            CollectLeavesRecursive(partition.LeftChild, leaves);
            CollectLeavesRecursive(partition.RightChild, leaves);
        }
    }

    public List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves, RoomConfig roomConfig, System.Random random)
    {
        var rooms = new List<RoomModel>();
        int roomIdCounter = 0;
        
        foreach (var leaf in leaves)
        {
            // Calculate maximum possible insets
            int maxHorizontalInset = (leaf.Bounds.width - roomConfig.MinRoomSize) / 2;
            int maxVerticalInset = (leaf.Bounds.height - roomConfig.MinRoomSize) / 2;
            
            // Clamp insets to safe values
            int leftInset = Mathf.Clamp(random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxHorizontalInset);
            int rightInset = Mathf.Clamp(random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxHorizontalInset);
            int bottomInset = Mathf.Clamp(random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxVerticalInset);
            int topInset = Mathf.Clamp(random.Next(roomConfig.MinInset, roomConfig.MaxInset + 1), 1, maxVerticalInset);

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

            RectInt roomBounds = new RectInt(
                leaf.Bounds.x + leftInset,
                leaf.Bounds.y + bottomInset,
                roomWidth,
                roomHeight
            );

            // Final safety check
            if (roomBounds.width >= 3 && roomBounds.height >= 3)
            {
                var room = new RoomModel(roomBounds, roomIdCounter++, RoomType.Combat);
                leaf.Room = room;
                rooms.Add(room);
                Debug.Log($"Created room {room.ID}: {roomBounds} (Size: {roomBounds.width}x{roomBounds.height})");
            }
            else
            {
                Debug.LogWarning($"Skipped room creation - bounds too small: {roomBounds}");
            }
        }
        
        Debug.Log($"Created {rooms.Count} rooms from {leaves.Count} partitions");
        return rooms;
    }

    public void FindAndAssignNeighbors(List<PartitionModel> partitions)
    {
        foreach (var partition in partitions)
            partition.Neighbors.Clear();

        var rightEdgeMap = new Dictionary<int, List<PartitionModel>>();
        var bottomEdgeMap = new Dictionary<int, List<PartitionModel>>();

        foreach (var partition in partitions)
        {
            if (!rightEdgeMap.ContainsKey(partition.Bounds.xMax))
                rightEdgeMap[partition.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[partition.Bounds.xMax].Add(partition);
            
            if (!bottomEdgeMap.ContainsKey(partition.Bounds.yMax))
                bottomEdgeMap[partition.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[partition.Bounds.yMax].Add(partition);
        }

        foreach (var partition in partitions)
        {
            if (rightEdgeMap.TryGetValue(partition.Bounds.xMin, out var horizontalCandidates))
            {
                foreach (var candidate in horizontalCandidates)
                {
                    if (ArePartitionsNeighbors(partition.Bounds, candidate.Bounds))
                    {
                        partition.Neighbors.Add(candidate);
                        candidate.Neighbors.Add(partition);
                    }
                }
            }

            if (bottomEdgeMap.TryGetValue(partition.Bounds.yMin, out var verticalCandidates))
            {
                foreach (var candidate in verticalCandidates)
                {
                    if (ArePartitionsNeighbors(partition.Bounds, candidate.Bounds))
                    {
                        partition.Neighbors.Add(candidate);
                        candidate.Neighbors.Add(partition);
                    }
                }
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