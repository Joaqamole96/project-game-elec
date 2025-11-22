// -------------------------------------------------- //
// Scripts/Generators/PartitionGenerator.cs
// -------------------------------------------------- //

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class PartitionGenerator
{
    public readonly System.Random _random;

    public PartitionGenerator(int seed) => _random = new(seed);

    // ------------------------- //

    public PartitionModel GeneratePartitionTree(LevelConfig levelConfig, PartitionConfig partitionConfig)
    {
        PartitionModel rootPart = new(new(0, 0, levelConfig.Width, levelConfig.Height));
        
        SplitRecursively(rootPart, partitionConfig);
        return rootPart;
    }

    private void SplitRecursively(PartitionModel part, PartitionConfig config)
    {
        if (part == null || config == null) return;

        // Ensure partitions are large enough to split
        bool canSplitVert = part.Bounds.width > config.MaxPartitionSize;
        bool canSplitHorz = part.Bounds.height > config.MaxPartitionSize;
        
        // If already within desired range, don't split
        if (part.Bounds.width <= config.MaxPartitionSize && part.Bounds.height <= config.MaxPartitionSize) return;

        // If too small, don't split
        if (part.Bounds.width <= config.MinPartitionSize || part.Bounds.height <= config.MinPartitionSize) return;

        bool splitVert;
        
        // Prefer splitting the larger dimension
        if (canSplitVert && canSplitHorz) splitVert = (part.Bounds.width > part.Bounds.height);
        else if (canSplitVert) splitVert = true;
        else if (canSplitHorz) splitVert = false;
        else return;

        // Use safer split ratio and ensure minimum sizes
        float splitRatio = (float)(_random.NextDouble() * 0.3f + 0.35f); // 0.35-0.65
        
        if (splitVert) SplitVert(part, config, splitRatio);
        else SplitHorz(part, config, splitRatio);

        // Recursively split children
        SplitRecursively(part.LeftChild, config);
        SplitRecursively(part.RightChild, config);
    }

    private void SplitVert(PartitionModel part, PartitionConfig config, float splitRatio)
    {
        int minSplit = config.MinPartitionSize;
        int maxSplit = part.Bounds.width - config.MinPartitionSize;
        
        if (maxSplit <= minSplit) return;
        
        int splitPoint = Mathf.Clamp(
            Mathf.RoundToInt(part.Bounds.width * splitRatio),
            minSplit,
            maxSplit
        );

        part.LeftChild = new(new RectInt(
            part.Bounds.x, part.Bounds.y, 
            splitPoint, part.Bounds.height));
        part.RightChild = new(new RectInt(
            part.Bounds.x + splitPoint, part.Bounds.y, 
            part.Bounds.width - splitPoint, part.Bounds.height));
    }

    private void SplitHorz(PartitionModel part, PartitionConfig config, float splitRatio)
    {
        int minSplit = config.MinPartitionSize;
        int maxSplit = part.Bounds.height - config.MinPartitionSize;
        
        if (maxSplit <= minSplit) return;
        
        int splitPoint = Mathf.Clamp(
            Mathf.RoundToInt(part.Bounds.height * splitRatio),
            minSplit,
            maxSplit
        );

        part.LeftChild = new(new RectInt(
            part.Bounds.x, part.Bounds.y, part.Bounds.width, splitPoint));
        part.RightChild = new(new RectInt(
            part.Bounds.x, part.Bounds.y + splitPoint, 
            part.Bounds.width, part.Bounds.height - splitPoint));
    }

    public List<PartitionModel> CollectLeaves(PartitionModel rootPart)
    {
        List<PartitionModel> leaves = new();

        CollectLeafRecursively(rootPart, leaves);

        return leaves;
    }

    private void CollectLeafRecursively(PartitionModel part, List<PartitionModel> leaves)
    {
        if (part == null) return;
        
        if (part.LeftChild == null && part.RightChild == null) leaves.Add(part);
        else
        {
            CollectLeafRecursively(part.LeftChild, leaves);
            CollectLeafRecursively(part.RightChild, leaves);
        }
    }
}