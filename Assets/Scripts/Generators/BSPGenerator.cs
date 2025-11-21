using System.Collections.Generic;
using UnityEngine;

public class BSPGenerator
{
    public PartitionModel GeneratePartitionTree(LevelModel layout, PartitionConfig partitionConfig, System.Random random)
    {
        if (layout == null || partitionConfig == null || random == null)
        {
            Debug.LogError("BSPGenerator: Null parameters provided to GeneratePartitionTree");
            return null;
        }

        RectInt rootBounds = new(0, 0, layout.Width, layout.Height);
        PartitionModel root = new(rootBounds);
        SplitPartition(root, partitionConfig, random);
        return root;
    }

    private void SplitPartition(PartitionModel partition, PartitionConfig config, System.Random random)
    {
        if (partition == null || config == null) return;

        bool canSplitVertically = (partition.Bounds.width > PartitionModel.MAX_SIZE);
        bool canSplitHorizontally = (partition.Bounds.height > PartitionModel.MAX_SIZE);
        
        if ((partition.Bounds.width <= PartitionModel.MIN_SIZE) || (partition.Bounds.height <= PartitionModel.MIN_SIZE))
            return;

        bool splitVertically;
        if (canSplitVertically && canSplitHorizontally)
            splitVertically = partition.Bounds.width > partition.Bounds.height;
        else if (canSplitVertically)
            splitVertically = true;
        else if (canSplitHorizontally)
            splitVertically = false;
        else
            return;

        float splitRatio = (float)(random.NextDouble() * (config.MaxSplitRatio - config.MinSplitRatio) + config.MinSplitRatio);
        
        if (splitVertically)
            SplitVertically(partition, config, splitRatio, random);
        else
            SplitHorizontally(partition, config, splitRatio, random);

        SplitPartition(partition.LeftChild, config, random);
        SplitPartition(partition.RightChild, config, random);
    }

    private void SplitVertically(PartitionModel partition, PartitionConfig config, float splitRatio, System.Random random)
    {
        int minSplit = PartitionModel.MIN_SIZE;
        int maxSplit = partition.Bounds.width - PartitionModel.MIN_SIZE;
        if (maxSplit <= minSplit) return;

        int splitPoint = Mathf.Clamp(
            Mathf.RoundToInt(partition.Bounds.width * splitRatio),
            minSplit,
            maxSplit
        );

        partition.LeftChild = new(new(partition.Bounds.x, partition.Bounds.y, splitPoint, partition.Bounds.height));
        partition.RightChild = new(new(partition.Bounds.x + splitPoint, partition.Bounds.y, partition.Bounds.width - splitPoint, partition.Bounds.height));
    }

    private void SplitHorizontally(PartitionModel partition, PartitionConfig config, float splitRatio, System.Random random)
    {
        int minSplit = PartitionModel.MIN_SIZE;
        int maxSplit = partition.Bounds.height - PartitionModel.MIN_SIZE;
        if (maxSplit <= minSplit) return;

        int splitPoint = Mathf.Clamp(
            Mathf.RoundToInt(partition.Bounds.height * splitRatio),
            minSplit,
            maxSplit
        );

        partition.LeftChild = new(new(partition.Bounds.x, partition.Bounds.y, partition.Bounds.width, splitPoint));
        partition.RightChild = new(new(partition.Bounds.x, partition.Bounds.y + splitPoint, partition.Bounds.width, partition.Bounds.height - splitPoint));
    }

    public List<PartitionModel> CollectLeafPartitions(PartitionModel root)
    {
        List<PartitionModel> leaves = new();
        CollectLeavesRecursive(root, leaves);
        return leaves;
    }

    private void CollectLeavesRecursive(PartitionModel partition, List<PartitionModel> leaves)
    {
        if (partition == null) return;
        
        if (partition.IsLeaf)
            leaves.Add(partition);
        else
        {
            CollectLeavesRecursive(partition.LeftChild, leaves);
            CollectLeavesRecursive(partition.RightChild, leaves);
        }
    }

    public List<RoomModel> CreateRoomsFromPartitions(List<PartitionModel> leaves, System.Random random)
    {
        List<RoomModel> rooms = new();
        int roomIdCounter = 0;
        
        foreach (var leaf in leaves)
        {
            if (leaf == null) continue;
            
            var room = CreateRoomInPartition(leaf, random, roomIdCounter);
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

    private RoomModel CreateRoomInPartition(PartitionModel leaf, System.Random random, int roomId)
    {
        int maxHorizontalInset = (leaf.Bounds.width - RoomModel.MIN_SIZE) / 2;
        int maxVerticalInset = (leaf.Bounds.height - RoomModel.MIN_SIZE) / 2;
        
        int leftInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxHorizontalInset);
        int rightInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxHorizontalInset);
        int bottomInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxVerticalInset);
        int topInset = Mathf.Clamp(random.Next(RoomModel.MIN_INSET, RoomModel.MAX_INSET + 1), 1, maxVerticalInset);

        int roomWidth = leaf.Bounds.width - (leftInset + rightInset);
        int roomHeight = leaf.Bounds.height - (bottomInset + topInset);

        // Ensure room meets minimum size requirements
        if (roomWidth < RoomModel.MIN_SIZE || roomHeight < RoomModel.MIN_SIZE)
        {
            int neededWidth = RoomModel.MIN_SIZE - roomWidth;
            int neededHeight = RoomModel.MIN_SIZE - roomHeight;
            
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

        if (roomBounds.width >= RoomModel.MIN_SIZE && roomBounds.height >= RoomModel.MIN_SIZE)
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

    public void FindAndAssignNeighbors(List<PartitionModel> partitions)
    {
        if (partitions == null) return;
        
        foreach (var partition in partitions)
            partition.Neighbors.Clear();

        var rightEdgeMap = new Dictionary<int, List<PartitionModel>>();
        var bottomEdgeMap = new Dictionary<int, List<PartitionModel>>();

        foreach (var partition in partitions)
        {
            if (partition == null) continue;
            
            if (!rightEdgeMap.ContainsKey(partition.Bounds.xMax))
                rightEdgeMap[partition.Bounds.xMax] = new List<PartitionModel>();
            rightEdgeMap[partition.Bounds.xMax].Add(partition);

            if (!bottomEdgeMap.ContainsKey(partition.Bounds.yMax))
                bottomEdgeMap[partition.Bounds.yMax] = new List<PartitionModel>();
            bottomEdgeMap[partition.Bounds.yMax].Add(partition);
        }

        foreach (var partition in partitions)
        {
            if (partition == null) continue;
            
            if (rightEdgeMap.TryGetValue(partition.Bounds.xMin, out var horizontalCandidates))
                FindValidNeighbors(partition, horizontalCandidates);
                
            if (bottomEdgeMap.TryGetValue(partition.Bounds.yMin, out var verticalCandidates))
                FindValidNeighbors(partition, verticalCandidates);
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
        bool touchHorizontally = (boundsA.xMax == boundsB.xMin) || (boundsB.xMax == boundsA.xMin);
        bool touchVertically = (boundsA.yMax == boundsB.yMin) || (boundsB.yMax == boundsA.yMin);
        bool overlapX = (boundsA.xMin < boundsB.xMax) && (boundsB.xMin < boundsA.xMax);
        bool overlapY = (boundsA.yMin < boundsB.yMax) && (boundsB.yMin < boundsA.yMax);
        
        return (touchHorizontally && overlapY) || (touchVertically && overlapX);
    }
}