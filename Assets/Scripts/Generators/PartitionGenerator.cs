// -------------------------------------------------- //
// Scripts/Generators/PartitionGenerator.cs
// -------------------------------------------------- //

using System.Collections.Generic;
using UnityEngine;

public class PartitionGenerator
{
    public PartitionModel GeneratePartitionTree(LevelConfig levelConfig, PartitionConfig partitionConfig, System.Random random)
    {
        if (levelConfig == null || partitionConfig == null || random == null)
        {
            Debug.LogError("PartitionGenerator: Null parameters provided to GeneratePartitionTree");
            return null;
        }

        var rootBounds = new RectInt(0, 0, levelConfig.Width, levelConfig.Height);
        var root = new PartitionModel(rootBounds);
        
        SplitPartition(root, partitionConfig, random);
        return root;
    }

    private void SplitPartition(PartitionModel partition, PartitionConfig config, System.Random random)
    {
        if (partition == null || config == null) return;

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
            SplitVertically(partition, config, splitRatio, random);
        }
        else
        {
            SplitHorizontally(partition, config, splitRatio, random);
        }

        // Recursively split children
        SplitPartition(partition.LeftChild, config, random);
        SplitPartition(partition.RightChild, config, random);
    }

    private void SplitVertically(PartitionModel partition, PartitionConfig config, float splitRatio, System.Random random)
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

    private void SplitHorizontally(PartitionModel partition, PartitionConfig config, float splitRatio, System.Random random)
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
}